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

#define MAX32670_FW_MAJOR_VERSION (1)    /* Major version*/
#define MAX32670_FW_MINOR_VERSION (0)    /* Minor version*/
#define MAX32670_FW_BUILD_VERSION (0)    /* Build version*/

static const max32670_fw_version FirmwareVersion =
{
   MAX32670_FW_MAJOR_VERSION,
   MAX32670_FW_MINOR_VERSION,
   MAX32670_FW_BUILD_VERSION
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

/*!
 * @brief        	getFWLibVersion
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
    sprintf((char*)boardDetails.hwType,"EVAL-ADIN1320");

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
    printf("ANALOG DEVICES ADIN1320 Ethernet Phy            \n");
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
		case PHY_TESTMODE_0:
		{
			  // Do nothing
		}
		break;
		case PHY_TESTMODE_1:
		{
			  // Do nothing
		}
		break;
		case PHY_TESTMODE_2:
		{
			  // Do nothing
		}
		break;
		case PHY_TESTMODE_3:
		{
			  // Do nothing
		}
		break;
		case RESERVED_4:
		{
			  // Do nothing
		}
		break;
		case RESERVED_5:
		{
			  // Do nothing
		}
		break;
		case RESERVED_6:
		{
			  // Do nothing
		}
		break;
		case INTERACTIVEMODE:
		{
			label = "GUI Mode";
		}
		break;

		case MAC_REMOTELB:
		{
			  // Do nothing
		}
		break;
		case FRAMEGENCHECK:
		{
			  // Do nothing
		}
		break;
		case RESERVED_A:
		{
			  // Do nothing
		}
		break;
		case RESERVED_B:
		{
			  // Do nothing
		}
		break;
		case RESERVED_C:
		{
			  // Do nothing
		}
		break;
		case MEDCONV_CU_SGMII:
		{
			  // Do nothing
		}
		break;
		case MEDCONV_CU_FI:
		{
			  // Do nothing
		}
		break;
		case MEDCONV_CU_CU:
		{
		  // Do nothing
		}
		break;

		default:
		  label = "GUI Mode";
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
            _boardDetails->fwMode = PHY_TESTMODE_0;
        break;
        case 0x0001:
            _boardDetails->fwMode = PHY_TESTMODE_1;
        break;
        case 0x2:
            _boardDetails->fwMode = PHY_TESTMODE_2;
        break;
        case 0x3:
            _boardDetails->fwMode = PHY_TESTMODE_3;
        break;
        case 0x4:
            _boardDetails->fwMode = RESERVED_4;
        break;
        case 0x5:
            _boardDetails->fwMode = RESERVED_5;
        break;
        case 0x6:
            _boardDetails->fwMode = RESERVED_6;
        break;
        case 0x7:
            _boardDetails->fwMode = INTERACTIVEMODE;
        break;
        case 0x8:
            _boardDetails->fwMode = MAC_REMOTELB;
        break;
        case 0x9:
            _boardDetails->fwMode = FRAMEGENCHECK;
        break;
        case 0xA:
            _boardDetails->fwMode = RESERVED_A;
        break;
        case 0xB:
            _boardDetails->fwMode = RESERVED_B;
        break;
        case 0xC:
            _boardDetails->fwMode = RESERVED_C;
        break;
        case 0xD:
            _boardDetails->fwMode = MEDCONV_CU_SGMII;
        break;
        case 0xE:
            _boardDetails->fwMode = MEDCONV_CU_FI;
        break;
        case 0xF:
            _boardDetails->fwMode = MEDCONV_CU_CU;
        break;
        default:
            _boardDetails->fwMode = MEDCONV_CU_CU;
        break;
    }
}

/*!
 * @brief           adin1320_checkIdentity
 *
 * @param [in]      hDevice     Device handle having PHY address of ADIN1320
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
static adi_eth_Result_e adin1320_checkIdentity(adi_phy_Device_t *hDevice)
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
    	printf("Error- ADIN1320 ID1 value read = 0x%x", val16);
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
    	printf("sError- ADIN1320 ID value not matching = 0x%x", val16);
        result = ADI_ETH_UNSUPPORTED_DEVICE;
    }
    return result;
}

/*!
 * @brief           adin1300_checkIdentity
 *
 * @param [in]      phyAddr     PHY address of ADIN1300
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
    	printf("Error ADIN1300 ID1 value read = 0x%x", val16);
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
    	printf("Error ADIN1300 ID2 value read = 0x%x", val16);
        result = ADI_ETH_UNSUPPORTED_DEVICE;
    }
    return result;
}

/*
 *
 * @details     	waits for the MDIO interface to come up
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
 * @brief           adin1320_discoverPhy
 *
 * @param [in]      hDevice     Device handle having PHY address of ADIN1320
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function reads the device ID of ADIN1320 and verifies.
 */
adi_eth_Result_e adin1320_discoverPhy(adi_phy_Device_t *hDevice)
{
    adi_eth_Result_e result = ADI_ETH_SUCCESS;

    /* If both the MCU and the ADIN1320 are reset simultaneously */
    /* using the RESET button on the board, the MCU may start    */
    /* scanning for ADIN1320 devices before the ADIN1320 has     */
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
    result = adin1320_checkIdentity(hDevice);
	return result;
}

/*!
 * @brief           adin1300_discoverPhy
 *
 * @param [in]      _boardDetails     Device handle having PHY address of ADIN1300
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function reads the device ID of ADIN1300 and verifies.
 */
adi_eth_Result_e adin1300_discoverPhy(board_t *_boardDetails)
{
    adi_eth_Result_e result = ADI_ETH_SUCCESS;

    /* If both the MCU and the ADIN1320 are reset simultaneously */
    /* using the RESET button on the board, the MCU may start    */
    /* scanning for ADIN1320 devices before the ADIN1320 has     */
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
 * @brief           adin1320_phyReset
 *
 * @param [in]      hDevice     Device handle having PHY address of ADIN1320
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function resets the ADIN1320 device.
 */
adi_eth_Result_e adin1320_phyReset(adi_phy_Device_t *hDevice)
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

	TimerDelay_ms(ADIN1320_SW_RESET_DELAY);

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
 * @brief           adin1300_phyReset
 *
 * @param [in]      phyAddr PHY HW address of ADIN1300
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function resets the ADIN1300
 *
 */
adi_eth_Result_e adin1300_phyReset(uint8_t phyAddr)
{
    adi_eth_Result_e result = ADI_ETH_SUCCESS;

	adi_MdioWrite(phyAddr,ADDR_EXT_REG_PTR, ADDR_GE_SFT_RST_CFG_EN);
	adi_MdioWrite(phyAddr,ADDR_EXT_REG_DATA, BITM_GE_SFT_RST_CFG_EN_GE_SFT_RST_CFG_EN);

	adi_MdioWrite(phyAddr,ADDR_EXT_REG_PTR, ADDR_GE_SFT_RST);
	adi_MdioWrite(phyAddr,ADDR_EXT_REG_DATA, BITM_GE_SFT_RST_GE_SFT_RST);

	TimerDelay_ms(ADIN1300_SW_RESET_DELAY);

	/* Wait until MDIO interface is up. */
	result = waitMdio(phyAddr);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

    return result;
}

/*!
 * @brief           adin1320_getSWPD
 *
 * @param [in]      hDevice   Device handle having PHY address of ADIN1320
 * @param [out]     enable    status of SWPD of adin100
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function gets the status of the ADIN1320 SWPD
 *
 */
adi_eth_Result_e adin1320_getSWPD(adi_phy_Device_t *hDevice, unsigned short *enable)
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
 * @brief           adin1320_setSWPD
 *
 * @param [in]      hDevice  Device handle having PHY address of ADIN1320
 * @param [out]     enable    enables or disables the SWPD in ADIN1320
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function enables or disables the SWPD in ADIN1320
 *
 */
adi_eth_Result_e adin1320_setSWPD(adi_phy_Device_t *hDevice, unsigned short enable)
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
        result = adin1320_getSWPD(hDevice, &swpd);
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
 * @brief           adin1300_getSWPD
 *
 * @param [in]      phyAddr PHY HW address of ADIN1300
 * @param [out]     enable  status of the SWPD
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function gives status of SWPD in ADIN1300
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
 * @param [in]      phyAddr PHY HW address of ADIN1300
 * @param [out]     enable  enables or disables SWPD
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function enables or disables SWPD in ADIN1300
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
 * @brief           adin1320_cfg
 *
 * @param [in]      _boardDetails pointer to board_t structure
 * @param [in]      hDevice  Device handle having PHY address of ADIN1320
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function configures the ADIN1320
 *
 */
adi_eth_Result_e adin1320_cfg(board_t *_boardDetails, adi_phy_Device_t *hDevice)
{
	adi_eth_Result_e result = ADI_ETH_SUCCESS;
    unsigned short val16 = 0;

	/* Discover ADIN1320 PHY*/
	result = adin1320_discoverPhy(hDevice);
    if(result != ADI_ETH_SUCCESS)
    {
    	_boardDetails->errorLed = TRUE;
        printf("Error - ADIN1320 discover PHY - %s \n\r", adi_eth_result_string[result]);
        goto end;
    }
    else
    {
    	printf("ADIN1320 MDIO address %x \n\r",hDevice->phyAddr);
    }

	/* Software Reset ADIN1320 */
    result = adin1320_phyReset(hDevice);
    if(result != ADI_ETH_SUCCESS)
    {
        printf("Error - ADIN1320 PHY reset - %s \n\r", adi_eth_result_string[result]);
		result = ADI_ETH_COMM_ERROR;
    }

    printf("ADIN1320 HW CFG: autoneg,");

    /* Check if ADIN1320 is in RGMII MAC mode */
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
    	printf("\n\r Error - ADIN1320 is not in RGMII mode \n\r");
    	_boardDetails->errorLed = true; /* Flag the RED LED when PHY not in RGMII Mode */
    }

    /* Check the ADIN1320 HW CFG setting Master/Slave
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

    /* Check the ADIN1320 HW CFG setting amplitude
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
    /* Put ADIN1320 in SW power down mode */
    result = adin1320_setSWPD(hDevice, TRUE);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf(" ADIN1320 is not in SWPD\n\r");
	}

    /* Set ADIN1320 LED0 [Green LED for Link UP] and LED1[Yellow LED for TX/RX Activity] behavior */
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

    /* Bring ADIN1320 out of SW power down mode */
    result = adin1320_setSWPD(hDevice, FALSE);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error adin1320 is not out of SWPD\n");
	}
    printf("\n================================================\n");

