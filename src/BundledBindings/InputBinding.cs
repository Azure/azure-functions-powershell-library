//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings
{
    public class InputBinding : IInputBinding
    {
        public override string BindingAttributeName => Constants.AttributeNames.InputBinding;

        public override string BindingType => Constants.BindingNames.NOT_USED;

        public override BindingInformation? ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation();
            bindingInformation.Direction = BindingDirection;

            string bindingType = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, "Type");
            string bindingName = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, "Name");

            List<string> problems = new List<string>();

            if (string.IsNullOrWhiteSpace(bindingType))
            {
                problems.Add(AzPowerShellSdkStrings.MissingType);
            }
            if (string.IsNullOrWhiteSpace(bindingName))
            {
                bindingName = parameter.Name.VariablePath.UserPath;
            }

            if (problems.Count > 0)
            {
                throw new Exception(AzPowerShellSdkStrings.InputBindingProblemsExist + string.Join("\n", problems));
            }

            bindingInformation.Type = bindingType;
            bindingInformation.Name = bindingName;

            return bindingInformation;
        }
    }
}
