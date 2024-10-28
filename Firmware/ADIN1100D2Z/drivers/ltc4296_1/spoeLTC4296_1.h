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

/** @addtogroup SPOE LTC4296_1 Driver
 *  @{
 */

#ifndef SPOELTC_H
#define SPOELTC_H

#include <stdint.h>
#include <stdbool.h>

/* LTC4296 various (datasheet) definitions*/
#define LTC4296_1_MAX_PORTS 5

#define LTC4296_1_LOCK_KEY     0x00A0
#define LTC4296_1_UNLOCK_KEY   0x0005
#define LTC4296_1_VGAIN        0.035230
#define LTC4296_1_IGAIN        0.1

#define LTC4296_1_VMIN         0
#define LTC4296_1_VMAX         1

#define RTESTLOAD              200 /*(ohm)*/

//typedef enum
//{
//  SPOE_CLASS10 = 0,
//  SPOE_CLASS11,
//  SPOE_CLASS12,
//  SPOE_CLASS13,
//  SPOE_CLASS14,
//  SPOE_CLASS15,
//  APL_CLASSA,
//  APL_CLASSA_NOAUTONEG,
//  APL_CLASSC,
//  APL_CLASS3,
//  PRODUCTION_POWER_TEST,
//  APL_CLASSA_OLD_DEMO,
//  SPOE_OFF,
//  PRODUCTION_DATA_TEST,
//  RESERVED,
//  DEBUGMODE
//}ltc4296_1_boardClass_e;

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
}ltc4296_1_boardClass_e;


typedef enum
{
	LOW_CKT_BRK_FAULT       = (1 << 0),
	MEMORY_FAULT 			= (1 << 1),
	PEC_FAULT 				= (1 << 2),
	COMMAND_FAULT 		    = (1 << 3),
	UVLO_DIGITAL			= (1 << 4)
} ltc4296_gevents_e;

typedef enum
{
	/* LTC4296 datasheet definitions - port events register */
	LSNS_REVERSE_FAULT 					= (1 << 0),
	SNS_FORWARD_FAULT 					= (1 << 1),
	PD_WAKEUP 							= (1 << 2),
	TINRUSH_TIMER_DONE 					= (1 << 3),
	MFVS_TIMEOUT						= (1 << 4),
	OVERLOAD_DETECTED_IPOWERED	        = (1 << 5),
	OVERLOAD_DETECTED_ISLEEP 		    = (1 << 6),
	TOFF_TIMER_DONE 					= (1 << 7),
	INVALID_SIGNATURE 					= (1 << 8),
	VALID_SIGNATURE						= (1 << 9)
} ltc4296_1_pevents_e;

typedef enum
{
    LTC_CFG_SCCP_MODE = 0,
	LTC_CFG_APL_MODE,
	LTC_CFG_RESET
}ltc4296_1_config_e;


typedef enum
{
    LTC_PORT0 = 0,
	LTC_PORT1,
	LTC_PORT2,
	LTC_PORT3,
	LTC_PORT4,
	LTC_NO_PORT
}ltc4296_1_port_e;

typedef enum
{
    LTC_PORT_EVENTS   = 0,
	LTC_PORT_STATUS   = 2,
	LTC_PORT_CFG0     = 3,
	LTC_PORT_CFG1     = 4,
	LTC_PORT_ADCCFG   = 5,
	LTC_PORT_ADCDAT   = 6,
	LTC_PORT_SELFTEST = 7
}ltc4296_1_portRegOffset_e;



typedef enum
{
    LTC_UNLOCKED = 0,
    LTC_LOCKED
}ltc4296_1_state_e;


typedef struct
{
	float ltc4296_1_Vin;
	float ltc4296_1_Vout;
	float ltc4296_1_Iout;
	bool  ltc4296_1_printVin;
}ltc4296_1_VI_t;

typedef enum
{
    LTC_PORT_DISABLED = 0,
    LTC_PORT_ENABLED
}ltc4296_1_portStatus_e;

