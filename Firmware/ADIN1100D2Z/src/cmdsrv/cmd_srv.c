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
#include <string.h>
#include <ctype.h>
#include "bsp\boardsupport.h"
#include "drivers\adinPhy\adi_phy.h"
#include "aux_functions.h"
#include "cmdsrv\cmd.h"
#include "cmdsrv\cmd_list.h"
#include "cmdsrv\cmd_aux.h"
#include "cmdsrv\cmd_srv.h"

extern char commandBuffer [100];
static uint8_t SendBuffer[100];

int nbBytes[2] = {0};
char  str1[500] = {0};

extern dataQueue_t dataQueue[1];

/*!
 * @brief           processUartCommand
 *
 * @details         This function calls the processCommand() to process in UART command
 *
 * @sa              processCommand()
 */
void processUartCommand(void)
{
	if(getUartCmdAvailable())
    {
        processCommand(commandBuffer,(char*) &SendBuffer[0], &nbBytes[0]);
        prepareDataPack(&dataQueue[0], SendBuffer, nbBytes[0]);
        setUartCmdAvailable(0);
        setUartDataAvailable(0);
    }
}

/*!
 * @brief           processUartData
 *
 * @details         This function sets the flags once the valid UART command is available.
 *
 */
void processUartData(void)
{
    if(getUartDataAvailable())
    {
        setUartCmdAvailable(1);
        setUartDataAvailable(0);
    }
    if (dataAvailable(&dataQueue[0]))
    {
        streamData(&dataQueue[0]);
    }
}

/*!
 * @brief           processCommand
 *
 * @param [in]      in     UART Command received
 * @param [in]      out    Buffer to hold the data to be Tx on UART
 * @param [out]     fd     length of the output string
 * @return
 *                  - #CMD_ERROR.
 *                  - #CMD_SUCCESS.
 *
 * @details         This functions executes the UART command by calling the callback function.
 *
 * @sa              processUartCommand()
 */
uint32_t processCommand(char *in, char *out, int* fd)
{
    uint32_t ret = CMD_SUCCESS;
    int len = 0;
    uint32_t i = 0;

    char *token, *saveptr1;
    char tempStr[200] =  "\r\n";
    saveptr1 = &tempStr[0];

    do
    {
        token = strtok_r(in, " ", &saveptr1);
        if(token == NULL)
        {
            sprintf(tempStr, "\r\nERROR: Invalid Command\r\n");
            sprintf(out, tempStr);
            len = strlen(out)+1;
            *fd = len;
            return CMD_ERROR;
        }
        if ((strncmp(token, "\n", 1) == 0) || (strncmp(token, "\r", 1) == 0))
        {
            sprintf(tempStr, "\r\nERROR: Invalid Command\r\n");
            sprintf(out, tempStr);
            len = strlen(out)+1;
            *fd = len;
            ret = CMD_SUCCESS;
            return (ret);
        }
        if ((strstr(token, "//") != NULL))// comment line
        {
            snprintf(out, 100, "%s %s", token, "\r\n");
            len = strlen(out)+1;
            *fd = len;
            ret = CMD_SUCCESS;
            return (ret);
        }
        for (i = 0; token[i] > 0; i++)
        {
            token[i] = tolower(token[i]);
        }
        for (i = 0; i < getAuxFuncSize(); i++)
        {
            if (strstr(token, aux_commands[i].cmd) != NULL)
            {
                boardDetails.uartCommand = 4;
                tempStr[0] = '\0'; /* init response to a null string */
                aux_commands[i].callback(saveptr1, tempStr, *fd);
                sprintf(out, tempStr);
                len = strlen(out)+1;
                *fd = len;
                ret = CMD_SUCCESS;
                return (ret);
            }
        }
    }
    while(0);
    return (ret);
}

