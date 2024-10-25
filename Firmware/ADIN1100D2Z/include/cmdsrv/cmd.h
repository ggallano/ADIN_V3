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

#ifndef ADI_CMD_H
#define ADI_CMD_H

#include <string.h>
#include <stdint.h>
#include "bsp\boardsupport.h"

/* Command  headers*/
#define SOP             0x7e    /* Start of packet */

#define CMD_ACK         0xFF    /* General Ack of PC Command */
#define CMD_NACK        0xFE    /* General NotAck of PC Command */
#define CMD_STREAM      0xFD    /* Stream data */
#define CMD_STATUS      0xFC    /* Stream  status data */
#define CMD_ERROR       0xFB    /* Not used in this FW */
#define CMD_SUCCESS     0x00    /* Command successful */

/*Maximun number of received commands pending in a queue*/
#define  MAX_COMMAND 2

/*Maximum incoming command length in bytes*/
#define  MAX_COMMAND_LENGTH 34

/*Maximun number of data streams pending in a queue for transmit*/
#define MAX_DATA 4

/*Maximun data stream length in bytes*/
#define DATA_BUFFER_SIZE 100

typedef struct
{
    uint16_t nStrDatSize; /* total datalength in bytes */
    uint8_t strAnswer;
    uint8_t strDataBuffer[DATA_BUFFER_SIZE];
}data_t;


typedef struct _dataQueue
{
    uint32_t streamCnt;  /* stream counter*/
    int32_t  nReadDat;   /* read index */
    int32_t  nWriteDat;  /* write index */
    data_t   dataStream[MAX_DATA];
} dataQueue_t;


typedef struct _Command
{
    uint8_t commandID;                   /* Command ID */
    uint8_t command[MAX_COMMAND_LENGTH]; /* Command body */
}command_t;

typedef struct _cmdQueue
{
    int32_t nReadCmd;
    int32_t nWriteCmd;
    command_t arrCommands[ MAX_COMMAND ];/*Commands Queue */
} cmdQueue_t;


uint32_t commandAvailable(cmdQueue_t* pQ);
uint32_t dataAvailable(dataQueue_t * pQ);

void writeCommand(cmdQueue_t* pQ, uint8_t commandID, uint8_t *command);
void readCommand(cmdQueue_t* pQ, uint8_t *pCommandID, uint8_t *pCommand );
void streamData(dataQueue_t * pQ);
void prepareDataPack(dataQueue_t * pQ, uint8_t* buffer, int nbBytes);

#endif /*ADI_*/
