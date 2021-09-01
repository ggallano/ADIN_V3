############################################################################################
#                          NSIS Installation Script                                        #
############################################################################################
!include "MUI2.nsh"
!include "MUI_EXTRAPAGES.nsh"

!define LICENSE_RTF_NAME "2019-01-22-ADINGUI Click Thru SLA.pdf"
!define LICENSE_RTF "..\Licensing\${LICENSE_RTF_NAME}"
!define OLD_APP_NAME "ADIN1100 Evaluation"
!define APP_NAME "Analog Devices Ethernet PHY Evaluation"
!define COMP_NAME "Analog Devices Inc.,"
!define VERSION "01.08.03.04"#incompatible version.versionfeature.bugfix.
!define COPYRIGHT "Analog Devices Inc. � 2021"
!define DESCRIPTION "Application"
!define INSTALLER_NAME "Analog Devices Ethernet PHY Installer_v1834.exe"#to add v1200-dev for internal.
!define MAIN_APP_EXE "ADIN1100-Eval.exe"
!define INSTALL_TYPE "SetShellVarContext current"
!define REG_ROOT "HKCU"
!define REG_APP_PATH "Software\Microsoft\Windows\CurrentVersion\App Paths\${MAIN_APP_EXE}"
!define UNINSTALL_PATH "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}"
!define PRODUCT_NAME "ADIN1100"

######################################################################

VIProductVersion  "${VERSION}"
VIAddVersionKey "ProductName"  "${APP_NAME}"
VIAddVersionKey "CompanyName"  "${COMP_NAME}"
VIAddVersionKey "LegalCopyright"  "${COPYRIGHT}"
VIAddVersionKey "FileDescription"  "${DESCRIPTION}"
VIAddVersionKey "FileVersion"  "${VERSION}"

######################################################################

SetCompressor ZLIB
Name "${APP_NAME}"
Caption "${APP_NAME}"
OutFile "${INSTALLER_NAME}"
BrandingText "${APP_NAME}"
XPStyle on
InstallDirRegKey "${REG_ROOT}" "${REG_APP_PATH}" ""
InstallDir "C:\Analog Devices\ADIN1100"

######################################################################

!include "MUI.nsh"

!define MUI_ABORTWARNING
!define MUI_UNABORTWARNING
!define MUI_ICON "ADI.ico"
!insertmacro MUI_PAGE_WELCOME


!insertmacro MUI_PAGE_README readme.rtf

!ifdef LICENSE_RTF
!insertmacro MUI_PAGE_LICENSE "${LICENSE_RTF}"
!endif

!define MUI_PAGE_HEADER_TEXT "Choose Analog Devices Ethernet PHY Software Install Location"

!insertmacro MUI_PAGE_DIRECTORY


!insertmacro MUI_PAGE_INSTFILES

#!define MUI_FINISHPAGE_RUN "$INSTDIR\${MAIN_APP_EXE}"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM

!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_UNPAGE_FINISH

;--------------------------------
;Languages
  !insertmacro MUI_LANGUAGE "English"

  ;Set up install lang strings for 1st lang
  ${ReadmeLanguage} "${LANG_ENGLISH}" \
          "Read Me" \
          "Please review the following important information." \
          "About $(^name):" \
          "$\n  Click on scrollbar arrows or press Page Down to review the entire text."

######################################################################

