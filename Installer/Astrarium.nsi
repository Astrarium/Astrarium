;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Astrarium NSIS installer script ;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

; The script requires following plugins:
;
; * Inetc: 
;	https://nsis.sourceforge.io/Inetc_plug-in
;
; * Nsisunz (Unicode version): 
;	https://nsis.sourceforge.io/Nsisunz_plug-in
;
; * Dialer:
;	https://nsis.sourceforge.io/Dialer_plug-in

; Build Unicode installer
Unicode True

!include "MUI.nsh"
!include "nsdialogs.nsh"
!include "LogicLib.nsh"

!ifndef VERSION
  !define VERSION 1.0.0
  !warning "$\n$\nVERSION command line parameter is not defined. $\n"
!endif

!define PRODUCT_NAME "Astrarium"
!define PRODUCT_PUBLISHER "Alexander Krutov"
!define PRODUCT_COPYRIGHT "© Alexander Krutov"
!define PRODUCT_WEB_SITE "https://astrarium.space"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\Astrarium.exe"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\Astrarium"
!define PRODUCT_REG_ROOT "HKLM"
!define DEPLOY_DIR "..\Deploy"

!define PRODUCT_STARTMENU_REGVAL "NSIS:StartMenuDir"
!define TEMP_DIR "$TEMP\Astrarium\Install"
!define DOWNLOAD_BASE_URL "https://github.com/Astrarium/Astrarium/releases/download/v${VERSION}"

# Installer file attributes
VIFileVersion "1.0.0.0"
VIProductVersion "1.0.0.0"
VIAddVersionKey "ProductName" "${PRODUCT_NAME}"
VIAddVersionKey "ProductVersion" "${VERSION}"
VIAddVersionKey "FileVersion" "${VERSION}"
VIAddVersionKey "LegalCopyright" "${PRODUCT_COPYRIGHT}"
VIAddVersionKey "FileDescription" "Open-Source .Net Planetarium for Windows"

; The name of the installer
Name "Astrarium"

; Set text displayed at bottom of the installer window
BrandingText "${PRODUCT_NAME} ${VERSION}"

; The file to write
OutFile "${DEPLOY_DIR}\${PRODUCT_NAME}-WebInstaller.exe"

; Need admin rights to write to Program Files
RequestExecutionLevel admin

; Do not display "Space required" text
;SpaceTexts None

; The default installation directory
InstallDir $PROGRAMFILES\Astrarium
InstallDirRegKey "${PRODUCT_REG_ROOT}" "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails show
ShowUnInstDetails show

# Show a message box with a warning when the user wants to close the installer
!define MUI_ABORTWARNING

# Images settings
!define MUI_WELCOMEFINISHPAGE_BITMAP "Images\installer.bmp"
!define MUI_ICON "Images\logo.ico"
!define MUI_UNICON "Images\logo.ico"

# Language Selection Dialog Settings
!define MUI_LANGDLL_REGISTRY_ROOT "${PRODUCT_REG_ROOT}"
!define MUI_LANGDLL_REGISTRY_KEY "${PRODUCT_UNINST_KEY}"
!define MUI_LANGDLL_REGISTRY_VALUENAME "NSIS:Language"
!define MUI_LANGDLL_ALWAYSSHOW

; Uninstaller pages
!insertmacro MUI_UNPAGE_COMPONENTS
!insertmacro MUI_UNPAGE_INSTFILES

!macro CalcFolderSize FolderName
  !tempfile __TEMP_NSH_FILE
  !execute 'cmd /Q /E:ON /V:OFF /C (SET sum=0) & (FOR /R "${DEPLOY_DIR}\${FolderName}" %A IN (*) DO SET /A "sum+=%~zA" > NUL ) & ( CALL ECHO StrCpy $R0 %sum% > "${__TEMP_NSH_FILE}" )'
  !include "${__TEMP_NSH_FILE}"
  !delfile "${__TEMP_NSH_FILE}"
  !undef __TEMP_NSH_FILE
  IntOp $R0 $R0 / 1024
  SectionSetSize ${${FolderName}} $R0  
