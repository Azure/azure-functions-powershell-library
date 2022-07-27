//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

namespace AzureFunctions.PowerShell.SDK
{
    public class BindingInformation
    {
        public enum Directions
        {
            In = 0,
            Out = 1, 
            Inout = 2
        }

        public int Direction { get; set; } = -1;
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public Dictionary<string, Object> otherInformation { get; set; } = new Dictionary<string, Object>();
    }
}
