﻿//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings
{
    public class DurableClientBinding : IInputBinding
    {
        public override string BindingAttributeName => Constants.AttributeNames.DurableClient;

        public override string BindingType => Constants.BindingNames.DurableClient;

        public override BindingInformation? ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation();
            string? name = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 0, Constants.DefaultDurableClientName);
            bindingInformation.Direction = BindingDirection;
            bindingInformation.Type = BindingType;
            if (name != null)
            {
                bindingInformation.Name = name;
            }
            return bindingInformation;
        }
    }
}