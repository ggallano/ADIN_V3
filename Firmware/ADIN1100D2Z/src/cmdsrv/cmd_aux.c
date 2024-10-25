/*
 *---------------------------------------------------------------------------
 *
 * Copyright (c) 2016 - 2020 Analog Devices, Inc. All Rights Reserved.
 * This software is proprietary to Analog Devices, Inc.
 * and its licensors.By using this software you agree to the terms of the
 * associated Analog Devices Software License Agreement.
 *
 *
 *---------------------------------------------------------------------------
 */
#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>

#include "cmdsrv\cmd_aux.h"
#include "cmdsrv\cmd_list.h"
#include "bsp\boardsupport.h"
#include "adi_common.h"
#include "platform\adi_platform.h"
#include "drivers\ltc4296_1\spoeLTC4296_1.h"
#include "aux_functions.h"

#define DEVTYPE(a)         (a >> 16)
#define REGADDR(a)         (a & 0xFFFF)

/*!
 * @brief           CMDAUX_mdioWrite
 *
 * @param [in]      arga   PHY register address
 * @param [in]      argb   data to be written to the register
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function calls the adi_MdioWrite() command to write to PHY
 *
 * @sa              CMDAUX_mdioRead()
 */
int CMDAUX_mdioWrite(char *arga, char *argb, int fd)
{
    char *token, *saveptr1;
    int phyAddr = 0;
    int regAddr = 0;
    int regData = 0;

    saveptr1 = arga;

    token = strtok_r(NULL, "", &saveptr1);
    if (token == NULL )
    {
        sprintf(argb, "ERROR: Invalid # of params\n");
        return 0;
    }
    if (sscanf(token, "%x,%x,%x", &phyAddr, &regAddr, &regData) != 3)
    {
        sprintf(argb, "ERROR: Invalid # of params\n");
        return 0;
    }

    adi_MdioWrite(phyAddr, regAddr, regData);

    sprintf(argb, "OK\n");
    return 0;
}

/*!
 * @brief           CMDAUX_mdioRead
 *
 * @param [in]      arga   PHY register address
 * @param [in]      argb   data to be read from the register
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function calls the adi_MdioRead() to read from PHY
 *
 * @sa              CMDAUX_mdioWrite()
 */
int CMDAUX_mdioRead(char *arga, char *argb, int fd)
{
    uint32_t error = 0;
	char tempBuff[100];
    char *token;
    char *saveptr1;
    int phyAddr = 0;
    int regAddr = 0;
    unsigned short regData = 0;

    token = &tempBuff[0];
    saveptr1 = arga;

    token = strtok_r(NULL, "", &saveptr1);
    if (token == NULL )
    {
        sprintf(argb, "ERROR: Invalid # of params\n");
        return 0;
    }
    if (sscanf(token, "%x,%x", &phyAddr, &regAddr ) != 2)
    {
        sprintf(argb, "ERROR: Invalid # of params\n");
        return 0;
    }
    error = adi_MdioRead(phyAddr, regAddr,&regData);
    if(error > 0)
    {
      sprintf(argb, "ERROR: MDIO read - no response from this address\n");
    }
    else
    {
      sprintf(argb, "%04X\n", regData);
    }
    return 0;
}

/*!
 * @brief           CMDAUX_mdioRead45Clause
 *
 * @param [in]      arga   PHY register address
 * @param [in]      argb   data to be read from the register
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function calls the adi_MdioRead_Cl45() to read from PHY
 *
 * @sa              CMDAUX_mdioWrite45Clause()
 */
int CMDAUX_mdioRead45Clause(char *arga, char *argb, int fd)
{
  uint32_t error = 0;
	char tempBuff[100];
    char *token;
    char *saveptr1;
    int phyAddr = 0;
    int devType = 0;
    unsigned short regData = 0;

    token = &tempBuff[0];
    saveptr1 = arga;

    token = strtok_r(NULL, "", &saveptr1);
    if (token == NULL )
    {
        sprintf(argb, "ERROR: Invalid # of params\n");
        return 0;
    }
    if (sscanf(token, "%x,%x", &phyAddr, &devType ) != 2)
    {
        sprintf(argb, "ERROR: Invalid # of params\n");
        return 0;
    }
    error = adi_MdioRead_Cl45(phyAddr, devType,&regData);
    if(error > 0)
    {
      sprintf(argb, "ERROR: MDIO read - no response from this address\n");
    }
    else
    {
      sprintf(argb, "%04X\n", regData);
    }
    return 0;
}

