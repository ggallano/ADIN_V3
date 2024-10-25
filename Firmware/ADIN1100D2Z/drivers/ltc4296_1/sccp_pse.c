/*
 *---------------------------------------------------------------------------
 *
 * Copyright (c) 2023 Analog Devices, Inc. All Rights Reserved.
 * This software is proprietary to Analog Devices, Inc.
 * and its licensors.By using this software you agree to the terms of the
 * associated Analog Devices Software License Agreement.
 *
 *---------------------------------------------------------------------------
 */

/** @addtogroup SPOE LTC4296_1 Driver
 *  @{
 */

#include <stdio.h>
#include <stdint.h>
#include "sccp_pse.h"
#include "gpio.h"
#include "bsp\boardsupport.h"
#include "spoeLTC4296_1.h"
#include "include\aux_functions.h"
#include "platform\adi_eth_error.h"
#include "sccp_defines.h"

extern volatile uint8_t gTimerFlag;
extern volatile uint8_t gStartTimer;
extern mxc_gpio_cfg_t gpio_uc_spoe_sccpi;
extern mxc_gpio_cfg_t gpio_uc_spoe_sccpo;

#define SCCP_TYPE_MASK         0xF000
#define SCCP_CLASS_TYPE_MASK   0x0FFF

/***********************************************************************************
global variable: const uint8_t class_compatibility[16][16]
This is 2 dimensional array returns the compatibility of PSE with PD class
Following an SCCP transaction class_compatibility[x][y] variable contains compatibility of PSE class x with PD class y
***********************************************************************************/

const uint8_t class_compatibility[16][16] = {
//PD								PSE
//			0	1	2	3	4	5	6	7	8	9	10	11	12	13	14	15
/*0*/		{1,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0},
/*1*/		{0,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0},
/*2*/		{0,	0,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0},
/*3*/		{0,	0,	0,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0},
/*4*/		{0,	0,	0,	0,	1,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0},
/*5*/		{0,	0,	0,	0,	0,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0},
/*6*/		{0,	0,	0,	0,	0,	0,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0},
/*7*/		{0,	0,	0,	0,	0,	0,	0,	1,	0,	0,	0,	0,	0,	0,	0,	0},
/*8*/		{0,	0,	0,	0,	0,	0,	0,	0,	1,	1,	0,	0,	0,	0,	0,	0},
/*9*/		{0,	0,	0,	0,	0,	0,	0,	0,	0,	1,	0,	0,	0,	0,	0,	0},
/*10*/		{0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	1,	1,	1,	0,	0,	0},
/*11*/		{0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	1,	1,	0,	0,	0},
/*12*/		{0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	1,	0,	0,	0},
/*13*/		{0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	1,	1,	1},
/*14*/		{0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	1,	1},
/*15*/		{0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	1}
};

/* Top 4 bits give the SCCP Types
 *                              A,   B,   C,   D,   E
*/
const uint8_t sccp_types[5] = { 0xE, 0xD, 0xB, 0x7, 0xC };

/* Bottom 12 bits give the SCCP class*/
#define SCCP_CLASS_SIZE 16
uint16_t sccp_classes[SCCP_CLASS_SIZE] = {
    0x3FE, // class 0
    0x3FD, // class 1
    0x3FB, // class 2
    0x3F7, // class 3
    0x3F7, // class 4
    0x3EF, // class 5
    0x3DF, // class 6
    0x3BF, // class 7
    0x37F, // class 8
    0x2FF, // class 9
    0x001, // class 10 /* Supported by D2Z Board*/
    0x002, // class 11 /* Supported by D2Z Board*/
    0x003, // class 12 /* Supported by D2Z Board*/
    0x004, // class 13 /* Supported by D2Z Board*/
    0x005, // class 14 /* Supported by D2Z Board*/
    0x006  // class 15 /* Supported by D2Z Board*/
};

/*!
 * @brief           get_CRC
 *
 * @param [in]      buf   data bytes and CRC received from PD
 *
 * @details         This function calculates CRC on the data bytes
 *
 */
static uint8_t get_CRC(uint8_t* buf)
{
	uint8_t byte, bit;
	uint8_t x3,x4,x7, in;

	uint8_t crc = 0;
	for(byte=0; byte<2; byte++)
	{
		for(bit=0; bit<8; bit++)
		{
			/* save some bits before shifting register */
			x3 = (crc>>3) & 0x01;
			x4 = (crc>>4) & 0x01;
			x7 = (crc>>7) & 0x01;
			in = (buf[byte] >> bit) & 0x01;
			in ^= x7;
			/* shift the register */
			crc  = (crc<<1) | in;

			/* clear bits 4 & 5 */
			crc &= ~(0x30);

			/* replace bits with xor of 'in' and prev bit */
			uint8_t temp = x3 ^ in;
			crc |= (temp<<4);
			temp = x4 ^ in;
			crc |= (temp<<5);
		}
	}
	return crc;
}

