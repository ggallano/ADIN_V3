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

/** @addtogroup aux Auxiliary Functions
 *  @{
 */

#include <math.h>
#include "bsp\boardsupport.h"
#include "drivers\adinPhy\adin1320.h"
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

/* Board Name for Logs */
char currentBoardName[9];

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
		case PHY_TESTMODE_1:
		case PHY_TESTMODE_2:
		case PHY_TESTMODE_3:
		case RESERVED_4:
		case RESERVED_5:
		case RESERVED_6:
		{
			label = "Unsupported Mode";
		}
		break;
		case INTERACTIVEMODE:
		{
			label = "GUI Mode";
		}
		break;
		case MAC_REMOTELB:
		case FRAMEGENCHECK:
		case RESERVED_A:
		case RESERVED_B:
		case RESERVED_C:
		case MEDCONV_CU_SGMII:
		case MEDCONV_CU_FI:
		case MEDCONV_CU_CU:
		{
			label = "Unsupported Mode";
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
        case 0x1:
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
 * @brief           checkIdentity
 *
 * @param [in]      phyAddr     PHY address of ADIN1320/1300
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
static adi_eth_Result_e checkIdentity(uint8_t phyAddr)
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
    	printf("Error %s ID1 value read = 0x%x", currentBoardName, val16);
        result = ADI_ETH_UNSUPPORTED_DEVICE;
    }

    result = adi_MdioRead(phyAddr, ADDR_PHY_ID_2, &val16);
    if(result != ADI_ETH_SUCCESS)
    {
    	printf("SPI adi_MdioRead failed! (0x%X)\n", result);
        result = ADI_ETH_UNSUPPORTED_DEVICE;
    }

    /*Check if the value of PHY_ID_2.OUI matches expected value */
    if ((val16 & BITM_PHY_ID_2_PHY_ID_2_OUI) != (ADI_PHY_DEV_ID2_OUI << BITP_PHY_ID_2_PHY_ID_2_OUI))
    {
    	printf("Error %s ID2 value read = 0x%x", currentBoardName, val16);
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
    	result = adi_MdioRead(phyAddr, ADDR_PHY_ID_1, &val16);
    }while ((result == ADI_ETH_COMM_ERROR) && (--iter));

    return result;
}

/*!
 * @brief           discoverPhy
 *
 * @param [in]      phyAddr     PHY address of ADIN1320/1300
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function reads the device ID of ADIN1320/1300 and verifies.
 */
adi_eth_Result_e discoverPhy(uint8_t phyAddr)
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
	result = waitMdio(phyAddr);
    if (result != ADI_ETH_SUCCESS)
    {
    	return result;
    }

	/* Checks the identity of the device based on reading of hardware ID registers */
	/* Ensures the device is supported by the driver, otherwise an error is reported. */
    result = checkIdentity(phyAddr);
	return result;
}

/*!
 * @brief           phyReset
 *
 * @param [in]      phyAddr     PHY address of ADIN1320/1300
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function resets the ADIN1320/1300
 *
 */
adi_eth_Result_e phyReset(uint8_t phyAddr)
{
    adi_eth_Result_e result = ADI_ETH_SUCCESS;

	adi_MdioWrite(phyAddr,ADDR_EXT_REG_PTR, ADDR_GE_SFT_RST_CFG_EN);
	adi_MdioWrite(phyAddr,ADDR_EXT_REG_DATA, BITM_GE_SFT_RST_CFG_EN_GE_SFT_RST_CFG_EN);

	adi_MdioWrite(phyAddr,ADDR_EXT_REG_PTR, ADDR_GE_SFT_RST);
	adi_MdioWrite(phyAddr,ADDR_EXT_REG_DATA, BITM_GE_SFT_RST_GE_SFT_RST);

	TimerDelay_ms(ADI_SW_RESET_DELAY);

	/* Wait until MDIO interface is up. */
	result = waitMdio(phyAddr);
	if(result != ADI_ETH_SUCCESS)
	{
		result = ADI_ETH_COMM_ERROR;
    }

    return result;
}

/*!
 * @brief           getSWPD
 *
 * @param [in]      phyAddr PHY HW address of ADIN1320/1300
 * @param [out]     enable  status of the SWPD
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function gives status of SWPD in ADIN1320/1300
 *
 */
adi_eth_Result_e getSWPD(uint8_t phyAddr, unsigned short *enable)
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
 * @brief           setSWPD
 *
 * @param [in]      phyAddr PHY HW address of ADIN1320/1300
 * @param [in]      enable  enables or disables SWPD
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function enables or disables SWPD in ADIN1320/1300
 *
 */
adi_eth_Result_e setSWPD(uint8_t phyAddr, unsigned short enable)
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
        result = getSWPD(phyAddr, &swpd);
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
 * @brief           generalCfg
 *
 * @param [in]      phyAddr PHY HW address of ADIN1320/1300
 * @return
 *                  - #ADI_ETH_SUCCESS.
 *                  - #ADI_ETH_COMM_ERROR.
 *
 * @details         This function configures the ADIN1320/1300
 *
 */
