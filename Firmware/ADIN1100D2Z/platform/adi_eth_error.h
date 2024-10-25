/*
 *---------------------------------------------------------------------------
 *
 * Copyright (c) 2020, 2021 Analog Devices, Inc. All Rights Reserved.
 * This software is proprietary to Analog Devices, Inc.
 * and its licensors.By using this software you agree to the terms of the
 * associated Analog Devices Software License Agreement.
 *
 *---------------------------------------------------------------------------
 */

#ifndef ADI_ETH_COMMON_H
#define ADI_ETH_COMMON_H


/*!
* @brief Status codes for the Ethernet devices.
*/
typedef enum
{

    ADI_ETH_SUCCESS,                    /*!< Success.                                                   */
    ADI_ETH_MDIO_TIMEOUT,               /*!< MDIO timeout.                                              */
    ADI_ETH_COMM_ERROR,                 /*!< Communication error.                                       */
    ADI_ETH_COMM_ERROR_SECOND,          /*!< Communication error.                                       */
    ADI_ETH_COMM_TIMEOUT,               /*!< Communications timeout with the host.                      */
    ADI_ETH_UNSUPPORTED_DEVICE,         /*!< Unsupported device.                                        */
    ADI_ETH_DEVICE_UNINITIALIZED,       /*!< Device not initialized.                                    */
    ADI_ETH_HW_ERROR,                   /*!< Hardware error.                                            */
    ADI_ETH_INVALID_PARAM,              /*!< Invalid parameter.                                         */
    ADI_ETH_PARAM_OUT_OF_RANGE,         /*!< Parameter out of range.                                    */
    ADI_ETH_INVALID_HANDLE,             /*!< Invalid device handle.                                     */
    ADI_ETH_IRQ_PENDING,                /*!< Interrupt request is pending.                              */
    ADI_ETH_READ_STATUS_TIMEOUT,        /*!< Timeout when reading status registers.                     */
    ADI_ETH_INVALID_POWER_STATE,        /*!< Invalid power state.                                       */
    ADI_ETH_SPI_ERROR,                  /*!< SPI error.                                                 */
    ADI_ETH_SW_RESET_TIMEOUT,           /*!< Software reset timeout.                                    */
    ADI_ETH_NOT_IMPLEMENTED,            /*!< Not implemented in hardware.                               */
    ADI_ETH_NOT_IMPLEMENTED_SOFTWARE,   /*!< Not implemented in software.                               */
    ADI_ETH_UNSUPPORTED_FEATURE,        /*!< Hardware feature not supported by the software driver.     */
} adi_eth_Result_e;


/*!
* @brief Callback function definition for the Ethernet devices.
*/
typedef void (* adi_eth_Callback_t) (
    void      *pCBParam,                /*!< Client-supplied callback parameter. */
    uint32_t   Event,                   /*!< Event ID specific to the Driver/Service. */
    void      *pArg                     /*!< Pointer to the event-specific argument. */

    );


extern char adi_eth_result_string[20][80];

#endif /* ADI_ETH_COMMON_H */

