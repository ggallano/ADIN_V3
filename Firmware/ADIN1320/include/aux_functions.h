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

#ifndef AUXFUNC_H
#define AUXFUNC_H

/** @addtogroup aux Auxiliary Functions
 *  @{
 */

#include <stdint.h>
#include <stdbool.h>
#include <stdio.h>
#include "drivers\adinPhy\adin1320.h"
#include "platform\adi_eth_error.h"

/*
* Driver Version
*/
#define ADIN1320_INIT_ITER   (5)
#define ADIN1320_PHY_ADDRESS (0)
#define ADIN1300_PHY_ADDRESS (4)

/*! Size of the PHY device structure, in bytes. Needs to be a multiple of 4. */
#define ADI_PHY_DEVICE_SIZE                (48)

/*! Hardware reset value of ADDR_PHY_ID_1 register, used for device identification. */
#define ADI_PHY_DEV_ID1                    (0x0283)
/*! Hardware reset value of ADDR_PHY_ID_2 register (OUI field), used for device identification. */
#define ADI_PHY_DEV_ID2_OUI                (0x2F) // The value read from PHY_ID_2 should be (0xBC30)

#define ADI_SW_RESET_DELAY             		(10) /* ms*/

#define SIZE_OF_BUFFER 1

typedef enum
{
  PHY_TESTMODE_0 = 0,
  PHY_TESTMODE_1,
  PHY_TESTMODE_2,
  PHY_TESTMODE_3,
  RESERVED_4,
  RESERVED_5,
  RESERVED_6,
  INTERACTIVEMODE,
  MAC_REMOTELB,
  FRAMEGENCHECK,
  RESERVED_A,
  RESERVED_B,
  RESERVED_C,
  MEDCONV_CU_SGMII,
  MEDCONV_CU_FI,
  MEDCONV_CU_CU
}firmwareMode_e;

typedef struct
{
    uint16_t structsize;

    uint8_t   hwType[100];                        /* hardware type */
    uint32_t  fwBuild;                            /* firmware Major and Minor version */
    uint32_t  fwVersion;                          /* firmware build version */
    uint8_t   adin1300PhyAddr;                    /* adin1300 PHY address */
    uint8_t   tempErrorLed;                       /* RED Led flag setup for temporary errors which are recovered in the next one-sec cycle */
    uint8_t   blueLed;                            /* Blue Led flag set when production power test is passed. */
    uint8_t   linkled;                            /* Green led to show link is up */

    int       uartCommand;                        /* there is UART command*/

    firmwareMode_e fwMode;                		  /* Config Switches on board */
}board_t;

extern adin1320_DeviceHandle_t hDevice;
extern adin1320_DeviceStruct_t dev;
extern board_t boardDetails;
extern uint8_t strBuf[100];
extern volatile int strCounter;
extern char commandBuffer [100];
extern uint8_t nBufferRx0[];

void configFirmware(void);
void readBoardDetails(board_t *_boardDetails);
void printMode(uint8_t mode);
void printGreetings(board_t *_boardDetails);
void readBoardConfigPins(board_t *_boardDetails);

adi_eth_Result_e discoverPhy(uint8_t phyAddr);
adi_eth_Result_e phyReset(uint8_t phyAddr);
adi_eth_Result_e getSWPD(uint8_t phyAddr, unsigned short *enable);
adi_eth_Result_e setSWPD(uint8_t phyAddr, unsigned short enable);
adi_eth_Result_e generalCfg(uint8_t phyAddr);

adi_eth_Result_e applyBoardConfig(board_t *_boardDetails,adi_phy_Device_t *hDevice);

void setBoardLED(bool en);
//void setGpio_LedGreen(bool en);
//void setGpio_LedRed(bool en);
//void setGpio_LedYellow(bool en);
//void setGpio_LedBlue(bool en);
//uint8_t readGpio_ConfigPins(void);
void read_ConfigToLed(void);
void setUartDataAvailable(uint32_t set);
uint32_t getUartDataAvailable(void);
void setUartCmdAvailable(uint32_t set);
uint32_t getUartCmdAvailable(void);

#endif /*AUXFUNC_H*/

/**@}*/

