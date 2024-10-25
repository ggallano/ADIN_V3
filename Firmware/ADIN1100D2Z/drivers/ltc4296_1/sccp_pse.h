/*
 *---------------------------------------------------------------------------
 *
 * Copyright (c) 2023 Analog Devices, Inc. All Rights Reserved.
 * This software is proprietary to Analog Devices, Inc.
 * and its licensors.By using this software you agree to the terms of the
 * associated Analog Devices Software License Agreement.
 *
 *---------------------------------------------------------------------------
 */

#ifndef SCCP_PSE_H_
#define SCCP_PSE_H_

/** @addtogroup SPOE LTC4296_1 Driver
 *  @{
 */

#include <stdint.h>
#include "spoeLTC4296_1.h"

#define HIGH  1
#define LOW  0

adi_ltc_Result_e sccpReadWritePD(uint8_t addr, uint8_t cmd, uint8_t* buf);
adi_ltc_Result_e sccpResetPulse();
adi_ltc_Result_e sccp_IsPD(uint8_t pseClass, uint16_t sccpResponseData, uint8_t *pdClass);

/**@}*/

#endif /* SCCP_PSE_H_ */



