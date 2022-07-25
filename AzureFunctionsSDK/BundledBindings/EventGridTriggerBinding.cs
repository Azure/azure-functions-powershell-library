using Microsoft.Azure.Functions.PowerShellWorker;
using System.Management.Automation.Language;

namespace AzureFunctionsSDK.BundledBindings
{
    public class EventGridTriggerBinding : IInputBinding
    {
        public override string BindingName => "EventGridTrigger";

        public override BindingInformation ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation();
            bindingInformation.Name = parameter.Name.VariablePath.UserPath;
            bindingInformation.Direction = (int)BindingDirection;
            bindingInformation.Type = "eventGridTrigger";
            return bindingInformation;
        }
    }
}
