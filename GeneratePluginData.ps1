# GeneratePluginData.ps1
param(
    [string]$SourceRoot = ".",
    [string]$OutputFile = "Installer\PluginData.iss"
)

# Find plugin files
$pluginProjects = Get-ChildItem -Path $SourceRoot -Filter "*.csproj" -Recurse | Where-Object {
    $_.DirectoryName -match "Astrarium\.Plugins\."
} | Sort-Object Name

if ($pluginProjects.Count -eq 0) {
    Write-Warning "No plugin projects found matching 'Astrarium.Plugins.'"
}

$pluginData = @"

; Auto-generated plugin data
; DO NOT EDIT MANUALLY

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
Astrarium = Application core
#define sz_Astrarium = CalcDirSize('..\Deploy\Astrarium')
sz_Astrarium = {#CalcPackageSize('..\Deploy\Astrarium')}
"@

$coreComponentData = @"

[Components]
Name: Astrarium; Description: {cm:Astrarium}; Types: full compact custom; Flags: fixed; ExtraDiskSpaceRequired: {#sz_Astrarium};
"@

$optionalComponentData = "`n"
$regularComponentData = "`n"

foreach ($project in $pluginProjects) {
    $projectName = $project.BaseName
    $pluginName = $projectName -replace "Astrarium\.Plugins\.", ""
    
    # Read csproj
    $csprojContent = Get-Content $project.FullName -Raw
    $description = ""
    
    # Try to find Description in csproj
    if ($csprojContent -match '<Description>(.*?)<\/Description>') {
        $description = $matches[1].Trim()
    }
    
    # If description not found
    if ([string]::IsNullOrEmpty($description)) {
        $description = $pluginName
    }
	
	$isCorePlugin = $false
    $isOptionalPlugin = $false
    
    # Check attributes are in csproj
	if ($csprojContent -match '<PluginType>skipped</PluginType>') {
		Get-ChildItem -Path "Deploy" -Filter "Astrarium.Plugins.${pluginName}-*.zip" -File | Remove-Item -Force
        continue
    }
	if ($csprojContent -match '<PluginType>core</PluginType>') {
        $isCorePlugin = $true
	}
    if ($csprojContent -match '<PluginType>optional</PluginType>') {
        $isOptionalPlugin = $true
    }
    
    # Add to custom messages
    $pluginData += @"

Astrarium_Plugins_${pluginName} = $pluginName : $description
#define sz_Astrarium_Plugins_${pluginName} = CalcDirSize('..\Deploy\Astrarium.Plugins.${pluginName}')
sz_Astrarium_Plugins_${pluginName} = {#CalcPackageSize('..\Deploy\Astrarium.Plugins.${pluginName}')}
"@

    # Make component line for Inno Setup script
    $componentLine = "Name: Astrarium_Plugins_${pluginName}; Description: {cm:Astrarium_Plugins_${pluginName}};"
    
    if ($isCorePlugin) {
        $componentLine += " Types: full compact custom; Flags: fixed;"
    } elseif ($isOptionalPlugin) {
        $componentLine += " Types: full;"
    } else {
        $componentLine += " Types: full compact custom;"
    }
    
    $componentLine += " ExtraDiskSpaceRequired: {#sz_Astrarium_Plugins_${pluginName}};"
    
    if ($isCorePlugin) {
        $coreComponentData += "`n$componentLine"
    } elseif ($isOptionalPlugin) {
        $optionalComponentData += "`n$componentLine"
    } else {
        $regularComponentData += "`n$componentLine"
    }
}

# Save result
$fullContent = $pluginData + "`n`n" + $coreComponentData + $regularComponentData + $optionalComponentData
$fullContent | Out-File -FilePath $OutputFile -Encoding UTF8

Write-Host "Plugin data generated to $OutputFile"