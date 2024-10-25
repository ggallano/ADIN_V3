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
#include <math.h>
#include "bsp\boardsupport.h"
#include "include\aux_functions.h"
#include "spoeLTC4296_1_rdef.h"
#include "spoeLTC4296_1.h"
#include "platform\adi_eth_error.h"
#include "sccp_defines.h"
#include "sccp_pse.h"

float gLTC4296_1_Vout=0;
float gLTC4296_1_Iout=0;

/* LTC4296_1_IGAIN / Rsense[portNo]
 * {(0.1/3), (0.1/1.5), (0.1/0.68), (0.1/0.25), (0.1/0.1)} */
const float ltc4296_1_SPOE_Rsense[LTC4296_1_MAX_PORTS] = {0.0333, 0.0666, 0.1470, 0.4, 1};

/* Value of RSense resistor for each port in Ohm */
const float ltc4296_1_SPOE_SenseResistor[LTC4296_1_MAX_PORTS] = {3, 1.5, 0.68, 0.25, 0.1};

/* SPOE Voltage Range for each class. [MIN] [MAX] values  */
float ltc4296_1_SPOE_VolRange[12][2] = { {20.0,30.0},  /* SPoE Class 10         */
        		                         {20.0,30.0},  /* SPoE Class 11         */
		                                 {20.0,30.0},  /* SPoE Class 12         */
		                                 {50.0,58.0},  /* SPoE Class 13         */
		                                 {50.0,58.0},  /* SPoE Class 14         */
		                                 {50.0,58.0},  /* SPoE Class 15         */
		                                 {9.6,15.0},   /* APL Class A           */
		                                 {9.6,15.0},   /* APL Class A noAutoNeg */
		                                 {9.6,15.0},   /* APL Class C           */
		                                 {46.0,50.0},  /* APL Class 3           */
		                                 {9.6,15.0},   /* Production Power Test */
		                                 {9.6,15.0}    /* APL Class A oldAPL    */
                                       };

uint8_t SetPortVout[LTC4296_1_MAX_PORTS] = {0x04, 0x06, 0x08, 0x0A, 0x0C};


/* Stirng to print the different classes of LTC4296-1  */
char* ltc4296_1_Class_Str[12][25] = { {"SPOE CLASS 10"},
		                              {"SPOE CLASS 11"},
									  {"SPOE CLASS 12"},
									  {"SPOE CLASS 13"},
								      {"SPOE CLASS 14"},
								      {"SPOE CLASS 15"},
								      {"APL CLASS A"},
								      {"APL CLASS A"}, /* No Autoneg */
								      {"APL CLASS C"},
								      {"APL CLASS 3"},
								      {"BOARD PRODUCTION TEST"},
								      {"APL CLASS A"} /* OLD Demo */
	                                };

/*
 * @details      This function calculates pec byte
 *
 */
static uint8_t get_pec_byte(uint8_t data, uint8_t seed)
{
	uint8_t pec = seed;
	uint8_t din, in0, in1, in2;
	int bit;
	for(bit=7; bit>=0; bit--)
	{
		din = (data>>bit) & 0x01;
		in0 = din ^ ( (pec>>7) & 0x01 );
		in1 = in0 ^ ( pec & 0x01);
		in2 = in0 ^ ( (pec>>1) & 0x01 );
		pec = (pec << 1);
		pec &= ~(0x07);
		pec = pec | in0 | (in1<<1) | (in2<<2);
	}
	return pec;
}

/*
 * @details      This function gets the corresponding port address with portNo and portoffset
 *
 */
static void getPortAddr(ltc4296_1_port_e portNo, ltc4296_1_portRegOffset_e portOffset, uint8_t *portAddr)
{
	*portAddr = ( ( (portNo+1)<<4) + portOffset);
}


/*!
 * @brief          ltc4296_1_write
 *
 * @param [in]      register_address   LTC4296-1 register address
 * @param [in]      value   value to be written to register
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function writes data to LTC4296-1 register
 *
 * @sa              ltc4296_1_read
 */
adi_ltc_Result_e ltc4296_1_write(uint8_t register_address, uint16_t value)
{
	uint8_t spi_buf[5];
	uint8_t pec;

	/* command byte: register address with r/w bit = 0 */
	spi_buf[0] = (uint8_t)(register_address << 1 ) & 0xFE;
	
	/* calculate pec byte from command byte */
	spi_buf[1] = get_pec_byte(spi_buf[0], 0x41);

	/* register value: 2 bytes, MSB first */
	spi_buf[2] = (uint8_t)(value >> 8);		// MSB
	spi_buf[3] = (uint8_t)(value & 0xFF); // LSB
	
	/* calculate pec byte from value word (by using pec calculation twice) */
	pec = get_pec_byte(spi_buf[2], 0x41);
	spi_buf[4] = get_pec_byte(spi_buf[3], pec);

	/* (transmit) 5 bytes over SPI */
	spi1_send_rcv(spi_buf, 5, spi_buf);
	
	return (ADI_LTC_SUCCESS);
}

/*!
 * @brief           ltc4296_1_read
 *
 * @param [in]      register_address   LTC4296-1 register address
 * @param [out]     value   value read from register
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function reads data from LTC4296-1 register
 *
 * @sa              ltc4296_1_write
 */
adi_ltc_Result_e ltc4296_1_read(uint8_t register_address, uint16_t *value)
{
	uint8_t spi_buf[5];
	uint8_t pec;

	// command byte: register address with r/w bit = 1
	spi_buf[0] = (uint8_t)(register_address << 1) | 0x01;
	
	// pec byte from command byte
	spi_buf[1] = get_pec_byte(spi_buf[0], 0x41);

	spi_buf[2] = 0;
	spi_buf[3] = 0;
	
	// exchange 5 bytes over SPI
	spi1_send_rcv(spi_buf, 5, spi_buf);

	// calculate pec byte from value word (by using pec calculation twice)
	pec = get_pec_byte(spi_buf[2], 0x41);
	pec = get_pec_byte(spi_buf[3], pec);

	// register value: 2 bytes, MSB first
	*value = (uint16_t)spi_buf[2] << 8;	// MSB
	*value |= (uint16_t)spi_buf[3];			// LSB

	// check calculated pec versus received pec
	if (pec != spi_buf[4])
	{
		printf("SPI PEC Error\n");
		return (ADI_ETH_SPI_ERROR);
	}
	else
		return (ADI_LTC_SUCCESS);
}

/*!
 * @brief           ltc4296_1_reset
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function does a SW reset of LTC4296-1
 *
 */
