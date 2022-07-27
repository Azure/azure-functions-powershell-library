//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

namespace AzureFunctions.PowerShell.SDK
{
    public class FunctionInformation
    {
        public string Directory { get; set; } = "";
        public string ScriptFile { get; set; } = "";
        public string Name { get; set; } = "";
        public string EntryPoint { get; set; } = "";
        public string FunctionId { get; set; } = "";
        public List<BindingInformation> Bindings { get; set; } = new List<BindingInformation>();
    }
}
