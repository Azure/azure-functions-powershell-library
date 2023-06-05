#!/usr/bin/env pwsh
#
# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

[CmdletBinding()]
param(
    [switch]
    $Clean,

    [switch]
    $Bootstrap,

    [switch]
    $Test,

    [switch]
    $NoBuild,

    [switch]
    $Deploy,

    [string]
    $CoreToolsDir,

    [string]
    $Configuration = "Debug",

    [string]
    $BuildNumber = '0'
)

#Requires -Version 7.2

$PowerShellVersion = '7.4'
$TargetFramework = 'net6.0'
$ModuleName = 'AzureFunctions.PowerShell.SDK'
$ModuleFiles = @(
    "AzureFunctions.PowerShell.SDK.dll"
    "AzureFunctions.AttributeDefinitions.ps1"
    "AzureFunctions.PowerShell.SDK.psd1"
)

Write-Host "Build configuration: $Configuration"

function Get-FunctionsCoreToolsDir {
    if ($CoreToolsDir) {
        $CoreToolsDir
    } else {
        $funcPath = (Get-Command func).Source
        if (-not $funcPath) {
            throw 'Cannot find "func" command. Please install Azure Functions Core Tools: ' +
                  'see https://github.com/Azure/azure-functions-core-tools#installing for instructions'
        }

        # func may be just a symbolic link, so we need to follow it until we find the true location
        while ((((Get-Item $funcPath).Attributes) -band 'ReparsePoint') -ne 0) {
            $funcPath = (Get-Item $funcPath).Target
        }

        $funcParentDir = Split-Path -Path $funcPath -Parent

        if (-not (Test-Path -Path $funcParentDir/workers/powershell -PathType Container)) {
            throw 'Cannot find Azure Function Core Tools installation directory. ' +
                  'Please provide the path in the CoreToolsDir parameter.'
        }

        $funcParentDir
    }
}

function Deploy-FunctionsSDKModule {
    $ErrorActionPreference = 'Stop'

    $publishDir = "./src/bin/$Configuration/$TargetFramework/publish"

    $manifestName = $ModuleName + '.psd1'
    $manifestFilePath = (Get-ChildItem -Path "$(Get-FunctionsCoreToolsDir)/workers/powershell/$PowerShellVersion/Modules/$ModuleName/*/$manifestName") |
                        Select-Object -First 1 | ForEach-Object { $_.FullName }
    Write-Log "manifest file path: $manifestFilePath"
    $powerShellWorkerModuleDir = Split-Path $manifestFilePath
    Write-Log "Deploying module to $powerShellWorkerModuleDir..."

    if (-not $IsWindows) {
        sudo chmod -R a+w $powerShellWorkerModuleDir
    }

    if (!(Test-Path $powerShellWorkerModuleDir)) {
        New-Item $powerShellWorkerModuleDir -ItemType directory
    }

    Remove-Item -Path $powerShellWorkerModuleDir/* -Recurse -Force -ErrorAction SilentlyContinue

    foreach ($fileName in $ModuleFiles)
    {
        $sourceFilePath = Join-Path $publishDir $fileName
        Write-Log "Copying file '$sourceFilePath' to  $powerShellWorkerModuleDir"
        Copy-Item -Path $sourceFilePath -Destination $powerShellWorkerModuleDir -Recurse -Force
    }

    Write-Log "Deployed module to $powerShellWorkerModuleDir"
}

Import-Module "$PSScriptRoot/tools/helper.psm1" -Force

# Bootstrap step
if ($Bootstrap.IsPresent) {
    Write-Log "Validate and install missing prerequisites for building ..."
    Install-Dotnet

    if (-not (Get-Module -Name Pester -ListAvailable)) {
        Write-Log -Warning "Module 'Pester' is missing. Installing 'Pester' ..."
        Install-Module -Name Pester
    }
}

# Clean step
if ($Clean.IsPresent) {
    Push-Location $PSScriptRoot
    git clean -fdX
    Pop-Location
}

# Common step required by both build and test
Find-Dotnet

# Build step
if (!$NoBuild.IsPresent) {
    
    # Generate C# files for resources
    Start-ResGen -Force

    Write-Host "Building at $PSScriptRoot"

    dotnet publish -c $Configuration "/p:BuildNumber=$BuildNumber" $PSScriptRoot

    $publishDir = "./src/bin/$Configuration/$TargetFramework/publish" 
    $buildDir = "./src/bin/$Configuration/$TargetFramework" 

    $psFiles = Get-ChildItem "$PSScriptRoot/src" | Where-Object -Property Extension -in -Value ".ps1", ".psm1", ".psd1"

    $psFiles | ForEach-Object { Copy-Item -Path $_.FullName -Destination $publishDir }
    $psFiles | ForEach-Object { Copy-Item -Path $_.FullName -Destination $buildDir }

    # dotnet pack -c $Configuration "/p:BuildNumber=$BuildNumber" "$PSScriptRoot/package"
}

# Test step
if ($Test.IsPresent) {
    # Dotnet test phase
    # dotnet test "$PSScriptRoot/test/Unit"
    # if ($LASTEXITCODE -ne 0) { throw "xunit tests failed." }

    # Pester test phase - step 1: get the newly built module into the PSModulePath
    # TODO: Need to test conflicts, module with same name existing in another PsModulePath folder
    $publishDir = "$PSScriptRoot/src/bin/$Configuration/$TargetFramework/publish/*" 
    $moduleDir = "$PSScriptRoot/src/bin/$Configuration/$TargetFramework/$ModuleName" 

    $moduleLocation = "$PSScriptRoot/src/bin/$Configuration/$TargetFramework" 

    # Copy the module into another folder with the correct name
    if (!(Test-Path $moduleDir)) {
        New-Item $moduleDir -ItemType directory
    }

    Remove-Item -Path $moduleDir/* -Recurse -Force
    Copy-Item -Path $publishDir -Destination $moduleDir -Recurse -Force

    # Add the folder containing the newly renamed module to the PSModulePath
    $psMP = $env:PSModulePath -Split ";"
    if (!($psMP -contains $moduleLocation)) {
        $psMP += $moduleLocation
        $psMP = $psMP -Join ";"
        $env:PSModulePath = $psMP
    }

    if (-not (Get-Module -Name Pester -ListAvailable)) {
        throw "Cannot find the 'Pester' module. Please specify '-Bootstrap' to install build dependencies."
    }

    Import-Module Pester -PassThru
    Invoke-Pester "./test/E2E/Get-FunctionsMetadata.Tests.ps1" -Output Detailed
    if ($Error[0].Fullyqualifiederrorid -eq 'PesterAssertionFailed') {throw 'Pester test failed'}        
}

if ($Deploy.IsPresent) {
    Deploy-FunctionsSDKModule
}