adi_ltc_Result_e ltc4296_1_reset(void)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	result = ltc4296_1_write(ADDR_GCMD, 0x7300);/*GCMD = sw_reset*/

    TimerDelay_ms(5); /* Delay of 5ms*/
	return (result);
}

/*!
 * @brief           ltc4296_1_clearGlobalfaults
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function clears global faults of LTC4296-1
 *
 */
adi_ltc_Result_e ltc4296_1_clearGlobalfaults(void)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	/* GFLTEV = clear all faults
	 * BITM_GFLTEV_LOW_CKT_BRK_FAULT | BITM_GFLTEV_MEMORY_FAULT |
	 * BITM_GFLTEV_PEC_FAULT | BITM_GFLTEV_COMMAND_FAULT | BITM_GFLTEV_UVLO_DIGITAL */
    result = ltc4296_1_write(ADDR_GFLTEV, 0x001F);

    return result;
}


/*!
 * @brief           ltc4296_1_clearCktBreaker
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function clears circuit breaker event in global fault event register
 *
 */
adi_ltc_Result_e ltc4296_1_clearCktBreaker(void)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint16_t val = 0;

    result = ltc4296_1_read(ADDR_GFLTEV, &val);
    val = val | BITM_GFLTEV_LOW_CKT_BRK_FAULT;

	/* GFLTEV = clear BITM_GFLTEV_LOW_CKT_BRK_FAULT by writing 1 to the bit*/
    result = ltc4296_1_write(ADDR_GFLTEV, val);

    return result;
}

/*!
 * @brief           ltc4296_1_readGlobalfaults
 *
 * @param [out]     gEvents  value of global fault event register
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function reads the global fault event register
 *
 */
adi_ltc_Result_e ltc4296_1_readGlobalfaults(uint16_t *gEvents)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	/* GFLTEV = clear all faults
	 * BITM_GFLTEV_LOW_CKT_BRK_FAULT | BITM_GFLTEV_MEMORY_FAULT |
	 * BITM_GFLTEV_PEC_FAULT | BITM_GFLTEV_COMMAND_FAULT | BITM_GFLTEV_UVLO_DIGITAL */
    result = ltc4296_1_read(ADDR_GFLTEV, gEvents);

    return result;
}


/*!
 * @brief           ltc4296_1_unlock
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function unlocks ltc4296-1
 *
 */
adi_ltc_Result_e ltc4296_1_unlock(void)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
    result = ltc4296_1_write(ADDR_GCMD, LTC4296_1_UNLOCK_KEY);/*GCMD = unlock_key,*/

    return result;
}

/*!
 * @brief           ltc4296_1_isLocked
 *
 * @param [out]     state  state of LTC4296-1 is returned
 *
 * @return
 *                  - #LTC_LOCKED.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function checks if ltc4296-1 is locked
 *
 */
adi_ltc_Result_e ltc4296_1_isLocked(ltc4296_1_state_e *state)
{
	uint16_t val16;
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;

	result = ltc4296_1_read(ADDR_GCMD, &val16);
	if(result != ADI_LTC_SUCCESS)
	{
	    return result;
	}

	if( (val16 & LTC4296_1_UNLOCK_KEY) == LTC4296_1_UNLOCK_KEY)
	{
		*state = LTC_UNLOCKED;
	}
	else
		*state = LTC_LOCKED;

	return result;
}


/*!
 * @brief           ltc4296_1_ReadGADC
 *
 * @param [out]     portVoltage  global ADC value
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function gives global ADC accumulation result
 *
 */
adi_ltc_Result_e ltc4296_1_ReadGADC(float *portVoltage)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint16_t val16;

    //TimerDelay_ms(4); /* Delay of 4ms*/
	result = ltc4296_1_read(ADDR_GADCDAT, &val16);
    if( (val16 & BITM_GADC_NEW) == BITM_GADC_NEW)
    {
        /* A new ADC value is available */
    	*portVoltage = ( ((val16 & BITM_GADC)-2049) * LTC4296_1_VGAIN);
    }
    else
    {
    	*portVoltage = (val16 & BITM_GADC);
    	return ADI_LTC_INVALID_ADC_VOLTAGE;
    }
	return result;
}

/*!
 * @brief           ltc4296_1_SetGADCVin
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function sets Global ADC to read Vin
 *
 */
adi_ltc_Result_e ltc4296_1_SetGADCVin(void)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;

	/* LTC4296-1 Set global ADC to measure Vin
	GADCCFG = ContModeLowGain | Vin */

    result = ltc4296_1_write(ADDR_GADCCFG, 0x0041);
    TimerDelay_ms(4); /* Delay of 4ms*/

    return result;
}

/*!
 * @brief           ltc4296_1_isVinValid
 *
 * @param [in]      portVIn     Vin voltage
 * @param [in]      ltcboardClass   PSE class
 * @param [out]     VinValid    Status of Vin Valid
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function checks if Vin is Valid
 *
 */
adi_ltc_Result_e ltc4296_1_isVinValid(float portVIn, ltc4296_1_boardClass_e ltcboardClass, bool *VinValid)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;

	if( (portVIn <= ltc4296_1_SPOE_VolRange[ltcboardClass][LTC4296_1_VMAX]) && (portVIn >= ltc4296_1_SPOE_VolRange[ltcboardClass][LTC4296_1_VMIN]) )
	{
		*VinValid = TRUE;
	}
	else
	{
		/* Voltage is out of range of the MIN/MAX value*/
		*VinValid = FALSE;
	}
    return result;
}


/*!
 * @brief           ltc4296_1_isVoutValid
 *
 * @param [in]      portVOut     Vout voltage
 * @param [in]      ltcboardClass   PSE class
 * @param [out]     VoutValid    Status of Vout Valid
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function checks if Vout is Valid
 *
 */
adi_ltc_Result_e ltc4296_1_isVoutValid(float portVOut, ltc4296_1_boardClass_e ltcboardClass, bool *VoutValid)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;

	if( (portVOut <= ltc4296_1_SPOE_VolRange[ltcboardClass][LTC4296_1_VMAX]) && (portVOut >= ltc4296_1_SPOE_VolRange[ltcboardClass][LTC4296_1_VMIN]) )
	{
		*VoutValid = TRUE;
	}
	else
	{
		/* Voltage is out of range of the MIN/MAX value*/
		*VoutValid = FALSE;
	}

    return result;
}

/*!
 * @brief           ltc4296_1_DisableGADC
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function disables global ADC
 *
 */
adi_ltc_Result_e ltc4296_1_DisableGADC(void)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;

	/* LTC4296-1 Disable global ADC
       spoewrite 0A,0000 // GADCCFG = Disabled  */

    result = ltc4296_1_write(ADDR_GADCCFG, 0x0000);
    TimerDelay_ms(4); /* Delay of 4ms*/

    return result;
}



