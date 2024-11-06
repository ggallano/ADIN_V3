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

#ifndef ADIN1320_H
#define ADIN1320_H

/** @addtogroup adin1320 ADIN1320 PHY Driver
 *  @{
 */

#include "adi_phy.h"

/*! ADIN1320 driver major version. */
#define ADIN1320_VERSION_MAJOR      (0)
/*! ADIN1320 driver minor version. */
#define ADIN1320_VERSION_MINOR      (1)
/*! ADIN1320 driver patch version. */
#define ADIN1320_VERSION_PATCH      (0)
/*! ADIN1320 driver extra version. */
#define ADIN1320_VERSION_EXTRA      (0)

/*! ADIN1320 driver version. */
#define ADIN1320_VERSION            ((ADIN1320_VERSION_MAJOR << 24) | \
                                     (ADIN1320_VERSION_MINOR << 16) | \
                                     (ADIN1320_VERSION_PATCH << 8) | \
                                     (ADIN1320_VERSION_EXTRA))


/*!
* @brief ADIN1320 device identification.
*/
typedef struct
{
    union {
        struct {
            uint32_t    revNum      : 4;    /*!< Revision number.           */
            uint32_t    modelNum    : 6;    /*!< Model number.              */
            uint32_t    oui         : 22;   /*!< OUI.                       */
        };
        uint32_t phyId;
    };
    uint16_t        digRevNum;              /*!< Digital revision number.   */
    uint8_t         pkgType;                /*!< Package type.              */
} adin1320_DeviceId_t;

/*!
* @brief ADIN1320 device structure.
*/
typedef struct
{
    adi_phy_Device_t    *pPhyDevice;    /*!< Pointer to the PHY device structure.   */
} adin1320_DeviceStruct_t;


/*!
* @brief ADIN1320 device structure handle.
*/
typedef adin1320_DeviceStruct_t*    adin1320_DeviceHandle_t;

/**@}*/

#endif /* ADIN1320_H */
