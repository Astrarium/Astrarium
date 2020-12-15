:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
::
:: Astarium deployment script
::
:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
::
:: The script asks version number needed for distribution, without "v" prefix, 
:: like "2020.11". The script creates bunch of ZIP files (i.e components) and 
:: single web-installer file. All files should be uploaded into GitHub as a new 
:: release named with "v" prefix, like "v2020.11". 
::
:: End user needs only web-installer to be downloaded manually. Components will 
:: be downloaded via installer automatically, according to user's choice in the 
:: installer UI.
::
:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

@echo off
SETLOCAL EnableDelayedExpansion
for /F "tokens=1,2 delims=#" %%a in ('"prompt #$H#$E# & echo on & for %%b in (1) do rem"') do (
  set "DEL=%%a"
)

set /p Version="Enter deployment version: "

rmdir /s /q "Deploy"

dotnet build Astrarium.sln -c Release /p:Deploy=True /p:DeploymentVersion=%Version%

makensis /DVERSION=%Version% Installer/Astrarium.nsi

@echo off
for /d %%a in ("Deploy\*") do rd "%%a" /q /s

echo(
call :ColorText 0a "DONE."
echo(

goto :eof

:ColorText
echo off
<nul set /p ".=%DEL%" > "%~2"
findstr /v /a:%1 /R "^$" "%~2" nul
del "%~2" > nul 2>&1
goto :eof