//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using AzureFunctions.PowerShell.SDK;
using Common;
using System.Management.Automation;

namespace Microsoft.Azure.Functions.PowerShell
{
    [Cmdlet(VerbsLifecycle.Register, "Binding")]
    public class RegisterBindingCommand : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string BindingType { get; set; } = string.Empty;

        [Parameter(Mandatory = true, Position = 1)]
        public object? ArgumentValues { get; set; }

        protected override void ProcessRecord()
        {
            BindingInformation bindingInfo = new BindingInformation();
            bindingInfo.Type = BindingType;
            bindingInfo.otherInformation = parseArgvs();
            BindingExtractor.unallocatedBindings.Add(bindingInfo);
        }
        protected override void EndProcessing()
        {
            WriteObject("");
        }

        Dictionary<string, object> parseArgvs()
        {
            if (ArgumentValues is Dictionary<string, object>)
            {
                return (Dictionary<string, object>)ArgumentValues;
            }
            return new Dictionary<string, object>();
        }
    }
}