/*!
 * @brief           CMDAUX_mdioWrite45Clause
 *
 * @param [in]      arga   PHY register address
 * @param [in]      argb   data to be written to the register
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function calls the adi_MdioWrite_Cl45() command to write to PHY
 *
 * @sa              CMDAUX_mdioRead45Clause()
 */
int CMDAUX_mdioWrite45Clause(char *arga, char *argb, int fd)
{
    char *token, *saveptr1;
    int phyAddr = 0;
    int devType = 0;
    int regData = 0;

    saveptr1 = arga;


    token = strtok_r(NULL, "", &saveptr1);
    if (token == NULL )
    {
        sprintf(argb, "ERROR: Invalid # of params\n");
        return 0;
    }
    if (sscanf(token, "%x,%x,%x", &phyAddr, &devType, &regData) != 3)
    {
        sprintf(argb, "ERROR: Invalid # of params\n");
        return 0;
    }

    adi_MdioWrite_Cl45(phyAddr, devType, regData);

    sprintf(argb, "OK\n");
    return 0;
}

/*!
 * @brief           CMDAUX_printGreetings
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function calls printGreetings() to print Welcome message on UART
 *
 */
int CMDAUX_printGreetings(char *arga, char *argb, int fd)
{
    printGreetings(&boardDetails);
    sprintf(argb, "\r\n");
    return 0;
}

/*
 * details    This function prints the command list on the UART.
 *
 */
static void printCommandList()
{
    int i=0,listofCmds;

    listofCmds = getAuxFuncSize();
    printf("==============================================\r\n");
    printf("List of Commands\r\n");
    printf("==============================================\r\n");

    for(i=0;i<listofCmds;i++)
    {
       if(aux_commands[i].public)
    	   printf("%s",aux_commands[i].cmd_details);
    }

    printf("==============================================\r\n");
 }

/*!
 * @brief           CMDAUX_printCommandList
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function calls printCommandList() to print Welcome message on UART
 *
 */
int CMDAUX_printCommandList(char *arga, char *argb, int fd)
{
   printCommandList();
   sprintf(argb, "\r\n");
   return 0;
}

/*
 * details    This function prints the modes list on the UART.
 *
 */
static void printModesList(void)
{
    printf("==============================================\r\n");
    printf("       List of Modes\r\n");
    printf("==============================================\r\n");

    printf( "0 SPOE CLASS 10\n" );
    printf( "1 SPOE CLASS 11\n" );
    printf( "2 SPOE CLASS 12\n" );
    printf( "3 SPOE CLASS 13\n" );
    printf( "4 SPOE CLASS 14\n" );
    printf( "5 SPOE CLASS 15\n" );
    printf( "6 APL CLASS A\n" );
    printf( "7 APL CLASS A NO AUTONEG\n" );
    printf( "8 APL CLASS C\n" );
    printf( "9 APL CLASS 3\n" );
    printf( "10 PRODUCTION POWER TEST\n" );
    printf( "11 APL CLASS A OLD DEMO\n" );
    printf( "12 SPOE OFF\n" );
    printf( "13 PRODUCTION DATA TEST\n" );
    printf( "14 RESERVED\n" );
    printf( "15 DEBUG \n" );
    printf( "==============================================\r\n");
 }


/*!
 * @brief           CMDAUX_printModes
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function calls the printModesList() to print modes on UART
 *
 */
int CMDAUX_printModes(char *arga, char *argb, int fd)
{
    printModesList();
    sprintf(argb, "\r\n");
    return 0;
}


/*!
 * @brief           CMDAUX_systemReset
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function calls the systemReset to reset the Micro controller
 *
 */
int CMDAUX_systemReset(char *arga, char *argb, int fd)
{
	systemReset();
    return 0;
}


/*!
 * @brief           CMDAUX_changeConfig
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function sets the firmware to the mode set by the user through command line
 *
 */