/*********************************************************************************************
* Port related Functions
*********************************************************************************************/

/*!
 * @brief           ltc4296_1_readPortEvents
 *
 * @param [in]      portNo     port number
 * @param [out]     portEvents    value of port event register
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function reads the port events
 *
 */
adi_ltc_Result_e ltc4296_1_readPortEvents(ltc4296_1_port_e portNo, uint16_t *portEvents)
{
	adi_ltc_Result_e result;
	uint8_t portAddr = 0;

    getPortAddr(portNo,LTC_PORT_EVENTS, &portAddr);
	result = ltc4296_1_read(portAddr, portEvents);

    return result;
}

/*!
 * @brief           ltc4296_1_clearPortEvents
 *
 * @param [in]      portNo     port number
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function clears the port events
 *
 */
adi_ltc_Result_e ltc4296_1_clearPortEvents(ltc4296_1_port_e portNo)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint8_t portAddr = 0;

    getPortAddr(portNo,LTC_PORT_EVENTS, &portAddr);
	/* Write 1 to clear 0-9 bits*/
	result = ltc4296_1_write(portAddr, 0x3FF);

    return result;
}

/*!
 * @brief           ltc4296_1_readPortStatus
 *
 * @param [in]      portNo     port number
 * @param [out]     portStatus    value of status register of the port
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function reads status of the port register
 *
 */
adi_ltc_Result_e ltc4296_1_readPortStatus(ltc4296_1_port_e portNo, uint16_t *portStatus)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint8_t portAddr = 0;

    getPortAddr(portNo,LTC_PORT_STATUS, &portAddr);
	result = ltc4296_1_read(portAddr, portStatus);

    return result;
}

/*!
 * @brief           ltc4296_1_IsPortDisabled
 *
 * @param [in]      portNo     port number
 * @param [out]     portChk    returns if port is disabled/enabled
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function reads and returns if port is disabled/enabled
 *
 */
adi_ltc_Result_e ltc4296_1_IsPortDisabled(ltc4296_1_port_e portNo, ltc4296_1_portStatus_e *portChk)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint16_t portStatus = 0;
	uint8_t portAddr = 0;

    getPortAddr(portNo,LTC_PORT_STATUS, &portAddr);
    result = ltc4296_1_read(portAddr, &portStatus);
	if( (portStatus & BITM_PSE_STATUS) == LTC_PSE_STATUS_DISABLED)
	{
        *portChk = LTC_PORT_DISABLED;
	}
	else
	{
		*portChk = LTC_PORT_ENABLED;
	}
	return result;
}

/*!
 * @brief           ltc4296_1_PortDisable
 *
 * @param [in]      portNo     port number
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function disables the port
 *
 */
adi_ltc_Result_e ltc4296_1_PortDisable(ltc4296_1_port_e portNo)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint8_t portAddr = 0;

    getPortAddr(portNo,LTC_PORT_CFG0, &portAddr);
	/* Write 0 to disable port*/
	result = ltc4296_1_write(portAddr, 0x0000);

    return result;
}


/*!
 * @brief           ltc4296_1_IsPortDeliverPwr
 *
 * @param [in]       portNo     port number
 * @param [out]      pwrStatus   status if port is delivering power
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function reads port status register and returns if port is delivering power
 *
 */
adi_ltc_Result_e ltc4296_1_IsPortDeliverPwr(ltc4296_1_port_e portNo, ltc4296_1_PSEStatus_e *pwrStatus)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint16_t portStatus;
	uint8_t portAddr = 0;

    getPortAddr(portNo,LTC_PORT_STATUS, &portAddr);
	result = ltc4296_1_read(portAddr, &portStatus);
	if( (portStatus & BITM_PSE_STATUS) == LTC_PSE_STATUS_DELIVERING)
	{
		*pwrStatus = LTC_PSE_STATUS_DELIVERING;
	}
	else
	{
		*pwrStatus = LTC_PSE_STATUS_UNKNOWN;
	}

	return result;
}


/*!
 * @brief           ltc4296_1_IsPortPwrStable
 *
 * @param [in]       portNo      port number
 * @param [out]      pwrStatus   if port power is stable
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function reads port status register and returns if port power is stable
 *
 */
adi_ltc_Result_e ltc4296_1_IsPortPwrStable(ltc4296_1_port_e portNo, bool *pwrStatus)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint16_t portStatus;
	uint8_t portAddr = 0;

    getPortAddr(portNo,LTC_PORT_STATUS, &portAddr);
	result = ltc4296_1_read(portAddr, &portStatus);
	if( (portStatus & BITM_POWER_STABLE) == BITM_POWER_STABLE)
	{
		*pwrStatus = TRUE;
	}
	else
	{
		*pwrStatus = FALSE;
	}
	return result;
}


/*!
 * @brief           ltc4296_1_ReadPortADC
 *
 * @param [in]       portNo     port number
 * @param [out]      portIout   value of port ADC DAT register
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function reads port ADC and value is valid only when NEW bit is set
 *
 */
adi_ltc_Result_e ltc4296_1_ReadPortADC(ltc4296_1_port_e portNo, float *portIout)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint8_t portAddr = 0;
	uint16_t val16;

    getPortAddr(portNo,LTC_PORT_ADCDAT, &portAddr);
	result = ltc4296_1_read(portAddr, &val16);
    if( (val16 & BITM_NEW) == BITM_NEW)
    {
        /* A new ADC value is available */
    	*portIout =( ((val16 & 0x0FFF)-2049) * ltc4296_1_SPOE_Rsense[portNo] );
    }
    else
    {
		*portIout = (val16 & BITM_SOURCE_CURRENT);
    	return ADI_LTC_INVALID_ADC_PORT_CURRENT;
    }
	return result;
}

/*!
 * @brief           ltc4296_1_PortPrebias
 *
 * @param [in]       portNo     port number
 * @param [in]       mode   LTC4296-1 Config mode
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function sets the prebias_overridegood bit only in SCCP mode
 *
 */
adi_ltc_Result_e ltc4296_1_PortPrebias(ltc4296_1_port_e portNo, ltc4296_1_config_e mode)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint8_t portAddr = 0;

    getPortAddr(portNo,LTC_PORT_CFG1, &portAddr);

    if(mode == LTC_CFG_SCCP_MODE)
    {
		/* NOTE: We need to set prebias_overridegood bit
		   because DEMO-ADIN1100D2Z board has all 5 port connected in parallel
		   and the other ports are pulling down their outputs
		   PxCFG1 = tinrush=56.2ms|prebias_overridegood */

		result = ltc4296_1_write(portAddr, 0x0108);
    }
    else if(mode == LTC_CFG_APL_MODE)
    {
		/* PxCFG1 = tinrush=56.2ms|prebias_overridegood|sig_override_good */
		result = ltc4296_1_write(portAddr, 0x0109);
    }
	return result;
}

