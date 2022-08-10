//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings
{
    public class EventHubOutputBinding : IOutputBinding
    {
        public override string BindingAttributeName => Constants.AttributeNames.EventHub;

        public override string BindingType => Constants.BindingNames.EventHub;

        public override BindingInformation? ExtractBinding(AttributeAst attribute)
        {
            BindingInformation bindingInformation = new BindingInformation();
            bindingInformation.Type = BindingType;
            bindingInformation.Direction = BindingDirection;
            string? bindingName = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 0, Constants.DefaultEventHubOutputName);
            string? eventHubName = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 1);
            string? connection = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 2);
            if (bindingName is not null && eventHubName is not null && connection is not null)
            {
                bindingInformation.Name = bindingName;
                bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.EventHubName, eventHubName);
                bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.Connection, connection);
                return bindingInformation;
            }
            else
            {
                throw new Exception(AzPowerShellSdkStrings.MalformedEventHubAttribute);
            }
        }
    }
}
