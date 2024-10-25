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

#ifndef _CMD_AUX_LIB_H_
#define _CMD_AUX_LIB_H_

int CMDAUX_mdioWrite(char *arga, char *argb, int fd);
int CMDAUX_mdioRead(char *arga, char *argb, int fd);
int CMDAUX_mdioRead45Clause(char *arga, char *argb, int fd);
int CMDAUX_mdioWrite45Clause(char *arga, char *argb, int fd);
int CMDAUX_printGreetings(char *arga, char *argb, int fd);
int CMDAUX_printCommandList(char *arga, char *argb, int fd);
int CMDAUX_printModes(char *arga, char *argb, int fd);
int CMDAUX_systemReset(char *arga, char *argb, int fd);
int CMDAUX_changeConfig(char *arga, char *argb, int fd);
int CMDAUX_phyStatus(char *arga, char *argb, int fd);
int CMDAUX_phytxdisabled(char *arga, char *argb, int fd);
int CMDAUX_phytestmode1(char *arga, char *argb, int fd);
int CMDAUX_phytestmode2(char *arga, char *argb, int fd);
int CMDAUX_phytestmode3(char *arga, char *argb, int fd);
int CMDAUX_adin1100RemLoopback(char *arga, char *argb, int fd);
int CMDAUX_adin1200RemLoopback(char *arga, char *argb, int fd);
int CMDAUX_frameGenACheck(char *arga, char *argb, int fd);
int CMDAUX_phyReset(char *arga, char *argb, int fd);
int CMDAUX_getfwVersion(char *arga, char *argb, int fd);
int CMDAUX_startUartReport(char *arga, char *argb, int fd);
int CMDAUX_stopUartReport(char *arga, char *argb, int fd);
int CMDAUX_clearFrameErrors(char *arga, char *argb, int fd);
int CMDAUX_spoeRead(char *arga, char *argb, int fd);
int CMDAUX_spoeWrite(char *arga, char *argb, int fd);
int CMDAUX_sccpReadWrite(char *arga, char *argb, int fd);
int CMDAUX_sccpReset(char *arga, char *argb, int fd);

#endif /* _CMD_AUX_LIB_H_ */
