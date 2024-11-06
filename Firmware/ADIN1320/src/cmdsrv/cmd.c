/*
 *---------------------------------------------------------------------------
 *
 * Copyright (c) 2024 Analog Devices, Inc. All Rights Reserved.
 * This software is proprietary to Analog Devices, Inc.
 * and its licensors.By using this software you agree to the terms of the
 * associated Analog Devices Software License Agreement.
 *
 *
 *---------------------------------------------------------------------------
 */
#include <stdlib.h>
#include "cmdsrv\cmd.h"

/*!
 * @brief           commandAvailable
 *
 * @param [in]      pQ    Pointer to command structure
 * @return
 *                  - 1 if there is command available
 *                  - 0 if no new record
 *
 * @details         This function checks if the UART command is available
 *
 */
uint32_t commandAvailable(cmdQueue_t* pQ)
{
    if (pQ->nWriteCmd != pQ->nReadCmd)
    {
        return 1;
    }
    return 0;
}

/*!
 * @brief           writeCommand
 *
 * @param [in]      pQ    Pointer to command structure
 * @param [in]      commandID ID of the command
 * @param [in]      pCommand Command data
 *
 * @details         Write a new command if there valid data packet from UART
 *
 */
void writeCommand(cmdQueue_t* pQ, uint8_t commandID, uint8_t *pCommand)
{
    pQ->arrCommands[pQ->nWriteCmd].commandID = commandID;
    memcpy(pQ->arrCommands[pQ->nWriteCmd].command, &pCommand[0], MAX_COMMAND_LENGTH);

    pQ->nWriteCmd++;
    pQ->nWriteCmd %= MAX_COMMAND;
}

/*!
 * @brief           readCommand
 *
 * @param [in]      pQ    Pointer to command structure
 * @param [in]      pCommandID ID of the command
 * @param [out]      pCommand Command data
 *
 * @details          Read first command in the Command queue
 *
 */
void readCommand(cmdQueue_t* pQ, uint8_t *pCommandID, uint8_t *pCommand)
{
    *pCommandID = pQ->arrCommands[pQ->nReadCmd].commandID;
    memcpy(&pCommand[0], &pQ->arrCommands[pQ->nReadCmd].command[0], MAX_COMMAND_LENGTH);
    pQ->nReadCmd++;
    pQ->nReadCmd %= MAX_COMMAND;
}

/*!
 * @brief           dataAvailable
 *
 * @param [in]      pQ    Pointer to command structure
 * @return
 *                  - 1 if there is data available
 *                  - 0 if no new record
 *
 * @details         Check is there a difference between write and read pointer
 *
 */

uint32_t dataAvailable(dataQueue_t * pQ)
{
    if (pQ->nWriteDat != pQ->nReadDat)
    {
        return 1;
    }
    return 0;
}

/*!
 * @brief           nbDataAvailable
 *
 * @param [in]      pQ    Pointer to command structure
 * @return
 *                  - difference between wr and rd
 *
 * @details         Returns difference between wr and rd pointers
 *
 */
uint32_t nbDataAvailable(dataQueue_t * pQ)
{
    return abs(pQ->nWriteDat - pQ->nReadDat);
}

/*!
 * @brief           initCmdQueue
 *
 * @param [in]      pQ    Pointer to command structure
 * @param [in]      nIndex Index to the queue
 *
 * @details         Command Queue init
 *
 */
void initCmdQueue(cmdQueue_t* pQ, uint32_t nIndex)
{
    pQ[nIndex].nReadCmd = 0;
    pQ[nIndex].nWriteCmd = 0;
}

/*!
 * @brief           initDataQueue
 *
 * @param [in]      pQ    Pointer to command structure
 * @param [in]      nIndex Index to the queue
 *
 * @details         Data Queue init
 *
 */
void initDataQueue(dataQueue_t* pQ, uint32_t nIndex)
{
    pQ[nIndex].nReadDat = 0;
    pQ[nIndex].nWriteDat = 0;
}

/*!
 * @brief           prepareDataPack
 *
 * @param [in]      pQ    Pointer to command structure
 * @param [in]      buffer buffer to hold the data
 * @param [in]      nbBytes number of bytes in the buffer
 *
 * @details         Prepare the data packets
 *
 */
void prepareDataPack(dataQueue_t * pQ, uint8_t* buffer, int nbBytes)
{
    memcpy(pQ->dataStream[ pQ->nReadDat ].strDataBuffer, buffer, nbBytes);
    pQ->dataStream[ pQ->nReadDat ].nStrDatSize = nbBytes;

    pQ->nWriteDat ++;
    pQ->nWriteDat %= MAX_DATA;
}

/*!
 * @brief           streamData
 *
 * @details         Transmit data to UART
 *
 */
void streamData(dataQueue_t * pQ)
{
	submitTxBuffer(pQ->dataStream[ pQ->nReadDat ].strDataBuffer,  pQ->dataStream[ pQ->nReadDat ].nStrDatSize);

    pQ->nReadDat ++;
    pQ->nReadDat %= MAX_DATA;
}

