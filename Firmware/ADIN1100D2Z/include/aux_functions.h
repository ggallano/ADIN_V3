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

#ifndef AUXFUNC_H
#define AUXFUNC_H

/** @addtogroup aux Auxiliary Functions
 *  @{
 */

#include <stdint.h>
#include <stdbool.h>
#include <stdio.h>
#include "..\drivers\ltc4296_1\spoeLTC4296_1.h"
#include "drivers\adinPhy\adin1100.h"
#include "platform\adi_eth_error.h"

/*
* Driver Version
*/
#define ADIN1320_INIT_ITER   (5)
#define ADIN1320_PHY_ADDRESS (0)
#define ADIN1300_PHY_ADDRESS (0)

/*! Size of the PHY device structure, in bytes. Needs to be a multiple of 4. */
#define ADI_PHY_DEVICE_SIZE                (48)

/*! Hardware reset value of ADDR_PHY_ID_1 register, used for device identification. */
#define ADI_PHY_DEV_ID1                    (0x0283)
/*! Hardware reset value of ADDR_PHY_ID_2 register (OUI field), used for device identification. */
#define ADI_PHY_DEV_ID2_OUI                (0x2F) // The value read from PHY_ID_2 should be (0xBC30)

/*! Hardware reset value of ADDR_PHY_ID_2 register (MODEL_NUM field), used for device identification. */
#define ADI_PHY_DEV_ID2_MODEL_NUM_ADIN1300  (0x3)

/*! Hardware reset value of ADDR_PHY_ID_2 register (REV_NUM field), used for device identification. */
#define ADI_PHY_DEV_ID2_REV_NUM             (0x0)

#define ADIN1320_SW_RESET_DELAY             (30) /* ms*/

#define ADIN1300_SW_RESET_DELAY             (10) /* ms*/

/* Delay for ADIN1200 to comeup */
#define ADIN1300_LINK_UP_DELAY              (5000) /* ms*/

#define SIZE_OF_BUFFER 1

typedef struct
{
    uint16_t structsize;

    uint8_t   hwType[100];                        /* hardware type */
    uint32_t  fwBuild;                            /* firmware Major and Minor version */
    uint32_t  fwVersion;                          /* firmware build version */
    uint8_t   adin1300PhyAddr;                    /* adin1200 PHY address */
    uint8_t   errorLed;                           /* Red Led flag setup for errors which are corrected only on reset. */
    uint8_t   tempErrorLed;                       /* RED Led flag setup for temporary errors which are recovered in the next one-sec cycle */
    uint8_t   blueLed;                            /* Blue Led flag set when production power test is passed. */
    uint8_t   linkled;                            /* Green led to show link is up */

    int       uartCommand;                        /* there is UART command*/

    ltc4296_1_boardClass_e fwMode;                /* Config Switches on board */
    ltc4296_1_port_e       ltc4296_1_Port;        /* LTC4296-1 Port (0-4) */
    ltc4296_1_config_e     ltc4296_1_CfgClass;    /* LTC4296-1 Classes */
    ltc4296_1_VI_t         ltc4296_1_VoutIout;    /* variables to hold Vout and Iout from ADC */
    bool                   ltc4296_1_pdPresent;   /* flag for PD present */
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

adi_eth_Result_e adin1320_discoverPhy(adi_phy_Device_t *hDevice);
adi_eth_Result_e adin1300_discoverPhy(board_t *_boardDetails);
adi_eth_Result_e adin1320_phyReset(adi_phy_Device_t *hDevice);
adi_eth_Result_e adin1300_phyReset(uint8_t phyAddr);
adi_eth_Result_e adin1320_getSWPD(adi_phy_Device_t *hDevice, unsigned short *enable);
adi_eth_Result_e adin1320_setSWPD(adi_phy_Device_t *hDevice, unsigned short enable);
adi_eth_Result_e adin1300_getSWPD(uint8_t phyAddr, unsigned short *enable);
adi_eth_Result_e adin1300_setSWPD(uint8_t phyAddr, unsigned short enable);
adi_eth_Result_e adin1320_cfg(board_t *_boardDetails, adi_phy_Device_t *hDevice);
adi_eth_Result_e adin1300_cfg(board_t *_boardDetails);

adi_eth_Result_e applyBoardConfig(board_t *_boardDetails,adi_phy_Device_t *hDevice);
adi_ltc_Result_e ltc4296_1_cfg(board_t *_boardDetails, ltc4296_1_config_e ltcCfgClass, ltc4296_1_port_e port);

void setBoardLED(bool en);
void setUartDataAvailable(uint32_t set);
uint32_t getUartDataAvailable(void);
void setUartCmdAvailable(uint32_t set);
uint32_t getUartCmdAvailable(void);

#endif /*AUXFUNC_H*/

/**@}*/