/*!
 * @brief           ltc4296_1_PortEnAndClassification
 *
 * @param [in]       portNo     port number
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function enables classification
 *
 */
adi_ltc_Result_e ltc4296_1_PortEnAndClassification(ltc4296_1_port_e portNo)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint8_t portAddr = 0;

    getPortAddr(portNo,LTC_PORT_CFG0, &portAddr);
	result = ltc4296_1_write(portAddr, (BITM_SW_EN|BITM_SW_PSE_READY|BITM_SET_CLASSIFICATION_MODE) );
	return result;
}

/*!
 * @brief           ltc4296_1_IsPDCompatible
 *
 * @param [in]      pseClass  PSE Class info
 * @param [in]      sccpResponseData   response bytes from PD
 * @param [out]     pdClass   PD class
 * @return
 *                  - #ADI_LTC_SCCP_PD_CLASS_COMPATIBLE.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details        This function verifies if PD is supported and compatible
 *
 */
adi_ltc_Result_e ltc4296_1_IsPDCompatible(ltc4296_1_boardClass_e pseClass, uint16_t sccpResponseData, uint8_t *pdClass)
{
	adi_ltc_Result_e result = ADI_LTC_SCCP_PD_CLASS_COMPATIBLE;

	result = sccp_IsPD(pseClass,sccpResponseData,pdClass);
    if(result == ADI_LTC_SCCP_PD_CLASS_NOT_SUPPORTED)
    {
    	printf("LTC4296-1 SCCP.. PD class not supported \n");
    }
    else if(result == ADI_LTC_SCCP_PD_CLASS_NOT_COMPATIBLE)
    {
    	printf("LTC4296-1 SCCP.. PD class not compatible \n");
    }
    return result;
}

/*!
 * @brief           ltc4296_1_SetPortMFVS
 *
 * @param [in]       portNo     port number
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function set Port ADC MFVS Threshold Value
 *
 */
adi_ltc_Result_e ltc4296_1_SetPortMFVS(ltc4296_1_port_e portNo)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint8_t portAddr = 0;
	uint16_t val16 = 0;
	float mfvsThreshold = 0,val=0.0;

    getPortAddr(portNo,LTC_PORT_ADCCFG, &portAddr);
    /* LTC4296-1 Set Port ADC MFVS Threshold Value
     */
    val = ltc4296_1_SPOE_SenseResistor[portNo];
    mfvsThreshold = (62.5 * val);
    /* Roundof to the nearest integer */
    val16 = round(mfvsThreshold);
	result = ltc4296_1_write(portAddr, val16);
	return result;
}

/*!
 * @brief           ltc4296_1_SetPortPwr
 *
 * @param [in]       portNo     port number
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details         This function sets the port power
 *
 */
adi_ltc_Result_e ltc4296_1_SetPortPwr(ltc4296_1_port_e portNo)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint8_t portAddr = 0;

    getPortAddr(portNo,LTC_PORT_CFG0, &portAddr);
    /* LTC4296-1 Port Set Power Available & End Classification
     * PxCFG0 = swen|power_available|pse_rdy|set_classification|end_classification
     */
	result = ltc4296_1_write(portAddr, (BITM_SW_EN|BITM_SW_POWER_AVAILABLE|BITM_SW_PSE_READY|BITM_SET_CLASSIFICATION_MODE|BITM_END_CLASSIFICATION) );
	return result;
}

/*!
 * @brief           ltc4296_1_ForcePortPwr
 *
 * @param [in]       portNo     port number
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details          LTC4296-1 Port set Power Available & End Classification
 *
 */
adi_ltc_Result_e ltc4296_1_ForcePortPwr(ltc4296_1_port_e portNo)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint8_t portAddr = 0;

    getPortAddr(portNo,LTC_PORT_CFG0, &portAddr);
    /* LTC4296-1 Port Set Power Available & End Classification
     * PxCFG0 = swen|power_available|pse_rdy|tmfvdo_timer_disable
     */
	result = ltc4296_1_write(portAddr, (BITM_SW_EN|BITM_SW_POWER_AVAILABLE|BITM_SW_PSE_READY|BITM_TMFVDO_TIMER_DISABLE) );
	return result;
}

/*!
 * @brief           ltc4296_1_PortPwrAvailable
 *
 * @param [in]       portNo     port number
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details          LTC4296-1 Port Clear Classification & PSE Ready, Set Power Available
 *
 */
adi_ltc_Result_e ltc4296_1_PortPwrAvailable(ltc4296_1_port_e portNo)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint8_t portAddr = 0;

    getPortAddr(portNo,LTC_PORT_CFG0, &portAddr);
    /* LTC4296-1 Port Clear Classification & PSE Ready, Set Power Available
     * PxCFG0=en|power_available
     */
	result = ltc4296_1_write(portAddr, (BITM_SW_EN|BITM_SW_POWER_AVAILABLE) );
	return result;
}

/*!
 * @brief           ltc4296_1_SetGADCVout
 *
 * @param [in]       portNo     port number
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details          Set global ADC to measure Port Vout
 *
 */
adi_ltc_Result_e ltc4296_1_SetGADCVout(ltc4296_1_port_e portNo)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint16_t val16 = 0;

	/* LTC4296-1 Set global ADC to measure Port Vout
	   GADCCFG=ContModeLowGain|VoutPort2 */

	val16 = (0x001F & SetPortVout[portNo]);
	/* Set Continuous mode with LOW GAIN bit */
    val16 = (val16|0x40);

    result = ltc4296_1_write(ADDR_GADCCFG, val16);

    return result;
}

/*!
 * @brief          ltc4296_1_SCCPResetPulse
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details        This function send a reset pulse and wait for presence pulse response
 *
 */
adi_ltc_Result_e ltc4296_1_SCCPResetPulse(uint8_t *pdPresent)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	*pdPresent = sccpResetPulse();
	return result;
}

/*!
 * @brief           ltc4296_1_SCCPPD
 *
 * @param [in]      resData  response bytes
 * @param [in]      broadCastAddr    broad Caste Address
 * @param [out]     readScratchPad   read ScratchPad
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details        This function sends the required bytes of SCCP to the PD and receives the response
 *
 */
