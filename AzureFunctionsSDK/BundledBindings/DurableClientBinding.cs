using Microsoft.Azure.Functions.PowerShellWorker;
using System.Management.Automation.Language;

namespace AzureFunctionsSDK.BundledBindings
{
    public class DurableClientBinding : IInputBinding
    {
        public override string BindingName => "DurableClient";

        public override BindingInformation ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation();
            string? name = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 0, "starter");
            bindingInformation.Direction = (int)BindingDirection;
            bindingInformation.Type = "durableClient";
            if (name != null)
            {
                bindingInformation.Name = name;
            }
            return bindingInformation;
        }
    }
}
