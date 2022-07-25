using Microsoft.Azure.Functions.PowerShellWorker;
using System.Management.Automation.Language;

namespace AzureFunctionsSDK.BundledBindings
{
    public class ActivityTriggerBinding : IInputBinding
    {
        public override string BindingName => "ActivityTrigger";

        public override BindingInformation ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation();
            bindingInformation.Name = parameter.Name.VariablePath.UserPath;
            bindingInformation.Direction = (int)BindingDirection;
            bindingInformation.Type = "activityTrigger";
            return bindingInformation;
        }
    }
}
