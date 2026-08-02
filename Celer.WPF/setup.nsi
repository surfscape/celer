!include "MUI2.nsh"
!include "FileFunc.nsh"
!define VERSION "1.0.0-beta.3"

Name "Celer"
Outfile "CelerSetup.exe"

InstallDir "$PROGRAMFILES64\SurfScape\Celer"

RequestExecutionLevel admin

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\Celer.exe"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "English"

Section

  SetShellVarContext all
  SetOutPath "$INSTDIR"

  WriteRegStr HKLM "Software\Celer" "Install_Dir" "$INSTDIR"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Celer" "DisplayName" "Celer"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Celer" "DisplayVersion" "${VERSION}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Celer" "DisplayIcon" "$\"$INSTDIR\celer.exe$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Celer" "Publisher" "SurfScape"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Celer" "UninstallString" "$\"$INSTDIR\uninstaller.exe$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Celer" "InstallLocation" "$INSTDIR"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Celer" "QuietUninstallString" "$\"$INSTDIR\uninstaller.exe$\" /S"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Celer" "URLInfoAbout" "https://surfscape.eu/celer/"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Celer" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Celer" "NoRepair" 1

  File /r "bin\Release\net10.0-windows10.0.18362.0\win-x64\publish\*.*"

  CreateDirectory "$SMPROGRAMS\Celer"

  CreateShortCut "$SMPROGRAMS\Celer\Celer.lnk" "$INSTDIR\Celer.exe"
  CreateShortCut "$DESKTOP\Celer.lnk" "$INSTDIR\Celer.exe"

  WriteUninstaller "$INSTDIR\uninstaller.exe"

  ${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
  IntFmt $0 "0x%08X" $0
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Celer" \
                 "EstimatedSize" "$0"

SectionEnd

Section "Uninstall"

  SetShellVarContext all

  RMDir /r "$INSTDIR"

  Delete "$DESKTOP\Celer.lnk"
  Delete "$SMPROGRAMS\Celer\Celer.lnk"
  RMDir "$SMPROGRAMS\Celer"

  nsExec::ExecToLog 'schtasks /Delete /TN "Run Celer at Startup" /F'
  Pop $R1

  DeleteRegKey HKCU "Software\Celer"
  DeleteRegKey HKLM "Software\Celer"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Celer"

SectionEnd
