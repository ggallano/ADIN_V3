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
#ifndef SCCP_DEFINES_H_
#define SCCP_DEFINES_H_

/** @addtogroup SPOE LTC4296_1 Driver
 *  @{
 */

#define ms2tcks(ms)		((uint16_t)(ms*32))

#define CMD_BLINKEM				0xDD
#define CMD_RESET_PD			0x66 // not legit IEEE cmd
#define CMD_BROADCAST_ADDR		0xCC
#define CMD_READ_SCRATCHPAD		0xAA
#define CMD_READ_V_PWR			0xBB
#define CMD_READ_VOLT_INFO		0xBB
#define CMD_READ_PWR_INFO		0x77
#define CMD_WRITE_PWR_ASSIGN	0x99
#define CMD_READ_PWR_ASSIGN		0x81
#define CMD_RW_REFUSE			0x99 // not legit IEEE cmd, used to tell PC that rw was not performed
#define CLASS_TYPE_E			0x0C

#define T_BUG			0.2

/* COMMON TIMING */
#define T_REC			0.32
#define T_W1L			0.3
#define T_W0L			2.0
#define T_R				0.25

/* RESET/PRESENSE PULSE TIMING */
#define T_RSTL_MIN		8.0   /* length of PSE reset pulse */
#define T_RSTL_MAX		10.5
#define T_RSTL_NOM		9
#define T_PDH			1.0   /* time between PSE releasing reset pulse and PD starting presense pulse */
#define T_PDL			3.8   /* period that PD holds presense pulse */
#define T_MSP			2     /* time between PSE releasing reset pulse and PSE sampling for PD's presense pulse */

#define T_POR_PULSE		450

/* WRITE SLOT TIMING */
#define T_WRITESLOT		2.75
#define T_SSW			1.1   /* time between start of PSE Pull down and middle of PD capture window */

/* READ SLOT TIMING */
#define T_READSLOT		3.0
#define T_READSLOT_MAX_TYPE_E 3.83

#define T_MSR           1     /* As per PD spec */
#define T_R0L			2.0   /*time between start of PSE pull down and PD releasing a pull down response (PD writing 0 to PSE) */

/**@}*/

#endif /* SCCP_DEFINES_H_ */


