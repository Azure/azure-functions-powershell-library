//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings
{
    public class EventHubTriggerBinding : IInputBinding
    {
        public override string BindingAttributeName => Constants.AttributeNames.EventHubTrigger;

        public override string BindingType => Constants.BindingNames.EventHubTrigger;

        public override BindingInformation? ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation();

            bindingInformation.Type = BindingType;
            bindingInformation.Direction = BindingDirection;
            bindingInformation.Name = parameter.Name.VariablePath.UserPath;

            string eventHubName = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, "EventHubName");
            string consumerGroup = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, "ConsumerGroup");
            string cardinality = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, "Cardinality");
            string connection = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, "Connection");

            if (!string.IsNullOrWhiteSpace(eventHubName) && !string.IsNullOrWhiteSpace(consumerGroup) && !string.IsNullOrWhiteSpace(cardinality) && !string.IsNullOrWhiteSpace(connection))
            {
                bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.EventHubName, eventHubName);
                bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.ConsumerGroup, consumerGroup);
                bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.Cardinality, cardinality);
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
