//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

namespace Microsoft.Azure.Functions.PowerShell.SDK.Common
{
    public class FunctionInformation
    {
        public string Directory { get; set; } = string.Empty;
        public string ScriptFile { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EntryPoint { get; set; } = string.Empty;
        public string FunctionId { get; set; } = string.Empty;
        public List<BindingInformation> Bindings { get; set; } = new List<BindingInformation>();
    }
}