adi_eth_Result_e generalCfg(uint8_t phyAddr)
{
	adi_eth_Result_e result = ADI_ETH_SUCCESS;
    unsigned short  val16=0;

	/* Discover ADIN1320/1300 PHY*/
	result = discoverPhy(phyAddr);
    if(result != ADI_ETH_SUCCESS)
    {
    	printf("Error %s PHY discovery - %s \n", currentBoardName, adi_eth_result_string[result]);
        goto end;
    }
    else
    	printf("%s MDIO address %d \n", currentBoardName,phyAddr);

	/* Software Reset ADIN1320/1300 */
    result = phyReset(phyAddr);
    if(result != ADI_ETH_SUCCESS)
    {
        printf("Error %s PHY reset - %s \n", currentBoardName, adi_eth_result_string[result]);
        result = ADI_ETH_COMM_ERROR;
    }

    /* Enter ADIN1320/1300 SW power down mode */
    result = setSWPD(phyAddr,TRUE);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error %s is not in SWPD\n", currentBoardName);
	}

//    /**********************************/
//    /* Setup LED for activity */
//    val16 = 0;
//	result = adi_MdioRead(phyAddr, ADDR_LED_CTRL_1, &val16);
//	if(result != ADI_ETH_SUCCESS)
//	{
//		result = ADI_ETH_COMM_ERROR;
//    }
//    val16 |= (BITM_LED_CTRL_1_LED_A_EXT_CFG_EN|BITM_LED_CTRL_1_LED_PUL_STR_EN);// Enable Extended LED Configuration
//	result = adi_MdioWrite(phyAddr, ADDR_LED_CTRL_1, val16);
//	if(result != ADI_ETH_SUCCESS)
//	{
//		result = ADI_ETH_COMM_ERROR;
//    }
//
//	//13201: blink on activity
//	val16 = 0x2109;
//	result = adi_MdioWrite(phyAddr, ADDR_LED_CTRL_2, val16);
//	if(result != ADI_ETH_SUCCESS)
//	{
//		result = ADI_ETH_COMM_ERROR;
//    }
//
//    /**********************************/
//    /* EXT_REG_PTR = 0xFF38 - Preamble recovery register */
//	/* FF38 register address is not available in the Data sheet */
//
//    val16 = 0xFF38;
//	result = adi_MdioWrite(phyAddr, ADDR_EXT_REG_PTR, val16);
//	if(result != ADI_ETH_SUCCESS)
//	{
//		result = ADI_ETH_COMM_ERROR;
//    }
//
//    /* EXT_REG_DATA = 0x0001 // Enable preamble recovery */
//    val16 = 0x0001;
//	result = adi_MdioWrite(phyAddr, ADDR_EXT_REG_DATA, val16);
//	if(result != ADI_ETH_SUCCESS)
//	{
//		result = ADI_ETH_COMM_ERROR;
//    }
//
//	if(result == ADI_ETH_SUCCESS)
//        printf("%s SW CFG: Board Reset Configuration ", currentBoardName);
//	else
//		printf("Error %s SW CFG \n", currentBoardName);
//
//    /*******************************************/

    /* Bring ADIN1320/1300 out of SW power down mode */
    result = setSWPD(phyAddr,FALSE);
    if(result != ADI_ETH_SUCCESS)
	{
    	printf("Error %s is not out of SWPD\n", currentBoardName);
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

    strcpy(currentBoardName, "ADIN1320");
	result = generalCfg(hDevice->phyAddr);
	if (result != ADI_ETH_SUCCESS)
	{
		// To bring back _boardDetails->errorLed = TRUE based on LED config
		printf("%s Error - %s \n", currentBoardName, adi_eth_result_string[result]);
	}

    strcpy(currentBoardName, "ADIN1300");
	result = generalCfg(_boardDetails->adin1300PhyAddr);
	if (result != ADI_ETH_SUCCESS)
	{
		// To bring back _boardDetails->errorLed = TRUE based on LED config
		printf("%s Error - %s \n", currentBoardName, adi_eth_result_string[result]);
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

/* Set LED gpio pins for MCU testing/verification */
void setGpio_LedGreen(bool en)
{
    if(en == TRUE)
    {
        BSPConfigLED(D2Z_BRD_GREEN_LED, true);
    }
    else
    {
        BSPConfigLED(D2Z_BRD_GREEN_LED, false);
    }
}

void setGpio_LedRed(bool en)
{
    if(en == TRUE)
    {
        BSPConfigLED(D2Z_BRD_RED_LED, true);
    }
    else
    {
        BSPConfigLED(D2Z_BRD_RED_LED, false);
    }
}

void setGpio_LedYellow(bool en)
{
    if(en == TRUE)
    {
        BSPConfigLED(D2Z_BRD_YELLOW_LED, true);
    }
    else
    {
        BSPConfigLED(D2Z_BRD_YELLOW_LED, false);
    }
}

void setGpio_LedBlue(bool en)
{
    if(en == TRUE)
    {
        BSPConfigLED(D2Z_BRD_BLUE_LED, true);
    }
    else
    {
        BSPConfigLED(D2Z_BRD_BLUE_LED, false);
    }
}

uint8_t readGpio_ConfigPins(void)
{
	uint8_t val = BSP_getConfigPins();

    return val;
}

void read_ConfigToLed(void)
{
	uint8_t val2 = readGpio_ConfigPins();
	uint8_t val = 0;

	val = val2 & 0x08;
	if (val >> 3)
	{
		setGpio_LedGreen(true);
	}
	else
	{
		setGpio_LedGreen(false);
	}

	val = val2 & 0x04;
	if (val >> 2)
	{
		setGpio_LedRed(true);
	}
	else
	{
		setGpio_LedRed(false);
	}

	val = val2 & 0x02;
	if (val >> 1)
	{
		setGpio_LedYellow(true);
	}
	else
	{
		setGpio_LedYellow(false);
	}

	val = val2 & 0x01;
	if (val)
	{
		setGpio_LedBlue(true);
	}
	else
	{
		setGpio_LedBlue(false);
	}
}
/**@}*/














