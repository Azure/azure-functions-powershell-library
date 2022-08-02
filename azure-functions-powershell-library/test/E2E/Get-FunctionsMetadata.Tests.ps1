#	
# Copyright (c) Microsoft. All rights reserved.	
# Licensed under the MIT license. See LICENSE file in the project root for full license information.	
#

using module AzureFunctions.PowerShell.SDK

Describe 'Empty App' {
    BeforeAll {
        $metadata = Get-FunctionsMetadata ((Get-Location).Path + "\apps\empty-app")
        $metadataObject = $metadata | ConvertFrom-Json

        Import-Module ".\helpers.psm1" -force
    }
    It 'Should return 0 functions' {
        $metadataObject.Count | Should -Be 0
    }
}

Describe 'Poorly formatted .ps1 file' {
    It 'Should throw an error with the file name' {
        { Get-FunctionsMetadata ((Get-Location).Path + "\apps\poorly-formatted") } 
            | Should -Throw '*apps\poorly-formatted\function.ps1*'
    }
}

Describe 'Hybrid or legacy app' {
    It 'Should throw an error when legacy functions are present' {
        { Get-FunctionsMetadata ((Get-Location).Path + "\apps\hybrid-model") } 
            | Should -Throw
    }
}

Describe 'Single Ps1 Function App' {
    BeforeAll {
        $metadata = Get-FunctionsMetadata ((Get-Location).Path + "\apps\single-ps1app")
        $metadataObject = $metadata | ConvertFrom-Json

        Import-Module ".\helpers.psm1" -force
    }
    It 'Should return 1 function' {
        $metadataObject.Count | Should -Be 1
    }

    It 'Function names should contain TestTrigger' {
        Get-FunctionNames $metadataObject | Should -Contain "TestTrigger"
    }

    It 'Function Bindings should be correct' {
        Get-BindingCount $metadataObject 'TestTrigger' | Should -Be 2

        Get-Binding $metadataObject 'TestTrigger' 'httpTrigger' | Should -Not -Be $null
        Get-Binding $metadataObject 'TestTrigger' 'http' | Should -Not -Be $null
    }
}

Describe 'Single Psm1 Function App' {
    BeforeAll {
        $metadata = Get-FunctionsMetadata ((Get-Location).Path + "\apps\single-psm1app")
        $metadataObject = $metadata | ConvertFrom-Json

        Import-Module ".\helpers.psm1" -force
    }
    It 'Should return 1 function' {
        $metadataObject.Count | Should -Be 1
    }

    It 'Function names should contain TestTrigger' {
        Get-FunctionNames $metadataObject | Should -Contain "TestTrigger"
    }

    It 'Function Bindings should be correct' {
        Get-BindingCount $metadataObject 'TestTrigger' | Should -Be 2

        Get-Binding $metadataObject 'TestTrigger' 'httpTrigger' | Should -Not -Be $null
        Get-Binding $metadataObject 'TestTrigger' 'http' | Should -Not -Be $null
    }
}

Describe 'Simple Durable App' {
    BeforeAll {
        $metadata = Get-FunctionsMetadata ((Get-Location).Path + "\apps\simple-durable")
        $metadataObject = $metadata | ConvertFrom-Json

        Import-Module ".\helpers.psm1" -force
    }
    It 'Should return 3 functions' {
        $metadataObject.Count | Should -Be 3
    }

    It 'Function names should be correct' {
        Get-FunctionNames $metadataObject | Should -Contain "Hello1"
        Get-FunctionNames $metadataObject {$_.Name} | Should -Contain "DurableFunctionsHttpStart1"
        Get-FunctionNames $metadataObject {$_.Name} | Should -Contain "DurableFunctionsOrchestrator1"
    }

    It 'Function Bindings should be correct' {
        Get-BindingCount $metadataObject 'Hello1' | Should -Be 1
        Get-BindingCount $metadataObject 'DurableFunctionsHttpStart1' | Should -Be 3
        Get-BindingCount $metadataObject 'DurableFunctionsOrchestrator1' | Should -Be 1

        Get-Binding $metadataObject 'Hello1' 'activityTrigger' | Should -Not -Be $null
        Get-Binding $metadataObject 'DurableFunctionsHttpStart1' 'httpTrigger' | Should -Not -Be $null
        Get-Binding $metadataObject 'DurableFunctionsHttpStart1' 'http' | Should -Not -Be $null
        Get-Binding $metadataObject 'DurableFunctionsHttpStart1' 'durableClient' | Should -Not -Be $null

        Get-Binding $metadataObject 'DurableFunctionsOrchestrator1' 'orchestrationTrigger' | Should -Not -Be $null
    }
}