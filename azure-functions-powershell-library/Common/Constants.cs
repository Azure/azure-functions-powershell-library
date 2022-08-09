//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

namespace AzureFunctions.PowerShell.SDK.Common
{
    internal static class Constants
    {
        public const string Ps1FileExtension = ".ps1";
        public const string Psm1FileExtension = ".psm1";

        public const string FunctionJson = "function.json";

        public const string DefaultHttpResponseName = "Response";
        public const string DefaultDurableClientName = "starter";
        public const string DefaultEventHubOutputName = "EventHubOutput";
        public const string DefaultHttpAuthLevel = "anonymous";
        public static List<string>? DefaultHttpMethods = new List<string>() { "GET", "POST" };

        internal class AttributeNames
        {
            public const string HttpOutput = "HttpOutput";
            public const string ActivityTrigger = "ActivityTrigger";
            public static string DurableClient = "DurableClient";
            public static string EventGridTrigger = "EventGridTrigger";
            public static string EventHub = "EventHubOutput";
            public static string EventHubTrigger = "EventHubTrigger";
            public static string HttpTrigger = "HttpTrigger";
            public static string OrchestrationTrigger = "OrchestrationTrigger";
            public static string TimerTrigger = "TimerTrigger";
            public static string Function = "Function";
            public static string GenericBinding = "GenericBinding";
            public static string AdditionalInformation = "AdditionalInformation";
        }

        internal class BindingNames
        {
            public const string Http = "http";
            public const string ActivityTrigger = "activityTrigger";
            public static string DurableClient = "durableClient";
            public static string EventGridTrigger = "eventGridTrigger";
            public static string EventHub = "eventHub";
            public static string EventHubTrigger = "eventHubTrigger";
            public static string HttpTrigger = "httpTrigger";
            public static string OrchestrationTrigger = "orchestrationTrigger";
            public static string TimerTrigger = "timerTrigger";
            public static string NOT_USED = "NOT_USED";
        }

        internal class JsonPropertyNames
        {
            public const string EventHubName = "eventHubName";
            public const string ConsumerGroup = "consumerGroup";
            public const string Cardinality = "cardinality";
            public const string Connection = "connection";
            public const string AuthLevel = "authLevel";
            public const string Methods = "methods";
            public const string Route = "route";
            internal static string Schedule = "schedule";
        }
    }
}
