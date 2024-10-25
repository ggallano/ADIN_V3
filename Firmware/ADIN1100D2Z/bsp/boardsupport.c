/*
 *---------------------------------------------------------------------------
 *
 * Copyright (c) 2022 Analog Devices, Inc. All Rights Reserved.
 * This software is proprietary to Analog Devices, Inc.
 * and its licensors.By using this software you agree to the terms of the
 * associated Analog Devices Software License Agreement.
 *
 *---------------------------------------------------------------------------
 */

/** @addtogroup bsp MAX32670 BSP
 *  @{
 */

#include <ctype.h>
#include <string.h>
#include "adi_common.h"
#include "boardsupport.h"
#include "aux_functions.h"
#include "gpio.h"
#include "nvic_table.h"
#include "tmr.h"
#include "platform\adi_eth_error.h"
#include "drivers\ltc4296_1\sccp_defines.h"

volatile uint8_t gTimerFlag = 0;
volatile uint8_t gStartTimer = 0;
volatile uint8_t gOneSecTFlag = 0;

/* for 128 prescalar and BRO clock source*/
#define MS_TICKS (57.6)
/* for 4 pre scalar and BRO clock source */
#define US_TICK  (1.84)
#define MXC_GCR_REGS *((volatile uint32_t *)0x40000004)

#define CLOCK_SOURCE   MXC_TMR_8M_CLK
/* Parameters for Continuous timer */
#define CONT_TIMER     MXC_TMR3
#define CONT_FREQ_LED  1
#define DELAY_TIMER    MXC_TMR2
#define DELAY_FREQ     14400

#define BUFF_SIZE 1
uint8_t RxData[BUFF_SIZE];
mxc_uart_req_t uart_read_req;

mxc_gpio_cfg_t gpio_led_green;
mxc_gpio_cfg_t gpio_led_red;
mxc_gpio_cfg_t gpio_led_yellow;
mxc_gpio_cfg_t gpio_led_blue;

mxc_gpio_cfg_t gpio_adin1100_linkSt;
mxc_gpio_cfg_t gpio_adin1100_phyInt;

mxc_gpio_cfg_t gpio_adin1200_linkSt;
mxc_gpio_cfg_t gpio_adin1200_phyInt;

mxc_gpio_cfg_t gpio_uc_cfg0;
mxc_gpio_cfg_t gpio_uc_cfg1;
mxc_gpio_cfg_t gpio_uc_cfg2;
mxc_gpio_cfg_t gpio_uc_cfg3;

mxc_gpio_cfg_t gpio_uc_boot;

mxc_gpio_cfg_t gpio_uc_spi0_sso;
mxc_gpio_cfg_t gpio_uc_spi1_sso;

mxc_gpio_cfg_t gpio_uc_spoe_sccpi;
mxc_gpio_cfg_t gpio_uc_spoe_sccpo;


/* @brief           BSP_getConfigPins
 *
 * @return          val8 value of the config pins
 *
 * @details         This function reads the HW Config pins [CF0, CF1, CF2, CF3] on the board
 *
 */
uint8_t BSP_getConfigPins(void)
{
	volatile uint32_t bit = 0;
	volatile uint8_t val8 = 0;

	bit = MXC_GPIO_InGet(gpio_uc_cfg0.port, gpio_uc_cfg0.mask);
	if(bit)
		val8 |= 0x01;

	bit = MXC_GPIO_InGet(gpio_uc_cfg1.port, gpio_uc_cfg1.mask);
	if(bit)
		val8 |= (0x01<<1);

	bit = MXC_GPIO_InGet(gpio_uc_cfg2.port, gpio_uc_cfg2.mask);
	if(bit)
		val8 |= (0x01<<2);

	bit = MXC_GPIO_InGet(gpio_uc_cfg3.port, gpio_uc_cfg3.mask);
	if(bit)
		val8 |= (0x01<<3);
	return val8;
}