int CMDAUX_changeConfig(char *arga, char *argb, int fd)
{
    char *token, *saveptr1;
    int mode = 0;
    saveptr1 = arga;

    token = strtok_r(NULL, "\r\n", &saveptr1);
    if (token == NULL )
    {
        sprintf(argb, "\r\nERROR: Invalid # of params\r\nType '<?><new line>' for the list of commands\r\n");
        return 0;
    }
    if (sscanf(token, "%d",  &mode) != 1)
    {
        sprintf(argb, "\r\nERROR: Invalid # of params\r\nType '<?><new line>' for the list of commands\r\n");
        return 0;
    }
    if(mode <0 || mode > 15)
    {
         sprintf(argb, "\r\nERROR: Mode not implemented\r\n");
         return 0;
    }

    configFirmware();

    boardDetails.fwMode = mode;
    if( (boardDetails.fwMode != RESERVED) && (boardDetails.fwMode != DEBUGMODE) )
    {
    	adin_phyReset(&boardDetails,hDevice->pPhyDevice);
    }

    BSP_StopTimer();

    setBoardLED(ON);
    TimerDelay_ms(1000);
    printGreetings(&boardDetails);
    setBoardLED(OFF);

	boardDetails.readMSE = FALSE;
	boardDetails.frameGenEnabled = FALSE;

    applyBoardConfig(&boardDetails, hDevice->pPhyDevice);

    BSP_StartTimer();

    sprintf(argb, "\r\n");
    return 0;
}


/*!
 * @brief           CMDAUX_phyStatus
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function calls adin_phyPrintLinkStatus() to read the phy status
 *
 */
int CMDAUX_phyStatus(char *arga, char *argb, int fd)
{
	/* Read the PHY latched data only when user requests */
	adin_phyPrintLinkStatus(&boardDetails);
    return 0;
}

/*!
 * @brief           CMDAUX_phytxdisabled
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function performs the test PHY TX Disabled
 *
 */
int CMDAUX_phytxdisabled(char *arga, char *argb, int fd)
{
    uint32_t result = 0;
	result  = phyTest_TxDisabled(hDevice);
	if (result != ADI_ETH_SUCCESS)
	{
	    sprintf(argb, "\r\nADIN1100 is not configured for Test TX disabled \r\n ");
	    sprintf(argb, "ADIN1100 Error - %s \r\n ", adi_eth_result_string[result]);
		boardDetails.errorLed = true;
		/* Fatal error - reset the board to clear the flag */
	}
	return 0;
}

/*!
 * @brief           CMDAUX_phytestmode1
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function performs the test PHY TESTMODE 1
 *
 */
int CMDAUX_phytestmode1(char *arga, char *argb, int fd)
{
    uint32_t result = 0;
	result  = phyTest_mode1(hDevice);
	if (result != ADI_ETH_SUCCESS)
	{
	    sprintf(argb, "\r\nADIN1100 is not configured for Test Mode1 \r\n");
	    sprintf(argb, "ADIN1100 Error - %s \r\n", adi_eth_result_string[result]);
		boardDetails.errorLed = true;
		/* Fatal error - reset the board to clear the flag */
	}
	return 0;
}


/*!
 * @brief           CMDAUX_phytestmode2
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function performs the test PHY TESTMODE 2
 *
 */
int CMDAUX_phytestmode2(char *arga, char *argb, int fd)
{
    uint32_t result = 0;
	result  = phyTest_mode2(hDevice);
	if (result != ADI_ETH_SUCCESS)
	{
	    sprintf(argb, "\r\nADIN1100 is not configured for Test Mode2 \r\n");
	    sprintf(argb, "ADIN1100 Error - %s \r\n", adi_eth_result_string[result]);
		boardDetails.errorLed = true;
		/* Fatal error - reset the board to clear the flag */
	}
	return 0;
}

/*!
 * @brief           CMDAUX_phytestmode3
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function performs the test PHY TESTMODE 3
 *
 */
int CMDAUX_phytestmode3(char *arga, char *argb, int fd)
{
    uint32_t result = 0;
	result  = phyTest_mode3(hDevice);
	if (result != ADI_ETH_SUCCESS)
	{
	    sprintf(argb, "\r\nADIN1100 is not configured for Test Mode 3 \r\n");
	    sprintf(argb, "ADIN1100 Error - %s \r\n", adi_eth_result_string[result]);
		boardDetails.errorLed = true;
		/* Fatal error - reset the board to clear the flag */
	}
	return 0;
}

/*!
 * @brief           CMDAUX_adin1100RemLoopback
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function sets the ADIN1100 in remote loop back mode
 *
 */
int CMDAUX_adin1100RemLoopback(char *arga, char *argb, int fd)
{
    uint32_t result = 0;

	result  = adin1100_remLoopback(&boardDetails, hDevice->pPhyDevice);
	if (result != ADI_ETH_SUCCESS)
	{
	    sprintf(argb, "\r\nRemote loopback failed \r\n");
	    sprintf(argb, "RemLoopback Error - %s \r\n", adi_eth_result_string[result]);
		boardDetails.errorLed = true;
		/* Fatal error - reset the board to clear the flag */
	}
    return 0;
}

