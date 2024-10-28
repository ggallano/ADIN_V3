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