/*!
 * @brief          BSPConfigLED
 *
 * @param [in]     led_num LED GPIO pin
 * @param [in]     en      true=LED ON; false=LED OFF
 *
 * @details        This function switches ON and OFF the board LEDs [Green, Red, Yellow, Blue]
 *
 */
void BSPConfigLED(adi_board_led_e led_num, bool en)
{
    switch(led_num)
	{
        case D2Z_BRD_GREEN_LED:
        {
        	if(!en)
        	    MXC_GPIO_OutSet(gpio_led_green.port, gpio_led_green.mask);
        	else
                MXC_GPIO_OutClr(gpio_led_green.port, gpio_led_green.mask);
        }
        break;
        case D2Z_BRD_RED_LED:
        {
        	if(!en)
        	    MXC_GPIO_OutSet(gpio_led_red.port, gpio_led_red.mask);
        	else
                MXC_GPIO_OutClr(gpio_led_red.port, gpio_led_red.mask);
        }
        break;
        case D2Z_BRD_YELLOW_LED:
        {
        	if(!en)
        	    MXC_GPIO_OutSet(gpio_led_yellow.port, gpio_led_yellow.mask);
        	else
                MXC_GPIO_OutClr(gpio_led_yellow.port, gpio_led_yellow.mask);
        }
        break;
        case D2Z_BRD_BLUE_LED:
        {
        	if(!en)
        	    MXC_GPIO_OutSet(gpio_led_blue.port, gpio_led_blue.mask);
        	else
                MXC_GPIO_OutClr(gpio_led_blue.port, gpio_led_blue.mask);
        }
        break;
	}
}

/*!
 * @brief          BSP_HeartBeat
 *
 * @details        This function toggles the Yellow heart beat led.
 *
 */
void BSP_HeartBeat(void)
{
	MXC_GPIO_OutToggle(gpio_led_yellow.port, gpio_led_yellow.mask);
}

/* @brief           spi0_init
 *
 * @return          ret return value of MXC_SPI_Init
 *
 * @details         This function initializes SPI0 for MDIO operations
 *
 */
int spi0_init(void)
{
    int ret = 0;
    int masterMode = 1;
    int quadModeUsed = 0;
    int numSlaves = 1;
    unsigned int ssPolarity = 0;

    ret = MXC_SPI_Init(SPI0_MDIO, masterMode, quadModeUsed, numSlaves, ssPolarity,SPI0_BAUD_RATE);
    if (ret) {
        return ret;
    }

    MXC_SPI_SetDataSize(SPI0_MDIO, 8);
    MXC_SPI_SetWidth(SPI0_MDIO, SPI_WIDTH_STANDARD);
    return ret;
}


/*!
 * @brief          spi0_send_rcv
 *
 * @param [in]     src      source buffer
 * @param [in]     srcLen   length of the buffer
 * @param [in]     dst      destination buffer
 *
 * @return         ret   return value of MXC_SPI transaction
 *
 * @details        SPI transaction done on SPI0 for MDIO operation
 *
 */
int spi0_send_rcv(unsigned char *src, unsigned int srcLen, unsigned char *dst)
{
    int ret = 0;
    mxc_spi_req_t req;

    req.spi = SPI0_MDIO;
    req.txData = (uint8_t *)src;
    req.rxData = (uint8_t *)dst;
    req.txLen = srcLen;
    req.rxLen = srcLen;
    req.ssIdx = 0; // SS0 is connected
    req.ssDeassert = 1;
    req.txCnt = 0;
    req.rxCnt = 0;
    req.completeCB = NULL;

    ret = MXC_SPI_MasterTransaction(&req);

    return ret;
}


/* @brief           spi1_init
 *
 * @return          ret return value of MXC_SPI_Init
 *
 * @details         This function initializes SPI0 for LTC4296-1 operations
 *
 */
