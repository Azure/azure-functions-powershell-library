using module AzureFunctions.PowerShell.SDK


Describe 'SinglePs1FunctionApp' {
        BeforeAll {
        $metadata = Get-FunctionsMetadata ((Get-Location).Path + "\apps\single-ps1app")
        $metadataObject = $metadata | ConvertFrom-Json

        Import-Module ".\helpers.psm1" -force
    }
    It 'Should return 1 function' {
        $metadataObject.Count | Should -Be 1
    }

    It 'Functions name should be correct' {
        Get-FunctionNames $metadataObject | Should -Contain "TestTrigger"
    }

    It 'Function Bindings should be correct' {
        Get-BindingCount $metadataObject 'TestTrigger' | Should -Be 2

        Get-Binding $metadataObject 'TestTrigger' 'httpTrigger' | Should -Not -Be $null
        Get-Binding $metadataObject 'TestTrigger' 'http' | Should -Not -Be $null
    }
}

Describe 'SinglePsm1FunctionApp' {
        BeforeAll {
        $metadata = Get-FunctionsMetadata ((Get-Location).Path + "\apps\single-psm1app")
        $metadataObject = $metadata | ConvertFrom-Json

        Import-Module ".\helpers.psm1" -force
    }
    It 'Should return 1 function' {
        $metadataObject.Count | Should -Be 1
    }

    It 'Functions name should be correct' {
        Get-FunctionNames $metadataObject | Should -Contain "TestTrigger"
    }

    It 'Function Bindings should be correct' {
        Get-BindingCount $metadataObject 'TestTrigger' | Should -Be 2

        Get-Binding $metadataObject 'TestTrigger' 'httpTrigger' | Should -Not -Be $null
        Get-Binding $metadataObject 'TestTrigger' 'http' | Should -Not -Be $null
    }
}

Describe 'Simple-DurableApp' {
    BeforeAll {
        $metadata = Get-FunctionsMetadata ((Get-Location).Path + "\apps\simple-durable")
        $metadataObject = $metadata | ConvertFrom-Json

        Import-Module ".\helpers.psm1" -force
    }
    It 'Should return 3 functions' {
        $metadataObject.Count | Should -Be 3
    }

    It 'Functions names should be correct' {
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