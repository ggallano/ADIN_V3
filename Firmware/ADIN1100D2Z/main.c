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

#include <stdbool.h>
#include <math.h>
#include "drivers\adinPhy\adi_phy.h"
#include "drivers\adinPhy\adin1100.h"
#include "bsp\boardsupport.h"
#include "aux_functions.h"
#include "cmdsrv\cmd_srv.h"
#include "adi_common.h"

adin1100_DeviceStruct_t              dev;
adin1100_DeviceHandle_t              hDevice = &dev;
board_t                              boardDetails;


/*!
 * @brief           main
 *
 * @details         The main function initializes and configures the system and firmware structures.
 *                  It reads the HW config pins and configures the firmware.
 *                  An Infinite loop which cyclicly handles the PHYLink status, SPOE and APL workflows, LED control
 *                  and UART TX and RX.
 *
 */
int main(void)
{

    uint8_t       run = 1;
    uint8_t       result;

    /****** System Init *****/
    result = BSP_InitSystem();
    if(result != ADI_ETH_SUCCESS)
        printf("BSP_InitSystem Error no - %d\n\r", result);

    configFirmware();
    readBoardConfigPins(&boardDetails);

    setBoardLED(ON);
    TimerDelay_ms(1000);
    printGreetings(&boardDetails);
    setBoardLED(OFF);

    applyBoardConfig(&boardDetails, hDevice->pPhyDevice);

    while(run)
    {
        processUartData();
        processUartCommand();
    }
}


/*!
 * @brief           configures Firmware
 *
 * @details         This functions Initializes the FW structures and
 *                  reads the FW version .
 */
void configFirmware(void)
{
    memset(&boardDetails, 0x00, sizeof(boardDetails));
    int size = sizeof(boardDetails);
    boardDetails.structsize = size;
    boardDetails.adin1300PhyAddr = ADIN1320_PHY_ADDRESS;
    hDevice->pPhyDevice->phyAddr = ADIN1300_PHY_ADDRESS;
    boardDetails.ltc4296_1_VoutIout.ltc4296_1_printVin = TRUE;
    readBoardDetails(&boardDetails);
}