end:
    return result;
}

/*!
 * @brief           adin1300_cfg
 *
 * @param [in]      _boardDetails PHY HW address of ADIN1300
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function configures the ADIN1300
 *
 */
adi_eth_Result_e adin1300_cfg(board_t *_boardDetails)
{
	adi_eth_Result_e result = ADI_ETH_SUCCESS;
    unsigned short  val16=0;

	/* Discover ADIN1300 PHY*/
	result = adin1300_discoverPhy(_boardDetails);
    if(result != ADI_ETH_SUCCESS)
    {
    	_boardDetails->errorLed = TRUE;
    	printf("Error ADIN1300 PHY discovery - %s \n\r", adi_eth_result_string[result]);
        goto end;
    }
    else
    	printf("ADIN1300 MDIO address %d \n\r",_boardDetails->adin1300PhyAddr);

	/* Software Reset ADIN1300 */
    result = adin1300_phyReset(_boardDetails->adin1300PhyAddr);
    if(result != ADI_ETH_SUCCESS)
    {
        printf("Error ADIN1300 PHY reset - %s \n\r", adi_eth_result_string[result]);
        result = ADI_ETH_COMM_ERROR;
    }

    /* Enter ADIN1300 SW power down mode */
    result = adin1300_setSWPD(_boardDetails->adin1300PhyAddr,TRUE);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error ADIN1300 is not in SWPD\n\r");
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

	//13201: blink on activity
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

	if(result == ADI_ETH_SUCCESS)
        printf("ADIN1300 SW CFG: autoneg 10Mbit Full Duplex Only ");
	else
		printf("Error ADIN1300 SW CFG \r\n");

    /*******************************************/

    /* Bring ADIN1300 out of SW power down mode */
    result = adin1300_setSWPD(_boardDetails->adin1300PhyAddr,FALSE);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error adin1300 is not out of SWPD\n\r");
	}
    printf("\n================================================\n");