int spi1_init(void)
{
    int ret = 0;
    int masterMode = 1;
    int quadModeUsed = 0;
    int numSlaves = 1;
    int ssPolarity = 0;

    ret = MXC_SPI_Init(SPI1_LTC4296_1, masterMode, quadModeUsed, numSlaves, ssPolarity, SPI1_BAUD_RATE);
    if (ret) {
        return ret;
    }

    MXC_SPI_SetDataSize(SPI1_LTC4296_1, 8);
    MXC_SPI_SetWidth(SPI1_LTC4296_1, SPI_WIDTH_STANDARD);
    return ret;
}


/*!
 * @brief          spi1_send_rcv
 *
 * @param [in]     src      source buffer
 * @param [in]     srcLen   length of the buffer
 * @param [in]     dst      destination buffer
 *
 * @return         ret   return value of MXC_SPI transaction
 *
 * @details        SPI transaction done on SPI1 for LTC4296 operation
 *
 */
int spi1_send_rcv(unsigned char *src, unsigned int srcLen, unsigned char *dst)
{
    int ret = 0;
    mxc_spi_req_t req;

    req.spi = SPI1_LTC4296_1;
    req.txData = (uint8_t *)src;
    req.rxData = (uint8_t *)dst;
    req.txLen = srcLen;
    req.rxLen = srcLen;
    req.ssIdx = 0; // SS0 is connected
    req.ssDeassert = 1;
    req.txCnt = 0;
    req.rxCnt = 0;
    req.completeCB = NULL;

    ret = MXC_SPI_MasterTransaction(&req);

    return ret;
}


/*!
 * @brief          bsp_sysReset
 *
 * @details        This function resets the microcontroller
 *
 */
void bsp_sysReset(void)
{
	MXC_GCR_REGS = MXC_F_GCR_RST0_SYS;
}


/*!
 * @brief          BSP_GpioInit
 *
 * @return         ret   ret value
 *
 * @details        Configures the GPIO Pins of the microcontroller
 *
 */
