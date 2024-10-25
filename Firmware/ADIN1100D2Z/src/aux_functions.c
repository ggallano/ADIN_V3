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

/** @addtogroup aux Auxiliary Functions
 *  @{
 */

#include <math.h>
#include "drivers\ltc4296_1\spoeLTC4296_1.h"
#include "bsp\boardsupport.h"
#include "drivers\adinPhy\adin1100.h"
#include "platform\adi_platform.h"
#include "aux_functions.h"
#include "cmdsrv\cmd.h"

uint8_t phyDevMem[ADI_PHY_DEVICE_SIZE];

adi_phy_DriverConfig_t phyDrvConfig = {
    .addr       = 0,
    .pDevMem    = (void *)phyDevMem,
    .devMemSize = sizeof(phyDevMem),
    .enableIrq  = true,
};

#define ADIN1100_PHY_ADDR 0x0
#define ADIN1200_PHY_ADDR 0x04

#define ADIN1200_PHYID1_REG_ADDR 0x02
#define ADIN1200_PHYID2_REG_ADDR 0x03

/*! Hardware reset value of ADDR_PHY_ID_1 register, used for device identification. */
#define ADIN1200_PHYID1               (0x0283)
/*! Hardware reset value of ADDR_PHY_ID_2 register (OUI field), used for device identification. */
#define ADIN1200_PHYID2_OUI           (0xBC20)

/*! Hardware reset value of ADDR_PHY_ID_1 register, used for device identification. */
#define ADIN1100_PHYID1               (0x0283)
/*! Hardware reset value of ADDR_PHY_ID_2 register (OUI field), used for device identification. */
#define ADIN1100_PHYID2_OUI           (0xBC81)

#define MAX32670_FW_MAJOR_VERSION (1)    /* Major version*/
#define MAX32670_FW_MINOR_VERSION (0)    /* Minor version*/
#define MAX32670_FW_BUILD_VERSION (0)    /* Build version*/

static const max32670_fw_version FirmwareVersion =
{
   MAX32670_FW_MAJOR_VERSION,
   MAX32670_FW_MINOR_VERSION,
   MAX32670_FW_BUILD_VERSION
};

#define ADIN1100_FRAME_COUNT  (500)

adi_phy_MSELinkQuality_t mseLinkQuality;
static const uint16_t convMseToSqi[ADI_PHY_SQI_NUM_ENTRIES - 1] = {
    0x0A74, 0x084E, 0x0698, 0x053D, 0x0429, 0x034E, 0x02A0
};

/* UART Data receive flags */
volatile uint32_t uartDataAvailable = 0;
volatile uint32_t uartCmdAvailable = 0;

uint8_t strBuf[100] = {0};
volatile int strCounter = 0;
char commandBuffer [100];
#define SIZE_OF_BUFFER 1

/* First Rx  Buffer. */
uint8_t nBufferRx0[SIZE_OF_BUFFER];
dataQueue_t dataQueue[1];

/*
 * @brief        getFWLibVersion
 *
 * @param [in]      none
 * @param [out]     none
 * @return FW version
 *
 * @details    This function reads and returns the FW version
 *
 * @sa         readBoardDetails()
 */
static uint32_t getFWLibVersion(void)
{
    return(FirmwareVersion.major << 24) | (FirmwareVersion.minor & 0x000000FF) << 16 | (FirmwareVersion.build & 0xFFFF);
}

/*!
 * @brief           reads board Details
 *
 * @param [in]      _boardDetails pointer to board_t structure
 *
 * @details         This function fills the hwType, fwVersion and fwbuild to the _boardDetails structure.
 *
 */
void readBoardDetails(board_t *_boardDetails)
{
    sprintf((char*)boardDetails.hwType,"DEMO-ADIN1100D2Z");

    boardDetails.fwVersion =  getFWLibVersion();
    boardDetails.fwBuild = FirmwareVersion.build;
}

/*!
 * @brief           Prints mode
 *
 * @param [in]      mode  HW Config mode number
 *
 * @details         This function prints the status of each switch of HW Config pins on UART
 *
 */
void printMode(uint8_t mode)
{
    char* label1;
    char* label2;
    char* label3;
    char* label4;

    if(mode & 0x01)
    {
        label1 = "ON";
    }
    else
    {
        label1 = "OFF";
    }
    if(mode & 0x02)
    {
        label2 = "ON";
    }
    else
    {
        label2 = "OFF";
    }
    if(mode & 0x04)
    {
        label3 = "ON";
    }
    else
    {
        label3 = "OFF";
    }
    if(mode & 0x08)
    {
        label4 = "ON";
    }
    else
    {
        label4 = "OFF";
    }
    printf("%s-%s-%s-%s", label4, label3, label2, label1);
}

/*!
 * @brief           Prints Greetings
 *
 * @param [in]      _boardDetails  pointer to board_t structure
 *
 * @details         This function prints Greetings on UART
 *
 */
void printGreetings(board_t *_boardDetails)
{
    char *label;

    printf("================================================\n");
    printf("ANALOG DEVICES 10BASE-T1L and SPOE DEMO         \n");
    printf("================================================\n");
    printf("(c) 2023 Analog Devices Inc. All rights reserved\n");
    printf("================================================\n");

    /*FW Version*/
    uint16_t tmp1 = 0;
    uint16_t tmp2 = 1;
    uint32_t tmp3 = 0;
    tmp1 = _boardDetails->fwVersion >> 24;
    tmp2 = (_boardDetails->fwVersion >> 16) & 0xFF;
    tmp3 = _boardDetails->fwBuild;
    printf("Firmware ver.  : %d.%d.%d \n", tmp1, tmp2, tmp3);

    /*HW Type*/
    printf("Hardware type  : %s\n",_boardDetails->hwType);

    /* board config */
    switch(_boardDetails->fwMode)
    {
		case SPOE_CLASS10:
		{
		  label = "Media converter PSE class 10";
		}
		break;
		case SPOE_CLASS11:
		{
		  label = "Media converter PSE class 11";
		}
		break;
		case SPOE_CLASS12:
		{
		  label = "Media converter PSE class 12";
		}
		break;
		case SPOE_CLASS13:
		{
		  label = "Media converter PSE class 13";
		}
		break;
		case SPOE_CLASS14:
		{
		  label = "Media converter PSE class 14";
		}
		break;
		case SPOE_CLASS15:
		{
		  label = "Media converter PSE class 15";
		}
		break;
		case APL_CLASSA:
		{
		  label = "Media converter APL class A";
		}
		break;
		case APL_CLASSA_NOAUTONEG:
		{
		  label = "Media converter APL class A NoAutoneg";
		}
		break;

		case APL_CLASSC:
		{
		  label = "Media converter APL class C";
		}
		break;
		case APL_CLASS3:
		{
		  label = "Media converter APL class 3";
		}
		break;
		case PRODUCTION_POWER_TEST:
		{
		  label = "Production Power Test";
		}
		break;
		case APL_CLASSA_OLD_DEMO:
		{
		  label = "Media converter APL class A old Demo";
		}
		break;
		case SPOE_OFF:
		{
		  label = "Media converter SPOE off";
		}
		break;
		case PRODUCTION_DATA_TEST:
		{
		  label = "Production Data Test";
		}
		break;
		case RESERVED:
		{
		  label = "Reserved";
		}
		break;
		case DEBUGMODE:
		{
		  label = "Debug";
		}
		break;

		default:
		  label = "Debug";
		break;
    }

    printf("uC CFG-3-2-1-0 : ");

    printMode(_boardDetails->fwMode);

    printf(" (Mode %d)\n", _boardDetails->fwMode);

    printf("Firmware Mode  : %s\n", label);

    printf("================================================\n");
    printf("Type '<?><new line>' for a list of commands\n");
    printf("================================================\n");

}

/*!
 * @brief           readBoardConfigPins
 *
 * @param [in]      _boardDetails  pointer to board_t structure
 *
 * @details         This function reads HW config pins on the board
 *
 */
void readBoardConfigPins(board_t *_boardDetails)
{
    uint8_t val8 = 0;

    val8 = BSP_getConfigPins();
    switch(val8)
    {
        case 0:
            _boardDetails->fwMode = SPOE_CLASS10;
        break;
        case 0x0001:
            _boardDetails->fwMode = SPOE_CLASS11;
        break;
        case 0x2:
            _boardDetails->fwMode = SPOE_CLASS12;
        break;
        case 0x3:
            _boardDetails->fwMode = SPOE_CLASS13;
        break;
        case 0x4:
            _boardDetails->fwMode = SPOE_CLASS14;
        break;
        case 0x5:
            _boardDetails->fwMode = SPOE_CLASS15;
        break;
        case 0x6:
            _boardDetails->fwMode = APL_CLASSA;
        break;
        case 0x7:
            _boardDetails->fwMode = APL_CLASSA_NOAUTONEG;
        break;
        case 0x8:
            _boardDetails->fwMode = APL_CLASSC;
        break;
        case 0x9:
            _boardDetails->fwMode = APL_CLASS3;
        break;
        case 0xA:
            _boardDetails->fwMode = PRODUCTION_POWER_TEST;
        break;
        case 0xB:
            _boardDetails->fwMode = APL_CLASSA_OLD_DEMO;
        break;
        case 0xC:
            _boardDetails->fwMode = SPOE_OFF;
        break;
        case 0xD:
            _boardDetails->fwMode = PRODUCTION_DATA_TEST;
        break;
        case 0xE:
            _boardDetails->fwMode = RESERVED;
        break;
        case 0xF:
            _boardDetails->fwMode = DEBUGMODE;
        break;
        default:
            _boardDetails->fwMode = DEBUGMODE;
        break;
    }
}

/*!
 * @brief           systemReset
 *
 * @details         This function performs the system reset
 *
 */
void systemReset(void)
{
	bsp_sysReset();
}

/*!
 * @brief           adin1300_checkIdentity
 *
 * @param [in]      phyAddr     PHY address of ADIN1200
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *                  - #ADI_ETH_UNSUPPORTED_DEVICE.
 *
 * @details         This function is called after a reset event and before the
 *                  initialization of the device is performed.
 *                  It reads the PHY_ID_1 /PHY_ID_2 registers and compares
 *                  them with expected values. If comparison is not successful,
 *                  return ADIN_PHY_UNSUPPORTED_DEVID
 */
static adi_eth_Result_e adin1300_checkIdentity(uint8_t phyAddr)
{
    adi_eth_Result_e    result = ADI_ETH_SUCCESS;
    uint16_t            val16;

    result = adi_MdioRead(phyAddr, ADDR_PHY_ID_1, &val16);
    if(result != ADI_ETH_SUCCESS)
    {
    	printf("SPI adi_MdioRead Failed! (0x%X)\n", result);
    	result = ADI_ETH_COMM_ERROR;
    }
    if (val16 != ADI_PHY_DEV_ID1)
    {
    	printf("Error ADIN1200 ID1 value read = 0x%x", val16);
        result = ADI_ETH_UNSUPPORTED_DEVICE;
    }

    result = adi_MdioRead(phyAddr, ADDR_PHY_ID_2, &val16);
    if(result != ADI_ETH_SUCCESS)
    {
    	printf("SPI adi_MdioRead failed! (0x%X)\n", result);
        result = ADI_ETH_UNSUPPORTED_DEVICE;
    }

    /*Check if the value of PHY_ID_2.OUI matches expected value */
    if((val16 & BITM_PHY_ID_2_PHY_ID_2_OUI) != (ADI_PHY_DEV_ID2_OUI << BITP_PHY_ID_2_PHY_ID_2_OUI))
    {
    	printf("Error ADIN1200 ID2 value read = 0x%x", val16);
        result = ADI_ETH_UNSUPPORTED_DEVICE;
    }
    return result;
}

/*!
 * @brief           adin1100_checkIdentity
 *
 * @param [in]      hDevice     Device handle having PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function is called after a reset event and before the
 *                  initialization of the device is performed.
 *                  It reads the MMD1_DEV_ID1/MMD1_DEV_ID2 registers and compares
 *                  them with expected values. If comparison is not successful,
 *                  return ADIN_PHY_UNSUPPORTED_DEVID
 */
static adi_eth_Result_e adin1100_checkIdentity(adi_phy_Device_t *hDevice)
{
    adi_eth_Result_e    result = ADI_ETH_SUCCESS;
    unsigned short      val16;

    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_MMD1_DEV_ID1, &val16);
    if (result != ADI_ETH_SUCCESS)
    {
    	printf("SPI adi_MdioRead Failed! (0x%X)\n", result);
    	result = ADI_ETH_COMM_ERROR;
    }
    if (val16 != ADI_PHY_DEVID1)
    {
    	printf("Error- ADIN1100 ID1 value read = 0x%x", val16);
        result = ADI_ETH_UNSUPPORTED_DEVICE;
    }

    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_MMD1_DEV_ID2, &val16);
    if (result != ADI_ETH_SUCCESS)
    {
    	printf("SPI adi_MdioRead Failed! (0x%X)\n", result);
    	result = ADI_ETH_COMM_ERROR;
    }

    /* Check if the value of MMD1_DEV_ID2.OUI matches expected value */
    if ((val16 & BITM_MMD1_DEV_ID2_MMD1_DEV_ID2_OUI) != (ADI_PHY_DEVID2_OUI << BITP_MMD1_DEV_ID2_MMD1_DEV_ID2_OUI))
    {
    	printf("sError- ADIN1100 ID value not matching = 0x%x", val16);
        result = ADI_ETH_UNSUPPORTED_DEVICE;
    }
    return result;
}

