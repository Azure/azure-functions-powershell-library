﻿//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using AzureFunctions.PowerShell.SDK;
using Microsoft.Azure.Functions.PowerShellWorker;
using System.Management.Automation;

namespace Microsoft.Azure.Functions.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "FunctionsMetadata")]
    public class GetFunctionsMetadataCommand : Cmdlet
    {
        /// <summary>
        /// The path of the Azure Functions directory
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string? FunctionsAppDir { get; set; }

        private string outputJson { get; set; } = "";

        protected override void ProcessRecord()
        {
            try
            {
                if (FunctionsAppDir != null && !string.IsNullOrEmpty(FunctionsAppDir) && Directory.Exists(FunctionsAppDir))
                {
                    List<FunctionInformation> bindingInformations = WorkerIndexingHelper.IndexFunctions(FunctionsAppDir);
                    outputJson = System.Text.Json.JsonSerializer.Serialize(bindingInformations);
                }
                else
                {
                    ThrowTerminatingError(new ErrorRecord(new Exception("Functions app directory parameter is required and must be a valid directory"), "Invalid function app directory", ErrorCategory.ParserError, null));
                }
            }
            catch (Exception ex) 
            {
                ThrowTerminatingError(new ErrorRecord(ex, "Failed to index the function app", ErrorCategory.ParserError, null));
            }
        }

        protected override void EndProcessing()
        {
            WriteObject(outputJson);
        }
    }
}