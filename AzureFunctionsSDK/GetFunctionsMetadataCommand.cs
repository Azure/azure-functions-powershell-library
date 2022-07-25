using AzureFunctionsSDK;
using AzureFunctionsSDK.BundledBindings;
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
        public string FunctionsAppDir { get; set; } = "";

        private string outputJson { get; set; } = "";

        protected override void ProcessRecord()
        {
            List<FunctionInformation> bindingInformations = WorkerIndexingHelper.IndexFunctions(FunctionsAppDir);
            outputJson = System.Text.Json.JsonSerializer.Serialize(bindingInformations);
        }

        protected override void EndProcessing()
        {
            WriteObject(outputJson);
        }
    }
}