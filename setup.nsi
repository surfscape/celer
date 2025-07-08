!include "MUI2.nsh"

Name "Celer"
Outfile "CelerSetup.exe"

InstallDir "$PROGRAMFILES64\SurfScape\Celer"
InstallDirRegKey HKCU "Software\Celer" "Install_Dir"

RequestExecutionLevel admin

!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "Portuguese"

Section

  WriteRegStr HKCU "Software\Celer" "Install_Dir" "$INSTDIR"

  SetOutPath "$INSTDIR"

  File /r "bin\x64\Debug\net9.0-windows10.0.18362.0\*.*"

  CreateDirectory "$SMPROGRAMS\Celer"

  CreateShortCut "$SMPROGRAMS\Celer\Celer.lnk" "$INSTDIR\Celer.exe"
  CreateShortCut "$DESKTOP\Celer.lnk" "$INSTDIR\Celer.exe"

  WriteUninstaller "$INSTDIR\uninstaller.exe"

SectionEnd

Section "Uninstall"

  RMDir /r "$INSTDIR"

  Delete "$DESKTOP\Celer.lnk"
  Delete "$SMPROGRAMS\Celer\Celer.lnk"
  RMDir "$SMPROGRAMS\Celer"

  DeleteRegKey HKCU "Software\Celer"

SectionEnd