/*
 *
 * @details     waits for the MDIO interface to come up
 *
 */
static adi_eth_Result_e waitMdio(uint8_t phyAddr)
{
    adi_eth_Result_e    result = ADI_ETH_SUCCESS;
    int32_t             iter = ADI_PHY_MDIO_POWERUP_ITER;
    unsigned short      val16;

    /* Wait until MDIO interface is up */
    /* The MDIO reads will return all 1s as data values before the interface is up, poll DEVID1 register. */
    do
    {
    	if(phyAddr == ADIN1300_PHY_ADDRESS)
    	{
            result = adi_MdioRead(phyAddr, ADDR_PHY_ID_1, &val16);
    	}
//        else if(phyAddr == ADIN1320_PHY_ADDRESS)
//        {
//            result = adi_MdioRead_Cl45(phyAddr, ADDR_MMD1_DEV_ID1, &val16);
//        }
    }while ((result == ADI_ETH_COMM_ERROR) && (--iter));

    return result;
}

/*!
 * @brief           adin1100_discoverPhy
 *
 * @param [in]      hDevice     Device handle having PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function reads the device ID of ADIN1100 and verifies.
 */
adi_eth_Result_e adin1100_discoverPhy(adi_phy_Device_t *hDevice)
{
    adi_eth_Result_e result = ADI_ETH_SUCCESS;

    /* If both the MCU and the ADIN1100 are reset simultaneously */
    /* using the RESET button on the board, the MCU may start    */
    /* scanning for ADIN1100 devices before the ADIN1100 has     */
    /* powered up. This is worse if PHY address is configured as */
    /* 0 (default configuration of the board).                   */
    /* This is taken care of by iterating more than once over    */
    /* the valid MDIO address space.                             */

	/* Wait until MDIO interface is up. */
	result = waitMdio(hDevice->phyAddr);
    if (result != ADI_ETH_SUCCESS)
    {
    	return result;
    }

	/* Checks the identity of the device based on reading of hardware ID registers */
	/* Ensures the device is supported by the driver, otherwise an error is reported. */
    result = adin1100_checkIdentity(hDevice);
	return result;
}

/*!
 * @brief           adin1300_discoverPhy
 *
 * @param [in]      _boardDetails     Device handle having PHY address of ADIN1200
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function reads the device ID of ADIN1200 and verifies.
 */
adi_eth_Result_e adin1300_discoverPhy(board_t *_boardDetails)
{
    adi_eth_Result_e result = ADI_ETH_SUCCESS;

    /* If both the MCU and the ADIN1100 are reset simultaneously */
    /* using the RESET button on the board, the MCU may start    */
    /* scanning for ADIN1100 devices before the ADIN1100 has     */
    /* powered up. This is worse if PHY address is configured as */
    /* 0 (default configuration of the board).                   */
    /* This is taken care of by iterating more than once over    */
    /* the valid MDIO address space.                             */

	/* Wait until MDIO interface is up. */
	result = waitMdio(_boardDetails->adin1300PhyAddr);
    if (result != ADI_ETH_SUCCESS)
    {
    	return result;
    }

	/* Checks the identity of the device based on reading of hardware ID registers */
	/* Ensures the device is supported by the driver, otherwise an error is reported. */
    result = adin1300_checkIdentity(_boardDetails->adin1300PhyAddr);
	return result;
}

/*!
 * @brief           adin1100_phyReset
 *
 * @param [in]      hDevice     Device handle having PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function resets the ADIN1100 device.
 */
adi_eth_Result_e adin1100_phyReset(adi_phy_Device_t *hDevice)
{
    adi_eth_Result_e result = ADI_ETH_SUCCESS;
    unsigned short      val16;
    int32_t             iter = ADI_PHY_MDIO_POWERUP_ITER;

    /* A full chip software reset is initiated by setting the software reset bit (CRSM_SFT_RST).
     * The system ready bit (CRSM_SYS_RDY) indicates that the start-up sequence is
     * complete and the system is ready for normal operation.
     */
    result = adi_MdioRead_Cl45(hDevice->phyAddr , ADDR_CRSM_SFT_RST, &val16);
    if (result != ADI_ETH_SUCCESS)
    {
        result = ADI_ETH_COMM_ERROR;
        goto end;
    }

    val16 |= (1 << BITP_CRSM_SFT_RST_CRSM_SFT_RST);
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_CRSM_SFT_RST, val16);
    if (result != ADI_ETH_SUCCESS)
    {
        result = ADI_ETH_COMM_ERROR;
        goto end;
    }

	TimerDelay_ms(ADIN1100_SW_RESET_DELAY);

    do{
        result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_CRSM_STAT, &val16);
        if (result != ADI_ETH_SUCCESS)
        {
            result = ADI_ETH_COMM_ERROR;
            break;
        }
        if( (val16 & BITM_CRSM_STAT_CRSM_SYS_RDY) == BITM_CRSM_STAT_CRSM_SYS_RDY)
        	break;
    }while(--iter);

end:
    return result;
}

/*!
 * @brief           adin1100_exitSWPD
 *
 * @param [in]      hDevice   Device handle having PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         Disable the Software power down of ADIN1100
 *
 */
adi_eth_Result_e adin1100_exitSWPD(adi_phy_Device_t *hDevice)
{
    unsigned short val16 = 0, swpd = 0;
    uint32_t result = ADI_ETH_SUCCESS;
    int32_t  iter = ADI_PHY_SOFT_PD_ITER;

    /* Clear the CRSM_SFT_PD bit */
    val16 &= ~BITM_CRSM_SFT_PD_CNTRL_CRSM_SFT_PD;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_CRSM_SFT_PD_CNTRL, val16);
    if (result != ADI_ETH_SUCCESS)
    {
	    result = ADI_ETH_COMM_ERROR;
	    goto end;
    }

    /* Wait with timeout for the PHY device to enter the desired state before returning. */
    do
    {
        val16 = 0;
        result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_CRSM_STAT, &swpd);
    	if (result != ADI_ETH_SUCCESS)
    	{
    	    result = ADI_ETH_COMM_ERROR;
    	}
    	if( (swpd & BITM_CRSM_STAT_CRSM_SFT_PD_RDY ) == 0)
    	    break;
    }while(--iter);

    if (iter <= 0)
    {
         result = ADI_ETH_READ_STATUS_TIMEOUT;
    }

end:
	return result;
}

/*!
 * @brief           adin1100_checkLinkPartner
 *
 * @param [in]      hDevice         Device handle having PHY address of ADIN1100
 * @param [in]      _boardDetails   pointer to board_t structure
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function checks the Partner adin1100 is in Master or Slave mode
 *
 */
adi_eth_Result_e adin1100_checkLinkPartner(board_t *_boardDetails, adi_phy_Device_t *hDevice)
{
	adi_eth_Result_e result = ADI_ETH_SUCCESS;
    uint16_t val16 = 0;

    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_AN_LP_ADV_ABILITY_L, &val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error adi_MdioRead_Cl45 failed \n\r");
		result = ADI_ETH_COMM_ERROR;
	}

    val16 = 0;
    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_AN_LP_ADV_ABILITY_M, &val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error adi_MdioRead_Cl45 failed \n\r");
		result = ADI_ETH_COMM_ERROR;
	}

    if( (val16 & BITM_AN_LP_ADV_ABILITY_M_AN_LP_ADV_MST) == BITM_AN_LP_ADV_ABILITY_M_AN_LP_ADV_MST)
    {
    	printf("Link Partner prefer Master \n");
    }
    else
    {
    	printf("Link Partner prefer Slave, Error- expected preferred Master \n");
    	_boardDetails->errorLed = TRUE;
    }

    val16 = 0;
    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_AN_LP_ADV_ABILITY_H, &val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error adi_MdioRead_Cl45 failed \n\r");
		result = ADI_ETH_COMM_ERROR;
	}

    if( (val16 & BITM_AN_LP_ADV_ABILITY_H_AN_LP_ADV_B10L_TX_LVL_HI_ABL) == BITM_AN_LP_ADV_ABILITY_H_AN_LP_ADV_B10L_TX_LVL_HI_ABL)
    {
    	printf("Link Partner Enabled 2.4V, Error- expected Enabled 1V \n");
    	_boardDetails->errorLed = TRUE;
    }
    else
    {
    	printf("Link Partner Enabled 1V \n");
    }
    return result;
}


/*!
 * @brief           adin1100_getSWPD
 *
 * @param [in]      hDevice   Device handle having PHY address of ADIN1100
 * @param [out]     enable    status of SWPD of adin100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function gets the status of the ADIN1100 SWPD
 *
 */
adi_eth_Result_e adin1100_getSWPD(adi_phy_Device_t *hDevice, unsigned short *enable)
{
    adi_eth_Result_e    result = ADI_ETH_SUCCESS;
    unsigned short      val16 = 0;

    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_CRSM_STAT, &val16);
    if (result != ADI_ETH_SUCCESS)
    {
        result = ADI_ETH_COMM_ERROR;
        goto end;
    }

    if( (val16 & BITM_CRSM_STAT_CRSM_SFT_PD_RDY) == BITM_CRSM_STAT_CRSM_SFT_PD_RDY )
    {
        *enable = true;
    }
    else
    {
	    *enable = false;
	}

end:
    return result;
}

/*!
 * @brief           adin1100_setSWPD
 *
 * @param [in]      hDevice  Device handle having PHY address of ADIN1100
 * @param [out]     enable    enables or disables the SWPD in ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function enables or disables the SWPD in ADIN1100
 *
 */
adi_eth_Result_e adin1100_setSWPD(adi_phy_Device_t *hDevice, unsigned short enable)
{
    adi_eth_Result_e    result = ADI_ETH_SUCCESS;
    unsigned short      val16 = 0, swpd = 0;
    uint16_t             iter = ADI_PHY_SOFT_PD_ITER;

    val16 = (enable)? 1:0;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_CRSM_SFT_PD_CNTRL, val16);
    if (result != ADI_ETH_SUCCESS)
    {
	    result = ADI_ETH_COMM_ERROR;
	    goto end;
    }

    /* Wait with timeout for the PHY device to enter the desired state before returning. */
    do
    {
        result = adin1100_getSWPD(hDevice, &swpd);
        if(val16 == swpd)
        	break;
    } while(--iter);

    if (iter <= 0)
    {
    	printf("Error SWPD settings \n\r");
        result = ADI_ETH_READ_STATUS_TIMEOUT;
        goto end;
    }

end:
    return result;
}

/*!
 * @brief           adin1300_phyReset
 *
 * @param [in]      phyAddr PHY HW address of ADIN1200
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function resets the ADIN1200
 *
 */
adi_eth_Result_e adin1300_phyReset(uint8_t phyAddr)
{
    adi_eth_Result_e result = ADI_ETH_SUCCESS;

	adi_MdioWrite(phyAddr,ADDR_EXT_REG_PTR, ADDR_GE_SFT_RST_CFG_EN);
	adi_MdioWrite(phyAddr,ADDR_EXT_REG_DATA, BITM_GE_SFT_RST_CFG_EN_GE_SFT_RST_CFG_EN);

	adi_MdioWrite(phyAddr,ADDR_EXT_REG_PTR, ADDR_GE_SFT_RST);
	adi_MdioWrite(phyAddr,ADDR_EXT_REG_DATA, BITM_GE_SFT_RST_GE_SFT_RST);

	TimerDelay_ms(ADIN1200_SW_RESET_DELAY);

	/* Wait until MDIO interface is up. */
	result = waitMdio(phyAddr);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

    return result;
}

/*!
 * @brief           adin1300_getSWPD
 *
 * @param [in]      phyAddr PHY HW address of ADIN1200
 * @param [out]     enable  status of the SWPD
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function gives status of SWPD in ADIN1200
 *
 */
adi_eth_Result_e adin1300_getSWPD(uint8_t phyAddr, unsigned short *enable)
{
    uint32_t        result = ADI_ETH_SUCCESS;
    unsigned short  val16 = 0;

	result = adi_MdioRead(phyAddr, ADDR_MII_CONTROL, &val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
	}
	if( (val16 & BITM_MII_CONTROL_SFT_PD) == BITM_MII_CONTROL_SFT_PD)
    {
        *enable = TRUE;
    }
    else
    {
	    *enable = FALSE;
	}
    return result;
}


/*!
 * @brief           adin1300_setSWPD
 *
 * @param [in]      phyAddr PHY HW address of ADIN1200
 * @param [out]     enable  enables or disables SWPD
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function enables or disables SWPD in ADIN1200
 *
 */
