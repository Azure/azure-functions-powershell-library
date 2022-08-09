//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings
{
    public class EventGridTriggerBinding : IInputBinding
    {
        public override string BindingAttributeName => Constants.AttributeNames.EventGridTrigger;

        public override string BindingType => Constants.BindingNames.EventGridTrigger;

        public override BindingInformation? ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation();
            bindingInformation.Name = parameter.Name.VariablePath.UserPath;
            bindingInformation.Direction = BindingDirection;
            bindingInformation.Type = BindingType;
            return bindingInformation;
        }
    }
}