adi_ltc_Result_e ltc4296_1_SCCPPD(uint16_t *resData, uint8_t broadCastAddr, uint8_t readScratchPad)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;

	printf("\naddr = 0x%x, cmd = 0x%x \n  ", broadCastAddr, readScratchPad);
	result = ltc4296_1_SCCPResPD(resData, broadCastAddr, readScratchPad);
    return result;
}

/*!
 * @brief           ltc4296_1_SCCPResPD
 *
 * @param [in]      resData  response bytes
 * @param [in]      broadCastAddr    broad Caste Address
 * @param [out]     readScratchPad   read ScratchPad
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details        This function sends the required bytes of SCCP to the PD and receives the response
 *
 */
adi_ltc_Result_e ltc4296_1_SCCPResPD(uint16_t *resData, uint8_t broadCastAddr, uint8_t readScratchPad)
{
    uint8_t sccp_buf[3] = {0, 0, 0};
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;

	result = sccpReadWritePD(broadCastAddr, readScratchPad, sccp_buf);
    if(result == ADI_LTC_SCCP_PD_LINE_NOT_LOW)
    {
    	printf("PD detection failed, PD line not LOW \n");
    	result = ADI_LTC_SCCP_PD_DETECTION_FAILED;
    }
    else if(result == ADI_LTC_SCCP_PD_NOT_PRESENT)
    {
    	//printf("LTC4296-1 SCCP.. PD Not Present \n");
    }
    else if(result == ADI_LTC_SCCP_PD_CRC_FAILED)
    {
    	printf("PD classification failed, CRC ERROR \n");
    }
    else if(result == ADI_LTC_SCCP_PD_LINE_NOT_HIGH)
    {
    	printf("PD classification failed, PD line not HIGH \n");
    }
    else if(result == ADI_LTC_SCCP_PD_PRESENT)
    {
		*resData = (uint16_t)(sccp_buf[1]) << 8;
		*resData |= (uint16_t)(sccp_buf[0]);
		//*pdCRC = sccp_buf[2];
    }
    return result;
}

/*!
 * @brief        ltc4296_1_printGlobalfaults
 *
 * @param [in]   gEvents  global events
 *
 * @details      This function prints global faults
 *
 */
void ltc4296_1_printGlobalfaults(uint16_t gEvents)
{
      if( (gEvents & BITM_GFLTEV_LOW_CKT_BRK_FAULT) == BITM_GFLTEV_LOW_CKT_BRK_FAULT)
    	  printf("LTC4296-1 low side fault, too high current or negative voltage on output \n");
      else if( (gEvents & BITM_GFLTEV_MEMORY_FAULT) == BITM_GFLTEV_MEMORY_FAULT)
    	  printf("LTC4296-1 memory fault \n");
      else if( (gEvents & BITM_GFLTEV_PEC_FAULT) == BITM_GFLTEV_PEC_FAULT)
    	  printf("LTC4296-1 pec fault \n");
      else if( (gEvents & BITM_GFLTEV_COMMAND_FAULT) == BITM_GFLTEV_COMMAND_FAULT)
    	  printf("LTC4296-1 command fault \n");
      else if( (gEvents & BITM_GFLTEV_UVLO_DIGITAL) == BITM_GFLTEV_UVLO_DIGITAL)
    	  printf("LTC4296-1  UVLO, too low input voltage fault \n");
}

/*!
 * @brief        ltc4296_1_printPortEvents
 *
 * @param [in]   portNo   port number
 * @param [in]   portEvents  port events
 *
 * @details      This function prints port events
 *
 */
void ltc4296_1_printPortEvents(ltc4296_1_port_e portNo, uint16_t portEvents)
{
    if( (portEvents & BITM_LSNS_REVERSE_FAULT) == BITM_LSNS_REVERSE_FAULT)
  	    printf("LTC4296-1 port%d LSNS_REVERSE_FAULT, negative low side current \n",portNo);

    if( (portEvents & BITM_LSNS_FORWARD_FAULT) == BITM_LSNS_FORWARD_FAULT)
  	    printf("LTC4296-1 port%d LSNS_FORWARD_FAULT, too high low side current \n",portNo);

    if( (portEvents & BITM_PD_WAKEUP) == BITM_PD_WAKEUP)
  	    printf("LTC4296-1 port%d PD_WAKEUP, wake up requested by PD \n",portNo);

    if( (portEvents & BITM_TINRUSH_TIMER_DONE) == BITM_TINRUSH_TIMER_DONE)
  	    printf("LTC4296-1 port%d TINRUSH_TIMER_DONE, too long time to power on \n",portNo);

	if( (portEvents & BITM_MFVS_TIMEOUT) == BITM_MFVS_TIMEOUT)
	    printf("LTC4296-1 port%d MFVS_TIMEOUT, PD disconnected \n",portNo);

	if( (portEvents & BITM_OVERLOAD_DETECTED_IPOWERED) == BITM_OVERLOAD_DETECTED_IPOWERED)
	    printf("LTC4296-1 port%d OVERLOAD_DETECTED_IPOWERED, too high output current \n",portNo);

	if( (portEvents & BITM_OVERLOAD_DETECTED_ISLEEP) == BITM_OVERLOAD_DETECTED_ISLEEP)
	    printf("LTC4296-1 port%d OVERLOAD_DETECTED_ISLEEP, un-powered port overloaded \n",portNo);

	if( (portEvents & BITM_TOFF_TIMER_DONE) == BITM_TOFF_TIMER_DONE)
	    printf("LTC4296-1 port%d TOFF_TIMER_DONE, too long time to power off \n",portNo);
}


/*!
 * @brief        ltc4296_1_chkGlobalEvents
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details      This function checks for global events
 *
 */
adi_ltc_Result_e ltc4296_1_chkGlobalEvents(void)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	ltc4296_1_state_e state;
	uint16_t globalEvents=0;

	/* Check if LTC4296-1 is locked */
	ltc4296_1_isLocked(&state);
	if(state == LTC_LOCKED)
	{
		/* Unlock the LTC4296_1 */
		printf("PSE initiated ... \n");
		ltc4296_1_reset();
		ltc4296_1_unlock();
		ltc4296_1_clearGlobalfaults();
		return ADI_LTC_DISCONTINUE_SCCP;
	}
	else if(state == LTC_UNLOCKED)
	{
		//printf("The LTC4296_1 is UNLocked \n");
		ltc4296_1_readGlobalfaults(&globalEvents);
		ltc4296_1_printGlobalfaults(globalEvents);
		if( (globalEvents & BITM_GFLTEV_LOW_CKT_BRK_FAULT) == BITM_GFLTEV_LOW_CKT_BRK_FAULT)
		{
			ltc4296_1_clearCktBreaker();
			return ADI_LTC_DISCONTINUE_SCCP;
		}
		else if( ((globalEvents & BITM_GFLTEV_MEMORY_FAULT) == BITM_GFLTEV_MEMORY_FAULT)   ||
		   		 ((globalEvents & BITM_GFLTEV_PEC_FAULT) == BITM_GFLTEV_PEC_FAULT)         ||
		         ((globalEvents & BITM_GFLTEV_COMMAND_FAULT) == BITM_GFLTEV_COMMAND_FAULT) ||
		         ((globalEvents & BITM_GFLTEV_UVLO_DIGITAL) == BITM_GFLTEV_UVLO_DIGITAL)   )
		{
			/* Global events other than circuit breaker ?*/
    		ltc4296_1_reset();
    		ltc4296_1_unlock();
    		ltc4296_1_clearGlobalfaults();
    		return ADI_LTC_DISCONTINUE_SCCP;
		}
	}
	return result;
}