uint32_t BSP_GpioInit(void)
{
	uint32_t ret = SUCCESS;
    /* LEDs Configuration */
    /* P0.23 GREEN LED - All configuration good, function good, both links up */
    gpio_led_green.port = MXC_GPIO_PORT0;
    gpio_led_green.mask = MXC_GPIO_LED_GREEN;
    gpio_led_green.pad = MXC_GPIO_PAD_NONE;
    gpio_led_green.func = MXC_GPIO_FUNC_OUT;
    MXC_GPIO_Config(&gpio_led_green);

    /* P0.24 RED LED - Any error condition */
    gpio_led_red.port = MXC_GPIO_PORT0;
    gpio_led_red.mask = MXC_GPIO_LED_RED;
    gpio_led_red.pad = MXC_GPIO_PAD_NONE;
    gpio_led_red.func = MXC_GPIO_FUNC_OUT;
    MXC_GPIO_Config(&gpio_led_red);

    /* P0.25 YELLOW LED - uC heart beat */
    gpio_led_yellow.port = MXC_GPIO_PORT0;
    gpio_led_yellow.mask = MXC_GPIO_LED_YELLOW;
    gpio_led_yellow.pad = MXC_GPIO_PAD_NONE;
    gpio_led_yellow.func = MXC_GPIO_FUNC_OUT;
    MXC_GPIO_Config(&gpio_led_yellow);

    /* P0.26 BLUE LED - SPoE delivering power to remote PD */
    gpio_led_blue.port = MXC_GPIO_PORT0;
    gpio_led_blue.mask = MXC_GPIO_LED_BLUE;
    gpio_led_blue.pad = MXC_GPIO_PAD_NONE;
    gpio_led_blue.func = MXC_GPIO_FUNC_OUT;
    MXC_GPIO_Config(&gpio_led_blue);

    /* P0.5 SPI0 Slave Select 0 */
    gpio_uc_spi0_sso.port = MXC_GPIO_PORT0;
    gpio_uc_spi0_sso.mask = MXC_GPIO_SPI0_SS0;
    gpio_uc_spi0_sso.pad = MXC_GPIO_PAD_NONE;
    gpio_uc_spi0_sso.func = MXC_GPIO_FUNC_OUT;
    MXC_GPIO_Config(&gpio_uc_spi0_sso);

    /* P0.17 SPI1 Slave Select 0 */
    gpio_uc_spi1_sso.port = MXC_GPIO_PORT0;
    gpio_uc_spi1_sso.mask = MXC_GPIO_SPI1_SS0;
    gpio_uc_spi1_sso.pad = MXC_GPIO_PAD_NONE;
    gpio_uc_spi1_sso.func = MXC_GPIO_FUNC_OUT;
    MXC_GPIO_Config(&gpio_uc_spi1_sso);

    /* P0.10 SPoE SCCP - SCCPI */
    gpio_uc_spoe_sccpi.port = MXC_GPIO_PORT0;
    gpio_uc_spoe_sccpi.mask = MXC_GPIO_SPoE_SCCPI;
    gpio_uc_spoe_sccpi.pad = MXC_GPIO_PAD_NONE; //MXC_GPIO_PAD_PULL_UP;
    gpio_uc_spoe_sccpi.func = MXC_GPIO_FUNC_IN;
    MXC_GPIO_Config(&gpio_uc_spoe_sccpi);

    /*Drive Low when not used */
    MXC_GPIO_OutClr(gpio_uc_spoe_sccpo.port, gpio_uc_spoe_sccpo.mask);

    /* P0.11 SPoE SCCP - SCCPO */
    gpio_uc_spoe_sccpo.port = MXC_GPIO_PORT0;
    gpio_uc_spoe_sccpo.mask = MXC_GPIO_SPoE_SCCPO;
    gpio_uc_spoe_sccpo.pad = MXC_GPIO_PAD_NONE;
    gpio_uc_spoe_sccpo.func = MXC_GPIO_FUNC_OUT;
    MXC_GPIO_Config(&gpio_uc_spoe_sccpo);

    /* ADIN1100 Configuration */
    /* Configure the ADIN1100-LED0 Green	10BASE-T1L Link
                     ADIN1100-LED1 Yellow	10BASE-T1L Activity*/

    /* Configure ADIN1100 PHY Link Status - P0.18 */
    gpio_adin1100_linkSt.port = MXC_GPIO_PORT0;
    gpio_adin1100_linkSt.mask = MXC_GPIO_ADIN1100_LINKST;
    gpio_adin1100_linkSt.pad  = MXC_GPIO_PAD_NONE;
    gpio_adin1100_linkSt.func = MXC_GPIO_FUNC_IN;
    MXC_GPIO_Config(&gpio_adin1100_linkSt);

    /* Configure ADIN1100 PHY Interrupt - P0.19 */
    gpio_adin1100_phyInt.port = MXC_GPIO_PORT0;
    gpio_adin1100_phyInt.mask = MXC_GPIO_ADIN1100_PHYINT;
    gpio_adin1100_phyInt.pad  = MXC_GPIO_PAD_NONE;
    gpio_adin1100_phyInt.func = MXC_GPIO_FUNC_IN;
    MXC_GPIO_Config(&gpio_adin1100_phyInt);

    /* ADIN1200 Configuration */

    /* Configure ADIN1200 PHY Link Status - P0.20 */
    gpio_adin1200_linkSt.port = MXC_GPIO_PORT0;
    gpio_adin1200_linkSt.mask = MXC_GPIO_ADIN1200_LINKST;
    gpio_adin1200_linkSt.pad  = MXC_GPIO_PAD_NONE;
    gpio_adin1200_linkSt.func = MXC_GPIO_FUNC_IN;
    MXC_GPIO_Config(&gpio_adin1200_linkSt);

    /* Configure ADIN1200 PHY Interrupt - P0.21 */
    gpio_adin1200_phyInt.port = MXC_GPIO_PORT0;
    gpio_adin1200_phyInt.mask = MXC_GPIO_ADIN1200_PHYINT;
    gpio_adin1200_phyInt.pad  = MXC_GPIO_PAD_NONE;
    gpio_adin1200_phyInt.func = MXC_GPIO_FUNC_IN;
    MXC_GPIO_Config(&gpio_adin1200_phyInt);

    /* UC Config Pins Configuration */

    /* gpio_uc_cfg0 - P0.27 */
    gpio_uc_cfg0.port = MXC_GPIO_PORT0;
    gpio_uc_cfg0.mask = MXC_GPIO_UC_CFG0;
    gpio_uc_cfg0.pad  = MXC_GPIO_PAD_NONE;
    gpio_uc_cfg0.func = MXC_GPIO_FUNC_IN;
    MXC_GPIO_Config(&gpio_uc_cfg0);

    /* gpio_uc_cfg1 - P0.28 */
    gpio_uc_cfg1.port = MXC_GPIO_PORT0;
    gpio_uc_cfg1.mask = MXC_GPIO_UC_CFG1;
    gpio_uc_cfg1.pad  = MXC_GPIO_PAD_NONE;
    gpio_uc_cfg1.func = MXC_GPIO_FUNC_IN;
    MXC_GPIO_Config(&gpio_uc_cfg1);

    /* gpio_uc_cfg2 - P0.29 */
    gpio_uc_cfg2.port = MXC_GPIO_PORT0;
    gpio_uc_cfg2.mask = MXC_GPIO_UC_CFG2;
    gpio_uc_cfg2.pad  = MXC_GPIO_PAD_NONE;
    gpio_uc_cfg2.func = MXC_GPIO_FUNC_IN;
    MXC_GPIO_Config(&gpio_uc_cfg2);

    /* gpio_uc_cfg3 - P0.30 */
    gpio_uc_cfg3.port = MXC_GPIO_PORT0;
    gpio_uc_cfg3.mask = MXC_GPIO_UC_CFG3;
    gpio_uc_cfg3.pad  = MXC_GPIO_PAD_NONE;
    gpio_uc_cfg3.func = MXC_GPIO_FUNC_IN;
    MXC_GPIO_Config(&gpio_uc_cfg3);

    /* Pushbutton "BOOT" (S402) */
    /* gpio_uc_boot - P0.22 */
    gpio_uc_boot.port = MXC_GPIO_PORT0;
    gpio_uc_boot.mask = MXC_GPIO_UC_BOOT;
    gpio_uc_boot.pad  = MXC_GPIO_PAD_NONE;
    gpio_uc_boot.func = MXC_GPIO_FUNC_IN;
    MXC_GPIO_Config(&gpio_uc_boot);

    return ret;
}