adi_eth_Result_e adin1300_setSWPD(uint8_t phyAddr, unsigned short enable)
{
    unsigned short  val16 = 0, swpd=0;
    uint32_t        result = ADI_ETH_SUCCESS;
    int32_t         iter = ADI_PHY_SOFT_PD_ITER;

    result = adi_MdioRead(phyAddr, ADDR_MII_CONTROL, &val16);
    if(result != ADI_ETH_SUCCESS)
    {
        return ADI_ETH_COMM_ERROR;
    }


    if(enable)
         val16 |= BITM_MII_CONTROL_SFT_PD;
     else
         val16 &= ~BITM_MII_CONTROL_SFT_PD;
    result = adi_MdioWrite(phyAddr, ADDR_MII_CONTROL, val16);
    if(result != ADI_ETH_SUCCESS)
    {
        result = ADI_ETH_COMM_ERROR;
    }

    /* Wait with timeout for the PHY device to enter the desired state before returning. */
    do
    {
        result = adin1300_getSWPD(phyAddr, &swpd);
        if(enable == swpd)
        	break;
    } while(--iter);

    if(iter <= 0)
    {
         result = ADI_ETH_READ_STATUS_TIMEOUT;
    }
    return result;
}

/*!
 * @brief           adin1100_sp_cfg
 *
 * @param [in]      _boardDetails  pointer to board_t structure
 * @param [in]      hDevice  PHY HW address of ADIN1100
 * @param [in]      cfgType  SPOE Class mode
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function configures the ADIN1100 depending on the cfgType (SPOE Class)
 *
 */
adi_eth_Result_e adin1100_sp_cfg(board_t *_boardDetails, adi_phy_Device_t *hDevice, ltc4296_1_boardClass_e cfgType)
{
	adi_eth_Result_e result = ADI_ETH_SUCCESS;
    unsigned short val16 = 0;

	/* Discover ADIN1100 PHY*/
	result = adin1100_discoverPhy(hDevice);
    if(result != ADI_ETH_SUCCESS)
    {
    	_boardDetails->errorLed = TRUE;
        printf("Error ADIN1100 Discover PHY - %s \n\r", adi_eth_result_string[result]);
    }
    else
    {
    	printf("ADIN1100 MDIO addr %d \n\r",hDevice->phyAddr);
    }

	/* Software Reset ADIN1100 */
    result = adin1100_phyReset(hDevice);
    if(result != ADI_ETH_SUCCESS)
    {
        printf("\n\r Error - ADIN1100 PHY Reset - %s \n\r", adi_eth_result_string[result]);
		result = ADI_ETH_COMM_ERROR;
    }

    if(cfgType == APL_CLASSA_OLD_DEMO)
    {
        printf("ADIN1100 HW Config: old demo,");
    }
    else
    {
        printf("ADIN1100 HW Config: no autoneg,");
    }

    /* Check if ADIN1100 is in RGMII MAC mode */
    /* CRSM_MAC_IF_CFG, reads 0x0001 in RGMII */
    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_CRSM_MAC_IF_CFG, &val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("\n\r Error - adi_MdioRead_Cl45 failed \n\r");
		result = ADI_ETH_COMM_ERROR;
	}
    /* Check if only CRSM_RGMII_EN is enabled */
    if(val16 != BITM_CRSM_MAC_IF_CFG_CRSM_RGMII_EN)
    {
    	printf("Error - ADIN1100 is not in RGMII Mode \n\r");
    	_boardDetails->errorLed = true; /* Flag the RED LED when PHY not in RGMII Mode */
    }

    /* Put ADIN1100 in SW power down mode */
    result = adin1100_setSWPD(hDevice, TRUE);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf(" ADIN1100 is not in SWPD\n\r");
	}

    if(cfgType == APL_CLASSA_OLD_DEMO)
    {
    	/* Disable ADIN1100 delimiter randomization
    	 * TxDelimRandEn clear bit 0 TxDelimRandEn = 0
    	 *  Disable delimiter randomization  */
        result = adi_MdioWrite_Cl45(hDevice->phyAddr, 0x038000, 0x0000);
        if(result != ADI_ETH_SUCCESS)
    	{
        	printf("Error - adi_MdioRead_Cl45 failed \n\r");
    		result = ADI_ETH_COMM_ERROR;
    	}
    }

    /* Disable ADIN1100 auto-negotiation */
    /* The only other bit in this register is AN_RESTART, need to write 0 to it */
    result = adi_MdioWrite_Cl45(hDevice->phyAddr,ADDR_AN_CONTROL,0x00);

    /* Enable ADIN1100 forced mode */
    result = adi_MdioWrite_Cl45(hDevice->phyAddr,ADDR_AN_FRC_MODE_EN,BITM_AN_FRC_MODE_EN_AN_FRC_MODE_EN);

    /* Check the ADIN1100 HW CFG setting Master/Slave
     * AN_ADV_ABILITY_M, check bit AN_ADV_MST */
    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_AN_ADV_ABILITY_M, &val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Erroradi_MdioRead_Cl45 failed \n\r");
		result = ADI_ETH_COMM_ERROR;
	}

    if( (val16 & BITM_AN_ADV_ABILITY_M_AN_ADV_MST) == BITM_AN_ADV_ABILITY_M_AN_ADV_MST)
    {
    	printf(" Master,");
    	adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_PMA_PMD_BT1_CONTROL,0xC002);
    }
    else
    {
    	printf(" slave,");
    	adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_PMA_PMD_BT1_CONTROL,0x8002);
    }

    /* Check the ADIN1100 HW CFG setting amplitude
     * B10L_PMA_CNTRL, check bit B10L_TX_LVL_HI_ABLE */
    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_B10L_PMA_CNTRL, &val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error adi_MdioRead_Cl45 failed \n\r");
		result = ADI_ETH_COMM_ERROR;
	}

    if( (val16 & BITM_B10L_PMA_CNTRL_B10L_TX_LVL_HI) == BITM_B10L_PMA_CNTRL_B10L_TX_LVL_HI)
    {
    	printf(" Tx 2.4V ");
    }
    else
    {
    	printf(" Tx 1.0V");
    }

    /* Set ADIN1100 LED0 [Green LED for Link UP] and LED1[Yellow LED for TX/RX Activity] behavior */
    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_DIGIO_PINMUX, &val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error - adi_MdioRead_Cl45 failed \n\r");
		result = ADI_ETH_COMM_ERROR;
	}
    val16 &= !(BITM_DIGIO_PINMUX_DIGIO_LED1_PINMUX);
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_DIGIO_PINMUX, val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error - adi_MdioWrite_Cl45 failed \n\r");
		result = ADI_ETH_COMM_ERROR;
	}

    val16 = 0x8487;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_LED_CNTRL, val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error - adi_MdioWrite_Cl45 failed \n\r");
		result = ADI_ETH_COMM_ERROR;
	}

    /* ADIN1100 in out of SW power down mode */
    result = adin1100_setSWPD(hDevice, FALSE);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error ADIN1100 is not out of SWPD\n\r");
	}
    printf("\n=================================================\n");
    return result;
}

/*!
 * @brief           adin1100_cfg
 *
 * @param [in]      _boardDetails pointer to board_t structure
 * @param [in]      hDevice  Device handle having PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function configures the ADIN1100
 *
 */
adi_eth_Result_e adin1100_cfg(board_t *_boardDetails, adi_phy_Device_t *hDevice)
{
	adi_eth_Result_e result = ADI_ETH_SUCCESS;
    unsigned short val16 = 0;

	/* Discover ADIN1100 PHY*/
	result = adin1100_discoverPhy(hDevice);
    if(result != ADI_ETH_SUCCESS)
    {
    	_boardDetails->errorLed = TRUE;
        printf("Error - ADIN1100 discover PHY - %s \n\r", adi_eth_result_string[result]);
        goto end;
    }
    else
    {
    	printf("ADIN1100 MDIO address %x \n\r",hDevice->phyAddr);
    }

	/* Software Reset ADIN1100 */
    result = adin1100_phyReset(hDevice);
    if(result != ADI_ETH_SUCCESS)
    {
        printf("Error - ADIN1100 PHY reset - %s \n\r", adi_eth_result_string[result]);
		result = ADI_ETH_COMM_ERROR;
    }

    printf("ADIN1100 HW CFG: autoneg,");

    /* Check if ADIN1100 is in RGMII MAC mode */
    /* CRSM_MAC_IF_CFG, reads 0x0001 in RGMII */
    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_CRSM_MAC_IF_CFG, &val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error - adi_MdioRead_Cl45 failed \n\r");
		result = ADI_ETH_COMM_ERROR;
	}

    /* Check if ONLY CRSM_RGMII_EN bit is enabled */
    if(val16 != BITM_CRSM_MAC_IF_CFG_CRSM_RGMII_EN)
    {
    	printf("\n\r Error - ADIN1100 is not in RGMII mode \n\r");
    	_boardDetails->errorLed = true; /* Flag the RED LED when PHY not in RGMII Mode */
    }

    /* Check the ADIN1100 HW CFG setting Master/Slave
     * AN_ADV_ABILITY_M, check bit AN_ADV_MST */
    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_AN_ADV_ABILITY_M, &val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error - adi_MdioRead_Cl45 failed \n\r");
		result = ADI_ETH_COMM_ERROR;
	}

    if( (val16 & BITM_AN_ADV_ABILITY_M_AN_ADV_MST) == BITM_AN_ADV_ABILITY_M_AN_ADV_MST)
    {
    	printf(" prefer Master,");
    }
    else
    	printf(" prefer Slave,");

    /* Check the ADIN1100 HW CFG setting amplitude
     * B10L_PMA_CNTRL, check bit B10L_TX_LVL_HI_ABLE */
    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_B10L_PMA_CNTRL, &val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error - adi_MdioRead_Cl45 failed \n\r");
		result = ADI_ETH_COMM_ERROR;
	}

    if( (val16 & BITM_B10L_PMA_CNTRL_B10L_TX_LVL_HI) == BITM_B10L_PMA_CNTRL_B10L_TX_LVL_HI)
    {
    	printf(" Tx 2.4V");
    }
    else
    {
    	printf(" Tx 1.0V");
    }
    /* Put ADIN1100 in SW power down mode */
    result = adin1100_setSWPD(hDevice, TRUE);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf(" ADIN1100 is not in SWPD\n\r");
	}

    /* Set ADIN1100 LED0 [Green LED for Link UP] and LED1[Yellow LED for TX/RX Activity] behavior */
    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_DIGIO_PINMUX, &val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error - adi_MdioRead_Cl45 failed \n\r");
		result = ADI_ETH_COMM_ERROR;
	}
    val16 &= !(BITM_DIGIO_PINMUX_DIGIO_LED1_PINMUX);
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_DIGIO_PINMUX, val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error - adi_MdioWrite_Cl45 failed \n");
		result = ADI_ETH_COMM_ERROR;
	}

    val16 = 0x8487;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_LED_CNTRL, val16);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error - adi_MdioWrite_Cl45 failed \n");
		result = ADI_ETH_COMM_ERROR;
	}

    /* Bring ADIN1100 out of SW power down mode */
    result = adin1100_setSWPD(hDevice, FALSE);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error adin1100 is not out of SWPD\n");
	}
    printf("\n================================================\n");

end:
    return result;
}

/*!
 * @brief           adin1300_cfg
 *
 * @param [in]      _boardDetails PHY HW address of ADIN1200
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function configures the ADIN1200
 *
 */
