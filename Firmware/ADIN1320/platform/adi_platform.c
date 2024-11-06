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

#include <stdint.h>
#include "adi_platform.h"
#include "bsp\boardsupport.h"

#define MMD_ACR_ADDRESS                 (0x0000U)
#define MMD_ACR_DATA 		            (0x4000U)
#define MMD_ACR_DATA_RW_INCREMENT       (0x8000U)
#define MMD_ACR_DATA_WRITE_INCREMENT    (0xC000U)

#define DEVTYPE(a)         (a >> 16)
#define REGADDR(a)         (a & 0xFFFF)

/*
 * @brief          adi_MdioRead
 *
 * @param [in]      phyAddr - Hardware PHY address
 * @param [in]      phyReg  - Register address
 * @param [out]     phyData - pointer to the data buffer
 * @return
 *               - error if TA bit is not pulled down by the slave
 *
 * @details   This function performs MDIO Read Clause 22 to the PHY device
 *
 * @sa        adi_MdioWrite()
 */
uint32_t adi_MdioRead(uint8_t phyAddr, uint8_t phyReg, uint16_t *phyData)
{
    uint32_t error = 0;
    int32_t i = 0;
    uint8_t tempBuffer[8] = {0};


    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;

    /* ST('01) + OP('10) + PHYAD(4 MSB) */
    tempBuffer[i] = 0x60;
    tempBuffer[i++] |= (phyAddr & 0x1F) >> 1;

    /* PHYAD(1 LSB) + REGADD (4) + 2 TA('00) */
    tempBuffer[i] = (phyAddr & 0x01) << 7 ;
    tempBuffer[i] |= (phyReg & 0x1F) << 2;
    tempBuffer[i++] |= ( 0x03) ;//3
    tempBuffer[i++] = 0xFF;
    tempBuffer[i++] = 0xFF;

    spi0_send_rcv(&tempBuffer[0], 8, &tempBuffer[0]);
    if((tempBuffer[5] & 0x01) == 0x01)
    {
        /* Communication error, MDIO slave doesn't respond */
        error = 1;
    }

    *phyData = ( (tempBuffer[6] << 8) | tempBuffer[7] );

    return error;
}

/*
 * @brief         adi_MdioWrite
 *
 * @param [in]    phyAddr - Hardware PHY address
 * @param [in]    phyReg  - Register address
 * @param [out]   phyData - data to write to register
 * @return        none
 *
 * @details       This function performs MDIO Write Clause22 to the PHY device
 *
 * @sa            adi_MdioRead()
 */
uint32_t adi_MdioWrite(uint8_t phyAddr, uint8_t phyReg, uint16_t phyData)
{
    int32_t i = 0;
    uint8_t tempBuffer[8] = {0};

    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;

    /*Preamble*/
    /* ST('01) + OP('01) + PHYAD(4 MSB) */
    tempBuffer[i] = 0x50 ;
    tempBuffer[i++] |= (phyAddr & 0x1F) >> 1;

    /* PHYAD(1 LSB) + REGADD (4) + 2 TA('10) */
    tempBuffer[i] = (phyAddr & 0x01) << 7 ;
    tempBuffer[i] |= (phyReg & 0x1F) << 2;
    tempBuffer[i++] |=  0x2 ;

    /* DATA */
    tempBuffer[i++] = phyData >> 8;
    tempBuffer[i++] = phyData & 0xFF;

    spi0_send_rcv(&tempBuffer[0], 8,  &tempBuffer[0]);
    return 0;
}


/*
 * @brief         mdioRead45Clause
 *
 * @param [in]    phyAddr - Hardware PHY address
 * @param [in]    devType - Device Type
 * @param [out]   phyData - pointer to data buffer
 * @return        error if TA bit is not pulled down by the slave
 *
 * @details       Helping function MDIO Read Clause45
 *
 * @sa            adi_MdioRead_Cl45()
 */
static uint32_t mdioRead45Clause(uint8_t phyAddr, uint8_t devType, uint16_t *phyData)
{
    uint32_t error = 0;
    int32_t i = 0;
    uint8_t tempBuffer[8] = {0};

    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;

    /* ST('00) + OP('11) + PHYAD(4 MSB) */
    tempBuffer[i] = 0x30;
    tempBuffer[i++] |= (phyAddr & 0x1F) >> 1;

    /* PHYAD(1 LSB) + DEVTYPE (4) + 2 TA('00) */
    tempBuffer[i] = (phyAddr & 0x01) << 7 ;
    tempBuffer[i] |= (devType & 0x1F) << 2;
    tempBuffer[i++] |= ( 0x03) ;
    tempBuffer[i++] = 0xFF;
    tempBuffer[i++] = 0xFF;

    spi0_send_rcv(&tempBuffer[0], 8,  &tempBuffer[0]);
    if((tempBuffer[5] & 0x01) == 0x01)
    {
        /* Communication error, MDIO slave doesn't respond */
        error = 1;
    }
    *phyData = (tempBuffer[6] << 8) | tempBuffer[7];

    return error;
}