/*!
 * @brief          BSP_SpiInit
 *
 * @return         ret   return value SPI init
 *
 * @details        Initializes SPI0 and SPI1
 *
 */
uint32_t BSP_SpiInit(void)
{
    int ret = 0;
    /* Configured to access MDIO */
    ret = spi0_init();
    /* Configured to access LTC4296-1 */
    ret = spi1_init();
    return ret;
}

/*
 * Functions used for UART transmit
 *
 */
void msgWrite(char * ptr)
{
   printf("%s",ptr);
}

void common_Fail(char *FailureReason)
{
    char fail[] = "Failed: ";
    char term[] = "\n\r";

    /* Ignore return codes since there's nothing we can do if it fails */
    msgWrite(fail);
    msgWrite(FailureReason);
    msgWrite(term);
 }

void common_Perf(char *InfoString)
{
    char term[] = "\n\r";

    /* Ignore return codes since there's nothing we can do if it fails */
    msgWrite(InfoString);
    msgWrite(term);
}

uint32_t submitTxBuffer(uint8_t *pBuffer, int nbBytes)
{
    uint32_t error = 0;
    printf("%s", pBuffer);
    return (error);
}

void UART0_Handler(void)
{
    MXC_UART_AsyncHandler(MXC_UART0);
}

void readCallback(mxc_uart_req_t *req, int error)
{
	strBuf[strCounter] = req->rxData[0];
	if((strBuf[strCounter] == 0x0A)||(strBuf[strCounter] == 0x0D))//"\n\r")
	{
	     memcpy(commandBuffer, strBuf, 100);
	     memset(strBuf,0x00, sizeof(strBuf) );
	     strCounter = 0;
	     setUartDataAvailable(ON);
    }
	if( (strBuf[strCounter] == '\0') && (strCounter == 0) )
	{
	    MXC_UART_TransactionAsync(&uart_read_req);
	}
	else
	{
	    strCounter++;
        MXC_UART_TransactionAsync(&uart_read_req);
	}
}