adi_eth_Result_e adin1300_cfg(board_t *_boardDetails)
{
	adi_eth_Result_e result = ADI_ETH_SUCCESS;
    unsigned short  val16=0;

	/* Discover ADIN1200 PHY*/
	result = adin1300_discoverPhy(_boardDetails);
    if(result != ADI_ETH_SUCCESS)
    {
    	_boardDetails->errorLed = TRUE;
    	printf("Error ADIN1200 PHY discovery - %s \n\r", adi_eth_result_string[result]);
        goto end;
    }
    else
    	printf("ADIN1200 MDIO address %d \n\r",_boardDetails->adin1300PhyAddr);

	/* Software Reset ADIN1200 */
    result = adin1300_phyReset(_boardDetails->adin1300PhyAddr);
    if(result != ADI_ETH_SUCCESS)
    {
        printf("Error ADIN1200 PHY reset - %s \n\r", adi_eth_result_string[result]);
        result = ADI_ETH_COMM_ERROR;
    }

    /* Enter ADIN1200 SW power down mode */
    result = adin1300_setSWPD(_boardDetails->adin1300PhyAddr,TRUE);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error ADIN1200 is not in SWPD\n\r");
	}

    /**********************************/
    /* Setup LED for activity */
    val16 = 0;
	result = adi_MdioRead(_boardDetails->adin1300PhyAddr, ADDR_LED_CTRL_1, &val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }
    val16 |= (BITM_LED_CTRL_1_LED_A_EXT_CFG_EN|BITM_LED_CTRL_1_LED_PUL_STR_EN);// Enable Extended LED Configuration
	result = adi_MdioWrite(_boardDetails->adin1300PhyAddr, ADDR_LED_CTRL_1, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

	//11001: blink on activity
	val16 = 0x2109;
	result = adi_MdioWrite(_boardDetails->adin1300PhyAddr, ADDR_LED_CTRL_2, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

    /**********************************/
    /* EXT_REG_PTR = 0xFF38 - Preamble recovery register */
	/* FF38 register address is not available in the Data sheet */

    val16 = 0xFF38;
	result = adi_MdioWrite(_boardDetails->adin1300PhyAddr, ADDR_EXT_REG_PTR, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

    /* EXT_REG_DATA = 0x0001 // Enable preamble recovery */
    val16 = 0x0001;
	result = adi_MdioWrite(_boardDetails->adin1300PhyAddr, ADDR_EXT_REG_DATA, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

    /**********************************/
	/* AUTONEG_ADV=FD_10_ADV | SELECTOR_ADV - Autoneg 10Mb FD  */
	/* 0x0041 = (BITM_AUTONEG_ADV_FD_10_ADV|0x01)
	 * BITP_AUTONEG_ADV_HD_100_ADV is set as 0
	 * BITP_AUTONEG_ADV_HD_10_ADV is set as 0 */
//    val16 = 0x0041;
//	result = adi_MdioWrite(_boardDetails->adin1300PhyAddr, ADDR_AUTONEG_ADV, val16);
//	if(result != ADI_ETH_SUCCESS)
//	{
//		result = ADI_ETH_COMM_ERROR;
//    }

    /*******************************************/
    /* MII_CONTROL=DPLX_MODE|RESTART_ANEG|AUTONEG_EN */
//    val16 = 0;
//    result = adi_MdioRead(_boardDetails->adin1300PhyAddr, ADDR_MII_CONTROL, &val16);
//    if(result != ADI_ETH_SUCCESS)
//    {
//        return ADI_ETH_COMM_ERROR;
//    }
//    val16 |= (BITM_MII_CONTROL_AUTONEG_EN | BITM_MII_CONTROL_RESTART_ANEG | BITM_MII_CONTROL_DPLX_MODE);
//    result = adi_MdioWrite(_boardDetails->adin1300PhyAddr, ADDR_MII_CONTROL, val16);
//    if(result != ADI_ETH_SUCCESS)
//    {
//        result = ADI_ETH_COMM_ERROR;
//    }

	if(result == ADI_ETH_SUCCESS)
        printf("ADIN1200 SW CFG: autoneg 10Mbit Full Duplex Only ");
	else
		printf("Error ADIN1200 SW CFG \r\n");

    /*******************************************/

    /* Bring ADIN1200 out of SW power down mode */
    result = adin1300_setSWPD(_boardDetails->adin1300PhyAddr,FALSE);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error adin1200 is not out of SWPD\n\r");
	}
    printf("\n================================================\n");
end:
    return result;
}

/*!
 * @brief           adin_phyReset
 *
 * @param [in]      boardDetails PHY HW address of ADIN1200
 * @param [in]      hDevice Device handle having PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function configures the ADIN1200
 *
 */
adi_eth_Result_e adin_phyReset(board_t * boardDetails, adi_phy_Device_t *hDevice)
{
    uint32_t result = ADI_ETH_SUCCESS;
	/* Resets the PHYs */
	result = adin1100_phyReset(hDevice);
	if(result != ADI_ETH_SUCCESS)
	{
		printf("\n\r ADIN1100 PHY Reset Error - %s \n\r", adi_eth_result_string[result]);
		result = ADI_ETH_COMM_ERROR;
	}

	result = adin1300_phyReset(boardDetails->adin1300PhyAddr);
	if(result != ADI_ETH_SUCCESS)
	{
		printf("\n\r ADIN1200 PHY Reset Error - %s \n\r", adi_eth_result_string[result]);
		result = ADI_ETH_COMM_ERROR;
	}
	return result;
}

/*!
 * @brief           phyTest_TxDisabled
 *
 * @param [in]      hDevice Device handle having PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function performs the PHY Test - TX Disabled
 *
 */
adi_eth_Result_e phyTest_TxDisabled(adin1100_DeviceHandle_t hDevice)
{
    uint32_t result = ADI_ETH_SUCCESS;
    unsigned short  val16=0;

    /*ADIN1100 SW CFG: Test TX Disabled (Return Loss) */

	/* Enter software powerdown*/
    result = adin1100_setSWPD(hDevice->pPhyDevice, TRUE);
	if(result != ADI_ETH_SUCCESS)
	{
		printf("\n\r ADIN1100 - Failed to enter SWPD mode");
		result = ADI_ETH_COMM_ERROR;
    }

	/*Disable auto negotiation*/
	val16 = 0;
	result = adi_MdioWrite_Cl45(hDevice->pPhyDevice->phyAddr, ADDR_AN_CONTROL, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

	/*	Enable forced mode */
	val16 = 1 << BITP_AN_FRC_MODE_EN_AN_FRC_MODE_EN;
	result = adi_MdioWrite_Cl45(hDevice->pPhyDevice->phyAddr, ADDR_AN_FRC_MODE_EN, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

	/*	mdiowr_cl45 0,0108F6,4000 // B10L_PMA_CNTRL set B10L_TX_DIS_MODE_EN = 1 // Tx disabled mode */
	/* Tx disabled mode - 10BASE-T1L Transmit Disable Mode. When this bit is set to 1, it disables output on the
       transmit path. Otherwise, it enables output on the transmit path*/
	val16 = 1 << BITP_B10L_PMA_CNTRL_B10L_TX_DIS_MODE_EN;
	result = adi_MdioWrite_Cl45(hDevice->pPhyDevice->phyAddr, ADDR_B10L_PMA_CNTRL, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

	/* Exit software powerdown*/
    result = adin1100_setSWPD(hDevice->pPhyDevice, FALSE);
	if(result != ADI_ETH_SUCCESS)
	{
		printf("ADIN1100 - Failed to exit SWPD mode");
		result = ADI_ETH_COMM_ERROR;
    }

	if(result == ADI_ETH_SUCCESS)
	{
		printf("PHY Test Tx Disabled Configured !\n\r");
    }
	return result;
}

/*!
 * @brief           phyTest_mode1
 *
 * @param [in]      hDevice Device handle having PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function performs the PHY Test Mode1
 *
 */
adi_eth_Result_e phyTest_mode1(adin1100_DeviceHandle_t hDevice)
{
    uint32_t result = ADI_ETH_SUCCESS;
    unsigned short  val16=0;

	/* ADIN1100 SW CFG: Test Mode 1 (Jitter)
	Reset board or enter another mode to exit test */

	/* Enter software powerdown*/
    result = adin1100_setSWPD(hDevice->pPhyDevice, TRUE);
	if(result != ADI_ETH_SUCCESS)
	{
		printf("\n\r ADIN1100 - Failed to enter SWPD mode");
		result = ADI_ETH_COMM_ERROR;
    }

	/*Disable auto negotiation*/
	val16 = 0;
	result = adi_MdioWrite_Cl45(hDevice->pPhyDevice->phyAddr, ADDR_AN_CONTROL, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

	/*	Enable forced mode */
	val16 = 1 << BITP_AN_FRC_MODE_EN_AN_FRC_MODE_EN;
	result = adi_MdioWrite_Cl45(hDevice->pPhyDevice->phyAddr, ADDR_AN_FRC_MODE_EN, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

	/* 	Test Mode 1, Jitter, +1/-1 symbols (3.75MHz)*/
	val16 = (ENUM_B10L_TEST_MODE_CNTRL_B10L_TX_TEST_MODE_IEEE_TX_TM_JITTER << BITP_B10L_TEST_MODE_CNTRL_B10L_TX_TEST_MODE);
	result = adi_MdioWrite_Cl45(hDevice->pPhyDevice->phyAddr, ADDR_B10L_TEST_MODE_CNTRL, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

	/* Exit software powerdown*/
    result = adin1100_setSWPD(hDevice->pPhyDevice, FALSE);
	if(result != ADI_ETH_SUCCESS)
	{
		printf("ADIN1100 - Failed to exit SWPD mode");
		result = ADI_ETH_COMM_ERROR;
    }

	if(result == ADI_ETH_SUCCESS)
	{
		printf("PHY Test Mode1 (Jitter) Configured !\n\r");
    }
	return result;
}

/*!
 * @brief           phyTest_mode12
 *
 * @param [in]      hDevice Device handle having PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function performs the PHY Test Mode2
 *
 */
adi_eth_Result_e phyTest_mode2(adin1100_DeviceHandle_t hDevice)
{
    uint32_t result = ADI_ETH_SUCCESS;
    unsigned short  val16=0;

	/* ADIN1100 SW CFG: Test Mode 2 (Droop)
	Reset board or enter another mode to exit test */

	/* Enter software powerdown*/
    result = adin1100_setSWPD(hDevice->pPhyDevice, TRUE);
	if(result != ADI_ETH_SUCCESS)
	{
		printf("\n\r ADIN1100 - Failed to enter SWPD mode");
		result = ADI_ETH_COMM_ERROR;
    }

	/*Disable auto negotiation*/
	val16 = 0;
	result = adi_MdioWrite_Cl45(hDevice->pPhyDevice->phyAddr, ADDR_AN_CONTROL, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

	/*	Enable forced mode */
	val16 = 1 << BITP_AN_FRC_MODE_EN_AN_FRC_MODE_EN;
	result = adi_MdioWrite_Cl45(hDevice->pPhyDevice->phyAddr, ADDR_AN_FRC_MODE_EN, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

	/* 	Test Mode 2, Droop, +1/-1 symbols (3.75MHz)*/
	val16 = (ENUM_B10L_TEST_MODE_CNTRL_B10L_TX_TEST_MODE_IEEE_TX_TM_DROOP << BITP_B10L_TEST_MODE_CNTRL_B10L_TX_TEST_MODE);
	result = adi_MdioWrite_Cl45(hDevice->pPhyDevice->phyAddr, ADDR_B10L_TEST_MODE_CNTRL, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

	/* Exit software powerdown*/
    result = adin1100_setSWPD(hDevice->pPhyDevice, FALSE);
	if(result != ADI_ETH_SUCCESS)
	{
		printf("ADIN1100 - Failed to exit SWPD mode");
		result = ADI_ETH_COMM_ERROR;
    }

	if(result == ADI_ETH_SUCCESS)
	{
		printf("PHY Test Mode2 (Droop) Configured !\n\r");
    }
	return result;
}

/*!
 * @brief           phyTest_mode3
 *
 * @param [in]      hDevice Device handle having PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function performs the PHY Test Mode3
 *
 */
adi_eth_Result_e phyTest_mode3(adin1100_DeviceHandle_t hDevice)
{
    uint32_t result = ADI_ETH_SUCCESS;
    unsigned short val16 = 0;

	/* ADIN1100 SW CFG: Test Mode 3 (Idle)
	Reset board or enter another mode to exit test */

	/* Enter software power down*/
    result = adin1100_setSWPD(hDevice->pPhyDevice, TRUE);
	if(result != ADI_ETH_SUCCESS)
	{
		printf("\n\r ADIN1100 - Failed to enter SWPD mode");
		result = ADI_ETH_COMM_ERROR;
    }

	/*Disable auto negotiation*/
	val16 = 0;
	result = adi_MdioWrite_Cl45(hDevice->pPhyDevice->phyAddr, ADDR_AN_CONTROL, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

	/*	Enable forced mode */
	val16 = 1 << BITP_AN_FRC_MODE_EN_AN_FRC_MODE_EN;
	result = adi_MdioWrite_Cl45(hDevice->pPhyDevice->phyAddr, ADDR_AN_FRC_MODE_EN, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

	/* 	Test Mode 3, Idle, +1/-1 symbols (3.75MHz)*/
	val16 = (ENUM_B10L_TEST_MODE_CNTRL_B10L_TX_TEST_MODE_IEEE_TX_TM_IDLE << BITP_B10L_TEST_MODE_CNTRL_B10L_TX_TEST_MODE);
	result = adi_MdioWrite_Cl45(hDevice->pPhyDevice->phyAddr, ADDR_B10L_TEST_MODE_CNTRL, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

	/* Exit software power down*/
    result = adin1100_setSWPD(hDevice->pPhyDevice, FALSE);
	if(result != ADI_ETH_SUCCESS)
	{
		printf("ADIN1100 - Failed to exit SWPD mode");
		result = ADI_ETH_COMM_ERROR;
    }

	if(result == ADI_ETH_SUCCESS)
	{
		printf("PHY Test Mode3 Idle Configured !\n\r");
    }
	return result;
}

/*!
 * @brief           adin1200_remLoopback
 *
 * @param [in]      boardDetails pointer to board_t struct, has PHY address of ADIN1200
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function sets ADIN1200 in remote loopback mode
 *
 */
adi_eth_Result_e adin1200_remLoopback(board_t *boardDetails)
{
	adi_eth_Result_e result = ADI_ETH_SUCCESS;
    unsigned short val16 = 0;

    /* Bring ADIN1200 into of SW power down mode */
    result = adin1300_setSWPD(boardDetails->adin1300PhyAddr,TRUE);
    if(result != ADI_ETH_SUCCESS)
    {
     	printf("ADIN1200 NOT out of SWPD\n");
        goto end;
 	}

     // ADIN1200 PHY_CTRL_STATUS_1 set LB_REMOTE_EN, ISOLATE_RX, LB_TX_SUP // rem.loopback
    val16 = 0x0341;
	result = adi_MdioWrite(boardDetails->adin1300PhyAddr, ADDR_PHY_CTRL_STATUS_1, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
        goto end;
    }

    /* Bring ADIN1200 out of SW power down mode */
    result = adin1300_setSWPD(boardDetails->adin1300PhyAddr,FALSE);
    if(result != ADI_ETH_SUCCESS)
    {
     	printf("ADIN1200 NOT out of SWPD\n");
        goto end;
 	}
    else
    {
        printf("ADIN1200 set in remote loopback mode\n");
    }
end:
    return result;
}


/*!
 * @brief           adin1100_remLoopback
 *
 * @param [in]      boardDetails pointer to board_t struct
 * @param [in]      hDevice  pointer to PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function sets ADIN1100 in remote loopback mode
 *
 */
adi_eth_Result_e adin1100_remLoopback(board_t *boardDetails, adi_phy_Device_t *hDevice)
{
	adi_eth_Result_e result = ADI_ETH_SUCCESS;
    unsigned short val16 = 0;

	/* Enter software power down*/
    result = adin1100_setSWPD(hDevice, TRUE);
	if(result != ADI_ETH_SUCCESS)
	{
		printf("\n\r ADIN1100 - Failed to enter SWPD mode");
		result = ADI_ETH_COMM_ERROR;
    }

     // MAC_IF_LOOPBACK set MAC_IF_LB_TX_SUP_EN | MAC_IF_REM_LB_EN | MAC_IF_REM_LB_RX_SUP_EN
    val16 = 0x000E;
	result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_MAC_IF_LOOPBACK, val16);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
        goto end;
    }

	/* Exit software powerdown*/
    result = adin1100_setSWPD(hDevice, FALSE);
	if(result != ADI_ETH_SUCCESS)
	{
		printf("ADIN1100 - Failed to exit SWPD mode");
		result = ADI_ETH_COMM_ERROR;
    }
    else
    {
        printf("ADIN1100 set in remote loopback mode\n");
    }

end:
    return result;
}

/*!
 * @brief           adin1100_frameGenACheck
 *
 * @param [in]      boardDetails pointer to board_t struct
 * @param [in]      hDevice  pointer to PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function calls adin1100_initFrameGen() to set ADIN1100 in frameGen mode
 *
 */
adi_eth_Result_e adin1100_frameGenACheck(board_t *boardDetails, adi_phy_Device_t *hDevice)
{
	adi_eth_Result_e result = ADI_ETH_SUCCESS;

	boardDetails->readMSE = TRUE;
	boardDetails->frameGenEnabled = TRUE;
	boardDetails->startFrameCheck = TRUE;

	result = adin1100_initFrameGen(hDevice);
    if(result == ADI_ETH_SUCCESS)
	{
        printf("ADIN1100 frame generator enabled \n");
	}
    return result;
}


/*!
 * @brief           adin1100_StartFrameGen
 *
 * @param [in]      hDevice  pointer to PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function starts ADIN1100 frame generation
 *
 */
adi_eth_Result_e adin1100_StartFrameGen(adi_phy_Device_t *hDevice)
{
    adi_eth_Result_e  result = ADI_ETH_SUCCESS;
    uint16_t val16 = 0;

    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_RX_ERR_CNT, &val16);
    if (result != ADI_ETH_SUCCESS)
    {
    	printf("\adin1100_StartFrameGen() - SPI adi_MdioRead_Cl45 Failed! (0x%X)\n", result);
    	result = ADI_ETH_COMM_ERROR;
    }

    val16 = 1;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_FG_EN, val16);
    if (result != ADI_ETH_SUCCESS)
    {
    	printf("\adin1100_StartFrameGen() - SPI adi_MdioWrite_Cl45 Failed! (0x%X)\n", result);
    	result = ADI_ETH_COMM_ERROR;
    }
    return result;
}


/*!
 * @brief           adin1100_initFrameGen
 *
 * @param [in]      hDevice  pointer to PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function initializes ADIN1100 to generate frames.
 *
 */
adi_eth_Result_e adin1100_initFrameGen(adi_phy_Device_t *hDevice)
{
    uint16_t val16 = 0;
    adi_eth_Result_e    result = ADI_ETH_SUCCESS;

	/* FG_EN Disable Frame Generator (to restart it) */
    val16 = 0x0;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_FG_EN, val16);

    /* FG_CNTRL_RSTRT Payload random - no restart */
    val16 = 0x1;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_FG_CNTRL_RSTRT, val16);

    /* FG_CONT_MODE_EN 0 = Burst mode 1 = Continuous mode */
    val16 = 0x0;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_FG_CONT_MODE_EN, val16);

    /* FG_IRQ_EN 0 = Interrupt disabled */
    val16 = 0x0;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_FG_IRQ_EN, val16);

    /* FG_NO_FCS_NO_HDR 0 = Include dest+src+FCS */
    val16 = 0x0;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, 0x1F8024, val16);

    /* FG_FRM_LEN net payload min=46 default =107 max=1500 */
    val16 = 0x05DC;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_FG_FRM_LEN, val16);

    /* FG_IFG_LEN Interframe gap default = 0x0C = 12 */
    val16 = 0xC;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_FG_IFG_LEN, val16);

    /* FG_NFRM_H # frames in burst-MSB */
    val16 = 0x0;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_FG_NFRM_H, val16);

    /* FG_NFRM_L #frames in burst-LSB set to 500 */
    val16 = ADIN1100_FRAME_COUNT;//0x1f4;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_FG_NFRM_L, val16);

    return result;
}

/*!
 * @brief           adin1100_restartFramegen
 *
 * @param [in]      hDevice  pointer to PHY address of ADIN1100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function restarts frame generation in ADIN1100
 *
 */
adi_eth_Result_e adin1100_restartFramegen(adi_phy_Device_t *hDevice)
{
    adi_eth_Result_e result = ADI_ETH_SUCCESS;
    uint16_t val16 = 0;

    val16 = 0x9;
    result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_FG_CNTRL_RSTRT, val16);
    if(result != ADI_ETH_SUCCESS)
    {
    	printf("\n restartFramegen() - SPI adi_MdioWrite_Cl45 Failed! (0x%X)\n", result);
    	result = ADI_ETH_COMM_ERROR;
    }
    return result;
}


/*
 * @brief           adin1100_ReadErrPackets
 *
 * @param [in]      hDevice      pointer to PHY address of ADIN1100
 * @param [out]     rxErr  number of received packets with errors
 * @param [out]     rxCnt  number of received packets
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details        Read Errors and total number of received frames
 *
 * @sa
 */
adi_eth_Result_e adin1100_ReadErrPackets(adi_phy_Device_t *hDevice, uint32_t *rxErr, uint32_t *rxCnt)
{
    adi_eth_Result_e result = ADI_ETH_SUCCESS;
    uint16_t val16 = 0;
    uint32_t packetCnt = 0;

    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_RX_ERR_CNT, &val16);
    if (result != ADI_ETH_SUCCESS)
    {
    	printf("\nadi_phy_ReadErrPackets() - SPI adi_MdioRead Failed! (0x%X)\n", result);
    	result = ADI_ETH_COMM_ERROR;
    }
    *rxErr = val16;

    val16 = 0;
    /* Read the Higher bits [31:16] */
    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_FC_FRM_CNT_H, &val16);
    if (result != ADI_ETH_SUCCESS)
    {
    	printf("\nadi_phy_ReadErrPackets() - SPI adi_MdioRead Failed! (0x%X)\n", result);
    }
    packetCnt = (uint32_t)(val16 * 65536);

    /* Read the Lower bits [15:0] */
    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_FC_FRM_CNT_L, &val16);
    if (result != ADI_ETH_SUCCESS)
    {
    	printf("\nadi_phy_ReadErrPackets() - SPI adi_MdioRead Failed! (0x%X)\n", result);
    }

    packetCnt += val16;
    *rxCnt = packetCnt;

    return result;
}

