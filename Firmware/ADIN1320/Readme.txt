## Description

This directory contains firmware for board EVAL-ADIN1320 
The firmware configures MCU, ADIN1320, ADIN1300 devices and LTC4296-1 on the board.
The UART commands and firmware modes supported by firmware are listed below.

Hardware Setup
==============
The hardware setup requires one EVAL-ADIN1320 board.
The board is powered by USB-C cable connected to host PC on one end and EVAL-ADIN1320 board on the other.
The MCU on board sends status to the host PC using the UART interface. 

Software Setup
==============
Maxim Micro SDK Installation is required. 
Eclipse IDE is provided along with MAXIM SDK installation.
For UART output, configure the PC-based serial terminal software to 8-N-1 and 115200baud.

Note: Workspace directory locations with spaces do not function properly in Eclipse.


*****************************************************
UART output after the ADIN1320 board is reset
*****************************************************

================================================
ANALOG DEVICES 10BASE-T1L and SPOE DEMO
================================================
(c) 2023 Analog Devices Inc. All rights reserved
================================================
Firmware ver.  : 1.0.0
Hardware type  : EVAL-ADIN1320
uC CFG-3-2-1-0 : OFF-OFF-OFF-OFF (Mode 0)
Firmware Mode  : Media converter PSE class 10
================================================
Type '<?><new line>' for a list of commands
================================================
LTC4296-1 reset
ADIN1320 MDIO address 0
ADIN1320 SW CFG: Board Reset Configuration 
================================================
ADIN1300 MDIO address 4
ADIN1300 SW CFG: Board Reset Configuration 
================================================

*****************************
UART output of command '?' 
*****************************
================================================
List of Commands
================================================
* MDIO (Clause 22) read from Phy, all numbers in hex.
  'mdioread <PhyAddr>,<RegAddr>'<newLine>
* MDIO (Clause 22) write to Phy, all numbers in hex.
  'mdiowrite <PhyAddr>,<RegAddr>,<Data>'<newLine>
* MDIO (Clause 45) read from Phy, all numbers in hex.
  'mdiord_cl45 <PhyAddr>,<RegAddr>'<newLine>
* MDIO (Clause 45) write to Phy, all numbers in hex.
  'mdiowr_cl45 <PhyAddr>,<RegAddr>,<Data>'<newLine>
* Show board information.
  'info'<newLine>
* Show list of commands.
  'help'<newLine>
* Show list of commands.
  '?'<newLine>
==============================================
