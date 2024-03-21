@ECHO OFF

set NSIS_PATH = C:\\Program Files (x86)\\NSIS\\makensis.exe
set NSIS_SCRIPT_NAME = ADIN1100_Eval.nsi
set /A majorNumber = 2
set /A minorNumber = 0
set /A buildNumber = 278
set /A revisionNumber = 469



"%NSIS_PATH%" /DVERSION=%majorNumber%.%minorNumber%.%buildNumber%.%revisionNumber% Installer\\%NSIS_SCRIPT_NAME%


PAUSE