/*!
 * @brief        ltc4296_1_chkPortEvents
 *
 * @param [in]   ltc4296_1_Port   port number
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details      This function checks for port events
 *
 */
adi_ltc_Result_e ltc4296_1_chkPortEvents(ltc4296_1_port_e ltc4296_1_Port)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint16_t portEvents = 0;

	/* Read the Port Events */
	ltc4296_1_readPortEvents(ltc4296_1_Port, &portEvents);
	/* Report only in case of port event other than signature */
	ltc4296_1_printPortEvents(ltc4296_1_Port, portEvents);
	ltc4296_1_clearPortEvents(ltc4296_1_Port);
	return result;
}


/*!
 * @brief        ltc4296_1_doAPL
 *
 * @param [in]   boardClass    board class
 * @param [in]   ltc4296_1_Port   port number
 * @param [in]   ltc4296_1_VI   port VI values
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details      This function performs the APL workflow
 *
 */
adi_ltc_Result_e ltc4296_1_doAPL(ltc4296_1_boardClass_e boardClass, ltc4296_1_port_e ltc4296_1_Port, ltc4296_1_VI_t *ltc4296_1_VI)
{
	adi_ltc_Result_e result = ADI_LTC_DISCONTINUE_APL;
	ltc4296_1_portStatus_e portStatus;
	ltc4296_1_PSEStatus_e portPwrStatus;
    float portVout,portVIn, portIout;
    bool portPwrStable, VinValid;

    result = ltc4296_1_IsPortDisabled(ltc4296_1_Port, &portStatus);
	if(portStatus == LTC_PORT_ENABLED)
	{
		result = ltc4296_1_IsPortDeliverPwr(ltc4296_1_Port,&portPwrStatus);
        if(portPwrStatus == LTC_PSE_STATUS_DELIVERING)
        {
    		result = ltc4296_1_IsPortPwrStable(ltc4296_1_Port,&portPwrStable);
            if(portPwrStable == TRUE)
    		{
				result = ltc4296_1_ReadGADC(&portVout);
	         	if(result == ADI_LTC_SUCCESS)
	        	{
					//printf("APL LTC4296-1 port %d - PSE Vout %4.1fV \n",ltc4296_1_Port, portVout);
					ltc4296_1_VI->ltc4296_1_Vout = portVout;
				}
				else
				{
					printf("LTC4296-1 port%d APL Vout N/A V \n",ltc4296_1_Port);
					return ADI_LTC_DISCONTINUE_APL;
				}

				result = ltc4296_1_ReadPortADC(ltc4296_1_Port, &portIout);
				if(result == ADI_LTC_SUCCESS)
				{
					//printf("APL LTC4296-1 port %d - PSE Iout %5.1fmA \n",ltc4296_1_Port, portIout);
					ltc4296_1_VI->ltc4296_1_Iout = portIout;
					return ADI_LTC_SUCCESS;
				}
				else
				{
					printf("LTC4296-1 port%d APL Iout N/A mA \n",ltc4296_1_Port);
					return ADI_LTC_DISCONTINUE_APL;
				}
				return ADI_LTC_APL_COMPLETE;
    		}
            else
            {
                printf("LTC4296-1 port%d Output power unstable \n",ltc4296_1_Port);
                return ADI_LTC_DISCONTINUE_APL;
            }
        }
        else
        {
            printf("LTC4296-1 port%d Port not delivering power \n",ltc4296_1_Port);
            return ADI_LTC_DISCONTINUE_APL;
        }
	}
	else if(portStatus == LTC_PORT_DISABLED)
	{
		/*ADC is set to a known state before setting it to read Vin*/
		result = ltc4296_1_write(ADDR_GADCCFG, 0x0000);   /* Disable ADC */
		TimerDelay_ms(4); /* Delay of 4ms */
		result = ltc4296_1_SetGADCVin();   /* To measure Vin*/
		TimerDelay_ms(4); /* Delay of 4ms */
	    result = ltc4296_1_ReadGADC(&portVIn); /* Read Global ADC and calculate Port Vin*/
     	if(result != ADI_LTC_SUCCESS)
    	{
    		printf("LTC4296-1 port%d Vin measurement not valid, power cannot be enabled \n",ltc4296_1_Port);
        	return ADI_LTC_DISCONTINUE_APL;
    	}
     	ltc4296_1_VI->ltc4296_1_Vin = portVIn;
    	result = ltc4296_1_isVinValid(portVIn,boardClass, &VinValid);
    	if(VinValid == TRUE)
    	{
        	if(ltc4296_1_VI->ltc4296_1_printVin == TRUE)
        	{
        		printf("LTC4296-1 Port%d Vin %4.1fV\n", ltc4296_1_Port, (double)portVIn);
        		ltc4296_1_VI->ltc4296_1_printVin = FALSE;
        	}
    	    /* NOTE: We are unconditionally turning on the port
    		   The APL devices do not perform any classification
    		   The APL devices expect the relevant voltage to be available to them when they get connected or at system power up
    		   The power stays on the port when the APL device is disconnected
    		   The power is always limited to the relevant max voltage and max output current
    		*/
        	result = ltc4296_1_PortPrebias(ltc4296_1_Port, LTC_CFG_APL_MODE);
		    result = ltc4296_1_ForcePortPwr(ltc4296_1_Port);
		    result = ltc4296_1_SetGADCVout(ltc4296_1_Port);
			printf("LTC4296-1 port%d power output enabled for %s \n",ltc4296_1_Port, *(ltc4296_1_Class_Str[boardClass]));
			return ADI_LTC_APL_COMPLETE;
    	}
    	else
    	{
			ltc4296_1_VI->ltc4296_1_printVin = TRUE;
        	printf("LTC4296-1 port%d Vin %4.1fV out of range, expected %5.1fV to %5.1fV \n",
            ltc4296_1_Port,(double) portVIn, (double)ltc4296_1_SPOE_VolRange[boardClass][LTC4296_1_VMIN], (double)ltc4296_1_SPOE_VolRange[boardClass][LTC4296_1_VMAX]);
        	return ADI_LTC_DISCONTINUE_APL;
    	}
	}
    return result;
}