!macroend

!macro AddSection SectionName In1 In2 IsRO IsBold

Section ${IsBold}$(TextSectionTitle.${SectionName}) ${SectionName}
  SectionIn ${In1} ${In2} ${IsRO}
  SetOverwrite try 
  !insertmacro InstallComponent "${SectionName}-${VERSION}.zip" $(TextSectionTitle.${SectionName})
SectionEnd

!macroend

!macro InstallComponent PluginFile PluginDescr
  
  !insertmacro CheckInternetConnection
  
  SetOutPath ${TEMP_DIR}
  inetc::get /RESUME "" /CAPTION "$(TextDownloading) ${PluginDescr}..." /QUESTION "$(TextCancelDownloadConfirmation)" /WEAKSECURITY "${DOWNLOAD_BASE_URL}/${PluginFile}" "${PluginFile}" /END
  Pop $R0
  
  StrCmp $R0 "Cancelled" 0 +3
  MessageBox MB_OK|MB_ICONEXCLAMATION $(TextDownloadCanceled) /SD IDOK
  Quit
  
  StrCmp $R0 "OK" +3 0
  MessageBox MB_OK|MB_ICONSTOP "Download error: $R0.$\nInstallation will be aborted." /SD IDOK
  Quit
  
  nsisunz::UnzipToLog "${TEMP_DIR}\${PluginFile}" "$INSTDIR"
  Delete "${TEMP_DIR}\${PluginFile}"
  
!macroend

!macro CheckInternetConnection

  Dialer::GetConnectedState
  Pop $R0

  StrCmp $R0 "offline" 0 +3
  MessageBox MB_OK|MB_ICONSTOP $(TextNoInternetConnection) /SD IDOK
  Quit
  
!macroend

# Types of installation
InstType $(TextIstallTypeBasic)
InstType $(TextIstallTypeFull)

# Sections

!insertmacro AddSection "Astrarium"	 							1 2 "RO" "!"
!insertmacro AddSection "Astrarium.Plugins.SolarSystem" 		1 2 "RO" ""
!insertmacro AddSection "Astrarium.Plugins.BrightStars" 		1 2 "RO" ""
!insertmacro AddSection "Astrarium.Plugins.DeepSky" 			1 2 ""   ""
!insertmacro AddSection "Astrarium.Plugins.Constellations" 		1 2 ""   ""
!insertmacro AddSection "Astrarium.Plugins.Grids" 				1 2 ""   ""
!insertmacro AddSection "Astrarium.Plugins.Horizon" 			1 2 ""   ""
!insertmacro AddSection "Astrarium.Plugins.MeasureTool" 		1 2 ""   ""
!insertmacro AddSection "Astrarium.Plugins.MilkyWay" 			1 2 ""   ""
!insertmacro AddSection "Astrarium.Plugins.MinorBodies"			1 2 ""   ""
!insertmacro AddSection "Astrarium.Plugins.Tracks"	 			1 2 ""   ""
!insertmacro AddSection "Astrarium.Plugins.FOV" 				1 2 ""   ""
!insertmacro AddSection "Astrarium.Plugins.Tycho2"  			2 2 ""   ""
!insertmacro AddSection "Astrarium.Plugins.ASCOM"  				2 2 ""   ""

Section -AdditionalIcons
  SetShellVarContext all
  WriteIniStr "$INSTDIR\${PRODUCT_NAME}.url" "InternetShortcut" "URL" "${PRODUCT_WEB_SITE}"
  CreateDirectory "$SMPROGRAMS\Astrarium"
  CreateShortCut "$SMPROGRAMS\Astrarium\Astrarium.lnk" "$INSTDIR\Astrarium.exe"
  CreateShortCut "$SMPROGRAMS\Astrarium\Website.lnk" "$INSTDIR\${PRODUCT_NAME}.url"
  CreateShortCut "$SMPROGRAMS\Astrarium\Uninstall.lnk" "$INSTDIR\uninst.exe"
  CreateShortCut "$DESKTOP\Astrarium.lnk" "$INSTDIR\Astrarium.exe"  
