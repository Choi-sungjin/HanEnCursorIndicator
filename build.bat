@echo off
setlocal

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

"%CSC%" /nologo /target:winexe /platform:anycpu /optimize+ /win32manifest:"%~dp0app.manifest" /out:"%~dp0CursorImeIndicator.exe" "%~dp0CursorImeIndicator.cs" /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll

if errorlevel 1 (
    echo Build failed.
    exit /b 1
)

echo Built "%~dp0CursorImeIndicator.exe"
