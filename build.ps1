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

#Requires -Version 6.0

$PowerShellVersion = '7.2'
$TargetFramework = 'net6.0'

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

    $publishDir = "./AzureFunctionsSDK/bin/$Configuration/$TargetFramework/publish/*" 

    $powerShellWorkerModuleDir = "$(Get-FunctionsCoreToolsDir)/workers/powershell/$PowerShellVersion/Modules/AzureFunctionsSDK"

    Write-Log "Deploying module to $powerShellWorkerModuleDir..."

    if (-not $IsWindows) {
        sudo chmod -R a+w $powerShellWorkerModuleDir
    }

    Remove-Item -Path $powerShellWorkerModuleDir/* -Recurse -Force
    Copy-Item -Path $publishDir -Destination $powerShellWorkerModuleDir -Recurse -Force

    Write-Log "Deployed module to $powerShellWorkerModuleDir"
}

Import-Module "$PSScriptRoot/tools/helper.psm1" -Force

# Bootstrap step
if ($Bootstrap.IsPresent) {
    Write-Log "Validate and install missing prerequisits for building ..."
    Install-Dotnet

    if (-not (Get-Module -Name PSDepend -ListAvailable)) {
        Write-Log -Warning "Module 'PSDepend' is missing. Installing 'PSDepend' ..."
        Install-Module -Name PSDepend -Scope CurrentUser -Force
    }
    if (-not (Get-Module -Name platyPS -ListAvailable)) {
        Write-Log -Warning "Module 'platyPS' is missing. Installing 'platyPS' ..."
        Install-Module -Name platyPS -Scope CurrentUser -Force
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
    if (-not (Get-Module -Name PSDepend -ListAvailable)) {
        throw "Cannot find the 'PSDepend' module. Please specify '-Bootstrap' to install build dependencies."
    }

    # Generate C# files for resources
    Start-ResGen

    dotnet publish -c $Configuration "/p:BuildNumber=$BuildNumber" $PSScriptRoot

    $publishDir = "./AzureFunctionsSDK/bin/$Configuration/$TargetFramework/publish" 
    $buildDir = "./AzureFunctionsSDK/bin/$Configuration/$TargetFramework" 

    $psFiles = Get-ChildItem "$PSScriptRoot/AzureFunctionsSDK" | Where-Object -Property Extension -in -Value ".ps1", ".psm1", ".psd1"

    $psFiles | ForEach-Object { Copy-Item -Path $_.FullName -Destination $publishDir }
    $psFiles | ForEach-Object { Copy-Item -Path $_.FullName -Destination $buildDir }

    # dotnet pack -c $Configuration "/p:BuildNumber=$BuildNumber" "$PSScriptRoot/package"
}

# Test step
if ($Test.IsPresent) {
    dotnet test "$PSScriptRoot/test/Unit"
    if ($LASTEXITCODE -ne 0) { throw "xunit tests failed." }
}

if ($Deploy.IsPresent) {
    Deploy-FunctionsSDKModule
}
