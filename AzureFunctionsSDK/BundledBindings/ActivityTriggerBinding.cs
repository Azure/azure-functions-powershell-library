using Microsoft.Azure.Functions.PowerShellWorker;
using System.Management.Automation.Language;

namespace AzureFunctionsSDK.BundledBindings
{
    public class ActivityTriggerBinding : IInputBinding
    {
        public override string BindingAttributeName => "ActivityTrigger";

        public override string BindingType => "activityTrigger";

        public override BindingInformation ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation();
            bindingInformation.Name = parameter.Name.VariablePath.UserPath;
            bindingInformation.Direction = (int)BindingDirection;
            bindingInformation.Type = BindingType;
            return bindingInformation;
        }
    }
}