/*!
 * @brief           READ_LINE
 *
 * @return
 *                  - 1 if HIGH
 *                  - 0 if LOW
 *
 * @details         The PSE reads the line and returns 1 if HIGH and 0 if LOW
 *
 */
uint8_t READ_LINE()
{
	volatile uint32_t bit=0;

	bit = MXC_GPIO_InGet(gpio_uc_spoe_sccpi.port, gpio_uc_spoe_sccpi.mask);
	if(bit)
	    return 1;
	else
	    return 0;
}

/*!
 * @brief           PULL_DOWN_LINE
 *
 *
 * @details         The PSE pulls the line down
 *
 */
void PULL_DOWN_LINE()
{
	/* GPIO P0.11 SCCPO*/
	MXC_GPIO_OutSet(gpio_uc_spoe_sccpo.port, gpio_uc_spoe_sccpo.mask);
}


/*!
 * @brief           RELEASE_LINE
 *
 * @details         The PSE release the line
 *
 */
void RELEASE_LINE()
{
	/* GPIO P0.11 SCCPO*/
	MXC_GPIO_OutClr(gpio_uc_spoe_sccpo.port, gpio_uc_spoe_sccpo.mask);
}

/*!
 * @brief          write_bit
 *
 * @param[in]      bit   data bytes and CRC received from PD
 *
 * @details        This function controls the pull down FET in order to send a bit of
 *                 the SCCP transaction from PSE to PD
 *
 */
void write_bit(uint8_t bit)
{
	PULL_DOWN_LINE();
	if (bit)
	{
		TimerDelay_msf(300);
		RELEASE_LINE();
		TimerDelay_msf(2150);//T_WRITESLOT-T_REC-T_W1L = 2.15
	}
	else
	{
		TimerDelay_msf(2450); //TW0L + 0.45 = 2.45
		RELEASE_LINE();
	}

	/*Recovery time after every bit transmit */
	TimerDelay_msf(320);//0.32
	return;
}


/*!
 * @brief          transmit_byte
 *
 * @param [in]     tx_byte   transmit byte
 *
 * @details        This function transmits the byte
 *
 */
void transmit_byte(uint8_t tx_byte)
{
	uint8_t bit_pos = 0;
	while (bit_pos < 8)
	{
		uint8_t bit = (tx_byte>>bit_pos) & 0x01;
		write_bit(bit);
		bit_pos++;
	}
	return;
}

/*!
 * @brief          read_bit
 *
 * @return
 *                  - read bit is returned
 *
 * @details        This function implements the GPIO pulse sequence required to receive
 *                 and read a bit on GPIO return the read bit.
 *
 */
uint8_t read_bit()
{
	uint8_t bit;

	PULL_DOWN_LINE();
	TimerDelay_msf(300); //T_W1L =0.3

	RELEASE_LINE();

	TimerDelay_msf(700); //T_MSR-T_W1L = 1.225-0.3 = 700

	bit = READ_LINE();

	TimerDelay_msf(2000); //T_READSLOT-T_MSR = 3-1 =2

	TimerDelay_msf(320); //T_REC
	return bit;
}

/*!
 * @brief          receive_response
 *
 * @param [out]     buf   response byte
 *
 * @details        This utilizes read_bit() to receive 2 bytes of data from the PD
 *
 */
void receive_response(uint8_t* buf)
{
	volatile uint8_t rx_byte = 0;
	volatile uint8_t bytes_rxd=0;
	volatile uint8_t bit_pos = 0;
    uint8_t sccp_buf[3] = {0, 0, 0};

	while (bytes_rxd < 3)
	{
		rx_byte = 0;
		bit_pos = 0;
		while(bit_pos < 8)
		{
			uint8_t bit = read_bit();
			rx_byte |= (bit<<bit_pos);
			bit_pos++;
		}
		sccp_buf[bytes_rxd] = rx_byte;
		bytes_rxd++;
		TimerDelay_ms(5);
	}
	buf[0]=sccp_buf[0];
	buf[1]=sccp_buf[1];
	buf[2]=sccp_buf[2];
	return;
}


/*!
 * @brief          sccpResetPulses
 *
 * @return
 *                  - value of PD presence is returned
 *
 * @details        This function send a reset pulse and wait for presence pulse response
 *
 */