/*
 * @brief           adin1100_checkFrameGen
 *
 * @param [in]      hDevice      pointer to PHY address of ADIN1100
 * @param [out]     frmGenDone  '1' if frame generator is done with transmission
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         check if Frame generator is done
 *
 */
adi_eth_Result_e adin1100_checkFrameGen(adi_phy_Device_t *hDevice, uint32_t *frmGenDone)
{
    adi_eth_Result_e    result = ADI_ETH_SUCCESS;
    uint16_t val16 = 0;

    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_FG_DONE, &val16);
    if (result != ADI_ETH_SUCCESS)
    {
        printf("\adin1100_checkFrameGen() - SPI adi_MdioRead_Cl45 Failed! (0x%X)\n", result);
     	result = ADI_ETH_COMM_ERROR;
    }
    if(val16 & 0x01)
    {
        *frmGenDone = 1;
    }
    else
    {
        *frmGenDone = 0;
    }
    return result;
}


/*
 * @brief           cyclicBoardLedControl
 *
 * @param [in]      boardDetails   pointer to board_t structure
 *
 * @details         enables/disables led depending on flags set
 *
 */
void cyclicBoardLedControl(board_t * boardDetails)
{
	if(boardDetails->errorLed == TRUE)
	{
		BSPConfigLED(D2Z_BRD_RED_LED, TRUE);
	}
	else if(boardDetails->errorLed == FALSE)
	{
		BSPConfigLED(D2Z_BRD_RED_LED, FALSE);
	}

	/* Only if there is no HW errors */
	if(boardDetails->errorLed == FALSE)
	{
		/* RED LED ON for temp errors and OFF on error recovery */
		if(boardDetails->tempErrorLed == TRUE)
		{
			BSPConfigLED(D2Z_BRD_RED_LED, TRUE);
		}
		else if(boardDetails->tempErrorLed == FALSE)
		{
			BSPConfigLED(D2Z_BRD_RED_LED, FALSE);
		}
	}

	/* Green LEDs show Link is UP */
	if( (boardDetails->adin1100LinkIsUp == TRUE) && (boardDetails->adin1300LinkIsUp == TRUE) )
	{
		boardDetails->linkled = TRUE; /* Green LED is on when Link is UP*/
		BSPConfigLED(D2Z_BRD_GREEN_LED, TRUE);
	}
	else
	{
		boardDetails->linkled = FALSE;
		BSPConfigLED(D2Z_BRD_GREEN_LED, FALSE);
    }

	if(boardDetails->blueLed == TRUE) /* Blue Led flag set when production power test is passed */
	{
		BSPConfigLED(D2Z_BRD_BLUE_LED, TRUE);
	}
	else if(boardDetails->blueLed == FALSE)
	{
		BSPConfigLED(D2Z_BRD_BLUE_LED, FALSE);
	}
}


/*
* @details  MSE in Db calculation
*/
static float calcMseDb(uint16_t mseVal)
{
	float pow2e18 = 262144.0;//2e18;
	float sym_pwr_exp = 0.64423;
	float valF = 0.0;

    valF = ( 10 * log10(((float)mseVal / pow2e18)/ sym_pwr_exp));
    return valF;
}

/*
 * @brief           adin1100_getMseLinkQuality
 *
 * @param [in]      hDevice      pointer to PHY address of ADIN1100
 * @param [out]     mseLinkQuality  MSE link quality status
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function gets the MSE link quality status
 *
 */
adi_eth_Result_e adin1100_getMseLinkQuality(adi_phy_Device_t *hDevice, adi_phy_MSELinkQuality_t *mseLinkQuality)
{
    adi_eth_Result_e  result = ADI_ETH_SUCCESS;
    uint16_t val16 = 0;

    result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_MSE_VAL, &val16);
    if (result != ADI_ETH_SUCCESS)
    {
        goto end;
    }
    mseLinkQuality->mseVal = val16;

    if (mseLinkQuality->mseVal > ADI_PHY_LINK_QUALITY_THR_POOR)
    {
        mseLinkQuality->linkQuality = ADI_PHY_LINK_QUAL_POOR;
    }
    else
    {
        if (mseLinkQuality->mseVal > ADI_PHY_LINK_QUALITY_THR_MARGINAL)
        {
            mseLinkQuality->linkQuality = ADI_PHY_LINK_QUAL_MARGINAL;
        }
        else
        {
            mseLinkQuality->linkQuality = ADI_PHY_LINK_QUAL_GOOD;
        }
    }

    for (mseLinkQuality->sqi = 0; mseLinkQuality->sqi < ADI_PHY_SQI_NUM_ENTRIES - 1; mseLinkQuality->sqi++)
    {
        if (mseLinkQuality->mseVal > convMseToSqi[mseLinkQuality->sqi])
        {
            break;
        }
    }

