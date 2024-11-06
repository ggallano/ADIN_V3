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
