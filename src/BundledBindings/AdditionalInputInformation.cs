//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings
{
    public class AdditionalInputInformation : IInputBinding
    {
        public override string BindingAttributeName => Constants.AttributeNames.AdditionalInformation;

        public override string BindingType => Constants.BindingNames.NOT_USED;

        public override BindingInformation? ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            string bindingName = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, Constants.BindingPropertyNames.BindingName);
            string name = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, Constants.BindingPropertyNames.Name);
            object value = WorkerIndexingHelper.GetNamedArgumentDefaultTypeValue(attribute, Constants.BindingPropertyNames.Value, "");

            List<string> problems = new List<string>();

            if (string.IsNullOrWhiteSpace(bindingName))
            {
                problems.Add(AzPowerShellSdkStrings.MissingBindingName);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                problems.Add(AzPowerShellSdkStrings.MissingName);
            }

            if (value.GetType() == typeof(string) && string.IsNullOrWhiteSpace((string)value))
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