SectionEnd

Section -Post
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr "${PRODUCT_REG_ROOT}" "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\Astrarium.exe"
  WriteRegStr ${PRODUCT_REG_ROOT} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_REG_ROOT} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_REG_ROOT} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\Astrarium.exe"
  WriteRegStr ${PRODUCT_REG_ROOT} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${VERSION}"
  WriteRegStr ${PRODUCT_REG_ROOT} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_REG_ROOT} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
SectionEnd

Section "un.Astrarium" Uninstall
  SetShellVarContext all
  Delete "$SMPROGRAMS\Astrarium\*.*"
  RMDir "$SMPROGRAMS\Astrarium"
  Delete "$DESKTOP\Astrarium.lnk"  
  Delete "$INSTDIR\*.*"
  RMDir /r "$INSTDIR"  
  DeleteRegKey "${PRODUCT_REG_ROOT}" "${PRODUCT_UNINST_KEY}"
  DeleteRegKey "${PRODUCT_REG_ROOT}" "${PRODUCT_DIR_REGKEY}"  
SectionEnd

# Section descriptions
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium} $(TextSectionDescr.Astrarium)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.BrightStars} $(TextSectionDescr.Astrarium.Plugins.BrightStars)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.SolarSystem} $(TextSectionDescr.Astrarium.Plugins.SolarSystem)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.Constellations} $(TextSectionDescr.Astrarium.Plugins.Constellations)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.DeepSky} $(TextSectionDescr.Astrarium.Plugins.DeepSky)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.Grids} $(TextSectionDescr.Astrarium.Plugins.Grids)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.Horizon} $(TextSectionDescr.Astrarium.Plugins.Horizon)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.MeasureTool} $(TextSectionDescr.Astrarium.Plugins.MeasureTool)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.MilkyWay} $(TextSectionDescr.Astrarium.Plugins.MilkyWay)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.MinorBodies} $(TextSectionDescr.Astrarium.Plugins.MinorBodies)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.Tracks} $(TextSectionDescr.Astrarium.Plugins.Tracks)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.FOV} $(TextSectionDescr.Astrarium.Plugins.FOV)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.Tycho2} $(TextSectionDescr.Astrarium.Plugins.Tycho2)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.ASCOM} $(TextSectionDescr.Astrarium.Plugins.ASCOM)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

# Section descriptions (uninstall)
!insertmacro MUI_UNFUNCTION_DESCRIPTION_BEGIN
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium} $(TextSectionDescr.Astrarium)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.BrightStars} $(TextSectionDescr.Astrarium.Plugins.BrightStars)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.SolarSystem} $(TextSectionDescr.Astrarium.Plugins.SolarSystem)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.Constellations} $(TextSectionDescr.Astrarium.Plugins.Constellations)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.DeepSky} $(TextSectionDescr.Astrarium.Plugins.DeepSky)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.Grids} $(TextSectionDescr.Astrarium.Plugins.Grids)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.Horizon} $(TextSectionDescr.Astrarium.Plugins.Horizon)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.MeasureTool} $(TextSectionDescr.Astrarium.Plugins.MeasureTool)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.MilkyWay} $(TextSectionDescr.Astrarium.Plugins.MilkyWay)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.MinorBodies} $(TextSectionDescr.Astrarium.Plugins.MinorBodies)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.Tracks} $(TextSectionDescr.Astrarium.Plugins.Tracks)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.FOV} $(TextSectionDescr.Astrarium.Plugins.FOV)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.Tycho2} $(TextSectionDescr.Astrarium.Plugins.Tycho2)
!insertmacro MUI_DESCRIPTION_TEXT ${Astrarium.Plugins.ASCOM} $(TextSectionDescr.Astrarium.Plugins.ASCOM)
!insertmacro MUI_UNFUNCTION_DESCRIPTION_END

# Welcome page
!insertmacro MUI_PAGE_WELCOME

# Directory page
!insertmacro MUI_PAGE_DIRECTORY

