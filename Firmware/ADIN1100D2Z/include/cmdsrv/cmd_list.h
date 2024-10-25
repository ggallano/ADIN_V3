/*
 *---------------------------------------------------------------------------
 *
 * Copyright (c) 2016 - 2020 Analog Devices, Inc. All Rights Reserved.
 * This software is proprietary to Analog Devices, Inc.
 * and its licensors.By using this software you agree to the terms of the
 * associated Analog Devices Software License Agreement.
 *
 *
 *---------------------------------------------------------------------------
 */

#ifndef _CMD_LIST_LIB_H_
#define _CMD_LIST_LIB_H_

#include "cmd_aux.h"

typedef struct command
{
    const char* cmd;
    int (*callback)(char*, char*, int);
    char public;
	const char* cmd_details;
}commandList_t;


int getAuxFuncSize(void);

extern commandList_t aux_commands[];

#endif /* _CMD_LIST_LIB_H_ */