/*!
 * @brief          BSP_UartInit
 *
 * @return         ret   return value UART init
 *
 * @details        Initializes UART
 *
 */
uint32_t BSP_UartInit(void)
{
    int ret = 0;

    NVIC_ClearPendingIRQ(UART0_IRQn);
    NVIC_DisableIRQ(UART0_IRQn);
    MXC_NVIC_SetVector(UART0_IRQn, UART0_Handler);
    NVIC_EnableIRQ(UART0_IRQn);

    uart_read_req.uart = MXC_UART0;
    uart_read_req.rxData = RxData;
    uart_read_req.rxLen = BUFF_SIZE;
    uart_read_req.txLen = 0;
    uart_read_req.callback = readCallback;
    uart_read_req.rxCnt =1;


    MXC_UART_TransactionAsync(&uart_read_req);

    return ret;
}


/*!
 * @brief           TimerDelay_ms
 *
 * @param [in]      msec  delay value
 *
 * @details         This function gives delay in milliseconds.
 *
 */
void TimerDelay_ms(volatile uint32_t msec)
{
	volatile uint32_t periodTicks;

    /*periodTicks = US_TICK*usec*/
	periodTicks = (volatile uint32_t)(US_TICK*(msec*1000));

    MXC_TMR_SetCompare(DELAY_TIMER, periodTicks);
    MXC_TMR_Start(DELAY_TIMER);

	gStartTimer = 1;
	while(gTimerFlag == 0)
	{
		/* drink a brew */
	}

	MXC_TMR_Stop(DELAY_TIMER);
	/* Reset the timer flags */
    gTimerFlag = 0;
	gStartTimer = 0;
}

/*!
 * @brief           TimerDelay_msf
 *
 * @param [in]      val  delay value
 *
 * @details         This function gives delay in milliseconds float.
 *
 */
void TimerDelay_msf(volatile uint32_t val)
{
	volatile uint32_t periodTicks;
	periodTicks = (volatile uint32_t)(US_TICK*val);
    MXC_TMR_SetCompare(DELAY_TIMER, periodTicks);
    MXC_TMR_Start(DELAY_TIMER);

	gStartTimer = 1;
	while(gTimerFlag == 0)
	{
		/* drink a brew */
	}

	MXC_TMR_Stop(DELAY_TIMER);
	/* Reset the timer */
    gTimerFlag = 0;
	gStartTimer = 0;
}


/*!
 * @brief           DelayTimerHandler
 *
 * @details         This function is delay timer handler.
 *
 */
void DelayTimerHandler(void)
{
    // Clear interrupt
    MXC_TMR_ClearFlags(DELAY_TIMER);
    if(gStartTimer == 1)
        gTimerFlag = 1;
}

/*!
 * @brief           DelayTimerConfig
 *
 * @details         Delay timer is set to provide delay of order of milli sec
 *
 */
void DelayTimerConfig(void)
{
    mxc_tmr_cfg_t tmr;
    /*
    Steps for configuring a timer for PWM mode:
    1. Disable the timer
    2. Set the prescale value
    3  Configure the timer for continuous mode
    4. Set polarity, timer parameters
    5. Enable Timer
    */

    MXC_TMR_Shutdown(DELAY_TIMER);

    /* CLK_SOURCE is 7372800 Hz */  /* PRESCALAR is 4 */
    /* 7372800/4 = 1,843,200 Hz for 1 sec*/
    /* 1 usec = 1.84 tick*/
    tmr.pres = TMR_PRES_4;
    tmr.mode = TMR_MODE_CONTINUOUS;
    tmr.clock = CLOCK_SOURCE;
    /*setting for resolution of 1secs*/
    tmr.cmp_cnt = (volatile uint32_t) (US_TICK*1000000); //SystemCoreClock*(1/interval_time);
    tmr.pol = 0;
    MXC_TMR_Init(DELAY_TIMER, &tmr, true);
    MXC_TMR_EnableInt(DELAY_TIMER);
}

