//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings
{
    public class OutputBinding : IOutputBinding
    {
        public override string BindingAttributeName => Constants.AttributeNames.OutputBinding;

        public override string BindingType => Constants.BindingNames.NOT_USED;

        public override BindingInformation? ExtractBinding(AttributeAst attribute)
        {
            BindingInformation bindingInformation = new BindingInformation();
            string bindingType = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, "Type");
            string bindingName = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, "Name");

            List<string> problems = new List<string>();

            if (string.IsNullOrWhiteSpace(bindingType))
            {
                problems.Add(AzPowerShellSdkStrings.MissingType);
            }
            if (string.IsNullOrWhiteSpace(bindingName))
            {
                problems.Add(AzPowerShellSdkStrings.MissingBindingName);
            }

            bindingInformation.Direction = BindingDirection;

            if (problems.Count > 0)
            {
                throw new Exception(AzPowerShellSdkStrings.OutputBindingProblemsExist + string.Join("\n", problems));
            }

            bindingInformation.Type = bindingType;
            bindingInformation.Name = bindingName;

            return bindingInformation;
        }
    }
}
