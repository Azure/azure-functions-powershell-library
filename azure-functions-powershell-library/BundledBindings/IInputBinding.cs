//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Common;
using System.Management.Automation.Language;

namespace AzureFunctions.PowerShell.SDK.BundledBindings
{
    public abstract class IInputBinding : IBinding
    {
        public List<BindingInformation> defaultOutputBindings = new List<BindingInformation>();

        public abstract BindingInformation ExtractBinding(AttributeAst attribute, ParameterAst parameter);
        
        public const BindingInformation.Directions BindingDirection = BindingInformation.Directions.In;
        public virtual bool ShouldUseDefaultOutputBindings(List<BindingInformation> existingOutputBindings)
        {
            return false;
        }
    }
}
