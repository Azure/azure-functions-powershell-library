//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings
{
    public abstract class IBinding
    {
        public IBinding() { } //This is necessary for Activator
        public abstract string BindingAttributeName { get; }
        public abstract string BindingType { get; }

        public bool BindingMatches(AttributeAst attribute)
        {
            if (attribute.TypeName.Name == BindingAttributeName)
            {
                return true;
            }
            return false;
        }
    }
}