/*!
 * @brief        ltc4296_1_doSpoeSccp
 *
 * @param [in]   boardClass    board class
 * @param [in]   ltc4296_1_Port   port number
 * @param [in]   ltc4296_1_VI   port VI values
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details      This function performs the SPOE SCCP workflow
 *
 */
adi_ltc_Result_e ltc4296_1_doSpoeSccp(ltc4296_1_boardClass_e boardClass, ltc4296_1_port_e ltc4296_1_Port, ltc4296_1_VI_t *ltc4296_1_VI)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	ltc4296_1_portStatus_e portChk;
	ltc4296_1_PSEStatus_e psePwrStatus;
    float portVout,portVIn, portIout;
    uint16_t portStatus, sccpResponseData;
    uint8_t PDClass;
    bool VinValid;

    /* Read the Port Status */
    result = ltc4296_1_IsPortDisabled(ltc4296_1_Port, &portChk);
	if(portChk == LTC_PORT_ENABLED)
	{
		result = ltc4296_1_IsPortDeliverPwr(ltc4296_1_Port,&psePwrStatus);
        if(psePwrStatus == LTC_PSE_STATUS_DELIVERING)
        {
         	result = ltc4296_1_ReadGADC(&portVout);
         	if(result == ADI_LTC_SUCCESS)
        	{
        	    //printf("LTC4296-1 Port%d Vout %4.1fV \n",ltc4296_1_Port, portVout);
        	    ltc4296_1_VI->ltc4296_1_Vout = portVout;
        	}
        	else
        	{
        		printf("LTC4296-1 Port%d Vout N/A \n",ltc4296_1_Port);
            	return ADI_LTC_DISCONTINUE_SCCP;
        	}
        	result = ltc4296_1_ReadPortADC(ltc4296_1_Port, &portIout);
        	if(result == ADI_LTC_SUCCESS)
        	{
        	    //printf("LTC4296-1 Port%d Iout %5.1fmA \n",ltc4296_1_Port, portIout);
        	    ltc4296_1_VI->ltc4296_1_Iout = portIout;
        	    return ADI_LTC_SUCCESS;
        	}
    	    else
    	    {
    		    printf("LTC4296-1 Port%d Iout N/A \n",ltc4296_1_Port);
    		    return ADI_LTC_DISCONTINUE_SCCP;
    	    }
        	return ADI_LTC_DISCONTINUE_SCCP;
        }
        else if(psePwrStatus == LTC_PSE_STATUS_UNKNOWN)
        {
        	result = ltc4296_1_PortDisable(ltc4296_1_Port);
        	printf("LTC4296-1 port %d disabling output \n",ltc4296_1_Port);
        	return ADI_LTC_DISCONTINUE_SCCP;
        }
	}
	else if(portChk == LTC_PORT_DISABLED)
	{
		/*ADC is set to a known state before setting it to read Vin*/
		result = ltc4296_1_DisableGADC();   /* Disable ADC */
		TimerDelay_ms(4); /* Delay of 4ms */
		result = ltc4296_1_SetGADCVin();   /* To measure Vin */
		TimerDelay_ms(4); /* Delay of 4ms */
	    result = ltc4296_1_ReadGADC(&portVIn); /* Read Global ADC and calculate Port Vin*/
     	if(result != ADI_LTC_SUCCESS)
    	{
    		printf("LTC4296-1 Port%d Vin measurement not valid, power cannot be enabled \n",ltc4296_1_Port);
        	return ADI_LTC_DISCONTINUE_SCCP;
    	}
     	ltc4296_1_VI->ltc4296_1_Vin = portVIn;
    	result = ltc4296_1_isVinValid(portVIn, boardClass, &VinValid);
    	if(VinValid == TRUE)
		{
        	if(ltc4296_1_VI->ltc4296_1_printVin == TRUE)
        	{
        		printf("LTC4296-1 Port%d Vin %4.1fV\n", ltc4296_1_Port, (double)portVIn);
        		ltc4296_1_VI->ltc4296_1_printVin = FALSE;
        	}
    		result = ltc4296_1_PortPrebias(ltc4296_1_Port, LTC_CFG_SCCP_MODE);
    		result = ltc4296_1_PortEnAndClassification(ltc4296_1_Port);
		    TimerDelay_ms(5); /* Delay of 5ms */

		    result = ltc4296_1_readPortStatus(ltc4296_1_Port, &portStatus);
			if( (portStatus & BITM_PSE_STATUS) == LTC_PSE_STATUS_SEARCHING)
			{
				/* This condition check is specific to DEMO-ADIN1100D2Z,
				 * as the board has all 5 port connected in parallel
				 */
                if( (portStatus & BITM_DET_VLOW) == BITM_DET_VLOW)
                {
                	result = ltc4296_1_PortDisable(ltc4296_1_Port);
    				printf("LTC4296-1 Port%d output voltage for classification too low \n",ltc4296_1_Port);
    				return ADI_LTC_DISCONTINUE_SCCP;
                }
                /* Perform Classification */
                result = ltc4296_1_SCCPResPD(&sccpResponseData, CMD_BROADCAST_ADDR, CMD_READ_SCRATCHPAD);
                if(result == ADI_LTC_SCCP_PD_PRESENT)
                {
                	result = ltc4296_1_IsPDCompatible(boardClass, sccpResponseData,&PDClass);
                    if(result == ADI_LTC_SCCP_PD_CLASS_COMPATIBLE)
                    {
                    	result = ltc4296_1_SetPortMFVS(ltc4296_1_Port);
                    	result = ltc4296_1_SetPortPwr(ltc4296_1_Port);

					    TimerDelay_ms(5); /* Delay of 5ms */

					    result = ltc4296_1_PortPwrAvailable(ltc4296_1_Port);
					    result = ltc4296_1_SetGADCVout(ltc4296_1_Port);
	    				printf("LTC4296-1 Port%d SCCP completed, PD class %d detected, PSE output enabled \n",ltc4296_1_Port, PDClass);
	    				return ADI_LTC_SCCP_COMPLETE;
                    }
                    else /* ADIN_LTC_SCCP_PD_OUTOFRANGE or ADIN_LTC_SCCP_PD_NOT_COMPATIBLE */
                    {
                    	result = ltc4296_1_PortDisable(ltc4296_1_Port);
    				    printf("LTC4296-1 Port%d detected PD class %d, not compatible with PSE %s \n",ltc4296_1_Port,PDClass, *(ltc4296_1_Class_Str[boardClass]));
    				    return ADI_LTC_DISCONTINUE_SCCP;
                    }
                }
                else if(result == ADI_LTC_SCCP_PD_NOT_PRESENT) /* ADIN_LTC_SCCP_PD_NOT_PRESENT */
                {
                	//printf("LTC4296-1 Port%d - PD NOT Present \n",ltc4296_1_Port);
                	ltc4296_1_PortDisable(ltc4296_1_Port);
    				return ADI_LTC_SCCP_PD_NOT_PRESENT;
                }
                else /* Error case - ADIN_LTC_SCCP_PD_DETECTION_FAILED or ADIN_LTC_SCCP_PD_CRC_FAILED or ADI_LTC_SCCP_PD_LINE_NOT_HIGH */
                {
                	result = ltc4296_1_PortDisable(ltc4296_1_Port);
    				return ADI_LTC_DISCONTINUE_SCCP;
                }
			}
			else
			{
				result = ltc4296_1_PortDisable(ltc4296_1_Port);
				printf("LTC4296-1 Port%d could not enter classification mode \n",ltc4296_1_Port);
				return ADI_LTC_DISCONTINUE_SCCP;
			}
		}
		else
		{
			ltc4296_1_VI->ltc4296_1_printVin = TRUE;
        	printf("LTC4296-1 Port%d Vin %4.1fV out of range, expected %5.1fV to %5.1fV \n",
   			ltc4296_1_Port, (double)portVIn, (double)ltc4296_1_SPOE_VolRange[boardClass][LTC4296_1_VMIN], (double)ltc4296_1_SPOE_VolRange[boardClass][LTC4296_1_VMAX]);
        	return ADI_LTC_DISCONTINUE_SCCP;
		}
	}
	return result;
}

