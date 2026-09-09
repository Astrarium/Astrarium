; Astrarium Inno Setup project file
; Copyright Alexander Krutov, 2018-2026
; https://astrarium.space/

#include "idp.iss"
#ifdef UNICODE
  #include "unicode\idplang\default.iss"
  #include "unicode\idplang\Russian.iss"
#else
  #include "ansi\idplang\default.iss"
  #include "ansi\idplang\Russian.iss"
#endif

; Include plugin data
#include "PluginData.iss"

#ifndef VERSION
  #define VERSION "1.0"
  #pragma warning "Version is not provided from command line, setting default value"
#endif

#define DOWNLOAD_BASE_URL "https://github.com/Astrarium/Astrarium/releases/download/v"
#define CurrentYear GetDateTimeString('yyyy', '', '')

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
AppCopyright          = "© Alexander Krutov, 2018-{#CurrentYear}"
AppPublisher          = Alexander Krutov
AppPublisherURL       = https://astrarium.space/
AppUpdatesURL         = https://astrarium.space/
AppVersion            = {#VERSION}
VersionInfoVersion    = {#VERSION}
ShowComponentSizes    = yes
UsePreviousSetupType  = no
WizardSizePercent     = 150,150

; Directories for setup compiler input and output
SourceDir             = "."
OutputDir             = "..\Deploy"

; Splash image: 164x314
WizardImageFile       = "Images\installer.bmp"
WizardImageStretch    = yes

; Small logo image: 64x71
WizardSmallImageFile  = "Images\logo.bmp"

[Languages]
Name: "en"; MessagesFile: "compiler:default.isl"; 
Name: "ru"; MessagesFile: "compiler:Languages\Russian.isl";

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

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
      idpClearFiles;
      Explode(Components, WizardSelectedComponents(False), ',');
      
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
  Explode(Components, WizardSelectedComponents(False), ',');
      
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
  idpDownloadAfter(wpReady);

  { Make download wizard page resizeable }
  IDPForm.TotalProgressBar.Anchors := [akLeft, akTop, akRight];
  IDPForm.FileProgressBar.Anchors := [akLeft, akTop, akRight];
  IDPForm.TotalDownloaded.Anchors := [akTop, akRight];
  IDPForm.FileDownloaded.Anchors := [akTop, akRight];
  IDPForm.DetailsButton.Anchors := [akTop, akRight];

end;