adi_ltc_Result_e sccpResetPulse()
{
	uint8_t level=0;
	//uint32_t i=0,j,k;
	adi_ltc_Result_e ret= ADI_LTC_SCCP_PD_PRESENT;

	/* check if the line is high before reset pulse */
	if(!READ_LINE())
	{
		return ADI_LTC_SCCP_PD_LINE_NOT_HIGH;
	}

	/* assert pulse */
	PULL_DOWN_LINE();

	/* check to make sure line is actually getting pulled down (protect pull down fet) */
	TimerDelay_ms(3);

	if(READ_LINE())
	{
		/* release because fet must be pulling down against stronger source than a classification v source */
		RELEASE_LINE();
		return ADI_LTC_SCCP_PD_LINE_NOT_LOW;
	}

	TimerDelay_ms(T_RSTL_NOM-3);

	RELEASE_LINE();

	TimerDelay_ms(T_MSP);

	/* look for presence pulse */
    level = READ_LINE();

	TimerDelay_ms(4);

	if(level == HIGH)
		ret = ADI_LTC_SCCP_PD_NOT_PRESENT;
	else if(level == LOW)
		ret = ADI_LTC_SCCP_PD_PRESENT;

	return ret;
}

/*!
 * @brief           sccpReadWritePD
 *
 * @param [in]      addr  broadCastAddr
 * @param [in]      cmd   readScratchPad
 * @param [out]     buf   response bytes
 *
 * @return
 *                  - value of PD presence is returned
 *
 * @details        This function sends the required bytes of SCCP to the PD and receives the response
 *
 */
adi_ltc_Result_e sccpReadWritePD(uint8_t addr, uint8_t cmd, uint8_t* buf)
{
	adi_ltc_Result_e ret;

	ret = sccpResetPulse();
	if(ret == ADI_LTC_SCCP_PD_NOT_PRESENT)
		return ADI_LTC_SCCP_PD_NOT_PRESENT; //PD is not present
	else if(ret == ADI_LTC_SCCP_PD_LINE_NOT_LOW)
		return ADI_LTC_SCCP_PD_LINE_NOT_LOW;
	else if(ret == ADI_LTC_SCCP_PD_LINE_NOT_HIGH)
		return ADI_LTC_SCCP_PD_LINE_NOT_HIGH;

	TimerDelay_ms(5);
	transmit_byte(addr);
	TimerDelay_ms(5);
	transmit_byte(cmd);
	TimerDelay_ms(5);
	receive_response(buf);

	/* Check if the received data from PD is valid */
	if (get_CRC(buf) != buf[2] )
	{
		printf("PD CRC Error, CRC is 0x%x \n\r", buf[2]);
		return ADI_LTC_SCCP_PD_CRC_FAILED; /* Wrong CRC*/
	}
	return ADI_LTC_SCCP_PD_PRESENT;
}

/*!
 * @brief          sccp_IsPD
 *
 * @param [in]      pseClass  PSE Class info
 * @param [in]      sccpResponseData   response bytes from PD
 * @param [out]     pdClass   PD class
 * @return
 *                  - value of PD response is returned
 *
 * @details        This function verifies if PD is supported and compatible
 *
 */
adi_ltc_Result_e sccp_IsPD(uint8_t pseClass, uint16_t sccpResponseData, uint8_t *pdClass)
{
	uint8_t i,sccpType;
	uint16_t val, pdType=0;
	adi_ltc_Result_e ret;

	/* bottom 12 bits of CLASS_TYPE_INFO codes for PD power classes 0 through 15 */
	val = (sccpResponseData & SCCP_CLASS_TYPE_MASK);

	/*we support classes 10- Classes15*/
	for(i=0; i<SCCP_CLASS_SIZE; i++)
	{
		if(sccp_classes[i] == val)
		{
			pdType = i;
			break;
		}
	}

	/* Check for SCCP type; MSB 4 bits */
	sccpType = ( (sccpResponseData & SCCP_TYPE_MASK) >> 12);
	if(sccpType != sccp_types[4])
	{
		*pdClass = i;
		return ADI_LTC_SCCP_PD_CLASS_NOT_SUPPORTED;
	}

	/* PD Type supported are Class10 to Class15*/
	if( (pdType < 10) || (pdType > 15) )
	{
		/* The PD class is not supported by the D2Z board*/
		*pdClass = i;
		return ADI_LTC_SCCP_PD_CLASS_NOT_SUPPORTED;
	}

	if( class_compatibility[pdType][pseClass+10] == 1)
	{
		ret = ADI_LTC_SCCP_PD_CLASS_COMPATIBLE;
		*pdClass = i;
	}
	else
	{
		ret = ADI_LTC_SCCP_PD_CLASS_NOT_SUPPORTED;
		*pdClass = i;
	}
    return ret;
}


/**@}*/

