//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings
{
    public class AdditionalInformation : IInputBinding
    {
        public override string BindingAttributeName => Constants.AttributeNames.AdditionalInformation;

        public override string BindingType => Constants.BindingNames.NOT_USED;

        public override BindingInformation? ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            string? bindingName = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 0);
            string? name = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 1);
            object? value = null;
            if (attribute.PositionalArguments.Count() >= 2 && attribute.PositionalArguments[2].GetType() == typeof(StringConstantExpressionAst))
            {
                value = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 2);
            }
            else if (attribute.PositionalArguments.Count() >= 2)
            {
                value = WorkerIndexingHelper.ExtractOneOrMore(attribute.PositionalArguments[2]);
            }

            List<string> problems = new List<string>();

            if (string.IsNullOrEmpty(bindingName))
            {
                problems.Add(AzPowerShellSdkStrings.MissingBindingName);
            }
            if (string.IsNullOrEmpty(name))
            {
                problems.Add(AzPowerShellSdkStrings.MissingName);
            }
            if (value == null)
            {
                problems.Add(AzPowerShellSdkStrings.MissingValue);
            }

            if (problems.Count > 0)
            {
                throw new Exception(string.Format(AzPowerShellSdkStrings.AdditionalInformationProblemsExist, string.Join("\n", problems)));
            }

            //This condition will never be false but hey, type enforcement makes us add it anyway thanks logic
            if (!string.IsNullOrEmpty(bindingName) && !string.IsNullOrEmpty(name) && value != null) 
            {
                WorkerIndexingHelper.AddBindingInformation(bindingName, name, value);
            }

            return null;
        }
    }
}
