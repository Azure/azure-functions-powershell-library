//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctions.PowerShell.SDK.BundledBindings
{
    public abstract class IOutputBinding : IBinding
    {
        public const BindingInformation.Directions BindingDirection = BindingInformation.Directions.Out;
        public abstract BindingInformation? ExtractBinding(AttributeAst attribute);
    }
}