end:
    return result;
}

/*
* @details gets the Slicer error value from register
*/
static adi_eth_Result_e GetSlicerError(board_t *_boardDetails, adi_phy_Device_t *hDevice)
{
    adi_eth_Result_e  result = ADI_ETH_SUCCESS;
    uint16_t val16 = 0;

    /*SLCR_ERR_MAX_ABS_VAL register 0x018308*/
    result = adi_MdioRead_Cl45(hDevice->phyAddr, 0x018308, &val16);
    _boardDetails->adin1100_slcrErrMaxAbsErr = (float) ((float)val16/4096);

    return result;
}

/*!
 * @brief           applyBoardConfig
 *
 * @param [in]      _boardDetails  pointer to board_t structure
 * @param [in]      hDevice  pointer to PHY address of ADIN1100
 *
 * @details         This function configures the firmware according to the HW Config pins
 *
 * @sa
 */
adi_eth_Result_e applyBoardConfig(board_t *_boardDetails, adi_phy_Device_t *hDevice)
{
	adi_eth_Result_e result = ADI_ETH_SUCCESS;
    uint16_t val16 = 0;

    /* Configuration of LTC4296-1 in different FW modes
     * Production PWR test is handled in this section  */
    switch(_boardDetails->fwMode)
	{
        case SPOE_CLASS10:
        {
            result = ltc4296_1_cfg(_boardDetails, LTC_CFG_SCCP_MODE, LTC_PORT1);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("LTC4296-1 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = true;
            }
        }
        break;

        case SPOE_CLASS11:
        {
            result = ltc4296_1_cfg(_boardDetails, LTC_CFG_SCCP_MODE, LTC_PORT2);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("LTC4296-1 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = true;
            }
        }
        break;

        case SPOE_CLASS12:
        {
            result = ltc4296_1_cfg(_boardDetails,LTC_CFG_SCCP_MODE, LTC_PORT3);
            if (result != ADI_ETH_SUCCESS)
           	{
                printf("LTC4296-1 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = true;
            }
        }
        break;

        case SPOE_CLASS13:
        {
            result = ltc4296_1_cfg(_boardDetails,LTC_CFG_SCCP_MODE,LTC_PORT2);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("LTC4296-1 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = true;
            }
        }
        break;

        case SPOE_CLASS14:
        {
            result = ltc4296_1_cfg(_boardDetails,LTC_CFG_SCCP_MODE,LTC_PORT3);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("LTC4296-1 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = true;
            }
        }
        break;

        case SPOE_CLASS15:
        {
            result = ltc4296_1_cfg(_boardDetails,LTC_CFG_SCCP_MODE,LTC_PORT4);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("LTC4296-1 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = true;
            }
        }
        break;

        case APL_CLASSA:
        {
            result = ltc4296_1_cfg(_boardDetails,LTC_CFG_APL_MODE,LTC_PORT0);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("LTC4296-1 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = true;
            }
        }
        break;

        case APL_CLASSA_NOAUTONEG:
        {
           	ltc4296_1_cfg(_boardDetails,LTC_CFG_APL_MODE, LTC_PORT0);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("LTC4296-1 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = true;
            }
        }
        break;

        case APL_CLASSC:
        {
            result = ltc4296_1_cfg(_boardDetails,LTC_CFG_APL_MODE,LTC_PORT1);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("LTC4296-1 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = true;
            }
        }
        break;

        case APL_CLASS3:
        {
           	result = ltc4296_1_cfg(_boardDetails,LTC_CFG_APL_MODE,LTC_PORT4);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("LTC4296-1 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = true;
            }
        }
        break;

        case PRODUCTION_POWER_TEST:
        {
        	printf("\nBoard Production Power Test\n");

        	printf("Power from USB, J651 PWR 12V (APL) jumper in\n");
            printf("Connect 200ohm test load to P101 10BASE-T1L \n");
           	result = ltc4296_1_cfg(_boardDetails,LTC_CFG_RESET,LTC_NO_PORT);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("LTC4296-1 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = true;
            }
        }
        break;

        case APL_CLASSA_OLD_DEMO:
        {
            result = ltc4296_1_cfg(_boardDetails,LTC_CFG_APL_MODE,LTC_PORT0);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("LTC4296-1 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = true;
            }
        }
        break;

        case SPOE_OFF: /* No Power Mode*/
        {
        	/* reset the LTC - */
            result = ltc4296_1_cfg(_boardDetails,LTC_CFG_RESET, LTC_NO_PORT);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("LTC4296-1 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = true;
            }
        }
        break;

        case PRODUCTION_DATA_TEST:
        {
        	/* No Power config for this mode */
        }
        break;

        case RESERVED:
        {
        	printf("The board is in reserved mode \n\r");
        }
        break;

        case DEBUGMODE:
        {
        	printf("The board is in debug mode \n\r");
        }
        break;

        default:
            _boardDetails->fwMode = DEBUGMODE;
        break;
    }

    /* Configuration of ADIN1100 and ADIN1200 in different FW modes
     * Product Data test is handled in this section */
	switch(_boardDetails->fwMode)
	{
		case SPOE_CLASS10:
        case SPOE_CLASS11:
        case SPOE_CLASS12:
        case SPOE_CLASS13:
        case SPOE_CLASS14:
        case SPOE_CLASS15:
        case APL_CLASSA:
        case APL_CLASSC:
        case APL_CLASS3:
        case SPOE_OFF:
		{
//			result = adin1100_cfg(_boardDetails, hDevice);
//			if (result != ADI_ETH_SUCCESS)
//			{
//				printf("ADIN1100 Error - %s \n\r", adi_eth_result_string[result]);
//				_boardDetails->errorLed = TRUE;
//				/* Fatal error - reset the board to clear the flag */
//			}

			result = adin1300_cfg(_boardDetails);
			if (result != ADI_ETH_SUCCESS)
			{
				printf("ADIN1300 Error - %s \n\r", adi_eth_result_string[result]);
				_boardDetails->errorLed = TRUE;
			}
		}
        break;

        case PRODUCTION_POWER_TEST:
        {
           	printf("ADIN1100 Config");
            /* Check the ADIN1100 HW CFG setting Master/Slave
             * AN_ADV_ABILITY_M, check bit AN_ADV_MST */
            result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_AN_ADV_ABILITY_M, &val16);
            if(result != ADI_ETH_SUCCESS)
        	{
            	printf("Error - adi_MdioRead_Cl45 failed \n\r");
        		result = ADI_ETH_COMM_ERROR;
        	}

            if( (val16 & BITM_AN_ADV_ABILITY_M_AN_ADV_MST) == BITM_AN_ADV_ABILITY_M_AN_ADV_MST)
            {
            	printf(" prefer Master, Error - expected preferred Slave \n");
            	_boardDetails->errorLed = TRUE;
            }
            else
            {
            	printf(" prefer Slave,");
            }

            /* Check the ADIN1100 HW CFG setting amplitude
             * B10L_PMA_CNTRL, check bit B10L_TX_LVL_HI_ABLE */
            result = adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_B10L_PMA_CNTRL, &val16);
            if(result != ADI_ETH_SUCCESS)
        	{
            	printf("Error - adi_MdioRead_Cl45 failed \n\r");
        		result = ADI_ETH_COMM_ERROR;
        	}

            if( (val16 & BITM_B10L_PMA_CNTRL_B10L_TX_LVL_HI) == BITM_B10L_PMA_CNTRL_B10L_TX_LVL_HI)
            {
            	printf(" Tx 2.4V\n");
            }
            else
            {
            	printf(" Enabled 1.0V, Error - expected Enabled 2.4V \n");
            	_boardDetails->errorLed = TRUE;
            }
        }
        break;

        case APL_CLASSA_NOAUTONEG:
        {
        	result = adin1100_sp_cfg(_boardDetails, hDevice, APL_CLASSA_NOAUTONEG);
        	if (result != ADI_ETH_SUCCESS)
        	{
        		printf("ADIN1100 Error - %s \n\r", adi_eth_result_string[result]);
        		_boardDetails->errorLed = TRUE;
        		/* Fatal error - reset the board to clear the flag */
        	}

            result = adin1300_cfg(_boardDetails);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("ADIN1200 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = TRUE;
            }
        }
        break;

        case APL_CLASSA_OLD_DEMO:
        {
        	result = adin1100_sp_cfg(_boardDetails, hDevice, APL_CLASSA_OLD_DEMO);
        	if (result != ADI_ETH_SUCCESS)
        	{
        		printf("ADIN1100 Error - %s \n\r", adi_eth_result_string[result]);
        		_boardDetails->errorLed = TRUE;
        		/* Fatal error - reset the board to clear the flag */
        	}

            result = adin1300_cfg(_boardDetails);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("ADIN1200 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = TRUE;
            }
        }
        break;

        case PRODUCTION_DATA_TEST:
        {
            printf("Board Production Data Mode ..... Begin Test\n");
        	result = adin1100_cfg(_boardDetails, hDevice);
        	if (result != ADI_ETH_SUCCESS)
        	{
        		printf("ADIN1100 Error - %s \n\r", adi_eth_result_string[result]);
        		_boardDetails->errorLed = TRUE;
        		/* Fatal error - reset the board to clear the flag */
        	}

            result = adin1300_cfg(_boardDetails);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("ADIN1200 Error - %s \n\r", adi_eth_result_string[result]);
            	_boardDetails->errorLed = TRUE;
            }

            result = adin1200_remLoopback(_boardDetails);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("\n\rADIN1200 Error - %s \n\r", adi_eth_result_string[result]);
            }

           	/* Delay for ADIN1200 to comeup */
           	TimerDelay_ms(ADIN1200_LINK_UP_DELAY);

            adin1100_checkLinkPartner(_boardDetails,hDevice);

           	result = adin1100_frameGenACheck(_boardDetails,hDevice);
           	if (result != ADI_ETH_SUCCESS)
           	{
                printf("ADIN1200 Error - %s \n\r", adi_eth_result_string[result]);
            }
        }
        break;

        default:
            _boardDetails->fwMode = DEBUGMODE;
        break;
	}
    return result;
}

/*
 * @brief           cyclicReadBoard
 *
 * @param [in]      boardDetails  pointer to board_t structure
 * @param [in]      hDevice  pointer to PHY address of ADIN1100
 *
 * @details         This function reports on UART PHY related data depending on flags
 *
 */
adi_eth_Result_e cyclicReadBoard(board_t *_boardDetails, adi_phy_Device_t *hDevice)
{
    adi_eth_Result_e result = ADI_ETH_SUCCESS;
    uint32_t frmGenDone = 0;
    uint32_t temprxErr = 0;
    uint32_t temprxCnt = 0;
    uint16_t val16 = 0;

    if( (_boardDetails->fwMode != RESERVED) && (_boardDetails->fwMode != DEBUGMODE) )
    {
		adin1100_ReadErrPackets(hDevice, &temprxErr, &temprxCnt);
		_boardDetails->rxErr += temprxErr;
		_boardDetails->rxCnt += temprxCnt;
    }

	if(_boardDetails->frameGenEnabled == TRUE)
	{
		/* FG_EN Disable Frame Generator (to restart it) */
		val16 = 0x0;
		result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_FG_EN, val16);
		adin1100_checkFrameGen(hDevice, &frmGenDone);
		if(frmGenDone == 1)
		{
			_boardDetails->txCnt += ADIN1100_FRAME_COUNT;//increment TX Number
		}
		/* FG_EN Enable Frame Generator */
		val16 = 0x1;
		result = adi_MdioWrite_Cl45(hDevice->phyAddr, ADDR_FG_EN, val16);

		if(_boardDetails->fwMode == PRODUCTION_DATA_TEST)
		{
			if( (_boardDetails->rxErr > 0) || (_boardDetails->txCnt - _boardDetails->rxCnt ))
			{
				_boardDetails->tempErrorLed = TRUE; //RED LED
				_boardDetails->blueLed = FALSE; //BLUE LED
			}

			if(_boardDetails->rxCnt >= 5000)
			{
				_boardDetails->blueLed = TRUE; //BLUE LED
			}
			if( (_boardDetails->rxCnt < 5000) || (_boardDetails->errorLed == TRUE) )
			{
				_boardDetails->blueLed = FALSE; //BLUE LED
			}
		}
	}

	if(_boardDetails->clrFrameErrors == TRUE)
	{
		_boardDetails->rxErr = 0;
		_boardDetails->rxCnt = 0;
		_boardDetails->txCnt = 0;
		_boardDetails->clrFrameErrors = 0;
		_boardDetails->tempErrorLed = FALSE;
		/* read to clear errors */
		adin1100_ReadErrPackets(hDevice, &temprxErr, &temprxCnt);
    }

	if(_boardDetails->uartReport == TRUE)
	{
		if(_boardDetails->readMSE == TRUE)
		{
			result = adin1100_getMseLinkQuality(hDevice, &mseLinkQuality);
			if(result != ADI_ETH_SUCCESS)
			{
				result = ADI_ETH_COMM_ERROR;
			}
			_boardDetails->adin1100_mseVal = calcMseDb(mseLinkQuality.mseVal);

			result = GetSlicerError(_boardDetails,hDevice);
			if(result != ADI_ETH_SUCCESS)
			{
				result = ADI_ETH_COMM_ERROR;
			}

			if(_boardDetails->adin1100LinkIsUp == TRUE)
			{
				printf("MSE %.2f dB, SLCRERR %.3f, ",(double)_boardDetails->adin1100_mseVal, (double)_boardDetails->adin1100_slcrErrMaxAbsErr);
			}
			else
			{
				printf("MSE N/A, SLCRERR N/A, ");
			}
		}
		if(_boardDetails->frameGenEnabled == TRUE)
		{
			printf("Tx %d, Rx %d, Diff %d, Err %d, ", _boardDetails->txCnt, _boardDetails->rxCnt,
				   (_boardDetails->txCnt - _boardDetails->rxCnt), _boardDetails->rxErr);
		}
		else
		{
			printf("Rx %d, Err %d, ", _boardDetails->rxCnt, _boardDetails->rxErr);
		}

        if(_boardDetails->ltc4296_1_pdPresent == TRUE)
        {
			/* Prints Voltage and Current read from LTC4296_1 ADC*/
			printf("Vout %3.1fV, ",(double)_boardDetails->ltc4296_1_VoutIout.ltc4296_1_Vout);
			printf("Iout %3.1fmA \n",(double)_boardDetails->ltc4296_1_VoutIout.ltc4296_1_Iout);
        }
        else
        {
        	if(_boardDetails->fwMode == SPOE_OFF)
        	{
			    printf("PSE Disabled\n");
        	}
        	else if( (_boardDetails->fwMode == SPOE_CLASS10) || (_boardDetails->fwMode == SPOE_CLASS11) || (_boardDetails->fwMode == SPOE_CLASS12)
        		|| (_boardDetails->fwMode == SPOE_CLASS13) || (_boardDetails->fwMode == SPOE_CLASS14) || (_boardDetails->fwMode == SPOE_CLASS15) 	)
         	{
        		printf("No PD\n");
        	}
        	else
         	{
        		printf("PSE N/A\n");
        	}
        }
	}
    return result;
}


