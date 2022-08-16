//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System.Management.Automation;
using System.Text.Json;

namespace Microsoft.Azure.Functions.PowerShell.SDK
{
    [Cmdlet(VerbsCommon.Get, "FunctionsMetadata")]
    public class GetFunctionsMetadataCommand : Cmdlet
    {
        /// <summary>
        /// The path of the Azure Functions directory
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public string FunctionsAppDirectoryString { get; set; } = "";

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
                // Attempt to normalize the user's input to a local path from where the cmdlet is running
                SessionState ss = new SessionState();
                string current = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(ss.Path.CurrentFileSystemLocation.Path);
                FunctionsAppDirectoryString = Path.GetFullPath(FunctionsAppDirectoryString);
                Directory.SetCurrentDirectory(current);

                Console.WriteLine(FunctionsAppDirectoryString);
                if (!Directory.Exists(FunctionsAppDirectoryString))
                {
                    ThrowTerminatingError(new ErrorRecord(new Exception(string.Format(AzPowerShellSdkStrings.InvalidFunctionsAppDirectory, FunctionsAppDirectoryString)), "InvalidFunctionAppDirectory", ErrorCategory.ParserError, null));
                }

                DirectoryInfo FunctionsAppDirectory = new DirectoryInfo(FunctionsAppDirectoryString);

                if(!IsValidFunctionsAppDirectory(FunctionsAppDirectory))
                {
                    ThrowTerminatingError(new ErrorRecord(new Exception(string.Format(AzPowerShellSdkStrings.MissingHostJson, FunctionsAppDirectory.FullName)), "InvalidFunctionAppDirectory", ErrorCategory.ParserError, null));
                }

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
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(ex, "FailToIndexFunctionApp", ErrorCategory.ParserError, null));
            }
        }

        private bool IsValidFunctionsAppDirectory(DirectoryInfo functionsAppDirectory)
        {
            return functionsAppDirectory.EnumerateFiles().Where(x => x.Name == "host.json").Any();
        }

        protected override void EndProcessing()
        {
            WriteObject(outputJson);
        }
    }
}