# Start menu page
var ICONS_GROUP
!define MUI_STARTMENUPAGE_NODISABLE
!define MUI_STARTMENUPAGE_DEFAULTFOLDER "${PRODUCT_NAME}"
!define MUI_STARTMENUPAGE_REGISTRY_ROOT "${PRODUCT_REG_ROOT}"
!define MUI_STARTMENUPAGE_REGISTRY_KEY "${PRODUCT_UNINST_KEY}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "${PRODUCT_STARTMENU_REGVAL}"
!insertmacro MUI_PAGE_STARTMENU Application $ICONS_GROUP

# Components page
!insertmacro MUI_PAGE_COMPONENTS

# Instfiles page
!insertmacro MUI_PAGE_INSTFILES

# Finish page
!define MUI_FINISHPAGE_RUN "$INSTDIR\Astrarium.exe"
!insertmacro MUI_PAGE_FINISH

# Available installation languages
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "Russian"

# Localization strings

; Basic installation type
LangString TextIstallTypeBasic ${LANG_ENGLISH} "Basic"
LangString TextIstallTypeBasic ${LANG_RUSSIAN} "Основная"

; Full installation type
LangString TextIstallTypeFull ${LANG_ENGLISH} "Full"
LangString TextIstallTypeFull ${LANG_RUSSIAN} "Полная"

LangString TextDownloading ${LANG_ENGLISH} "Downloading"
LangString TextDownloading ${LANG_RUSSIAN} "Загрузка"

; Section "Astrarium"
LangString TextSectionTitle.Astrarium ${LANG_ENGLISH} "Astrarium"
LangString TextSectionTitle.Astrarium ${LANG_RUSSIAN} "Astrarium"
LangString TextSectionDescr.Astrarium ${LANG_ENGLISH} "Contains application core and main components needed for running the planetarium"
LangString TextSectionDescr.Astrarium ${LANG_RUSSIAN} "Содержит ядро программы и основные библиотеки, нужные для запуска планетария"

; Section "BrightStars"
LangString TextSectionTitle.Astrarium.Plugins.BrightStars ${LANG_ENGLISH} "Bright Stars Catalogue"
LangString TextSectionTitle.Astrarium.Plugins.BrightStars ${LANG_RUSSIAN} "Каталог ярких звёзд"
LangString TextSectionDescr.Astrarium.Plugins.BrightStars ${LANG_ENGLISH} "Contains Yale Bright Star catalogue"
LangString TextSectionDescr.Astrarium.Plugins.BrightStars ${LANG_RUSSIAN} "Содержит общий каталог ярких звёзд"

; Section "SolarSystem"
LangString TextSectionTitle.Astrarium.Plugins.SolarSystem ${LANG_ENGLISH} "Sun, Moon and planets"
LangString TextSectionTitle.Astrarium.Plugins.SolarSystem ${LANG_RUSSIAN} "Солнце, Луна и планеты"
LangString TextSectionDescr.Astrarium.Plugins.SolarSystem ${LANG_ENGLISH} "Displays Sun, Moon and eight major planets and theirs satellites"
LangString TextSectionDescr.Astrarium.Plugins.SolarSystem ${LANG_RUSSIAN} "Отображает Солнце, Луну и восемь больших планет и их спутники"

; Section "DeepSky"
LangString TextSectionTitle.Astrarium.Plugins.DeepSky ${LANG_ENGLISH} "DeepSky Objects Plugin"
LangString TextSectionTitle.Astrarium.Plugins.DeepSky ${LANG_RUSSIAN} "Плагин Объекты дальнего космоса"
LangString TextSectionDescr.Astrarium.Plugins.DeepSky ${LANG_ENGLISH} "Contains objects from NGC and IC catalogues of deep sky objects"
LangString TextSectionDescr.Astrarium.Plugins.DeepSky ${LANG_RUSSIAN} "Содержит каталоги объектов дальнего космоса NGC и IC"