/*!
 * @brief           CMDAUX_adin1200RemLoopback
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function sets the ADIN1200 in remote loop back mode
 *
 */
int CMDAUX_adin1200RemLoopback(char *arga, char *argb, int fd)
{
    uint32_t result = 0;

	result  = adin1200_remLoopback(&boardDetails);
	if (result != ADI_ETH_SUCCESS)
	{
	    sprintf(argb, "\r\nRemote loopback failed \r\n");
	    sprintf(argb, "RemLoopback Error - %s \r\n", adi_eth_result_string[result]);
		boardDetails.errorLed = true;
		/* Fatal error - reset the board to clear the flag */
	}
    return 0;
}

/*!
 * @brief           CMDAUX_frameGenAChecks
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - Execute the command successfully
 *
 * @details         This function sets the ADIN1100 in frameGen mode
 *
 */
int CMDAUX_frameGenACheck(char *arga, char *argb, int fd)
{
    uint32_t result = 0;
	result  = adin1100_frameGenACheck(&boardDetails, hDevice->pPhyDevice);
	if (result != ADI_ETH_SUCCESS)
	{
	    sprintf(argb, "Frame Gen and check not enabled \r\n");
	    sprintf(argb, "frameGen Error - %s \r\n", adi_eth_result_string[result]);
		boardDetails.errorLed = true;
		/* Fatal error - reset the board to clear the flag */
	}
    return 0;
}


/*
 * @brief          CMDAUX_phyReset
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #Execute the command successfully
 *
 * @details         The function performs a SW resets on ADIN1100 and ADIN1200 PHY device.
 *
 */
int CMDAUX_phyReset(char *arga, char *argb, int fd)
{
    uint32_t result = 0;
	result = adin_phyReset(&boardDetails, hDevice->pPhyDevice);
	if (result != ADI_ETH_SUCCESS)
	{
		sprintf(argb,"\n\rERROR - PHY Reset done");
	}
	else
	{
	    sprintf(argb,"\r\nPHY RESET OK \r\n");
	}
    return 0;
}

/*
 * @brief           CMDAUX_getfwVersion
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #Execute the command successfully
 *
 * @details         The function gets the FW version.
 *
 */
int CMDAUX_getfwVersion(char *arga, char *argb, int fd)
{
  uint16_t tmp1 = 0;
  uint16_t tmp2 = 0;
  uint32_t tmp3 = 0;
  tmp1 = boardDetails.fwVersion >> 24;
  tmp2 = (boardDetails.fwVersion >> 16) & 0xFF;
  tmp3 = boardDetails.fwBuild;

  sprintf(argb, "\r\n%d.%d.%x\r\n", tmp1, tmp2, tmp3);

  return 0;
}

/*
 * @brief           CMDAUX_startUartReport
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #Execute the command successfully
 *
 * @details         The function sets the flag to report on UART.
 *
 */
int CMDAUX_startUartReport(char *arga, char *argb, int fd)
{
    if ((boardDetails.fwMode == RESERVED) || (boardDetails.fwMode == DEBUGMODE) )
    {
      sprintf(argb, "ERROR: Board is in RESERVED MODE \r\n");
      return 1;
    }
    else
    {
    	boardDetails.uartReport = TRUE;
        boardDetails.readMSE = TRUE;
    }
    sprintf(argb, "\r\nOK\r\n");
    return 0;
}

/*
 * @brief           CMDAUX_stopUartReport
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #Execute the command successfully
 *
 * @details         The function resets the flag to report on UART.
 *
 */
int CMDAUX_stopUartReport(char *arga, char *argb, int fd)
{
	boardDetails.uartReport = FALSE;
    boardDetails.readMSE = FALSE;
    boardDetails.frameGenEnabled = FALSE;

    sprintf(argb,"\r\nOK \r\n");
    return 0;
}

/*
 * @brief           CMDAUX_clearFrameErrors
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline chars
 * @return
 *                  - #Execute the command successfully
 *
 * @details         The function clears the flags.
 *
 * @sa               cyclicReadBoard()
 */
int CMDAUX_clearFrameErrors(char *arga, char *argb, int fd)
{
    boardDetails.clrFrameErrors = 1;

    sprintf(argb,"\r\nOK\r\n");// No OK returned
    return 0;
}

