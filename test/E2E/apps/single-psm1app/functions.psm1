using namespace System.Net
using module AzureFunctions.PowerShell.SDK

function TestTrigger {
    [Function()]
    param(
        [HttpTrigger('anonymous', ('get', 'post'))]
        $Request, 
        $TriggerMetadata
    )

    $value =  ([HttpResponseContext]@{
        StatusCode = [HttpStatusCode]::OK
        Body = 'The Http trigger invocation was successful'
    })

    $value | Push-OutputBinding -Name Response
}