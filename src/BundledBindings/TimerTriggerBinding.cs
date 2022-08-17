//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings
{
    public class TimerTriggerBinding : IInputBinding
    {
        public override string BindingAttributeName => Constants.AttributeNames.TimerTrigger;

        public override string BindingType => Constants.BindingNames.TimerTrigger;

        public override BindingInformation? ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation();
            bindingInformation.Name = parameter.Name.VariablePath.UserPath;
            string chronExpression = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, "Chron");
            bindingInformation.Direction = BindingDirection;
            bindingInformation.Type = BindingType;

            if (string.IsNullOrWhiteSpace(chronExpression))
            {
                throw new Exception();
            }

            bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.Schedule, chronExpression);
            
            return bindingInformation;
        }
    }
}
