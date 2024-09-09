; Astrarium Inno Setup project file
; Copyright Alexander Krutov, 2018-2024
; https://astrarium.space/

#include "idp.iss"
#ifdef UNICODE
  #include "unicode\idplang\default.iss"
  #include "unicode\idplang\Russian.iss"
#else
  #include "ansi\idplang\default.iss"
  #include "ansi\idplang\Russian.iss"
#endif

#ifndef VERSION
  #define VERSION "1.0"
  #pragma warning "Version is not provided from command line, setting default value"
#endif

#define DOWNLOAD_BASE_URL "https://github.com/Astrarium/Astrarium/releases/download/v"

[Setup] 
AppName               = "Astrarium"
AppVerName            = "Astrarium"
WizardStyle           = modern
DefaultDirName        = "{autopf}\Astrarium"
DefaultGroupName      = "Astrarium"
UninstallDisplayIcon  = {app}\Astrarium\Astrarium.exe
Compression           = lzma2
SolidCompression      = yes
OutputBaseFilename    = Astrarium-setup
AppCopyright          = "© Alexander Krutov, 2018-2024"
AppPublisher          = Alexander Krutov
AppPublisherURL       = https://astrarium.space/
AppUpdatesURL         = https://astrarium.space/
AppVersion            = {#VERSION}
VersionInfoVersion    = {#VERSION}
ShowComponentSizes    = yes
UsePreviousSetupType  = no

; Directories for setup compiler input and output
SourceDir             = "."
OutputDir             = "..\Deploy"

; Splash image: 164x314
WizardImageFile       = "Images\installer.bmp"
WizardImageStretch    = yes

; Small logo image: 55x55
WizardSmallImageFile  = "Images\logo.bmp"

[Languages]
Name: "en"; MessagesFile: "compiler:default.isl"; 
Name: "ru"; MessagesFile: "compiler:Languages\Russian.isl";

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

#define private CalcDirSize(str path, int size = -1) \
    CalcFileSize(path, FindFirst(AddBackSlash(path) + '*.*', faAnyFile), size)

#define private CalcPackageSize(str path) \
    FileSize(path + '-' + VERSION + '.zip')

#define private CalcFileSize(str path, int handle, int size) \
    handle ? CalcFileSizeFilterPath(path, handle, size) : size

#define private CalcFileSizeFilterPath(str path, int handle, int size) \
    FindGetFilename(handle) == '.' || FindGetFilename(handle) == '..' ? \
        GoToNextFile(path, handle, size) : \
        CalcFileSizeTestIfDir(path, handle, size, AddBackSlash(path) + FindGetFilename(handle))

#define private GoToNextFile(str path, int handle, int size) \
    FindNext(handle) ? CalcFileSizeFilterPath(path, handle, size) : size

#define private CalcFileSizeTestIfDir(str path, int handle, int size, str filename) \
    DirExists(filename) ? CalcDirSize(filename) + GoToNextFile(path, handle, size) : \
        GoToNextFile(path, handle, size + FileSize(filename))

[CustomMessages]
ru.Astrarium                                = Ядро программы Astrarium 
en.Astrarium                                = Astrarium application core
#define sz_Astrarium                        = CalcDirSize('..\Deploy\Astrarium')
sz_Astrarium                                = {#CalcPackageSize('..\Deploy\Astrarium')}

ru.Astrarium_Plugins_SolarSystem            = Солнце, Луна и планеты
en.Astrarium_Plugins_SolarSystem            = Sun, Moon and planets
#define sz_Astrarium_Plugins_SolarSystem    = CalcDirSize('..\Deploy\Astrarium.Plugins.SolarSystem')
sz_Astrarium_Plugins_SolarSystem            = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.SolarSystem')}

ru.Astrarium_Plugins_BrightStars            = Каталог ярких звёзд
en.Astrarium_Plugins_BrightStars            = Bright Stars Catalogue
#define sz_Astrarium_Plugins_BrightStars    = CalcDirSize('..\Deploy\Astrarium.Plugins.BrightStars')
sz_Astrarium_Plugins_BrightStars            = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.BrightStars')}

ru.Astrarium_Plugins_Constellations         = Линии, названия и границы созвездий
en.Astrarium_Plugins_Constellations         = Constellations lines, names and boundaries
#define sz_Astrarium_Plugins_Constellations = CalcDirSize('..\Deploy\Astrarium.Plugins.Constellations')
sz_Astrarium_Plugins_Constellations         = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.Constellations')}

ru.Astrarium_Plugins_Grids                  = Линии и сетки
en.Astrarium_Plugins_Grids                  = Celestial grids and lines
#define sz_Astrarium_Plugins_Grids          = CalcDirSize('..\Deploy\Astrarium.Plugins.Grids')
sz_Astrarium_Plugins_Grids                  = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.Grids')}

ru.Astrarium_Plugins_DeepSky                = Объекты дальнего космоса
en.Astrarium_Plugins_DeepSky                = Deep Sky Objects
#define sz_Astrarium_Plugins_DeepSky        = CalcDirSize('..\Deploy\Astrarium.Plugins.DeepSky')
sz_Astrarium_Plugins_DeepSky                = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.DeepSky')}

ru.Astrarium_Plugins_MinorBodies            = Астероиды и кометы
en.Astrarium_Plugins_MinorBodies            = Asteroids and comets
#define sz_Astrarium_Plugins_MinorBodies    = CalcDirSize('..\Deploy\Astrarium.Plugins.MinorBodies')
sz_Astrarium_Plugins_MinorBodies            = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.MinorBodies')}

ru.Astrarium_Plugins_Meteors            	  = Метеорные потоки
en.Astrarium_Plugins_Meteors            	  = Meteor showers
#define sz_Astrarium_Plugins_Meteors    	  = CalcDirSize('..\Deploy\Astrarium.Plugins.Meteors')
sz_Astrarium_Plugins_Meteors            	  = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.Meteors')}

ru.Astrarium_Plugins_Novae          		    = Новые звёзды
en.Astrarium_Plugins_Novae             		  = Novae stars
#define sz_Astrarium_Plugins_Novae     		  = CalcDirSize('..\Deploy\Astrarium.Plugins.Novae')
sz_Astrarium_Plugins_Novae              	  = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.Novae')}

ru.Astrarium_Plugins_Supernovae          	 = Сверхновые звёзды
en.Astrarium_Plugins_Supernovae             = Supernovae stars
#define sz_Astrarium_Plugins_Supernovae     = CalcDirSize('..\Deploy\Astrarium.Plugins.Supernovae')
sz_Astrarium_Plugins_Supernovae              = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.Supernovae')}

ru.Astrarium_Plugins_Satellites          	= Искусственные спутники Земли
en.Astrarium_Plugins_Satellites             = Artificial satellites
#define sz_Astrarium_Plugins_Satellites     = CalcDirSize('..\Deploy\Astrarium.Plugins.Satellites')
sz_Astrarium_Plugins_Satellites             = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.Satellites')}

ru.Astrarium_Plugins_Eclipses               = Затмения
en.Astrarium_Plugins_Eclipses               = Eclipses
#define sz_Astrarium_Plugins_Eclipses       = CalcDirSize('..\Deploy\Astrarium.Plugins.Eclipses')
sz_Astrarium_Plugins_Eclipses               = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.Eclipses')}

ru.Astrarium_Plugins_JupiterMoons           = Спутники Юпитера и БКП
en.Astrarium_Plugins_JupiterMoons           = Jupiter moons events and GRS
#define sz_Astrarium_Plugins_JupiterMoons   = CalcDirSize('..\Deploy\Astrarium.Plugins.JupiterMoons')
sz_Astrarium_Plugins_JupiterMoons           = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.JupiterMoons')}

ru.Astrarium_Plugins_Horizon                = Горизонт и ландшафт
en.Astrarium_Plugins_Horizon                = Horizon and landscape
#define sz_Astrarium_Plugins_Horizon        = CalcDirSize('..\Deploy\Astrarium.Plugins.Horizon')
sz_Astrarium_Plugins_Horizon                = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.Horizon')}

ru.Astrarium_Plugins_Atmosphere             = Атмосфера
en.Astrarium_Plugins_Atmosphere             = Atmosphere 
#define sz_Astrarium_Plugins_Atmosphere     = CalcDirSize('..\Deploy\Astrarium.Plugins.Atmosphere')
sz_Astrarium_Plugins_Atmosphere             = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.Atmosphere')}

ru.Astrarium_Plugins_MilkyWay               = Контур Млечного пути
en.Astrarium_Plugins_MilkyWay               = Milky Way outline
#define sz_Astrarium_Plugins_MilkyWay       = CalcDirSize('..\Deploy\Astrarium.Plugins.MilkyWay')
sz_Astrarium_Plugins_MilkyWay               = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.MilkyWay')}

ru.Astrarium_Plugins_MeasureTool            = Инструмент Линейка
en.Astrarium_Plugins_MeasureTool            = Measure tool
#define sz_Astrarium_Plugins_MeasureTool    = CalcDirSize('..\Deploy\Astrarium.Plugins.MeasureTool')
sz_Astrarium_Plugins_MeasureTool            = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.MeasureTool')}

ru.Astrarium_Plugins_Tracks                 = Инструмент Треки движения
en.Astrarium_Plugins_Tracks                 = Motion tracks tool
#define sz_Astrarium_Plugins_Tracks         = CalcDirSize('..\Deploy\Astrarium.Plugins.Tracks')
sz_Astrarium_Plugins_Tracks                 = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.Tracks')}

ru.Astrarium_Plugins_FOV                    = Инструмент Поле зрения телескопа и окуляра
en.Astrarium_Plugins_FOV                    = Field Of View plugin
#define sz_Astrarium_Plugins_FOV            = CalcDirSize('..\Deploy\Astrarium.Plugins.FOV')
sz_Astrarium_Plugins_FOV                    = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.FOV')}

ru.Astrarium_Plugins_Tycho2                 = Звёздный каталог Tycho2
en.Astrarium_Plugins_Tycho2                 = Tycho2 star catalogue
#define sz_Astrarium_Plugins_Tycho2         = CalcDirSize('..\Deploy\Astrarium.Plugins.Tycho2')
sz_Astrarium_Plugins_Tycho2                 = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.Tycho2')}

ru.Astrarium_Plugins_UCAC4                  = Звёздный каталог UCAC4
en.Astrarium_Plugins_UCAC4                  = UCAC4 star catalogue
#define sz_Astrarium_Plugins_UCAC4          = CalcDirSize('..\Deploy\Astrarium.Plugins.UCAC4')
sz_Astrarium_Plugins_UCAC4                  = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.UCAC4')}

ru.Astrarium_Plugins_ASCOM                  = Управление телескопом через ASCOM
en.Astrarium_Plugins_ASCOM                  = ASCOM telescope control
#define sz_Astrarium_Plugins_ASCOM          = CalcDirSize('..\Deploy\Astrarium.Plugins.ASCOM')
sz_Astrarium_Plugins_ASCOM                  = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.ASCOM')}

ru.Astrarium_Plugins_Planner                = Планировщик наблюдений
en.Astrarium_Plugins_Planner                = Observation planner
#define sz_Astrarium_Plugins_Planner        = CalcDirSize('..\Deploy\Astrarium.Plugins.Planner')
sz_Astrarium_Plugins_Planner                = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.Planner')}

[Components]
Name: Astrarium;                        Description: {cm:Astrarium};                        Types: full compact custom; Flags: fixed; ExtraDiskSpaceRequired: {#sz_Astrarium};
Name: Astrarium_Plugins_SolarSystem;    Description: {cm:Astrarium_Plugins_SolarSystem};    Types: full compact custom; Flags: fixed; ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_SolarSystem};  
Name: Astrarium_Plugins_BrightStars;    Description: {cm:Astrarium_Plugins_BrightStars};    Types: full compact custom; Flags: fixed; ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_BrightStars};  
Name: Astrarium_Plugins_Constellations; Description: {cm:Astrarium_Plugins_Constellations}; Types: full compact custom; Flags: fixed; ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_Constellations};
Name: Astrarium_Plugins_Grids;          Description: {cm:Astrarium_Plugins_Grids};          Types: full compact custom; Flags: fixed; ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_Grids};
Name: Astrarium_Plugins_Horizon;        Description: {cm:Astrarium_Plugins_Horizon};        Types: full compact custom; Flags: fixed; ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_Horizon};
Name: Astrarium_Plugins_Atmosphere;     Description: {cm:Astrarium_Plugins_Atmosphere};     Types: full compact custom; Flags: fixed; ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_Atmosphere};
Name: Astrarium_Plugins_DeepSky;        Description: {cm:Astrarium_Plugins_DeepSky};        Types: full compact custom;               ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_DeepSky};
Name: Astrarium_Plugins_MinorBodies;    Description: {cm:Astrarium_Plugins_MinorBodies};    Types: full compact custom;               ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_MinorBodies};
Name: Astrarium_Plugins_Meteors;    	Description: {cm:Astrarium_Plugins_Meteors};    	Types: full compact custom;               ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_Meteors};
Name: Astrarium_Plugins_Novae;    		Description: {cm:Astrarium_Plugins_Novae};    		Types: full compact custom;               ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_Novae};
Name: Astrarium_Plugins_Supernovae;    	Description: {cm:Astrarium_Plugins_Supernovae};    	Types: full compact custom;               ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_Supernovae};
Name: Astrarium_Plugins_Satellites;    	Description: {cm:Astrarium_Plugins_Satellites};    	Types: full;               ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_Satellites};
Name: Astrarium_Plugins_JupiterMoons;   Description: {cm:Astrarium_Plugins_JupiterMoons};   Types: full compact custom;               ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_JupiterMoons};
Name: Astrarium_Plugins_Eclipses;       Description: {cm:Astrarium_Plugins_Eclipses};       Types: full compact custom;               ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_Eclipses};
Name: Astrarium_Plugins_MilkyWay;       Description: {cm:Astrarium_Plugins_MilkyWay};       Types: full compact custom;               ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_MilkyWay};
Name: Astrarium_Plugins_MeasureTool;    Description: {cm:Astrarium_Plugins_MeasureTool};    Types: full compact custom;               ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_MeasureTool};
Name: Astrarium_Plugins_Tracks;         Description: {cm:Astrarium_Plugins_Tracks};         Types: full compact custom;               ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_Tracks};
Name: Astrarium_Plugins_FOV;            Description: {cm:Astrarium_Plugins_FOV};            Types: full compact custom;               ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_FOV};
Name: Astrarium_Plugins_Tycho2;         Description: {cm:Astrarium_Plugins_Tycho2};         Types: full;                              ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_Tycho2};
Name: Astrarium_Plugins_UCAC4;          Description: {cm:Astrarium_Plugins_UCAC4};          Types: full;                              ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_UCAC4};
Name: Astrarium_Plugins_ASCOM;          Description: {cm:Astrarium_Plugins_ASCOM};          Types: full;                              ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_ASCOM};
Name: Astrarium_Plugins_Planner;        Description: {cm:Astrarium_Plugins_Planner};        Types: full;                              ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_Planner};


