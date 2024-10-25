## Description

This directory contains firmware for board DEMO-ADIN1100D2Z.
The firmware configures MCU, ADIN1100, ADIN1200 devices and LTC4296-1 on the board.
The UART commands and firmware modes supported by firmware are listed below.

Hardware Setup
==============
The hardware setup requires one DEMO-ADIN1100D2Z board.
The board is powered by USB-C cable connected to host PC on one end and DEMO-ADIN1100D2Z board on the other.
The MCU on board sends status to the host PC using the UART interface. 

Board LED functionality:
- BLUE LED   (uC0) : ON Production DATA Test or Production PWR Test is passed.
- YELLOW LED (uC1) : Heart beat LED Toggles every one second.
- RED LED    (uC2) : ON when there is an error. 
- GREEN LED  (uC3) : ON when link is established.


Software Setup
==============
Maxim Micro SDK Installation is required. 
Eclipse IDE is provided along with MAXIM SDK installation.
For UART output, configure the PC-based serial terminal software to 8-N-1 and 115200baud.

Note: Workspace directory locations with spaces do not function properly in Eclipse.

To run this Example
===================
Refer the docs\DEMO-ADIN1100D2Z+Getting+Started+Guide.pdf which comes along with this installer.


*****************************************************
UART output after the DEMO-ADIN1100D2Z board is reset
*****************************************************

================================================
ANALOG DEVICES 10BASE-T1L and SPOE DEMO
================================================
(c) 2023 Analog Devices Inc. All rights reserved
================================================
Firmware ver.  : 1.0.0
Hardware type  : DEMO-ADIN1100D2Z
uC CFG-3-2-1-0 : OFF-OFF-OFF-OFF (Mode 0)
Firmware Mode  : Media converter PSE class 10
================================================
Type '<?><new line>' for a list of commands
================================================
LTC4296-1 reset
ADIN1100 MDIO address 0
ADIN1100 HW CFG: autoneg, pref.slave, Tx 2.4V
================================================
ADIN1200 MDIO address 4
ADIN1200 SW CFG: autoneg 10Mbit Full Duplex Only
================================================
PSE initiated ...
LTC4296-1 Port1 Vin 20.6V

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
* SPOE read from LTC4296-1, all numbers in hex.
  'spoeread <RegAddr>'<newLine>
* SPOE write from LTC4296-1, all numbers in hex.
  'spoewrite <RegAddr>,<Data>'<newLine>
* SCCP needs specific LTC4296-1 setup.
  'sccprw <BroadcastAddr>,<ScratchpadCommand>' <newLine>
* Phy status and link properties.
  'phystatus'<newLine>
* Start sending periodic diagnostics.
  'start' <newLine>
* Stop sending periodic diagnostics.
  'stop'  <newLine>
* Clear / reset diagnostics counters.
  'clear' <newLine>
* Phy (hardware) reset
  'phyreset'<newLine>
* uC software Reset.
  'reset'<newLine>
* PHY TX Disabled.
  'adin1100txdis'<newLine>
* PHY Test1 - Jitter.
  'adin1100test1'<newLine>
* PHY Test2 - Droop .
  'adin1100test2'<newLine>
* PHY Test3 - Idle .
  'adin1100test3'<newLine>
* ADIN1100 Remote Loopback
  'adin1100loopback' <newLine>
* Frame Generator and Checker
  'adin1100frameGen' <newLine>
* ADIN1200 Remote Loopback
  'adin1200loopback' <newLine>
* Get Firmware Version.
  'getfwversion' <newLine>
* Show list of implemented modes.
  'listmodes'<newLine>
* Mode change, overwrites HW CFG, until next Reset.
  'mode <number>' <newLine>
* Show board information.
  'info'<newLine>
* Show list of commands.
  'help'<newLine>
* Show list of commands.
  '?'<newLine>
==============================================

*********************************
UART output of command listmodes
*********************************

==============================================
       List of Modes
==============================================
0 SPOE CLASS 10
1 SPOE CLASS 11
2 SPOE CLASS 12
3 SPOE CLASS 13
4 SPOE CLASS 14
5 SPOE CLASS 15
6 APL CLASS A
7 APL CLASS A NO AUTONEG
8 APL CLASS C
9 APL CLASS 3
10 PRODUCTION POWER TEST
11 APL CLASS A OLD DEMO
12 SPOE OFF
13 PRODUCTION DATA TEST
14 RESERVED
15 DEBUG
==============================================