/*
 * @brief           cyclicLinkStatus
 *
 * @param [in]      boardDetails    pointer to board_t structure
 * @param [in]      hDevice         Device handle having PHY address of ADIN1100
 *
 * @details         This function collects the PHY link status in the one second loop
 *
 */
adi_eth_Result_e cyclicLinkStatus(board_t *_boardDetails, adi_phy_Device_t *hDevice)
{
    adi_eth_Result_e  result = ADI_ETH_SUCCESS;
    uint16_t val16 = 0;

    if((_boardDetails->fwMode == RESERVED) || (_boardDetails->fwMode == DEBUGMODE) )
    {
    	/* No MDIO Reads in this mode*/
        return ADI_ETH_NOT_IMPLEMENTED_SOFTWARE;
    }
    else
    {
	    /* ADIN1100 PHY Link Status */

		adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_AN_PHY_INST_STATUS, &val16);
		if(result != ADI_ETH_SUCCESS)
		{
			result = ADI_ETH_COMM_ERROR;
		}
		if((val16 & 0x80) == 0x80)//Link OK - bit 7 IS_LINK_STATUS_OK (Not in the data sheet!!)
		{
			_boardDetails->adin1100LinkIsUp = TRUE;
		}
		else
		{
			_boardDetails->adin1100LinkIsUp = FALSE;
		}

		adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_PMA_PMD_STAT1, &val16);
		if(result != ADI_ETH_SUCCESS)
		{
			result = ADI_ETH_COMM_ERROR;
		}

		if( (val16 & BITM_PMA_PMD_STAT1_PMA_LINK_STAT_OK_LL) == BITM_PMA_PMD_STAT1_PMA_LINK_STAT_OK_LL)
		{
			/* Dont do anything */
		}
		else
		{
			_boardDetails->adin1100LinkWasDown = TRUE;
			_boardDetails->adin1100LinkIsUp = FALSE;
		}

		val16 = 0;
		adi_MdioRead_Cl45(hDevice->phyAddr, ADDR_AN_PHY_INST_STATUS, &val16);
		if(result != ADI_ETH_SUCCESS)
		{
			result = ADI_ETH_COMM_ERROR;
		}
		if((_boardDetails->adin1100LinkIsUp == TRUE))
		{
			if((val16 & 0x40) == 0x40)//An Enabled - bit 6 IS_AN_EN (Not in the datasheet!!)
			{
				_boardDetails->adin1100AnEn = TRUE;//An Enabled
			}
			else
			{
				_boardDetails->adin1100AnEn = FALSE;//An Disabled
			}

			/* bit 5 IS_FRC_LINK_CFG_MODE (Not in the datasheet!!)  */
			if((val16 & 0x20) == 0x20)
			{
				_boardDetails->adin1100LinkForced = TRUE;
			}
			else
			{
				_boardDetails->adin1100LinkForced = FALSE;
			}

			if((val16 & BITM_AN_PHY_INST_STATUS_IS_CFG_MST) == BITM_AN_PHY_INST_STATUS_IS_CFG_MST)//Master
			{
				_boardDetails->adin1100MasterEn = TRUE;//Master Enabled
			}
			else
			{
				_boardDetails->adin1100MasterEn = FALSE;//Master Disabled
			}

			if((val16 & BITM_AN_PHY_INST_STATUS_IS_CFG_SLV) == BITM_AN_PHY_INST_STATUS_IS_CFG_SLV)//Slave
			{
				_boardDetails->adin1100SlaveEn = TRUE;//Slave Enabled
			}
			else
			{
				_boardDetails->adin1100SlaveEn = FALSE;//Slave Disabled
			}

			if((val16 & BITM_AN_PHY_INST_STATUS_IS_TX_LVL_HI) == BITM_AN_PHY_INST_STATUS_IS_TX_LVL_HI)//TxLevel High
			{
				_boardDetails->adin1100TxVHi = TRUE;//TxLevel 2.4V
			}
			else
			{
				_boardDetails->adin1100TxVHi = FALSE;//TxLevel 2.4V
			}

			if((val16 & BITM_AN_PHY_INST_STATUS_IS_TX_LVL_LO) == BITM_AN_PHY_INST_STATUS_IS_TX_LVL_LO)//TxLevel Low
			{
				_boardDetails->adin1100TxVLow = TRUE;//TxLevel 1.0 V
			}
			else
			{
				_boardDetails->adin1100TxVLow = FALSE;//TxLevel 1.0 V
			}
		}

		/* ADIN1200 PHY Link Status */

		/* Read the Latched data bit2 (LINK_STAT_LAT) in MII_STATUS register */
		val16 = 0;

		/* Read the current link status data bit6 (LINK_STAT) in PHY_STATUS_1 register */
		adi_MdioRead(_boardDetails->adin1300PhyAddr,  ADDR_PHY_STATUS_1, &val16);
		if(result != ADI_ETH_SUCCESS)
		{
			result = ADI_ETH_COMM_ERROR;
		}
		if((val16 & BITM_PHY_STATUS_1_LINK_STAT) == BITM_PHY_STATUS_1_LINK_STAT)
		{
			_boardDetails->adin1300LinkIsUp = TRUE;
		}
		else
		{
			_boardDetails->adin1300LinkIsUp = FALSE;
		}

		val16 = 0;
		adi_MdioRead(_boardDetails->adin1300PhyAddr,  ADDR_MII_STATUS, &val16);
		if(result != ADI_ETH_SUCCESS)
		{
			result = ADI_ETH_COMM_ERROR;
		}
		if((val16 & BITM_MII_STATUS_LINK_STAT_LAT) == BITM_MII_STATUS_LINK_STAT_LAT)
		{
			/* Dont do anything */
		}
		else
		{
			_boardDetails->adin1300LinkWasDown = TRUE;
		}
    }
    return result;
}

/*
 * @brief           ltc4296_1_cfg
 *
 * @param [in]      boardDetails    pointer to board_t structure
 * @param [in]      ltcCfgClass     LTC4296-1 class
 * @param [in]      ltcCfgClass     LTC4296-1 port number (0-4)
 *
 * @details         This function resets LTC4296-1 and sets the class and port parameters
 *
 */
adi_ltc_Result_e ltc4296_1_cfg(board_t *_boardDetails, ltc4296_1_config_e ltcCfgClass, ltc4296_1_port_e port)
{
	adi_ltc_Result_e result ;

	result = ltc4296_1_reset();
	if(result != ADI_LTC_SUCCESS)
	{
		return result;
	}

	printf("LTC4296-1 reset \n");

	/* Set the LTC config Class and Port parameters */
	_boardDetails->ltc4296_1_CfgClass = ltcCfgClass;
	_boardDetails->ltc4296_1_Port = port;

    return ADI_LTC_SUCCESS;
}

/*
 * @brief           cyclicSPOEControl
 *
 * @param [in]      boardDetails     pointer to board_t structure
 *
 * @details         This function performs SPOE/SCCP or APL work flow depending on the firmware mode
 *
 */