Section -MainProgram
Call CheckAndDownloadDotNet45
${INSTALL_TYPE}
SetOverwrite on
SetOutPath "$INSTDIR"
File "..\ADIN1100-Eval\bin\Release\ADIN1100-Eval.exe"
File "..\ADIN1100-Eval\bin\Release\DeviceCommunication.dll"
File "..\ADIN1100-Eval\bin\Release\FTD2XX_NET.dll"
File "readme.rtf"
File "${LICENSE_RTF}"
File "..\ADIN1100-Eval\bin\Release\TargetInterface.dll"
File "..\Telerik\Telerik.Windows.Controls.Data.dll"
File "..\Telerik\Telerik.Windows.Controls.DataVisualization.dll"
File "..\Telerik\Telerik.Windows.Controls.dll"
File "..\Telerik\Telerik.Windows.Controls.Input.dll"
File "..\Telerik\Telerik.Windows.Controls.GridView.dll"
File "..\Telerik\Telerik.Windows.Controls.GridView.Export.dll"
File "..\Telerik\Telerik.Windows.Controls.Navigation.dll"
File "..\Telerik\Telerik.Windows.Data.dll"
File "..\ADIN1100-Eval\bin\Release\Utilities.dll"
SetOutPath "$INSTDIR\registers"
File "..\ADIN1100-Eval\bin\Release\registers\registers_adin1100_S1.json"
File "..\ADIN1100-Eval\bin\Release\registers\registers_adin1100_S2.json"
File "..\ADIN1100-Eval\bin\Release\registers\registers_adin1300.json"
File "..\ADIN1100-Eval\bin\Release\registers\registers_adin1301.json"
File "..\ADIN1100-Eval\bin\Release\registers\registers_adin1200.json"
SetOutPath "$INSTDIR\scripts"
File "..\ADIN1100-Eval\bin\Release\scripts\Enable Link LED_scripts.json"
File "..\ADIN1100-Eval\bin\Release\scripts\Reset Auto-Negotiation_scripts.json"
File "..\ADIN1100-Eval\bin\Release\scripts\SftPd Down&Up_scripts.json"


#SetOutPath "$INSTDIR\doc"
#File "EVAL-ADIN1100FMCZ-UG-XXXX_RevPrA.pdf"
#File "02-063798-01-b_SCH_NDA.pdf"
#File "08-063798-01-b_PCB_NDA.pdf"
SectionEnd

######################################################################

Section -Icons_Reg
SetOutPath "$INSTDIR"
WriteUninstaller "$INSTDIR\uninstall.exe"

CreateDirectory "$SMPROGRAMS\Analog Devices\ADIN1100"
CreateShortCut "$SMPROGRAMS\Analog Devices\ADIN1100\${APP_NAME}.lnk" "$INSTDIR\${MAIN_APP_EXE}"
CreateShortCut "$DESKTOP\${APP_NAME}.lnk" "$INSTDIR\${MAIN_APP_EXE}"
CreateShortCut "$SMPROGRAMS\Analog Devices\ADIN1100\Uninstall ${APP_NAME}.lnk" "$INSTDIR\uninstall.exe"

CreateShortCut "$SMPROGRAMS\Analog Devices\ADIN1100\ADIN1100 User Guide.lnk" "$INSTDIR\doc\EVAL-ADIN1100FMCZ-UG-XXXX_RevPrA.pdf"

CreateShortCut "$SMPROGRAMS\Analog Devices\ADIN1100\ADIN1100 Schematics.lnk" "$INSTDIR\doc\02-063798-01-b_SCH_NDA.pdf"

CreateShortCut "$SMPROGRAMS\Analog Devices\ADIN1100\ADIN1100 PCB.lnk" "$INSTDIR\doc\08-063798-01-b_PCB_NDA.pdf"
#CreateShortCut "$SMPROGRAMS\Analog Devices\ADIN1100\ADIN1300 Datasheet.lnk" "$INSTDIR\doc\ADIN1300_Datasheet.pdf"
#CreateShortCut "$SMPROGRAMS\Analog Devices\ADIN1100\ADIN1300 User Guide.lnk" "$INSTDIR\doc\EVAL-ADIN1300FMCZ-UG-1635.pdf"
#CreateShortCut "$SMPROGRAMS\Analog Devices\ADIN1100\ADIN1200 Datasheet.lnk" "$INSTDIR\doc\ADIN1200_Datasheet.pdf"
#CreateShortCut "$SMPROGRAMS\Analog Devices\ADIN1100\ADIN1200 User Guide.lnk" "$INSTDIR\doc\EVAL-ADIN1200FMCZ-UG-1673.pdf"

WriteRegStr ${REG_ROOT} "${REG_APP_PATH}" "" "$INSTDIR\${MAIN_APP_EXE}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "DisplayName" "${APP_NAME}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "UninstallString" "$INSTDIR\uninstall.exe"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "DisplayIcon" "$INSTDIR\${MAIN_APP_EXE}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "DisplayVersion" "${VERSION}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "Publisher" "${COMP_NAME}"

# Delete the old shortcuts to avoid confusion
Delete "$DESKTOP\${OLD_APP_NAME}.lnk"
Delete "$SMPROGRAMS\Analog Devices\ADIN1100\${OLD_APP_NAME}.lnk" 
Delete "$SMPROGRAMS\Analog Devices\ADIN1100\Uninstall ${OLD_APP_NAME}.lnk"