/*!
 * @brief           BSP_StartTimer
 *
 * @details         This function start the 1sec(cont) Timer
 *
 */
void BSP_StartTimer(void)
{
    MXC_TMR_Start(CONT_TIMER);
}

/*!
 * @brief           BSP_StopTimer
 *
 * @details         This function stops the 1sec (cont) Timer
 *
 */
void BSP_StopTimer(void)
{
    MXC_TMR_Stop(CONT_TIMER);
}

/*!
 * @brief           ContinuousTimerHandler
 *
 * @details         This function is continues timer handler.
 *                  It sets the global One Second flag.
 *
 */
void ContinuousTimerHandler(void)
{
    // Clear interrupt
    MXC_TMR_ClearFlags(CONT_TIMER);
    gOneSecTFlag = 1;
}

/*!
 * @brief           ContinuousTimer
 *
 * @details         Continues timer is set to provide delay of 1sec
 *
 */
void ContinuousTimer(void)
{
    // Declare variables
    mxc_tmr_cfg_t tmr;
    volatile uint32_t periodTicks = MXC_TMR_GetPeriod(CONT_TIMER, CLOCK_SOURCE, 128, CONT_FREQ_LED);

    /*
    Steps for configuring a timer for PWM mode:
    1. Disable the timer
    2. Set the prescale value
    3  Configure the timer for continuous mode
    4. Set polarity, timer parameters
    5. Enable Timer
    6. Timer is configured to blink ON and OFF in ONE Sec; CONT_FREQ_LED = 2
    */

    MXC_TMR_Shutdown(CONT_TIMER);

    tmr.pres = TMR_PRES_128;
    tmr.mode = TMR_MODE_CONTINUOUS;
    tmr.clock = CLOCK_SOURCE;
    tmr.cmp_cnt = periodTicks; //SystemCoreClock*(1/interval_time);
    tmr.pol = 0;

    MXC_TMR_Init(CONT_TIMER, &tmr, true);
    MXC_TMR_EnableInt(CONT_TIMER);
    MXC_TMR_Start(CONT_TIMER);
}

/*!
 * @brief           BSP_TimerInit
 *
 * @return          error status of the initialization functions
 *
 * @details         This function initialize Timer
 *
 */
uint32_t BSP_TimerInit(void)
{
    uint32_t ret=0;

    MXC_NVIC_SetVector(TMR3_IRQn, ContinuousTimerHandler);
    NVIC_EnableIRQ(TMR3_IRQn);
    ContinuousTimer();

    MXC_NVIC_SetVector(TMR2_IRQn, DelayTimerHandler);
    NVIC_EnableIRQ(TMR2_IRQn);
    DelayTimerConfig();

    return ret;
}

/*!
 * @brief           BSP_InitSystem
 *
 * @return          error status of the initialization functions
 *
 * @details         This function initialize the microcontroller peripherals
 *
 */
uint8_t BSP_InitSystem(void)
{
	uint8_t error = 0;
    do
    {
        /*Clk is configured */
    	SystemInit();

    	/*UART0 is configured for the console */

    	/* Configure UART Interrupts */
        if((error = BSP_UartInit())!= SUCCESS)
        {
            break;
        }

        if((error = BSP_TimerInit()) != SUCCESS)
        {
            break;
        }


        if((error = BSP_GpioInit())!= SUCCESS)
        {
            break;
        }

        if((error = BSP_SpiInit()) != SUCCESS)
        {
            break;
        }
    }while(0);
    return (error);
}

/**@}*/

