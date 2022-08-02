//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using AzureFunctions.PowerShell.SDK.Common;
using Microsoft.Azure.Functions.PowerShellWorker;
using System.Management.Automation.Language;

namespace AzureFunctions.PowerShell.SDK.BundledBindings
{
    public class EventHubTriggerBinding : IInputBinding
    {
        public override string BindingAttributeName => Constants.AttributeNames.EventHubTrigger;

        public override string BindingType => Constants.BindingNames.EventHubTrigger;

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
                bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.EventHubName, eventHubName);
                bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.ConsumerGroup, consumerGroup);
                bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.Cardinality, cardinality);
                bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.Connection, connection);
                return bindingInformation;
            }
            else
            {
                throw new Exception("Incorrectly formatted Event Hub attribute");
            }
        }
    }
}