/*!
 * @brief        ltc4296_1_pwr_test
 *
 * @param [in]   boardClass    board class
 *
 * @return
 *                  - #ADI_ETH_SPI_ERROR.
 *                  - #ADI_LTC_SUCCESS.
 *
 * @details      This function performs production power test
 *
 */
adi_ltc_Result_e ltc4296_1_pwr_test(ltc4296_1_boardClass_e boardClass)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	ltc4296_1_port_e portNo = LTC_PORT0;
	float portVout,portVIn, portIout, Iexpected;
    bool VinValid, VoutValid;
    uint8_t i = 0;
    bool testResults[LTC4296_1_MAX_PORTS];

	/*ADC is set to a known state before setting it to read Vin*/
	ltc4296_1_DisableGADC();   /* Disable ADC */
	TimerDelay_ms(4); /* Delay of 4ms */
	ltc4296_1_SetGADCVin();   /* To measure Vin */
	TimerDelay_ms(4); /* Delay of 4ms */
	ltc4296_1_ReadGADC(&portVIn); /* Read Global ADC and calculate Port Vin*/
 	if(result != ADI_LTC_SUCCESS)
	{
		printf("Vin measurement not valid, power cannot be enabled, test discontinued\n");
    	return ADI_LTC_DISCONTINUE_APL;
	}
	ltc4296_1_isVinValid(portVIn, boardClass, &VinValid);
	if(VinValid == TRUE)
	{
    	for(i=0; i<LTC4296_1_MAX_PORTS; i++)
    	{
    		portNo = i;
    		ltc4296_1_PortPrebias(portNo, LTC_CFG_APL_MODE);
    		ltc4296_1_ForcePortPwr(portNo);
		    ltc4296_1_SetGADCVout(portNo);
		    TimerDelay_ms(10); /* Delay of 5ms */

         	result = ltc4296_1_ReadGADC(&portVout);
         	if(result == ADI_LTC_SUCCESS)
        	{
         		ltc4296_1_isVoutValid(portVout, boardClass, &VoutValid);
         		if(VoutValid == TRUE)
         		{
        	        printf("LTC4296-1 Port%d Vout %4.1fV \n",portNo,(double)portVout);
         		}
         		else
         		{
         			printf("LTC4296-1 Port%d Vout %4.1fV out of limits \n",portNo,(double)portVout);
         		}
        	}
        	else
        	{
        		printf("LTC4296-1 Port%d Vout N/A \n",portNo);
        	}
        	result = ltc4296_1_ReadPortADC(portNo, &portIout);
        	if(result == ADI_LTC_SUCCESS)
        	{
        	    printf("LTC4296-1 Port%d Iout %3.2fmA\n",portNo, (double)portIout);
        	    Iexpected = (float)(portVout/RTESTLOAD)*1000; //mA
        	    ltc4296_1_PortDisable(portNo);
        	    /* IOUT should be +5% or -5% of the Iexpected*/
        	    if( (portIout <= (Iexpected + (Iexpected*0.05))) && (portIout >= (Iexpected-(Iexpected*0.05))) )
        	    {
        	    	printf("LTC4296-1 Port%d Iexpected %3.2fmA\n",portNo,(double)Iexpected);
        	    	testResults[portNo] = TRUE;
        	    	printf("LTC4296-1 Port%d TEST PASSED\n",portNo);
        	    }
        	    else
        	    {
        	    	printf("LTC4296-1 Port%d Iexpected %3.2fmA, Iout is out of range \n",portNo,(double)Iexpected);
        	    	testResults[portNo] = FALSE;
        	    	printf("LTC4296-1 Port%d - TEST FAILED\n",portNo);
        	    }
        	}
    	    else
    	    {
    		    printf("LTC4296-1 Port%d Iout N/A \n",portNo);
    		    testResults[portNo] = FALSE;
    		    printf("LTC4296-1 Port%d TEST FAILED\n",portNo);
    	    }
    	}
	}
	else
	{
       	printf("LTC4296-1 Vin %2.1fV out of range, expected %5.1fV to %5.1fV \n", (double)portVIn, (double)ltc4296_1_SPOE_VolRange[boardClass][LTC4296_1_VMIN], (double)ltc4296_1_SPOE_VolRange[boardClass][LTC4296_1_VMAX]);
       	return ADI_LTC_DISCONTINUE_TEST;
	}
	printf("LTC4296-1 Production PWR TEST Results... ");
	for(i=0; i<LTC4296_1_MAX_PORTS; i++)
	{
	    if(testResults[i] == FALSE)
	    {
	    	printf("FAILED\n");
	    	return ADI_LTC_TEST_FAILED;
	    }
	}
	printf("PASSED\n");
	return ADI_LTC_TEST_COMPLETE;
}

/**@}*/
