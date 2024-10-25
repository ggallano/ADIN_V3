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
#define ADIN1100_INIT_ITER   (5)
#define ADIN1320_PHY_ADDRESS (0)
#define ADIN1300_PHY_ADDRESS (0)

/*! Size of the PHY device structure, in bytes. Needs to be a multiple of 4. */
#define ADI_PHY_DEVICE_SIZE                (48)

/*! Hardware reset value of ADDR_PHY_ID_1 register, used for device identification. */
#define ADI_PHY_DEV_ID1                    (0x0283)
/*! Hardware reset value of ADDR_PHY_ID_2 register (OUI field), used for device identification. */
#define ADI_PHY_DEV_ID2_OUI                (0x2F) // The value read from PHY_ID_2 should be (0xBC20)

/*! Hardware reset value of ADDR_PHY_ID_2 register (MODEL_NUM field), used for device identification. */
#define ADI_PHY_DEV_ID2_MODEL_NUM_ADIN1200  (0x2)

/*! Hardware reset value of ADDR_PHY_ID_2 register (MODEL_NUM field), used for device identification. */
#define ADI_PHY_DEV_ID2_MODEL_NUM_ADIN1300  (0x3)

/*! Hardware reset value of ADDR_PHY_ID_2 register (REV_NUM field), used for device identification. */
#define ADI_PHY_DEV_ID2_REV_NUM             (0x0)

#define ADIN1100_SW_RESET_DELAY             (30) /* ms*/

#define ADIN1200_SW_RESET_DELAY             (10) /* ms*/

/* Delay for ADIN1200 to comeup */
#define ADIN1200_LINK_UP_DELAY              (5000) /* ms*/

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

    uint8_t   adin1100LinkWasDown;                /* Indicates the Link was down */
    uint8_t   adin1100LinkIsUp;                   /* Indicates the Link is up */

    uint8_t   adin1300LinkWasDown;                /* Indicates the Link was down */
    uint8_t   adin1300LinkIsUp;                   /* Indicates the Link is up */

    uint8_t   adin1100MasterEn;                   /* Master enabled/disabled */
    uint8_t   adin1100SlaveEn;                    /* Slave enabled/disabled */
    uint8_t   adin1100TxVLow;                     /* Tx Level Low Status   */
    uint8_t   adin1100TxVHi;                      /* Tx Level High Status */

    uint8_t   adin1100LinkForced;                 /* Link is Forced */
    uint8_t   adin1100AnEn;                       /* Auto negotiation Enable/Disable */

    uint32_t  readMSE;                            /* Read RX MSE on time basis*/
    float     adin1100_mseVal;                    /* MSE Value read */
    float     adin1100_slcrErrMaxAbsErr;          /* Slicer input maximum absolute error */

    uint32_t  frameGenEnabled;                    /* Frame generation enabled */
    uint32_t  uartReport;                         /* Report to UART */
    uint32_t  rxCnt;                              /* Frame count during RX */
    uint32_t  txCnt;                              /* Frame count during TX */
    uint32_t  rxErr;                              /* Error Frame count during RX */
    uint8_t   clrFrameErrors;                     /* Clear running frame err counters */
    uint32_t  startFrameCheck;                    /* Start Frame check*/
    int       uartCommand;                        /* there is UART command*/

    ltc4296_1_boardClass_e fwMode;                /* Config Switches on board */
    ltc4296_1_port_e       ltc4296_1_Port;        /* LTC4296-1 Port (0-4) */
    ltc4296_1_config_e     ltc4296_1_CfgClass;    /* LTC4296-1 Classes */
    ltc4296_1_VI_t         ltc4296_1_VoutIout;    /* variables to hold Vout and Iout from ADC */
    bool                   ltc4296_1_pdPresent;   /* flag for PD present */
}board_t;

extern adin1100_DeviceHandle_t hDevice;
extern adin1100_DeviceStruct_t dev;
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
void systemReset(void);

void setBoardLED(bool en);
void setUartDataAvailable(uint32_t set);
uint32_t getUartDataAvailable(void);
void setUartCmdAvailable(uint32_t set);
uint32_t getUartCmdAvailable(void);