end:
    return result;
}

/*!
 * @brief           applyBoardConfig
 *
 * @param [in]      _boardDetails  pointer to board_t structure
 * @param [in]      hDevice  pointer to PHY address of ADIN1320
 *
 * @details         This function configures the firmware according to the HW Config pins
 *
 * @sa
 */
adi_eth_Result_e applyBoardConfig(board_t *_boardDetails, adi_phy_Device_t *hDevice)
{
	adi_eth_Result_e result = ADI_ETH_SUCCESS;
    uint16_t val16 = 0;


	result = ltc4296_1_cfg(_boardDetails, LTC_CFG_SCCP_MODE, LTC_PORT1);
	if (result != ADI_ETH_SUCCESS)
	{
		printf("LTC4296-1 Error - %s \n\r", adi_eth_result_string[result]);
		_boardDetails->errorLed = true;
	}

	result = adin1320_cfg(_boardDetails, hDevice);
	if (result != ADI_ETH_SUCCESS)
	{
		printf("ADIN1320 Error - %s \n\r", adi_eth_result_string[result]);
		_boardDetails->errorLed = TRUE;
		/* Fatal error - reset the board to clear the flag */
	}

	result = adin1300_cfg(_boardDetails);
	if (result != ADI_ETH_SUCCESS)
	{
		printf("ADIN1300 Error - %s \n\r", adi_eth_result_string[result]);
		_boardDetails->errorLed = TRUE;
	}

	switch(_boardDetails->fwMode)
	{
		case PHY_TESTMODE_0:
		{
			// insert reg write testmode 0
		}
		break;

		case PHY_TESTMODE_1:
		{
			// insert reg write testmode 1
		}
		break;

		case PHY_TESTMODE_2:
		{
			// insert reg write testmode 2
		}
		break;

		case PHY_TESTMODE_3:
		{
			// insert reg write testmode 3
		}
		break;

		case INTERACTIVEMODE:
		{
			// Do nothing
		}
		break;

		case MAC_REMOTELB:
		{
			// insert sequence
		}
		break;

		case FRAMEGENCHECK:
		{
			// insert sequence to enable frame generator
			// do frame checker at cyclic tasks
		}
		break;

		case MEDCONV_CU_SGMII:
		{
			// insert reg writes to set MEDIA CONVERTER for COPPER to SGMII
		}
		break;

		case MEDCONV_CU_FI:
		{
			// insert reg writes to set MEDIA CONVERTER for COPPER to FIBER
		}
		break;

		case MEDCONV_CU_CU: // default configuration
		default:
		{
			// insert reg writes to set MEDIA CONVERTER for COPPER to COPPER
		}
		break;
	}

    return result;
}

/*
 * @brief           ltc4296_1_cfg
 *
 * @param [in]      boardDetails    pointer to board_t structure
 * @param [in]      ltcCfgClass     LTC4296-1 class
 * @param [in]      ltcCfgClass     LTC4296-1 port num\ber (0-4)
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
 * @brief        	setUartDataAvailable
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
 * @brief        	getUartDataAvailable
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
 * @brief        	setUartCmdAvailable
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
 * @brief        	getUartCmdAvailable
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

