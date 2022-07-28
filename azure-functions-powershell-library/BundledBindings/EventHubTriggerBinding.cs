//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShellWorker;
using System.Management.Automation.Language;

namespace AzureFunctions.PowerShell.SDK.BundledBindings
{
    public class EventHubTriggerBinding : IInputBinding
    {
        public override string BindingAttributeName => "EventHubTrigger";

        public override string BindingType => "eventHubTrigger";

        public override BindingInformation ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation();
            bindingInformation.Type = BindingType;
            bindingInformation.Direction = (int)BindingDirection;
            bindingInformation.Name = parameter.Name.VariablePath.UserPath;
            string? eventHubName = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 0);
            string? consumerGroup = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 1);
            string? cardinality = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 2);
            string? connection = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 3);
            if (eventHubName is not null && consumerGroup is not null && cardinality is not null && connection is not null)
            {
                bindingInformation.otherInformation.Add("eventHubName", eventHubName);
                bindingInformation.otherInformation.Add("consumerGroup", consumerGroup);
                bindingInformation.otherInformation.Add("cardinality", cardinality);
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