adi_eth_Result_e adin1100_discoverPhy(adi_phy_Device_t *hDevice);
adi_eth_Result_e adin1100_phyReset(adi_phy_Device_t *hDevice);
adi_eth_Result_e adin1100_exitSWPD(adi_phy_Device_t *hDevice);
adi_eth_Result_e adin1100_getSWPD(adi_phy_Device_t *hDevice, unsigned short *enable);
adi_eth_Result_e adin1100_setSWPD(adi_phy_Device_t *hDevice, unsigned short enable);
adi_eth_Result_e adin1100_cfg(board_t *_boardDetails, adi_phy_Device_t *hDevice);
adi_eth_Result_e adin1100_sp_cfg(board_t *_boardDetails, adi_phy_Device_t *hDevice, ltc4296_1_boardClass_e cfgType);
adi_eth_Result_e adin1100_remLoopback(board_t *boardDetails, adi_phy_Device_t *hDevice);
adi_eth_Result_e adin1100_frameGenACheck(board_t *boardDetails, adi_phy_Device_t *hDevice);
adi_eth_Result_e adin1100_StartFrameGen(adi_phy_Device_t *hDevice);
adi_eth_Result_e adin1100_initFrameGen(adi_phy_Device_t *hDevice);
adi_eth_Result_e adin1100_restartFramegen(adi_phy_Device_t *hDevice);
adi_eth_Result_e adin1100_ReadErrPackets(adi_phy_Device_t *hDevice, uint32_t *rxErr, uint32_t *rxCnt);
adi_eth_Result_e adin1100_checkFrameGen(adi_phy_Device_t *hDevice, uint32_t *frmGenDone);
adi_eth_Result_e adin1100_getMseLinkQuality(adi_phy_Device_t *hDevice, adi_phy_MSELinkQuality_t *mseLinkQuality);
adi_eth_Result_e adin1200_cfg(board_t *_boardDetails);
adi_eth_Result_e adin1200_getSWPD(uint8_t phyAddr, unsigned short *enable);
adi_eth_Result_e adin1200_setSWPD(uint8_t phyAddr, unsigned short enable);
adi_eth_Result_e adin1200_phyReset(uint8_t phyAddr);
adi_eth_Result_e adin1200_discoverPhy(board_t *_boardDetails);
adi_eth_Result_e adin1200_remLoopback(board_t *boardDetails);
adi_eth_Result_e adin_phyReset(board_t * boardDetails,adi_phy_Device_t *hDevice);
void adin_phyPrintLinkStatus(board_t *_boardDetails);

adi_eth_Result_e adin1100_EnterSoftwarePowerdown(adin1100_DeviceHandle_t hDevice);
adi_eth_Result_e adin1100_ExitSoftwarePowerdown(adin1100_DeviceHandle_t hDevice);
adi_eth_Result_e adin1100_PhyWrite(adin1100_DeviceHandle_t hDevice, uint32_t regAddr, uint16_t regData);
adi_eth_Result_e adin1100_PhyRead(adin1100_DeviceHandle_t hDevice, uint32_t regAddr, uint16_t *regData);

adi_eth_Result_e phyTest_TxDisabled(adin1100_DeviceHandle_t hDevice);
adi_eth_Result_e phyTest_mode1(adin1100_DeviceHandle_t hDevice);
adi_eth_Result_e phyTest_mode2(adin1100_DeviceHandle_t hDevice);
adi_eth_Result_e phyTest_mode3(adin1100_DeviceHandle_t hDevice);

adi_eth_Result_e applyBoardConfig(board_t *_boardDetails,adi_phy_Device_t *hDevice);
void cyclicBoardLedControl(board_t * boardDetails);
adi_eth_Result_e cyclicReadBoard(board_t *_boardDetails, adi_phy_Device_t *hDevice);
adi_eth_Result_e cyclicLinkStatus(board_t *_boardDetails, adi_phy_Device_t *hDevice);
adi_eth_Result_e cyclicSPOEControl(board_t *_boardDetails);
adi_ltc_Result_e ltc4296_1_cfg(board_t *_boardDetails, ltc4296_1_config_e ltcCfgClass, ltc4296_1_port_e port);

#endif /*AUXFUNC_H*/

/**@}*/