; Section "Constellations"
LangString TextSectionTitle.Astrarium.Plugins.Constellations ${LANG_ENGLISH} "Constellations Plugin"
LangString TextSectionTitle.Astrarium.Plugins.Constellations ${LANG_RUSSIAN} "Плагин Созвездия"
LangString TextSectionDescr.Astrarium.Plugins.Constellations ${LANG_ENGLISH} "Displays constellations lines, names and boundaries"
LangString TextSectionDescr.Astrarium.Plugins.Constellations ${LANG_RUSSIAN} "Отображает названия, границы и линии созвездий"

; Section "Grids"
LangString TextSectionTitle.Astrarium.Plugins.Grids ${LANG_ENGLISH} "Grids Plugin"
LangString TextSectionTitle.Astrarium.Plugins.Grids ${LANG_RUSSIAN} "Плагин Линии и сетки"
LangString TextSectionDescr.Astrarium.Plugins.Grids ${LANG_ENGLISH} "Displays coordinate grids, lines of ecliptic and galactical equator"
LangString TextSectionDescr.Astrarium.Plugins.Grids ${LANG_RUSSIAN} "Отображает координатные сетки, линии эклиптики и галактического экватора"

; Section "Horizon"
LangString TextSectionTitle.Astrarium.Plugins.Horizon ${LANG_ENGLISH} "Horizon Plugin"
LangString TextSectionTitle.Astrarium.Plugins.Horizon ${LANG_RUSSIAN} "Плагин Линия горизонта"
LangString TextSectionDescr.Astrarium.Plugins.Horizon ${LANG_ENGLISH} "Displays horizon line and directions labels"
LangString TextSectionDescr.Astrarium.Plugins.Horizon ${LANG_RUSSIAN} "Отображает линию горизонта и стороны света"

; Section "MeasureTool"
LangString TextSectionTitle.Astrarium.Plugins.MeasureTool ${LANG_ENGLISH} "Measure Tool Plugin"
LangString TextSectionTitle.Astrarium.Plugins.MeasureTool ${LANG_RUSSIAN} "Плагин Линейка"
LangString TextSectionDescr.Astrarium.Plugins.MeasureTool ${LANG_ENGLISH} "Adds measure tool to the application"
LangString TextSectionDescr.Astrarium.Plugins.MeasureTool ${LANG_RUSSIAN} "Добавляет инструмент Линейка для измерения угловых расстояний между объектами"

; Section "MilkyWay"
LangString TextSectionTitle.Astrarium.Plugins.MilkyWay ${LANG_ENGLISH} "Milky Way Plugin"
LangString TextSectionTitle.Astrarium.Plugins.MilkyWay ${LANG_RUSSIAN} "Плагин Млечный путь"
LangString TextSectionDescr.Astrarium.Plugins.MilkyWay ${LANG_ENGLISH} "Adds Milky Way outline to the map"
LangString TextSectionDescr.Astrarium.Plugins.MilkyWay ${LANG_RUSSIAN} "Добавляет контур Млечного пути на карту"

; Section "MinorBodies"
LangString TextSectionTitle.Astrarium.Plugins.MinorBodies ${LANG_ENGLISH} "Asteroids and Comets Plugin"
LangString TextSectionTitle.Astrarium.Plugins.MinorBodies ${LANG_RUSSIAN} "Плагин Астероиды и кометы"
LangString TextSectionDescr.Astrarium.Plugins.MinorBodies ${LANG_ENGLISH} "Shows asteroids and comets on the map"
LangString TextSectionDescr.Astrarium.Plugins.MinorBodies ${LANG_RUSSIAN} "Отображает астероды и кометы на карте"

; Section "Tracks"
LangString TextSectionTitle.Astrarium.Plugins.Tracks ${LANG_ENGLISH} "Motion tracks Plugin"
LangString TextSectionTitle.Astrarium.Plugins.Tracks ${LANG_RUSSIAN} "Плагин Траектории движения"
LangString TextSectionDescr.Astrarium.Plugins.Tracks ${LANG_ENGLISH} "Shows motion tracks for celestial bodies"
LangString TextSectionDescr.Astrarium.Plugins.Tracks ${LANG_RUSSIAN} "Позволяет строить треки движения небесных объектов на карте"