[Files]
Source: "7za.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; AfterInstall: AfterInstallProc

[Icons]
Name: "{group}\Astrarium"; Filename: "{app}\Astrarium\Astrarium.exe"
Name: "{userdesktop}\Astrarium"; Filename: "{app}\Astrarium\Astrarium.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\Astrarium"; Filename: "{app}\Astrarium"; Tasks: quicklaunchicon

[InstallDelete]
Type: filesandordirs; Name: "{app}\Astrarium"

[UninstallDelete]
Type: filesandordirs; Name: "{app}\Astrarium"

[Run]
Filename: {app}\Astrarium\Astrarium.exe; Description: {cm:LaunchProgram,Astrarium}; Flags: nowait postinstall skipifsilent

[Code]

///////////////////////////////////////////////////////////////////////////////////////////////////
{ gets package file name by component name }
function GetPackageFileName(ComponentName: String) : String;
var
  FileName: String;
begin
  FileName := ComponentName;
  StringChangeEx(FileName, '_', '.', True);
  Result := FileName + '-{#VERSION}.zip';
end;

///////////////////////////////////////////////////////////////////////////////////////////////////
{ adds component to download list }
procedure AddComponent(ComponentName: String);
var
  FileName: String;
  DownloadSource: String;
  TargetName: String;
  Size: Integer;
