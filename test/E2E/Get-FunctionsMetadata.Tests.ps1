#	
# Copyright (c) Microsoft. All rights reserved.	
# Licensed under the MIT license. See LICENSE file in the project root for full license information.	
#

using module AzureFunctions.PowerShell.SDK

Describe 'Empty App' {
    BeforeAll {
        $metadata = Get-FunctionsMetadata ("$PSScriptRoot/apps/empty-app")
        $metadataObject = $metadata | ConvertFrom-Json

        Import-Module "$PSScriptRoot/testUtilities.psm1" -force
    }

    It 'Should return 0 functions' {
        $metadataObject.Count | Should -Be 0
    }
}

Describe 'Poorly formatted .ps1 file' {
    It 'Should throw an error with the file name' {
        { Get-FunctionsMetadata ("$PSScriptRoot/apps/poorly-formatted") -ErrorAction Stop }
            | Should -Throw '*function.ps1*' 
    }

    It 'The error should be non-terminating' {
        { Get-FunctionsMetadata ("$PSScriptRoot/apps/poorly-formatted") -ErrorAction Ignore } 
            | Should -Not -Throw 
    }
}

Describe 'Hybrid or legacy app' {
    It 'Should throw an error when legacy functions are present' {
        { Get-FunctionsMetadata ("$PSScriptRoot/apps/hybrid-model") } 
            | Should -Throw 
    }
}

Describe 'Duplicate binding names' {
    It 'Should throw an error when a function gets two bindings with the same name' {
        { Get-FunctionsMetadata ("$PSScriptRoot/apps/duplicate-bindings")
            | Should -Throw '*Multiple bindings with name Request in function TestTrigger*'}
    }
}

Describe 'AdditionalInformaton before binding' {
    It 'Should throw an error when AdditionalInformaton is used before the binding is declared (positive)' {
        { Get-FunctionsMetadata ("$PSScriptRoot/apps/generic-binding-bad-order") -ErrorAction Stop }
            | Should -Throw '*Could not add additional information with name*'
    }

    It 'The error should be non-terminating' {
        { Get-FunctionsMetadata ("$PSScriptRoot/apps/generic-binding-bad-order") -ErrorAction Ignore }
            | Should -Not -Throw 
    }
}

Describe 'Single Ps1 Function App' {
    BeforeAll {
        $metadata = Get-FunctionsMetadata ("$PSScriptRoot/apps/single-ps1app")
        $metadataObject = $metadata | ConvertFrom-Json

        Import-Module "$PSScriptRoot/testUtilities.psm1" -force
    }

    It 'Should return 1 function' {
        $metadataObject.Count | Should -Be 1
    }

    It 'Function names should contain TestTrigger' {
        Get-FunctionNames $metadataObject | Should -Contain "TestTrigger"
    }

    It 'Function Bindings should be httpTrigger and http' {
        Get-BindingCount $metadataObject 'TestTrigger' | Should -Be 2

        Get-Binding $metadataObject 'TestTrigger' 'httpTrigger' | Should -Not -Be $null
        Get-Binding $metadataObject 'TestTrigger' 'http' | Should -Not -Be $null
    }
}

Describe 'Single Psm1 Function App' {
    BeforeAll {
        $metadata = Get-FunctionsMetadata ("$PSScriptRoot/apps/single-psm1app")
        $metadataObject = $metadata | ConvertFrom-Json

        Import-Module "$PSScriptRoot/testUtilities.psm1" -force
    }

    It 'Should return 1 function' {
        $metadataObject.Count | Should -Be 1
    }

    It 'Function names should contain TestTrigger' {
        Get-FunctionNames $metadataObject | Should -Contain "TestTrigger"
    }

    It 'Function Bindings should be httpTrigger and http' {
        Get-BindingCount $metadataObject 'TestTrigger' | Should -Be 2

        Get-Binding $metadataObject 'TestTrigger' 'httpTrigger' | Should -Not -Be $null
        Get-Binding $metadataObject 'TestTrigger' 'http' | Should -Not -Be $null
    }
}

Describe 'GenericBinding' {
    BeforeAll {
        $metadata = Get-FunctionsMetadata ("$PSScriptRoot/apps/generic-binding-good")
        $metadataObject = $metadata | ConvertFrom-Json

        Import-Module "$PSScriptRoot/testUtilities.psm1" -force
    }

    It 'Should return 1 function' {
        $metadataObject.Count | Should -Be 1
    }

    It 'Function names should contain TestTrigger' {
        Get-FunctionNames $metadataObject | Should -Contain "TestTrigger"
    }

    It 'Function Bindings should be httpTrigger and http' {
        Get-BindingCount $metadataObject 'TestTrigger' | Should -Be 2

        Get-Binding $metadataObject 'TestTrigger' 'httpTrigger' | Should -Not -Be $null
        Get-Binding $metadataObject 'TestTrigger' 'http' | Should -Not -Be $null
    }
}

Describe 'Simple Durable App' {
    BeforeAll {
        $metadata = Get-FunctionsMetadata ("$PSScriptRoot/apps/simple-durable")
        $metadataObject = $metadata | ConvertFrom-Json

        Import-Module "$PSScriptRoot/testUtilities.psm1" -force
    }

    It 'Should return 3 functions' {
        $metadataObject.Count | Should -Be 3
    }

    $expectedFunctionNames = @( @{FunctionName = 'Hello1'; NumBindings = 1}, 
                                @{FunctionName = 'DurableFunctionsHttpStart1'; NumBindings = 3}, 
                                @{FunctionName = 'DurableFunctionsOrchestrator1'; NumBindings = 1})

    It "Function names should contain <functionName>" -ForEach $expectedFunctionNames {
        Get-FunctionNames $metadataObject | Should -Contain $functionName
    }

    It 'Function called <functionName> should have <numBindings> bindings' -ForEach $expectedFunctionNames {
        Get-BindingCount $metadataObject $functionName | Should -Be $numBindings
    }

    $expectedBindingNames = @( @{FunctionName = 'Hello1'; BindingName = 'activityTrigger'},
                               @{FunctionName = 'DurableFunctionsHttpStart1'; BindingName = 'httpTrigger'},
                               @{FunctionName = 'DurableFunctionsHttpStart1'; BindingName = 'http'},
                               @{FunctionName = 'DurableFunctionsHttpStart1'; BindingName = 'durableClient'},
                               @{FunctionName = 'DurableFunctionsOrchestrator1'; BindingName = 'orchestrationTrigger'})

    It 'Function <functionName> should have binding <bindingName>' -ForEach $expectedBindingNames {
        Get-Binding $metadataObject $functionName $bindingName | Should -Not -Be $null
    }
}