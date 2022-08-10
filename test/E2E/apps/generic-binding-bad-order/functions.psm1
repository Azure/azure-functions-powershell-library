using namespace System.Net
using module AzureFunctions.PowerShell.SDK

function TestTrigger {
    [Function()]
    param(
        [AdditionalInformation('Request', 'authLevel', 'anonymous')]
        [GenericBinding('httpTrigger', 'Request', 'in')]
        [AdditionalInformation('Request', 'methods', ('GET', 'POST'))]
        $Request, 
        $TriggerMetadata
    )

    $value =  ([HttpResponseContext]@{
        StatusCode = [HttpStatusCode]::OK
        Body = 'The Http trigger invocation was successful'
    })

    $value | Push-OutputBinding -Name Response
}