begin
  FileName := GetPackageFileName(ComponentName);
  StringChangeEx(FileName, '_', '.', True);
  DownloadSource := ExpandConstant('{#DOWNLOAD_BASE_URL}{#VERSION}/' + FileName);  
  TargetName := ExpandConstant('{tmp}\' + FileName);


  Size := StrToInt(ExpandConstant('{cm:sz_'+ ComponentName + '}'));
  //WizardForm.ComponentsList.FindComponent('core')

  idpAddFileSizeComp(DownloadSource, TargetName, Size, ComponentName);
end;

///////////////////////////////////////////////////////////////////////////////////////////////////
{ splits comma-separated string to array of strings }
procedure Explode(var Dest: TArrayOfString; Text: String; Separator: String);
var
  i, p: Integer;
begin
  i := 0;
  repeat
    SetArrayLength(Dest, i+1);
    p := Pos(Separator,Text);
    if p > 0 then begin
      Dest[i] := Copy(Text, 1, p-1);
      Text := Copy(Text, p + Length(Separator), Length(Text));
      i := i + 1;
    end else begin
      Dest[i] := Text;
      Text := '';
    end;
  until Length(Text)=0;
end;

///////////////////////////////////////////////////////////////////////////////////////////////////
function NextButtonClick(CurPageID: integer): boolean;
var 
  Components: TArrayOfString;
  i: Integer;
begin
    Result := True;
    if(CurPageID = wpSelectComponents) then
    begin
      { clear list of files to be downloaded }
      idpClearFiles;
      
      { split comma-separated string of selected components to array of Components }
      Explode(Components, WizardSelectedComponents(False), ',');
      
      { add components to download list}
      for i:=0 to GetArrayLength(Components)-1 do begin
         AddComponent(Components[i]);
      end;
    end;
end;

///////////////////////////////////////////////////////////////////////////////////////////////////
procedure AfterInstallProc();
var 
  Components: TArrayOfString;
  FileName: String;
  i: Integer;
  ResultCode: Integer;
begin
  { split comma-separated string of selected components to array of Components }
  Explode(Components, WizardSelectedComponents(False), ',');
      
  { add components to download list}
  for i:=0 to GetArrayLength(Components)-1 do begin
    FileName := GetPackageFileName(Components[i]);

    Log('Extracting ' + FileName + '...'); 
    Exec(ExpandConstant('{tmp}\7za.exe'), ExpandConstant('x "{tmp}\' + FileName + '" -o"{app}\Astrarium\" * -r -aoa'), '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Log('Extraction result ' + IntToStr(ResultCode)); 
  end;
end;

///////////////////////////////////////////////////////////////////////////////////////////////////
procedure InitializeWizard();
begin

  WizardForm.DiskSpaceLabel.Visible := False;
  //WizardForm.ComponentsDiskSpaceLabel.Visible := False;

  idpDownloadAfter(wpReady);

  { Make download wizard page resizeable }
  IDPForm.TotalProgressBar.Anchors := [akLeft, akTop, akRight];
  IDPForm.FileProgressBar.Anchors := [akLeft, akTop, akRight];
  IDPForm.TotalDownloaded.Anchors := [akTop, akRight];
  IDPForm.FileDownloaded.Anchors := [akTop, akRight];
  IDPForm.DetailsButton.Anchors := [akTop, akRight];

end;


