//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings
{
    public class AdditionalOutputInformation : IOutputBinding, IAdditionalInformation
    {
        public override string BindingAttributeName => Constants.AttributeNames.AdditionalInformation;

        public override string BindingType => Constants.BindingNames.NOT_USED;

        public override BindingInformation? ExtractBinding(AttributeAst attribute)
        {
            ((IAdditionalInformation)this).AddAdditionalInformation(attribute);

            return null;
        }
    }
}
