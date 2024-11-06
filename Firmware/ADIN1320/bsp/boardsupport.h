/*
 *---------------------------------------------------------------------------
 *
 * Copyright (c) 2024 Analog Devices, Inc. All Rights Reserved.
 * This software is proprietary to Analog Devices, Inc.
 * and its licensors.By using this software you agree to the terms of the
 * associated Analog Devices Software License Agreement.
 *
 *---------------------------------------------------------------------------
 */
/** @addtogroup bsp MAX32670 BSP
 *  @{
 */
#ifndef ADI_BOARDSUPPORT_H
#define ADI_BOARDSUPPORT_H

#include <stdint.h>
#include <stdbool.h>
#include <stdio.h>
#include <stdlib.h>

#include "uart.h"
#include "board.h"

#include "max32670.h"
#include "spi_regs.h"
#include "spi.h"

#define MXC_GPIO_PORT0            MXC_GPIO0

#define MXC_GPIO_SPI0_SS0         MXC_GPIO_PIN_5
#define MXC_GPIO_SPI1_SS0         MXC_GPIO_PIN_17

#define MXC_GPIO_SPoE_SCCPI       MXC_GPIO_PIN_10
#define MXC_GPIO_SPoE_SCCPO       MXC_GPIO_PIN_11

#define MXC_GPIO_ADIN1320_LINKST  MXC_GPIO_PIN_18
#define MXC_GPIO_ADIN1320_PHYINT  MXC_GPIO_PIN_19

#define MXC_GPIO_ADIN1300_LINKST  MXC_GPIO_PIN_20
#define MXC_GPIO_ADIN1300_PHYINT  MXC_GPIO_PIN_21

#define MXC_GPIO_UC_BOOT          MXC_GPIO_PIN_22

/* P0.23 is Green LED on D2z board and GREEN LED on MAX32670-EV-kit*/
#define MXC_GPIO_LED_GREEN        MXC_GPIO_PIN_23
#define MXC_GPIO_LED_RED          MXC_GPIO_PIN_24
#define MXC_GPIO_LED_YELLOW       MXC_GPIO_PIN_25
#define MXC_GPIO_LED_BLUE         MXC_GPIO_PIN_26

#define MXC_GPIO_UC_CFG0          MXC_GPIO_PIN_27
#define MXC_GPIO_UC_CFG1          MXC_GPIO_PIN_28
#define MXC_GPIO_UC_CFG2          MXC_GPIO_PIN_29
#define MXC_GPIO_UC_CFG3          MXC_GPIO_PIN_30


/*!
 * @brief ADIN100D2Z board LEDs
 */
typedef enum
{
    D2Z_BRD_GREEN_LED = 0,                              /*!< All config good, function good, both links up  */
	D2Z_BRD_RED_LED,                                    /*!< Any error condition                            */
	D2Z_BRD_YELLOW_LED,                                 /*!< uC heart beat                                  */
	D2Z_BRD_BLUE_LED,                                   /*!< SPoE delivering power to remote PD             */
} adi_board_led_e;


/*** SPI MDIO Configuration ***/
#define SPI0_MDIO MXC_SPI0
#define SPI0_BAUD_RATE (volatile unsigned int) 2500000

/*** SPI MDIO Configuration ***/
#define SPI1_LTC4296_1 MXC_SPI1
#define SPI1_BAUD_RATE (volatile  unsigned int) 1000000

/*** Functions ***/
int spi0_init(void);
int spi0_send_rcv(unsigned char *src, unsigned int srcLen, unsigned char *dst);

/*
* Driver Version
*/
typedef struct
{
  uint16_t major;        /*!< Major version */
  uint16_t minor;        /*!< Minor version*/
  uint16_t build;        /*!< Build version*/
} max32670_fw_version;

/*Functions prototypes*/
uint8_t BSP_InitSystem(void);
void TimerDelay_msf(volatile uint32_t val);
void DelayTimerConfig(void);
void TimerDelay_ms(volatile uint32_t msec);
uint8_t BSP_getConfigPins(void);
uint32_t submitTxBuffer(uint8_t *pBuffer, int nbBytes);
void BSPConfigLED(adi_board_led_e led_num, bool en);
void bsp_sysReset(void);
int spi0_send_rcv(unsigned char *src, unsigned int srcLen, unsigned char *dst);
int spi1_send_rcv(unsigned char *src, unsigned int srcLen, unsigned char *dst);
void BSP_StartTimer(void);
void BSP_StopTimer(void);
void BSP_HeartBeat(void);
void msgWrite(char * ptr);

#endif /* ADI_BOARDSUPPORT_H */

/**@}*/

