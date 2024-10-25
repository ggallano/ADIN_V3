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

/** @addtogroup aux Auxiliary Functions
 *  @{
 */

#ifndef ADI_PLATFORM_H
#define ADI_PLATFORM_H

#include <stdint.h>

uint32_t adi_MdioRead(uint8_t phyAddr, uint8_t phyReg, uint16_t *phyData);
uint32_t adi_MdioWrite(uint8_t phyAddr, uint8_t phyReg, uint16_t phyData);

uint32_t adi_MdioRead_Cl22(uint8_t phyAddr, uint32_t phyReg, uint16_t *phyData);
uint32_t adi_MdioWrite_Cl22(uint8_t phyAddr, uint32_t phyReg, uint16_t phyData);

uint32_t adi_MdioRead_Cl45(uint8_t phyAddr, uint32_t phyReg, uint16_t *phyData);
uint32_t adi_MdioWrite_Cl45(uint8_t phyAddr, uint32_t phyReg, uint16_t phyData);

#endif /* ADI_PLATFORM_H */

/**@}*/
