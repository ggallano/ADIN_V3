/*
 *---------------------------------------------------------------------------
 *
 * Copyright (c) 2024 Analog Devices, Inc. All Rights Reserved.
 * This software is proprietary to Analog Devices, Inc.
 * and its licensors.By using this software you agree to the terms of the
 * associated Analog Devices Software License Agreement.
 *
 *
 *---------------------------------------------------------------------------
 */
#ifndef __CMD_SRV_H_
#define __CMD_SRV_H_

uint32_t processCommand(char *in, char *out, int* fd);
void processUartCommand(void);
void processUartData(void);
#endif
