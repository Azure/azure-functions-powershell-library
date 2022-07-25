using Microsoft.Azure.Functions.PowerShellWorker;
using System.Management.Automation.Language;

namespace AzureFunctionsSDK.BundledBindings
{
    public class TimerTriggerBinding : IInputBinding
    {
        public override string BindingName => "TimerTrigger";

        public override BindingInformation ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation();
            bindingInformation.Name = parameter.Name.VariablePath.UserPath;
            string? chronExpression = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 0);
            bindingInformation.Direction = (int)BindingDirection;
            bindingInformation.Type = "timerTrigger";
            if (chronExpression != null)
            {
                bindingInformation.otherInformation.Add("schedule", chronExpression);
            }
            return bindingInformation;
        }
    }
}