/*!
 * @brief           CMDAUX_spoeRead
 *
 * @param [in]      arga   register address
 * @param [in]      argb   data to be read from the register
 * @param [in]      fd     takes newline chars
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function calls ltc4296_1_read() to read from LTC4296-1
 *
 * @sa              CMDAUX_spoeWrite()
 */
int CMDAUX_spoeRead(char *arga, char *argb, int fd)
{
	adi_ltc_Result_e error = 0;
    char tempBuff[100];
    char *token;
    char *saveptr1;
    int regAddr = 0;
    uint16_t regData = 0;

    token = &tempBuff[0];
    saveptr1 = arga;

    token = strtok_r(NULL, "\r\n", &saveptr1);
    if (token == NULL )
    {
        sprintf(argb, "\r\nERROR: Invalid # of params\r\n");
        return 0;
    }
    if (sscanf(token, "%x", &regAddr ) != 1)
    {
        sprintf(argb, "\r\nERROR: Invalid # of params\r\n");
        return 0;
    }
    error = ltc4296_1_read(regAddr,&regData);
    if(error > 0)
    {
      sprintf(argb, "\r\nERROR: SPOE read - no response from this address\r\n");
    }
    else
    {
        sprintf(argb, "\r\n%04X \r\n", regData);
    }
    return 0;
}

/*!
 * @brief           CMDAUX_spoeWrite
 *
 * @param [in]      arga   register address
 * @param [in]      argb   data to write to register
 * @param [in]      fd     takes newline chars
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function calls ltc4296_1_write() to write to LTC4296-1
 *
 * @sa              CMDAUX_spoeRead()
 */
int CMDAUX_spoeWrite(char *arga, char *argb, int fd)
{
    char *token, *saveptr1;
    int regAddr = 0;
    int regData = 0;
    adi_ltc_Result_e error;
    saveptr1 = arga;

    token = strtok_r(NULL, "\r\n", &saveptr1);
    if (token == NULL )
    {
        sprintf(argb, "\r\nERROR: Invalid # of params\r\n");
        return 0;
    }
    if (sscanf(token, "%x,%x", &regAddr, &regData) != 2)
    {
        sprintf(argb, "\r\nERROR: Invalid # of params\r\n");
        return 0;
    }
    error = ltc4296_1_write(regAddr, regData);
    if(error > 0)
    {
      sprintf(argb, "\r\nERROR: SPOE write \r\n");
    }
    sprintf(argb, "\r\nOK\r\n");
    return 0;
}

/*!
 * @brief           CMDAUX_sccpReadWrite
 *
 * @param [in]      arga   boradcast address
 * @param [in]      argb   scratchpad command
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function performs SCCP on the PD.
 *                  Need to have a PD connected for valid response.
 *
 */
int CMDAUX_sccpReadWrite(char *arga, char *argb, int fd)
{
    char *saveptr1;
    int broadcastAddr = 0;
    int scratchpageCmd = 0;
    uint16_t resData = 0;
    adi_ltc_Result_e error;
	char tempBuff[100];
    char *token;

    saveptr1 = arga;
    token = &tempBuff[0];
    token = strtok_r(NULL, "\r\n", &saveptr1);
    if (token == NULL )
    {
        sprintf(argb, "ERROR: Invalid # of params\r\n");
        return 0;
    }
    if (sscanf(token, "%x,%x", &broadcastAddr, &scratchpageCmd) != 2)
    {
        sprintf(argb, "ERROR: Invalid # of params\r\n");
        return 0;
    }

    error = ltc4296_1_SCCPPD(&resData,broadcastAddr,scratchpageCmd);
    if(error != ADI_LTC_SCCP_PD_PRESENT)
    {
      sprintf(argb, "\r\nERROR: SCCP R/W - no response from this address\r\n");
    }
    else
    {
        printf("PD present, PD response = 0x%04X\r\n", resData);
    }
    return 0;
}

/*!
 * @brief           CMDAUX_sccpReset
 *
 * @param [in]      arga   Input arguments
 * @param [in]      argb   Input arguments
 * @param [in]      fd     takes newline char
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This function performs SCCP reset.
 *
 */
int CMDAUX_sccpReset(char *arga, char *argb, int fd)
{
	uint8_t level = 0;
    adi_eth_Result_e error;

    error = ltc4296_1_SCCPResetPulse(&level);
    if(error > 0)
    {
      sprintf(argb, "ERROR: SCCP Reset \r\n");
    }
    else
    {
		if(level == 1)
			sprintf(argb, "PD present \r\n");
		else
			sprintf(argb, "ERROR: SCCPReset - no response from PD\r\n");
    }
    return 0;
}