; Section "FOV"
LangString TextSectionTitle.Astrarium.Plugins.FOV ${LANG_ENGLISH} "Field Of View Plugin"
LangString TextSectionTitle.Astrarium.Plugins.FOV ${LANG_RUSSIAN} "Плагин Поле зрения"
LangString TextSectionDescr.Astrarium.Plugins.FOV ${LANG_ENGLISH} "Allows to display field of view of telescope, binocular or digital camera"
LangString TextSectionDescr.Astrarium.Plugins.FOV ${LANG_RUSSIAN} "Позволяет отобразить на карте поле зрения телескопа, бинокля или камеры"

; Section "Tycho2"
LangString TextSectionTitle.Astrarium.Plugins.Tycho2 ${LANG_ENGLISH} "Tycho2 Star Catalogue"
LangString TextSectionTitle.Astrarium.Plugins.Tycho2 ${LANG_RUSSIAN} "Звёздный каталог Tycho2"
LangString TextSectionDescr.Astrarium.Plugins.Tycho2 ${LANG_ENGLISH} "Contains about 2.5M stars from Tycho2 star catalogue"
LangString TextSectionDescr.Astrarium.Plugins.Tycho2 ${LANG_RUSSIAN} "Содержит около 2.5 млн звёзд из каталога Tycho2"

; Section "ASCOM"
LangString TextSectionTitle.Astrarium.Plugins.ASCOM ${LANG_ENGLISH} "ASCOM Telescope Control"
LangString TextSectionTitle.Astrarium.Plugins.ASCOM ${LANG_RUSSIAN} "Управление телескопом через платформу ASCOM"
LangString TextSectionDescr.Astrarium.Plugins.ASCOM ${LANG_ENGLISH} "Allows to telescope control via ASCOM platform"
LangString TextSectionDescr.Astrarium.Plugins.ASCOM ${LANG_RUSSIAN} "Позволяет управлять телескопом или монтировкой через платформу ASCOM"

LangString TextDownloadCanceled ${LANG_ENGLISH} "Download canceled.$\nInstallation will be aborted."
LangString TextDownloadCanceled ${LANG_RUSSIAN} "Скачивание отменено.$\nУстановка будет прервана."

LangString TextNoInternetConnection ${LANG_ENGLISH} "No internet connection. Please check you connection and try again.$\nInstallation will be aborted."
LangString TextNoInternetConnection ${LANG_RUSSIAN} "Нет подключения к сети. Пожалуйста, проверьте подключение к интернет.$\nУстановка будет прервана."

LangString TextCancelDownloadConfirmation ${LANG_ENGLISH} "Are you sure to cancel downloading?$\nInstallation will be aborted."
LangString TextCancelDownloadConfirmation ${LANG_RUSSIAN} "Прервать скачивание?$\nУстановка будет прервана."

Function .onInit
  !insertmacro MUI_LANGDLL_DISPLAY  
  !insertmacro CalcFolderSize "Astrarium" 
  !insertmacro CalcFolderSize "Astrarium.Plugins.BrightStars"
  !insertmacro CalcFolderSize "Astrarium.Plugins.Constellations"
  !insertmacro CalcFolderSize "Astrarium.Plugins.DeepSky"
  !insertmacro CalcFolderSize "Astrarium.Plugins.FOV"
  !insertmacro CalcFolderSize "Astrarium.Plugins.Grids"
  !insertmacro CalcFolderSize "Astrarium.Plugins.Horizon"
  !insertmacro CalcFolderSize "Astrarium.Plugins.MeasureTool"
  !insertmacro CalcFolderSize "Astrarium.Plugins.MilkyWay"
  !insertmacro CalcFolderSize "Astrarium.Plugins.MinorBodies"
  !insertmacro CalcFolderSize "Astrarium.Plugins.SolarSystem"
  !insertmacro CalcFolderSize "Astrarium.Plugins.Tracks"
  !insertmacro CalcFolderSize "Astrarium.Plugins.Tycho2"  
FunctionEnd

Function un.onInit
  !insertmacro MUI_LANGDLL_DISPLAY
FunctionEnd


