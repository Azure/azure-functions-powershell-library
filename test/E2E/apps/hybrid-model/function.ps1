using namespace System.Net
using module AzureFunctions.PowerShell.SDK

[Function(Name='TestTrigger')]
param(
    [HttpTrigger(AuthLevel='function', Methods=('get'))]
    $Request, 
    $TriggerMetadata
)

$value =  ([HttpResponseContext]@{
    StatusCode = [HttpStatusCode]::OK
    Body = 'The Http trigger invocation was successful'
})

$value | Push-OutputBinding -Name Response
