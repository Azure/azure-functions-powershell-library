using namespace System.Net
using module AzureFunctions.PowerShell.SDK

[Function(Name='TestTrigger')]
param(
    [HttpTrigger(AuthLevel='function', Methods=('get'))]
    $Request, 
    $TriggerMetadata
# We are missing the closing parenthesis here, making this an invalid .ps1 file