/*
 * @brief        mdioWrite45Clause
 *
 * @param [in]   phyAddr - Hardware PHY address
 * @param [in]   devType - Device Type
 * @param [out]  phyData - data to write
 * @return       none
 *
 * @details      Helping function MDIO Write Clause45
 *
 * @sa           adi_MdioWrite_Cl45()
 */
static uint32_t mdioWrite45Clause(uint8_t phyAddr, uint8_t devType, uint16_t phyData)
{
    int32_t i = 0;
    uint8_t tempBuffer[8] = {0};

    /*Preamble*/
    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;

    /* ST('00) + OP('01) + PHYAD(4 MSB) */
    tempBuffer[i] = 0x10 ;
    tempBuffer[i++] |= (phyAddr & 0x1F) >> 1;

    /* PHYAD(1 LSB) + DEVTYPE (4) + 2 TA('10) */
    tempBuffer[i] = (phyAddr & 0x01) << 7 ;
    tempBuffer[i] |= (devType & 0x1F) << 2;
    tempBuffer[i++] |=  0x2 ;

    /* DATA */
    tempBuffer[i++] = phyData >> 8;
    tempBuffer[i++] = phyData & 0xFF;

    spi0_send_rcv(&tempBuffer[0], 8,  &tempBuffer[0]);

    return 0;
}

/*
 * @brief          mdioAddr45Clause
 *
 * @param [in]     phyAddr - Hardware PHY address
 * @param [in]     devType -  Device Type
 * @param [in]     regAddr - Register Address
 * @return         none
 *
 * @details         Helping function MDIO Writing the Device Type and Register Address Clause45
 *
 * @sa              adi_MdioRead_Cl45(),  adi_MdioWrite_Cl45()
 */
static uint32_t mdioAddr45Clause(uint8_t phyAddr, uint8_t devType, uint16_t regAddr)
{
    int32_t i = 0;
    uint8_t tempBuffer[8] = {0};

    /*Preamble*/
    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;
    tempBuffer[i++] = 0xff;

    /* ST('00) + OP('00) + PHYAD(4 MSB) */
    tempBuffer[i] = 0x00 ;
    tempBuffer[i++] |= (phyAddr & 0x1F) >> 1;

    /* PHYAD(1 LSB) + DEVTYPE (4) + 2 TA('10) */
    tempBuffer[i] = (phyAddr & 0x01) << 7 ;
    tempBuffer[i] |= (devType & 0x1F) << 2;
    tempBuffer[i++] |=  0x2 ;

    /* DATA */
    tempBuffer[i++] = regAddr >> 8;
    tempBuffer[i++] = regAddr & 0xFF;

    spi0_send_rcv(&tempBuffer[0], 8,  &tempBuffer[0]);

    return 0;
}

/*
 * @brief           adi_MdioRead_Cl45
 *
 * @param [in]      phyAddr - Hardware PHY address
 * @param [in]      phyReg - Register address in clause 45 combined devType and regAddr
 * @param [out]     phyData - pointer to the data buffer
 * @return          error if TA bit is not pulled down by the slave
 *
 * @details         This function does MDIO Read Clause45
 *
 * @sa              adi_MdioWrite_Cl45()
 */
uint32_t adi_MdioRead_Cl45(uint8_t phyAddr, uint32_t phyReg, uint16_t *phyData)
{
    /*Applying the device type */
    mdioAddr45Clause(phyAddr, DEVTYPE(phyReg), REGADDR(phyReg));

    /*Reading from register address*/
    return mdioRead45Clause(phyAddr, DEVTYPE(phyReg), phyData);
}

/*
 * @brief             adi_MdioWrite_Cl45
 *
 * @param [in]        phyAddr - Hardware PHY address
 * @param [in]        phyReg  - Register address in clause 45 combined devAddr and regAddr
 * @param [out]       phyData - data
 * @return            none
 *
 * @details           This function does MDIO Write Clause45
 *
 * @sa                adi_MdioRead_Cl45()
 */
uint32_t adi_MdioWrite_Cl45(uint8_t phyAddr, uint32_t phyReg, uint16_t phyData)
{
    /*Applying the device type*/
    mdioAddr45Clause(phyAddr, DEVTYPE(phyReg), REGADDR(phyReg));

    /*Writing data to register address*/
    return mdioWrite45Clause(phyAddr, DEVTYPE(phyReg), phyData);
}

