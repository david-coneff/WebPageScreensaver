; Installer for the FRAMEWORK-DEPENDENT build (Properties/PublishProfiles/PublishX64FrameworkDependent.pubxml).
; It installs the ~1 MB WebPageScreensaver.scr and makes sure the .NET 10 Desktop Runtime is present:
; when it is missing, it downloads Microsoft's own runtime installer and runs it, rather than
; bundling the runtime. The self-contained build needs no installer: it is a single .scr.
;
; Build (NSIS 3, zlib licence; makensis runs on Windows, Linux and macOS):
;   dotnet publish -c Release -p:PublishProfile=PublishX64FrameworkDependent
;   makensis -DVERSION=<x.y.z> installer/WebPageScreensaver.nsi
; Output: installer/WebPageScreensaver-<version>-setup.exe

Unicode true
!include "MUI2.nsh"
!include "x64.nsh"
!include "LogicLib.nsh"

!ifndef VERSION
  !define VERSION "0.0.0"
!endif
!define APP "WebPageScreensaver"
!define SRC "..\Publish\Release\x64-framework-dependent\${APP}.scr"
!define UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP}"
; Microsoft's permanent link to the latest .NET 10 Windows Desktop Runtime installer for x64.
!define RUNTIME_URL "https://aka.ms/dotnet/10.0/windowsdesktop-runtime-win-x64.exe"

Name "Web Page Screensaver ${VERSION}"
OutFile "${APP}-${VERSION}-setup.exe"
RequestExecutionLevel admin
InstallDir "$PROGRAMFILES64\${APP}"
SetCompressor /SOLID lzma

!insertmacro MUI_PAGE_LICENSE "..\LICENSE.md"
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

Function .onInit
  ${IfNot} ${RunningX64}
    MessageBox MB_ICONSTOP "This installer is for 64-bit Windows. Use the self-contained build, or build PublishX86 yourself." /SD IDOK
    Abort
  ${EndIf}
  SetRegView 64
FunctionEnd

; Sets $0 to 1 when a .NET 10 Windows Desktop Runtime is installed, else 0. This folder is where the
; .NET host looks for the runtime the .scr asks for (Microsoft.WindowsDesktop.App 10.0).
Function HasRuntime
  StrCpy $0 0
  FindFirst $1 $2 "$PROGRAMFILES64\dotnet\shared\Microsoft.WindowsDesktop.App\10.*"
  ${If} $2 != ""
    StrCpy $0 1
  ${EndIf}
  FindClose $1
FunctionEnd

Section "Install"
  Call HasRuntime
  ${If} $0 == 0
    MessageBox MB_YESNO|MB_ICONQUESTION "The .NET 10 Desktop Runtime is not installed. Download and install it from Microsoft now?" /SD IDYES IDYES get
    MessageBox MB_ICONSTOP "The screensaver needs the .NET 10 Desktop Runtime. Install it, or use the self-contained build." /SD IDOK
    Abort
    get:
    DetailPrint "Downloading the .NET 10 Desktop Runtime from Microsoft..."
    ; curl.exe ships with Windows 10 (1803) and later; NSIS's own downloader has no HTTPS.
    nsExec::ExecToLog '"$WINDIR\System32\curl.exe" -fL --retry 3 -o "$TEMP\windowsdesktop-runtime-10-x64.exe" "${RUNTIME_URL}"'
    Pop $1
    ${If} $1 != 0
      MessageBox MB_ICONSTOP "The download failed (curl exit $1). Get it from https://dotnet.microsoft.com/download/dotnet/10.0, install it, and run this setup again." /SD IDOK
      Abort
    ${EndIf}
    DetailPrint "Running Microsoft's .NET 10 Desktop Runtime installer..."
    ExecWait '"$TEMP\windowsdesktop-runtime-10-x64.exe" /install /quiet /norestart' $1
    Delete "$TEMP\windowsdesktop-runtime-10-x64.exe"
    ; 3010: installed, a restart is pending; the runtime is usable now.
    ${If} $1 != 0
    ${AndIf} $1 != 3010
      MessageBox MB_ICONSTOP "The .NET runtime installer failed (exit $1)." /SD IDOK
      Abort
    ${EndIf}
  ${EndIf}

  ; A screensaver is listed in the Screen Saver Settings when it lives in System32. This setup is
  ; 32-bit, so file-system redirection is turned off to write the real (64-bit) System32.
  ${DisableX64FSRedirection}
  SetOutPath "$WINDIR\System32"
  File "${SRC}"
  ${EnableX64FSRedirection}

  SetOutPath "$INSTDIR"
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  WriteRegStr HKLM "${UNINST_KEY}" "DisplayName" "Web Page Screensaver"
  WriteRegStr HKLM "${UNINST_KEY}" "DisplayVersion" "${VERSION}"
  WriteRegStr HKLM "${UNINST_KEY}" "Publisher" "WebPageScreensaver"
  WriteRegStr HKLM "${UNINST_KEY}" "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegDWORD HKLM "${UNINST_KEY}" "NoModify" 1
  WriteRegDWORD HKLM "${UNINST_KEY}" "NoRepair" 1

  ; Open the Screen Saver Settings with this screensaver selected (what right-click > Install does).
  ${IfNot} ${Silent}
    ${DisableX64FSRedirection}
    Exec '"$WINDIR\System32\rundll32.exe" desk.cpl,InstallScreenSaver "$WINDIR\System32\${APP}.scr"'
    ${EnableX64FSRedirection}
  ${EndIf}
SectionEnd

Function un.onInit
  SetRegView 64
FunctionEnd

Section "Uninstall"
  ${DisableX64FSRedirection}
  Delete "$WINDIR\System32\${APP}.scr"
  ${EnableX64FSRedirection}
  Delete "$INSTDIR\Uninstall.exe"
  RMDir "$INSTDIR"
  DeleteRegKey HKLM "${UNINST_KEY}"
  ; The .NET runtime is left installed: other applications may use it.
SectionEnd
