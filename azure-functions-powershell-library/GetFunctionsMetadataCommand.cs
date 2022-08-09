//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Common;
using Microsoft.Azure.Functions.PowerShellWorker;
using System.Management.Automation;
using System.Text.Json;

namespace Microsoft.Azure.Functions.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "FunctionsMetadata")]
    public class GetFunctionsMetadataCommand : Cmdlet
    {
        /// <summary>
        /// The path of the Azure Functions directory
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string FunctionsAppDirectory { get; set; } = string.Empty;

        private string outputJson { get; set; } = string.Empty;

        /// <summary>
        /// (Optional) If specified, will force the value to be set for a specified output binding.
        /// </summary>
        [Parameter]
        public SwitchParameter PrettyPrint { get; set; }


        protected override void ProcessRecord()
        {
            try
            {
                if (!string.IsNullOrEmpty(FunctionsAppDirectory) && Directory.Exists(FunctionsAppDirectory))
                {
                    List<FunctionInformation> bindingInformations = WorkerIndexingHelper.IndexFunctions(FunctionsAppDirectory, out List<ErrorRecord> errors);
                    foreach (ErrorRecord error in errors)
                    {
                        WriteError(error);
                    }
                    if (PrettyPrint.IsPresent)
                    {
                        outputJson = JsonSerializer.Serialize(bindingInformations, new JsonSerializerOptions { WriteIndented = true });
                    }
                    else
                    {
                        outputJson = JsonSerializer.Serialize(bindingInformations);
                    }
                }
                else
                {
                    Console.WriteLine("Throwing a terminating error");
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