﻿//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

namespace Microsoft.Azure.Functions.PowerShell.SDK.Common
{
    public class BindingInformation
    {
        public enum Directions
        {
            Unknown = -1,
            In = 0,
            Out = 1,
            Inout = 2
        }

        public Directions Direction { get; set; } = Directions.Unknown;
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        // OtherInformation contains any other binding information not encapsulated above. When it is reserialized in the worker,
        // the key-value pairs here will be included in the generated function.json equivalent
        // Example: Chron expressions for TimerTrigger, or method/auth level for HttpTrigger. 
        public Dictionary<string, object> otherInformation { get; set; } = new Dictionary<string, object>();
    }
}