adi_eth_Result_e cyclicSPOEControl(board_t *_boardDetails)
{
	adi_ltc_Result_e result = ADI_LTC_SUCCESS;
	uint8_t i=0;
	adi_eth_Result_e ret = ADI_ETH_SUCCESS;

    switch(_boardDetails->fwMode)
	{
        case SPOE_CLASS10:
        {
        	result = ltc4296_1_chkGlobalEvents();
        	if(result == ADI_LTC_DISCONTINUE_SCCP)
        	{
        		break;
        	}

        	result = ltc4296_1_chkPortEvents(_boardDetails->ltc4296_1_Port);
        	if(result == ADI_LTC_DISCONTINUE_SCCP)
        	{
        		break;
        	}
    		result = ltc4296_1_doSpoeSccp(_boardDetails->fwMode, _boardDetails->ltc4296_1_Port,&(_boardDetails->ltc4296_1_VoutIout));
    		if( (result == ADI_LTC_SUCCESS) || (result == ADI_LTC_SCCP_COMPLETE) )
    		{
    			_boardDetails->ltc4296_1_pdPresent = TRUE;
    		}
    		else if(result == ADI_LTC_DISCONTINUE_SCCP)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    			_boardDetails->tempErrorLed = TRUE;
    		}
    		else if(result == ADI_LTC_SCCP_PD_NOT_PRESENT)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    		}
        }
        break;

        case SPOE_CLASS11:
        {
        	result = ltc4296_1_chkGlobalEvents();
        	if(result == ADI_LTC_DISCONTINUE_SCCP)
        	{
        		break;
        	}

        	result = ltc4296_1_chkPortEvents(_boardDetails->ltc4296_1_Port);
        	if(result == ADI_LTC_DISCONTINUE_SCCP)
        	{
        		break;
        	}
    		result = ltc4296_1_doSpoeSccp(_boardDetails->fwMode, _boardDetails->ltc4296_1_Port,&(_boardDetails->ltc4296_1_VoutIout));
    		if( (result == ADI_LTC_SUCCESS) || (result == ADI_LTC_SCCP_COMPLETE) )
    		{
    			_boardDetails->ltc4296_1_pdPresent = TRUE;
    		}
    		else if(result == ADI_LTC_DISCONTINUE_SCCP)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    			_boardDetails->tempErrorLed = TRUE;
    		}
    		else if(result == ADI_LTC_SCCP_PD_NOT_PRESENT)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    		}
        }
        break;

        case SPOE_CLASS12:
        {
        	result = ltc4296_1_chkGlobalEvents();
        	if(result == ADI_LTC_DISCONTINUE_SCCP)
        	{
        		break;
        	}

        	result = ltc4296_1_chkPortEvents(_boardDetails->ltc4296_1_Port);
        	if(result == ADI_LTC_DISCONTINUE_SCCP)
        	{
        		break;
        	}
    		result = ltc4296_1_doSpoeSccp(_boardDetails->fwMode, _boardDetails->ltc4296_1_Port,&(_boardDetails->ltc4296_1_VoutIout));
    		if( (result == ADI_LTC_SUCCESS) || (result == ADI_LTC_SCCP_COMPLETE) )
    		{
    			_boardDetails->ltc4296_1_pdPresent = TRUE;
    		}
    		else if(result == ADI_LTC_DISCONTINUE_SCCP)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    			_boardDetails->tempErrorLed = TRUE;
    		}
    		else if(result == ADI_LTC_SCCP_PD_NOT_PRESENT)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    		}
        }
        break;

        case SPOE_CLASS13:
        {
        	result = ltc4296_1_chkGlobalEvents();
        	if(result == ADI_LTC_DISCONTINUE_SCCP)
        	{
        		break;
        	}

        	result = ltc4296_1_chkPortEvents(_boardDetails->ltc4296_1_Port);
        	if(result == ADI_LTC_DISCONTINUE_SCCP)
        	{
        		break;
        	}
    		result = ltc4296_1_doSpoeSccp(_boardDetails->fwMode, _boardDetails->ltc4296_1_Port,&(_boardDetails->ltc4296_1_VoutIout));
    		if( (result == ADI_LTC_SUCCESS) || (result == ADI_LTC_SCCP_COMPLETE) )
    		{
    			_boardDetails->ltc4296_1_pdPresent = TRUE;
    		}
    		else if(result == ADI_LTC_DISCONTINUE_SCCP)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    			_boardDetails->tempErrorLed = TRUE;
    		}
    		else if(result == ADI_LTC_SCCP_PD_NOT_PRESENT)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    		}
        }
        break;
        case SPOE_CLASS14:
        {
        	result = ltc4296_1_chkGlobalEvents();
        	if(result == ADI_LTC_DISCONTINUE_SCCP)
        	{
        		break;
        	}

        	result = ltc4296_1_chkPortEvents(_boardDetails->ltc4296_1_Port);
        	if(result == ADI_LTC_DISCONTINUE_SCCP)
        	{
        		break;
        	}
    		result = ltc4296_1_doSpoeSccp(_boardDetails->fwMode, _boardDetails->ltc4296_1_Port,&(_boardDetails->ltc4296_1_VoutIout));
    		if( (result == ADI_LTC_SUCCESS) || (result == ADI_LTC_SCCP_COMPLETE) )
    		{
    			_boardDetails->ltc4296_1_pdPresent = TRUE;
    		}
    		else if(result == ADI_LTC_DISCONTINUE_SCCP)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    			_boardDetails->tempErrorLed = TRUE;
    		}
    		else if(result == ADI_LTC_SCCP_PD_NOT_PRESENT)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    		}
        }
        break;
        case SPOE_CLASS15:
        {
        	result = ltc4296_1_chkGlobalEvents();
        	if(result == ADI_LTC_DISCONTINUE_SCCP)
        	{
        		break;
        	}

        	result = ltc4296_1_chkPortEvents(_boardDetails->ltc4296_1_Port);
        	if(result == ADI_LTC_DISCONTINUE_SCCP)
        	{
        		break;
        	}
    		result = ltc4296_1_doSpoeSccp(_boardDetails->fwMode, _boardDetails->ltc4296_1_Port,&(_boardDetails->ltc4296_1_VoutIout));
    		if( (result == ADI_LTC_SUCCESS) || (result == ADI_LTC_SCCP_COMPLETE) )
    		{
    			_boardDetails->ltc4296_1_pdPresent = TRUE;
    		}
    		else if(result == ADI_LTC_DISCONTINUE_SCCP)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    			_boardDetails->tempErrorLed = TRUE;
    		}
    		else if(result == ADI_LTC_SCCP_PD_NOT_PRESENT)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    		}
        }
        break;
        case APL_CLASSA:
        {
        	result = ltc4296_1_chkGlobalEvents();
        	if(result == ADI_LTC_DISCONTINUE_APL)
        	{
        		break;
        	}

        	result = ltc4296_1_chkPortEvents(_boardDetails->ltc4296_1_Port);
        	if(result == ADI_LTC_DISCONTINUE_APL)
        	{
        		break;
        	}
    		result = ltc4296_1_doAPL(_boardDetails->fwMode, _boardDetails->ltc4296_1_Port, &(_boardDetails->ltc4296_1_VoutIout));
    		if( (result == ADI_LTC_SUCCESS) || (result == ADI_LTC_APL_COMPLETE) )
    		{
    			_boardDetails->ltc4296_1_pdPresent = TRUE;
    		}
    		else if(result == ADI_LTC_DISCONTINUE_APL)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    			_boardDetails->tempErrorLed = TRUE;
    		}
        }
        break;
        case APL_CLASSA_NOAUTONEG:
        {
        	result = ltc4296_1_chkGlobalEvents();
        	if(result == ADI_LTC_DISCONTINUE_APL)
        	{
        		break;
        	}

        	result = ltc4296_1_chkPortEvents(_boardDetails->ltc4296_1_Port);
        	if(result == ADI_LTC_DISCONTINUE_APL)
        	{
        		break;
        	}
    		result = ltc4296_1_doAPL(_boardDetails->fwMode, _boardDetails->ltc4296_1_Port, &(_boardDetails->ltc4296_1_VoutIout));
    		if( (result == ADI_LTC_SUCCESS) || (result == ADI_LTC_APL_COMPLETE) )
    		{
    			_boardDetails->ltc4296_1_pdPresent = TRUE;
    		}
    		else if(result == ADI_LTC_DISCONTINUE_APL)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    			_boardDetails->tempErrorLed = TRUE;
    		}
        }
        break;

        case APL_CLASSC:
        {
        	result = ltc4296_1_chkGlobalEvents();
        	if(result == ADI_LTC_DISCONTINUE_APL)
        	{
        		break;
        	}

        	result = ltc4296_1_chkPortEvents(_boardDetails->ltc4296_1_Port);
        	if(result == ADI_LTC_DISCONTINUE_APL)
        	{
        		break;
        	}
    		result = ltc4296_1_doAPL(_boardDetails->fwMode, _boardDetails->ltc4296_1_Port, &(_boardDetails->ltc4296_1_VoutIout));
    		if( (result == ADI_LTC_SUCCESS) || (result == ADI_LTC_APL_COMPLETE) )
    		{
    			_boardDetails->ltc4296_1_pdPresent = TRUE;
    		}
    		else if(result == ADI_LTC_DISCONTINUE_APL)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    			_boardDetails->tempErrorLed = TRUE;
    		}
        }
        break;

        case APL_CLASS3:
        {
        	result = ltc4296_1_chkGlobalEvents();
        	if(result == ADI_LTC_DISCONTINUE_APL)
        	{
        		break;
        	}

        	result = ltc4296_1_chkPortEvents(_boardDetails->ltc4296_1_Port);
        	if(result == ADI_LTC_DISCONTINUE_APL)
        	{
        		break;
        	}
    		result = ltc4296_1_doAPL(_boardDetails->fwMode, _boardDetails->ltc4296_1_Port, &(_boardDetails->ltc4296_1_VoutIout));
    		if( (result == ADI_LTC_SUCCESS) || (result == ADI_LTC_APL_COMPLETE) )
    		{
    			_boardDetails->ltc4296_1_pdPresent = TRUE;
    		}
    		else if(result == ADI_LTC_DISCONTINUE_APL)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    			_boardDetails->tempErrorLed = TRUE;
    		}
        }
        break;

        case PRODUCTION_POWER_TEST:
        {
        	result = ltc4296_1_chkGlobalEvents();
        	if(result == ADI_LTC_DISCONTINUE_APL)
        	{
        		break;
        	}

        	for(i=0; i<LTC_NO_PORT; i++)
        	{
				result = ltc4296_1_chkPortEvents(LTC_PORT0+i);
				if(result == ADI_LTC_DISCONTINUE_APL)
				{
					break;
				}
        	}

        	/* Test is done for all the ports 0-4*/
    		result = ltc4296_1_pwr_test(_boardDetails->fwMode);
    		if(result == ADI_LTC_DISCONTINUE_TEST)
    		{
    			printf("LTC4296-1 Production PWR TEST Discontinued...    Failed\n\r");
    			_boardDetails->errorLed = TRUE;
    		}
    		else if(result == ADI_LTC_TEST_FAILED)
    		{
    			_boardDetails->errorLed = TRUE;
    		}
    		else if(result == ADI_LTC_TEST_COMPLETE)
    		{
    			_boardDetails->blueLed = TRUE;
    		}
        }
        break;

        case APL_CLASSA_OLD_DEMO:
        {
        	result = ltc4296_1_chkGlobalEvents();
        	if(result == ADI_LTC_DISCONTINUE_APL)
        	{
        		break;
        	}

        	result = ltc4296_1_chkPortEvents(_boardDetails->ltc4296_1_Port);
        	if(result == ADI_LTC_DISCONTINUE_APL)
        	{
        		break;
        	}

    		/* SPOE SCCP FLOW */
    		result = ltc4296_1_doAPL(_boardDetails->fwMode, _boardDetails->ltc4296_1_Port, &(_boardDetails->ltc4296_1_VoutIout));
    		if( (result == ADI_LTC_SUCCESS) || (result == ADI_LTC_APL_COMPLETE) )
    		{
    			_boardDetails->ltc4296_1_pdPresent = TRUE;
    		}
    		else if(result == ADI_LTC_DISCONTINUE_APL)
    		{
    			_boardDetails->ltc4296_1_pdPresent = FALSE;
    			_boardDetails->tempErrorLed = TRUE;
    		}
        }
        break;
        case SPOE_OFF: /* No Power Mode*/
        case PRODUCTION_DATA_TEST: /* Frames are monitored in the cyclicReadBoard() function */
        case RESERVED: /* For future use */
        case DEBUGMODE:
        default:
        	/* Do not do anything just return */
        break;
    }
    return ret;
}


/*
 * @brief           adin_phyPrintLinkStatus
 *
 * @param [in]      boardDetails pointer to board_t structure
 *
 * @details         This function prints the PHY link status
 *
 */
void adin_phyPrintLinkStatus(board_t *_boardDetails)
{
    if(_boardDetails->adin1100LinkIsUp == TRUE)
    {
        printf("ADIN1100 Link ");

        if(_boardDetails->adin1100LinkWasDown == TRUE)
        {
            printf("was Down, ");
            _boardDetails->adin1100LinkWasDown = FALSE;
        }

        if(_boardDetails->adin1100LinkIsUp == TRUE)
        {
            printf("is Up ");
        }

        if(_boardDetails->adin1100LinkForced == TRUE)
        {
            printf("Forced, ");
        }

        if(_boardDetails->adin1100AnEn == TRUE)
        {
        	printf("Autoneg, ");
        }
        if(_boardDetails->adin1100MasterEn == TRUE)
        {
        	printf("Master, ");
        }
        if(_boardDetails->adin1100SlaveEn == TRUE)
        {
        	printf("Slave, ");
        }
        if(_boardDetails->adin1100TxVLow == TRUE)
        {
      	    printf("1.0 V ");
        }
        if(_boardDetails->adin1100TxVHi == TRUE)
        {
     	    printf("2.4 V ");
        }
        printf("\n");
    }
    else
    {
        printf("ADIN1100 Link is Down \n");
    }

    if(_boardDetails->adin1300LinkIsUp == TRUE)
    {
        printf("ADIN1200 Link ");
        if(_boardDetails->adin1300LinkWasDown == TRUE)
        {
            printf("was Down, ");
            _boardDetails->adin1300LinkWasDown = FALSE;
        }

        if(_boardDetails->adin1300LinkIsUp == TRUE)
        {
            printf("is Up ");
        }
        printf("\n");
    }
    else
    {
        printf("ADIN1200 Link is Down \n");
    }
}


/*!
 * @brief          setBoardLED
 *
 * @param [in]     en  1 = LED is ON
 *                     0 = LED is OFF
 *
 * @details         This function switches ON and OFF the board LEDs [Green, Red, Yellow, Blue]
 *
 * @sa
 */
void setBoardLED(bool en)
{
    if(en == TRUE)
    {
        BSPConfigLED(D2Z_BRD_GREEN_LED, true);
        BSPConfigLED(D2Z_BRD_RED_LED, true);
        BSPConfigLED(D2Z_BRD_YELLOW_LED, true);
        BSPConfigLED(D2Z_BRD_BLUE_LED, true);
    }
    else
    {
        BSPConfigLED(D2Z_BRD_GREEN_LED, false);
        BSPConfigLED(D2Z_BRD_RED_LED, false);
        BSPConfigLED(D2Z_BRD_YELLOW_LED, false);
        BSPConfigLED(D2Z_BRD_BLUE_LED, false);
    }
}

/**
 * @brief        setBoardLED
 *
 * @param        set value to set UART data availability
 *
 * @details      UART receive buffer full flag setter, to be used only in the UART callback
 *
 */
void setUartDataAvailable(uint32_t set)
{
	uartDataAvailable = set;
}

/**
 * @brief        getUartDataAvailable
 *
 * @details      UART receive buffer full flag getter, use this function to check for
 *               flag change
 *
 * @return        Rx Full flag state
 *
 */
uint32_t getUartDataAvailable(void)
{
	return (uartDataAvailable);
}

/**
 * @brief        setUartCmdAvailable
 *
 * @param        set value to set UART command availability
 *
 * @details      New command from UART has been received
 *
 */
void setUartCmdAvailable(uint32_t set)
{
    uartCmdAvailable = set;
}

/**
 * @brief        getUartCmdAvailable
 *
 * @details      Command Available flag getter, use this function to check for
 *               flag change
 *
 * @return        uartCmdAvailable flag state
 *
 */
uint32_t getUartCmdAvailable(void)
{
    return uartCmdAvailable;
}
/**@}*/

