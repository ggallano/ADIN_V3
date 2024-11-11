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

#ifndef _CMD_AUX_LIB_H_
#define _CMD_AUX_LIB_H_

int CMDAUX_mdioWrite(char *arga, char *argb, int fd);
int CMDAUX_mdioRead(char *arga, char *argb, int fd);
int CMDAUX_mdioRead45Clause(char *arga, char *argb, int fd);
int CMDAUX_mdioWrite45Clause(char *arga, char *argb, int fd);
int CMDAUX_printGreetings(char *arga, char *argb, int fd);
int CMDAUX_printCommandList(char *arga, char *argb, int fd);

#endif /* _CMD_AUX_LIB_H_ */
