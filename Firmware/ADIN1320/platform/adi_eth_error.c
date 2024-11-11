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
#include "adi_eth_error.h"

/*!
* @brief Status codes for the PHY devices.
*/

char adi_eth_result_string[20][80] = {

		   "Success !!  ",                                             /*!< Success.                                                   */
		   "MDIO timeout. ",                                           /*!< MDIO timeout.                                              */
		   "Communication error. ",                                    /*!< Communication error.                                       */
		   "Communication error. ",                                    /*!< Communication error.                                       */
		   "Communications timeout with the host. ",                   /*!< Communications timeout with the host.                      */
		   "Unsupported device.  ",                                    /*!< Unsupported device.                                        */
		   "Device not initialized.  ",                                /*!< Device not initialized.                                    */
		   "Hardware error.  ",                                        /*!< Hardware error.                                            */
		   "Invalid parameter. ",                                      /*!< Invalid parameter.                                         */
		   "Parameter out of range. ",                                 /*!< Parameter out of range.                                    */
		   "Invalid device handle.  ",                                 /*!< Invalid device handle.                                     */
		   "Interrupt request is pending. ",                           /*!< Interrupt request is pending.                              */
		   "Timeout when reading status registers. ",                  /*!< Timeout when reading status registers.                     */
		   "Invalid power state. ",                                    /*!< Invalid power state.                                       */
		   "SPI error. ",                                              /*!< SPI error.                                                 */
		   "Software reset timeout.  ",                                /*!< Software reset timeout.                                    */
		   "Not implemented in hardware.  ",                           /*!< Not implemented in hardware.                               */
		   "Not implemented in software.  ",                           /*!< Not implemented in software.                               */
		   "Hardware feature not supported by the software driver. ",  /*!< Hardware feature not supported by the software driver.     */
		   "Unassigned (placeholder) error.  "                         /*!< Unassigned (placeholder) error.                            */
};