/* PSE status decoded as below */
typedef enum {
	LTC_PSE_STATUS_DISABLED = 0,      /*  000b � Port is disabled                */
	LTC_PSE_STATUS_SLEEPING,          /*  001b � Port is in sleeping             */
	LTC_PSE_STATUS_DELIVERING,        /*  010b � Port is delivering power        */
	LTC_PSE_STATUS_SEARCHING,         /*  011b � Port is searching               */
	LTC_PSE_STATUS_ERROR,             /*  100b � Port is in error                */
	LTC_PSE_STATUS_IDLE,              /*  101b � Port is idle                    */
 	LTC_PSE_STATUS_PREPDET,           /*  110b � Port is preparing for detection */
	LTC_PSE_STATUS_UNKNOWN            /*  111b � Port is in an unknown state     */
} ltc4296_1_PSEStatus_e;

/*!
* @brief Status codes for the LTC4296-1 devices.
*/
typedef enum
{
	ADI_LTC_SUCCESS = 0,                  /*!< Success                                                    */
	ADI_LTC_DISCONTINUE_SCCP,             /*!< Discontinue the SCCP configuration cycle.                  */
	ADI_LTC_SCCP_COMPLETE,                /*!< Complete SCCP configuration cycle.                         */
	ADI_LTC_SCCP_PD_DETECTION_FAILED,     /*!< PD Detection failed                                        */
	ADI_LTC_SCCP_PD_NOT_PRESENT,          /*!< SCCP  PD not present                                       */
	ADI_LTC_SCCP_PD_RES_INVALID,          /*!< PD Response is invalid                                     */
	ADI_LTC_SCCP_PD_PRESENT,              /*!< PD is present.                                             */
	ADI_LTC_SCCP_PD_CLASS_COMPATIBLE,     /*!< PD Class is compatible                                     */
	ADI_LTC_SCCP_PD_CLASS_NOT_SUPPORTED,  /*!< PD Class is out of range                                   */
	ADI_LTC_SCCP_PD_CLASS_NOT_COMPATIBLE, /*!< PD Class is not compatible                                 */
	ADI_LTC_SCCP_PD_LINE_NOT_HIGH,        /*!< PD line has not gone HIGH                                  */
	ADI_LTC_SCCP_PD_LINE_NOT_LOW,         /*!< PD line has not gone LOW                                   */
	ADI_LTC_SCCP_PD_CRC_FAILED,           /*!< CRC received from PD is incorrect                          */
	ADI_LTC_APL_COMPLETE,                 /*!< Complete APL configuration cycle.                          */
	ADI_LTC_DISCONTINUE_APL,              /*!< Discontinue the APL configuration cycle.                   */
	ADI_LTC_INVALID_ADC_VOLTAGE,          /*!< Invalid ADC Accumulation result.                           */
	ADI_LTC_INVALID_ADC_PORT_CURRENT,     /*!< Invalid ADC Port Current                                   */
	ADI_LTC_TEST_COMPLETE,                /*!< LTC Test complete.                                         */
	ADI_LTC_DISCONTINUE_TEST,             /*!< LTC Discontinue Test.                                      */
	ADI_LTC_TEST_FAILED,                  /*!< LTC Test Failed.                                           */
	ADI_LTC_INVALID_VIN                   /*!< VIN is invalid                                             */
} adi_ltc_Result_e;

void ltc4296_1_printGlobalfaults(uint16_t gEvents);
void ltc4296_1_printPortEvents(ltc4296_1_port_e portNo, uint16_t portEvents);

adi_ltc_Result_e ltc4296_1_read(uint8_t register_address, uint16_t *value);
adi_ltc_Result_e ltc4296_1_write(uint8_t register_address, uint16_t value);