# Delete the ADIN1200 files , and files from the older installations to avoid confusion
# "$INSTDIR\doc\EVAL-ADIN1100FMCZ-UG-XXXX_RevPrA.pdf"
#Delete "$INSTDIR\doc\02-063798-01-b_SCH_NDA.pdf"
#Delete "$INSTDIR\doc\08-063798-01-b_PCB_NDA.pdf"
SectionEnd

######################################################################

Section Uninstall
${INSTALL_TYPE}
Delete "$INSTDIR\uninstall.exe"
Delete "$INSTDIR\ADIN1100-Eval.exe"
Delete "$INSTDIR\DeviceCommunication.dll"
Delete "$INSTDIR\FTD2XX_NET.dll"
Delete "$INSTDIR\readme.rtf"
Delete "$INSTDIR\${LICENSE_RTF_NAME}"
Delete "$INSTDIR\registers\registers_adin1100.json"
Delete "$INSTDIR\registers\registers_adin1100_S1.json"
Delete "$INSTDIR\registers\registers_adin1100_S2.json"
Delete "$INSTDIR\registers\registers_adin1300.json"
Delete "$INSTDIR\registers\registers_adin1301.json"
Delete "$INSTDIR\registers\registers_adin1200.json"
Delete "$INSTDIR\scripts\Enable Link LED_scripts.json"
Delete "$INSTDIR\scripts\Reset Auto-Negotiation_scripts.json"
Delete "$INSTDIR\scripts\SftPd Down&Up_scripts.json"
Delete "$INSTDIR\TargetInterface.dll"
Delete "$INSTDIR\Telerik.Windows.Controls.Data.dll"
Delete "$INSTDIR\Telerik.Windows.Controls.DataVisualization.dll"
Delete "$INSTDIR\Telerik.Windows.Controls.dll"
Delete "$INSTDIR\Telerik.Windows.Controls.Input.dll"
Delete "$INSTDIR\Telerik.Windows.Controls.Navigation.dll"
Delete "$INSTDIR\Telerik.Windows.Data.dll"
Delete "$INSTDIR\Telerik.Windows.Controls.GridView.dll"
Delete "$INSTDIR\Telerik.Windows.Controls.GridView.Export.dll"

Delete "$INSTDIR\Utilities.dll"

#Delete "$INSTDIR\doc\EVAL-ADIN1100FMCZ-UG-XXXX_RevPrA.pdf"
#Delete "$INSTDIR\doc\02-063798-01-b_SCH_NDA.pdf"
#Delete "$INSTDIR\doc\08-063798-01-b_PCB_NDA.pdf"
#Delete "$INSTDIR\doc\ADIN1300_Datasheet.pdf"
#Delete "$INSTDIR\doc\EVAL-ADIN1300FMCZ-UG-1635.pdf"
#Delete "$INSTDIR\doc\ADIN1200_Datasheet.pdf"
#Delete "$INSTDIR\doc\EVAL-ADIN1200FMCZ-UG-1673.pdf"

# Delete the ADIN1200 files for now, and files from the older installations


RmDir "$INSTDIR\doc"
RmDir "$INSTDIR\registers"
RmDir "$INSTDIR\scripts"
RmDir "$INSTDIR"

!ifdef REG_START_MENU
!insertmacro MUI_STARTMENU_GETFOLDER "Application" $SM_Folder
Delete "$SMPROGRAMS\$SM_Folder\${APP_NAME}.lnk"
Delete "$SMPROGRAMS\$SM_Folder\Uninstall ${APP_NAME}.lnk"

Delete "$DESKTOP\${APP_NAME}.lnk"
Delete "$DESKTOP\${OLD_APP_NAME}.lnk"
RmDir "$SMPROGRAMS\$SM_Folder"
!endif

!ifndef REG_START_MENU
Delete "$SMPROGRAMS\Analog Devices\ADIN1100\${APP_NAME}.lnk"
#Delete "$SMPROGRAMS\Analog Devices\ADIN1100\ADIN1100_10BASE-T1L_PHY_PrD_NDA.lnk"
#Delete "$SMPROGRAMS\Analog Devices\ADIN1100\ADIN1300 Datasheet.lnk" 
Delete "$SMPROGRAMS\Analog Devices\ADIN1100\ADIN1100 User Guide.lnk"
Delete "$SMPROGRAMS\Analog Devices\ADIN1100\ADIN1100 Schematics.lnk"
Delete "$SMPROGRAMS\Analog Devices\ADIN1100\ADIN1100 PCB.lnk"


