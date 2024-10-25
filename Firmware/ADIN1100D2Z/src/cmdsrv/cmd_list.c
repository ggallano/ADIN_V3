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

#include "cmdsrv\cmd_list.h"

/*!
 *
 * @details         List the CLI commands
 *
 */
commandList_t aux_commands[] =
{
  {"mdioread",            CMDAUX_mdioRead,              1,    "* MDIO (Clause 22) read from Phy, all numbers in hex.\n  'mdioread <PhyAddr>,<RegAddr>'<newLine>\n"},
  {"mdiowrite",           CMDAUX_mdioWrite,             1,    "* MDIO (Clause 22) write to Phy, all numbers in hex.\n  'mdiowrite <PhyAddr>,<RegAddr>,<Data>'<newLine>\n"},
  {"mdiord_cl45",         CMDAUX_mdioRead45Clause,      1,    "* MDIO (Clause 45) read from Phy, all numbers in hex.\n  'mdiord_cl45 <PhyAddr>,<RegAddr>'<newLine>\n"},
  {"mdiowr_cl45",         CMDAUX_mdioWrite45Clause,     1,    "* MDIO (Clause 45) write to Phy, all numbers in hex.\n  'mdiowr_cl45 <PhyAddr>,<RegAddr>,<Data>'<newLine>\n"},

//  {"spoeread",            CMDAUX_spoeRead,              1,    "* SPOE read from LTC4296-1, all numbers in hex.\n  'spoeread <RegAddr>'<newLine>\n"},
//  {"spoewrite",           CMDAUX_spoeWrite,             1,    "* SPOE write from LTC4296-1, all numbers in hex.\n  'spoewrite <RegAddr>,<Data>'<newLine>\n"},
//
//  {"sccpreset",           CMDAUX_sccpReset,             0,    "* SCCP Reset Pulse. \n  'sccpreset'<newLine>\n"},
//  {"sccprw",              CMDAUX_sccpReadWrite,         1,    "* SCCP needs specific LTC4296-1 setup.\n  'sccprw <BroadcastAddr>,<ScratchpadCommand>'<newLine>\n"},
//
//  {"phystatus",           CMDAUX_phyStatus,             1,    "* Phy status and link properties.\n  'phystatus'<newLine>\n"},
//  {"start",               CMDAUX_startUartReport,       1,    "* Start sending periodic diagnostics.\n  'start'<newLine>\n"},
//  {"stop",                CMDAUX_stopUartReport,        1,    "* Stop sending periodic diagnostics.\n  'stop'<newLine>\n"},
//  {"clear",               CMDAUX_clearFrameErrors,      1,    "* Clear / reset diagnostics counters.\n  'clear'<newLine>\n"},
//
//  {"phyreset",            CMDAUX_phyReset,              1,    "* Phy (hardware) reset. \n  'phyreset'<newLine>\n"},
//  {"reset",               CMDAUX_systemReset,           1,    "* uC software Reset. \n  'reset'<newLine>\n"},
//
//  {"adin1100txdis",       CMDAUX_phytxdisabled,         1,    "* PHY TX Disabled.\n  'adin1100txdis'<newLine>\n"},
//  {"adin1100test1",       CMDAUX_phytestmode1,          1,    "* PHY Test1 - Jitter.\n  'adin1100test1'<newLine>\n"},
//  {"adin1100test2",       CMDAUX_phytestmode2,          1,    "* PHY Test2 - Droop.\n  'adin1100test2'<newLine>\n"},
//  {"adin1100test3",       CMDAUX_phytestmode3,          1,    "* PHY Test3 - Idle.\n  'adin1100test3'<newLine>\n"},
//
//  {"adin1100loopback",    CMDAUX_adin1100RemLoopback,   1,    "* ADIN1100 Remote Loopback. \n  'adin1100loopback'<newLine>\n"},
//  {"adin1100framegen",    CMDAUX_frameGenACheck,        1,    "* Frame Generator and Checker.\n  'adin1100frameGen'<newLine>\n"},
//  {"adin1200loopback",    CMDAUX_adin1200RemLoopback,   1,    "* ADIN1200 Remote Loopback. \n  'adin1200loopback'<newLine>\n"},

  {"getfwversion",        CMDAUX_getfwVersion,          1,    "* Get Firmware Version.  \n  'getfwversion'<newLine>\n" },
//  {"listmodes",           CMDAUX_printModes,            1,    "* Show list of implemented modes.\n  'listmodes'<newLine>\n"},
//  {"mode",                CMDAUX_changeConfig,          1,    "* Mode change, overwrites HW CFG, until next Reset.\n  'mode <number>'<newLine>\n"},
  {"info",                CMDAUX_printGreetings,        1,    "* Show board information.\n  'info'<newLine>\n"},
  {"help",                CMDAUX_printCommandList,      1,    "* Show list of commands.\n  'help'<newLine>\n"},
  {"?",                   CMDAUX_printCommandList,      1,    "* Show list of commands.\n  '?'<newLine>\n"},
};

/*!
 *
 * @return
 *                  - Returns the size of aux_commands.
 *
 * @details         Gets the size of struct aux_commands
 *
 */
int getAuxFuncSize(void)
{
    return sizeof(aux_commands)/sizeof(aux_commands[0]);
}
