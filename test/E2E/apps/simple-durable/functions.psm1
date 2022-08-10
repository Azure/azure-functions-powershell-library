using namespace System.Net
using module AzureFunctions.PowerShell.SDK

function DurableFunctionsHttpStart1 {
    [Function()]
    param(
        [DurableClient('starter')]
        [HttpTrigger('anonymous', ('get', 'post'), 'DurableStart')]
        $Request, 
        $TriggerMetadata
    )

    $FunctionName = "DurableFunctionsOrchestrator1"
    $InstanceId = Start-DurableOrchestration -FunctionName $FunctionName
    Write-Host "Started orchestration with ID = '$InstanceId'"

    $Response = New-DurableOrchestrationCheckStatusResponse -Request $Request -InstanceId $InstanceId
    Push-OutputBinding -Name Response -Value $Response
}

function DurableFunctionsOrchestrator1 {
    [Function()]
    param(
        [OrchestrationTrigger()]
        $Context
    )

    $output = @()

    $output += Invoke-DurableActivity -FunctionName 'Hello1' -Input 'Tokyo'
    $output += Invoke-DurableActivity -FunctionName 'Hello1' -Input 'Seattle'
    $output += Invoke-DurableActivity -FunctionName 'Hello1' -Input 'London'

    $output
}

function Hello1 {
    [Function()]
    param(
        [ActivityTrigger()]
        $name
    )

    "Hello $name!"
}
