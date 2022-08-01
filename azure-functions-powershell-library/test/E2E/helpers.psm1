
function Get-FunctionNames {
    param(
        $functionObject
    ) 
    return $functionObject | ForEach-Object {$_.Name}
}

function Get-BindingCount {
    param( 
        $functionObject,
        [string]$functionName
    )
    return ($functionObject | Where-Object {$_.Name -eq $functionName})[0].Bindings.Count
}


function Get-Binding {
    param( 
        $functionObject,
        [string]$functionName,
        [string]$bindingType
    )
    if ((($functionObject | Where-Object {$_.Name -eq $functionName})[0].Bindings | Where-Object {$_.Type -eq $bindingType}).Count -eq 1) {
        return (($functionObject | Where-Object {$_.Name -eq $functionName})[0].Bindings | Where-Object {$_.Type -eq $bindingType})[0]
    }
    return $null
}