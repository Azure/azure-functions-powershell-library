using namespace System.Net
using module AzureFunctions.PowerShell.SDK

function TestTrigger {
    [Function()]
    [OutputBinding(Type='http', Name='Response')]
    [AdditionalInformation(BindingName='Response', Name='authLevel', Value='anonymous')]
    param(
        [InputBinding(Type='httpTrigger')]
        [AdditionalInformation(BindingName='Request', Name='authLevel', Value='anonymous')]
        [AdditionalInformation(BindingName='Request', Name='methods', Value=('GET', 'POST'))]
        $Request, 
        $TriggerMetadata
    )

    $value =  ([HttpResponseContext]@{
        StatusCode = [HttpStatusCode]::OK
        Body = 'The Http trigger invocation was successful'
    })

    $value | Push-OutputBinding -Name Response
}