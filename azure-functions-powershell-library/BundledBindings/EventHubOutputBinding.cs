//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShellWorker;
using System.Management.Automation.Language;

namespace AzureFunctions.PowerShell.SDK.BundledBindings
{
    public class EventHubOutputBinding : IOutputBinding
    {
        public override string BindingAttributeName => "EventHubOutput";

        public override string BindingType => "eventHub";

        public override BindingInformation ExtractBinding(AttributeAst attribute)
        {
            BindingInformation bindingInformation = new BindingInformation();
            bindingInformation.Type = BindingType;
            bindingInformation.Direction = (int)BindingDirection;
            string? bindingName = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 0, "EventHubOutput");
            string? eventHubName = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 1);
            string? connection = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 2);
            if (bindingName is not null && eventHubName is not null && connection is not null)
            {
                bindingInformation.Name = bindingName;
                bindingInformation.otherInformation.Add("eventHubName", eventHubName);
                bindingInformation.otherInformation.Add("connection", connection);
                return bindingInformation;
            }
            else
            {
                throw new Exception("Incorrectly formatted Event Hub attribute");
            }
        }
    }
}
