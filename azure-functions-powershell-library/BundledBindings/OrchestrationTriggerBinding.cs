//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using AzureFunctions.PowerShell.SDK.Common;
using Common;
using Microsoft.Azure.Functions.PowerShellWorker;
using System.Management.Automation.Language;

namespace AzureFunctions.PowerShell.SDK.BundledBindings
{
    public class OrchestrationTriggerBinding : IInputBinding
    {
        public override string BindingAttributeName => Constants.AttributeNames.OrchestrationTrigger;

        public override string BindingType => Constants.BindingNames.OrchestrationTrigger;

        public override BindingInformation ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation();
            bindingInformation.Name = parameter.Name.VariablePath.UserPath;
            bindingInformation.Direction = BindingDirection;
            bindingInformation.Type = BindingType;
            return bindingInformation;
        }
    }
}
