; EmailConverge NSIS Installer Script
; Build first: dotnet publish -c Release

;--------------------------------
; Basic definitions
!define APPNAME "EmailConverge"
!define COMPANYNAME "EmailConverge"
!define DESCRIPTION "Email Converge Application"
!define VERSIONMAJOR 1
!define VERSIONMINOR 0
!define VERSIONBUILD 0
!define INSTALLSIZE 150000

; .NET 9 Desktop Runtime definitions
!define DOTNET_RUNTIME_VERSION "9.0.12"
!define DOTNET_RUNTIME_INSTALLER "windowsdesktop-runtime-9.0.12-win-x64.exe"

;--------------------------------
; Include Modern UI
!include "MUI2.nsh"

;--------------------------------
; Basic settings
Name "${APPNAME}"
OutFile "EmailConverge_Setup.exe"
InstallDir "$PROGRAMFILES64\${APPNAME}"
InstallDirRegKey HKLM "Software\${APPNAME}" "InstallDir"
RequestExecutionLevel admin

;--------------------------------
; UI settings
!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

;--------------------------------
; Install pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE.txt"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES

; Finish page - Run application option
!define MUI_FINISHPAGE_RUN "wscript.exe"
!define MUI_FINISHPAGE_RUN_PARAMETERS "$\"$INSTDIR\StartEmailConverge.vbs$\""
!define MUI_FINISHPAGE_RUN_TEXT "Run EmailConverge"
!insertmacro MUI_PAGE_FINISH

;--------------------------------
; Uninstall pages
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------
; Languages
!insertmacro MUI_LANGUAGE "SimpChinese"
!insertmacro MUI_LANGUAGE "English"

;--------------------------------
; Check if .NET 9 Desktop Runtime is installed
Function CheckDotNetRuntime
    ; Check registry for .NET 9 Desktop Runtime
    ReadRegStr $0 HKLM "SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App" "${DOTNET_RUNTIME_VERSION}"
    StrCmp $0 "" 0 DotNetFound
    
    ; Also check for newer versions (9.0.x)
    EnumRegKey $1 HKLM "SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App" 0
    StrCmp $1 "" DotNetNotFound
    
    ; Check if version starts with 9.0
    StrCpy $2 $1 3
    StrCmp $2 "9.0" DotNetFound DotNetNotFound
    
DotNetNotFound:
    ; .NET Runtime not installed, prompt to install
    MessageBox MB_YESNO ".NET 9 Desktop Runtime is not installed.$\n$\nInstall it now?" IDYES InstallDotNet IDNO AbortInstall
    
InstallDotNet:
    DetailPrint "Installing .NET 9 Desktop Runtime..."
    SetOutPath $TEMP
    File "EmailConverge\bin\Release\net9.0\${DOTNET_RUNTIME_INSTALLER}"
    ExecWait '"$TEMP\${DOTNET_RUNTIME_INSTALLER}" /install /quiet /norestart' $0
    Delete "$TEMP\${DOTNET_RUNTIME_INSTALLER}"
    
    ; Check install result
    IntCmp $0 0 DotNetFound
    IntCmp $0 3010 DotNetFound ; 3010 = reboot required
    MessageBox MB_OK "Failed to install .NET 9 Desktop Runtime (Error: $0).$\nPlease download and install manually."
    Abort
    
AbortInstall:
    MessageBox MB_OK "Installation cancelled. .NET 9 Desktop Runtime is required."
    Abort
    
DotNetFound:
FunctionEnd

;--------------------------------
; Install section
Section "Install"
    ; First check .NET Runtime
    Call CheckDotNetRuntime
    
    SetOutPath $INSTDIR
    
    ; Copy published files
    ; Make sure to run: dotnet publish -c Release first
    File /r "EmailConverge\bin\Release\net9.0\publish\*.*"
    
    ; Write registry
    WriteRegStr HKLM "Software\${APPNAME}" "InstallDir" "$INSTDIR"
    
    ; Create uninstaller
    WriteUninstaller "$INSTDIR\Uninstall.exe"
    
    ; Create launcher script (hide cmd window)
    FileOpen $0 "$INSTDIR\StartEmailConverge.vbs" w
    FileWrite $0 'Set WshShell = CreateObject("WScript.Shell")$\r$\n'
    FileWrite $0 'WshShell.CurrentDirectory = "$INSTDIR"$\r$\n'
    FileWrite $0 'WshShell.Run "dotnet EmailConverge.dll", 0, False$\r$\n'
    FileClose $0
    
    ; Create start menu shortcuts
    CreateDirectory "$SMPROGRAMS\${APPNAME}"
    CreateShortcut "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk" "wscript.exe" '"$INSTDIR\StartEmailConverge.vbs"' "$INSTDIR"
    CreateShortcut "$SMPROGRAMS\${APPNAME}\Uninstall ${APPNAME}.lnk" "$INSTDIR\Uninstall.exe"
    
    ; Create desktop shortcut
    CreateShortcut "$DESKTOP\${APPNAME}.lnk" "wscript.exe" '"$INSTDIR\StartEmailConverge.vbs"' "$INSTDIR"
    
    ; Add/Remove programs registration
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$\"$INSTDIR\Uninstall.exe$\""
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "QuietUninstallString" "$\"$INSTDIR\Uninstall.exe$\" /S"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "InstallLocation" "$\"$INSTDIR$\""
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayIcon" "$\"$INSTDIR\EmailConverge.exe$\""
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "Publisher" "${COMPANYNAME}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayVersion" "${VERSIONMAJOR}.${VERSIONMINOR}.${VERSIONBUILD}"
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "VersionMajor" ${VERSIONMAJOR}
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "VersionMinor" ${VERSIONMINOR}
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoModify" 1
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoRepair" 1
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "EstimatedSize" ${INSTALLSIZE}
SectionEnd

;--------------------------------
; Uninstall section
Section "Uninstall"
    ; Delete files
    RMDir /r "$INSTDIR"
    
    ; Delete start menu shortcuts
    RMDir /r "$SMPROGRAMS\${APPNAME}"
    
    ; Delete desktop shortcut
    Delete "$DESKTOP\${APPNAME}.lnk"
    
    ; Delete registry keys
    DeleteRegKey HKLM "Software\${APPNAME}"
    DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
SectionEnd