adi_ltc_Result_e ltc4296_1_doAPL(ltc4296_1_boardClass_e boardClass, ltc4296_1_port_e ltc4296_1_Port,ltc4296_1_VI_t *ltc4296_1_VI);
adi_ltc_Result_e ltc4296_1_doSpoeSccp(ltc4296_1_boardClass_e boardClass, ltc4296_1_port_e ltc4296_1_Port, ltc4296_1_VI_t *ltc4296_1_VI);
adi_ltc_Result_e ltc4296_1_chkGlobalEvents(void);
adi_ltc_Result_e ltc4296_1_chkPortEvents( ltc4296_1_port_e ltc4296_1_Port);

adi_ltc_Result_e ltc4296_1_reset(void);
adi_ltc_Result_e ltc4296_1_unlock(void);
adi_ltc_Result_e ltc4296_1_isLocked(ltc4296_1_state_e *state);

adi_ltc_Result_e ltc4296_1_readGlobalfaults(uint16_t *gEvents);
adi_ltc_Result_e ltc4296_1_clearGlobalfaults(void);
adi_ltc_Result_e ltc4296_1_clearCktBreaker(void);
adi_ltc_Result_e ltc4296_1_ReadGADC(float *portVout);
adi_ltc_Result_e ltc4296_1_SetGADCVin(void);
adi_ltc_Result_e ltc4296_1_isVinValid(float portVIn, ltc4296_1_boardClass_e ltcboardClass, bool *VinValid);


adi_ltc_Result_e ltc4296_1_clearPortEvents(ltc4296_1_port_e portNo);
adi_ltc_Result_e ltc4296_1_readPortEvents(ltc4296_1_port_e portNo, uint16_t *portEvents);
adi_ltc_Result_e ltc4296_1_readPortStatus(ltc4296_1_port_e portNo, uint16_t *portStatus);
adi_ltc_Result_e ltc4296_1_IsPortDisabled(ltc4296_1_port_e portNo, ltc4296_1_portStatus_e *portChk);
adi_ltc_Result_e ltc4296_1_PortDisable(ltc4296_1_port_e portNo);
adi_ltc_Result_e ltc4296_1_IsPortDeliverPwr(ltc4296_1_port_e portNo, ltc4296_1_PSEStatus_e *pwrStatus);
adi_ltc_Result_e ltc4296_1_IsPortPwrStable(ltc4296_1_port_e portNo, bool *pwrStatus);

adi_ltc_Result_e ltc4296_1_ReadPortADC(ltc4296_1_port_e portNo, float *portIout);
adi_ltc_Result_e ltc4296_1_PortPrebias(ltc4296_1_port_e portNo, ltc4296_1_config_e mode);
adi_ltc_Result_e ltc4296_1_PortEnAndClassification(ltc4296_1_port_e portNo);
adi_ltc_Result_e ltc4296_1_IsPDCompatible(ltc4296_1_boardClass_e pseClass, uint16_t sccpResponseData, uint8_t *pdClass);
adi_ltc_Result_e ltc4296_1_SetPortMFVS(ltc4296_1_port_e portNo);
adi_ltc_Result_e ltc4296_1_SetPortPwr(ltc4296_1_port_e portNo);
adi_ltc_Result_e ltc4296_1_ForcePortPwr(ltc4296_1_port_e portNo);
adi_ltc_Result_e ltc4296_1_PortPwrAvailable(ltc4296_1_port_e portNo);
adi_ltc_Result_e ltc4296_1_SetGADCVout(ltc4296_1_port_e portNo);


adi_ltc_Result_e ltc4296_1_SCCPResPD(uint16_t *resData, uint8_t broadCastAddr, uint8_t readScratchPad);
adi_ltc_Result_e ltc4296_1_SCCPPD(uint16_t *resData, uint8_t broadCastAddr, uint8_t readScratchPad);
adi_ltc_Result_e ltc4296_1_SCCPResetPulse(uint8_t *pdPresent);

adi_ltc_Result_e ltc4296_1_pwr_test(ltc4296_1_boardClass_e boardClass);

#endif

/**@}*/