#Delete "$SMPROGRAMS\Analog Devices\ADIN1100\ADIN1200 Datasheet.lnk" 
#Delete "$SMPROGRAMS\Analog Devices\ADIN1100\ADIN1200 User Guide.lnk"
Delete "$SMPROGRAMS\Analog Devices\ADIN1100\Uninstall ${APP_NAME}.lnk"

Delete "$DESKTOP\${APP_NAME}.lnk"

RmDir "$SMPROGRAMS\Analog Devices\ADIN1100"
!endif

DeleteRegKey ${REG_ROOT} "${REG_APP_PATH}"
DeleteRegKey ${REG_ROOT} "${UNINSTALL_PATH}"
SectionEnd

######################################################################

 Function CheckAndDownloadDotNet45
	# Let's see if the user has the .NET Framework 4.5 installed on their system or not
 
	# Set up our Variables
	Var /GLOBAL dotNET45IsThere
	Var /GLOBAL dotNET_CMD_LINE
	Var /GLOBAL EXIT_CODE
 
        # We are reading a version release DWORD that Microsoft says is the documented
        # way to determine if .NET Framework 4.5 is installed
	ReadRegDWORD $dotNET45IsThere HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"
	IntCmp $dotNET45IsThere 379893 is_equal is_less is_greater
 
	is_equal:
		Goto done_compare_not_needed
	is_greater:
		# Useful if, for example, Microsoft releases .NET 4.5.2
		# We want to be able to simply skip install since it's not
		# needed on this system
		Goto done_compare_not_needed
	is_less:
		Goto done_compare_needed
 
	done_compare_needed:
		#.NET Framework 4.5.2 install is *NEEDED*
 
		# Setup looks for components\NDP452-KB2901954-Web.exe relative to the install EXE location
		# This allows the installer to be placed on a USB stick (for computers without internet connections)
		# If the .NET Framework 4.5.2 installer is *NOT* found, Setup will connect to Microsoft's website
		# and download it for you
 
		# Reboot Required with these Exit Codes:
		# 1641 or 3010 
 
		# Let's see if the user is doing a Silent install or not
		IfSilent is_quiet is_not_quiet
 
		is_quiet:
			StrCpy $dotNET_CMD_LINE "/q /norestart"
			Goto LookForLocalFile
		is_not_quiet:
			StrCpy $dotNET_CMD_LINE "/showrmui /passive /norestart"
			Goto LookForLocalFile
 
		LookForLocalFile:
			# Let's see if the user stored the Full Installer
			IfFileExists "$EXEPATH\components\NDP452-KB2901954-Web.exe" do_local_install do_network_install
 
			do_local_install:
				# .NET Framework found on the local disk.  Use this copy
 
				ExecWait '"$EXEPATH\components\NDP452-KB2901954-Web.exe" $dotNET_CMD_LINE' $EXIT_CODE
				Goto is_reboot_requested
 
			# Now, let's Download the .NET
			do_network_install:
 
				NSISdl::download "https://download.microsoft.com/download/B/4/1/B4119C11-0423-477B-80EE-7A474314B347/NDP452-KB2901954-Web.exe" "$TEMP\NDP452-KB2901954-Web.exe" 
 
				Pop $0
				StrCmp $0 "success" success
				MessageBox MB_OK|MB_ICONEXCLAMATION "Unable to download .NET Framework.  ${PRODUCT_NAME} will be installed, but will not function without the Framework!"
				Goto done_dotNET_function
				success:
					ExecWait '"$TEMP\NDP452-KB2901954-Web.exe" $dotNET_CMD_LINE' $EXIT_CODE
					Goto is_reboot_requested

 
				# $EXIT_CODE contains the return codes.  1641 and 3010 means a Reboot has been requested
				is_reboot_requested:
					${If} $EXIT_CODE = 1641
					${OrIf} $EXIT_CODE = 3010
						SetRebootFlag true
					${EndIf}
 
	done_compare_not_needed:
		# Done dotNET Install
		Goto done_dotNET_function
 
	#exit the function
	done_dotNET_function:
 
    FunctionEnd

