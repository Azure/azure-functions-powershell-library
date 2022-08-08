//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using AzureFunctions.PowerShell.SDK.BundledBindings;
using Common;
using System.Management.Automation.Language;
using System.Reflection;

namespace AzureFunctions.PowerShell.SDK
{
    public static class BindingExtractor
    {
        public static List<BindingInformation> unallocatedBindings = new List<BindingInformation>();
    }
}
