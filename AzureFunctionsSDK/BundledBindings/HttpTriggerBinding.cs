using Microsoft.Azure.Functions.PowerShellWorker;
using System.Management.Automation.Language;

namespace AzureFunctionsSDK.BundledBindings
{
    public class HttpTriggerBinding : IInputBinding
    {
        public HttpTriggerBinding()
        {
            defaultOutputBindings.Add(HttpOutputBinding.Create());
        }

        public override string BindingAttributeName => "HttpTrigger";

        public override string BindingType => "httpTrigger";

        public override BindingInformation ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation();

            bindingInformation.Name = parameter.Name.VariablePath.UserPath;
            //Todo: Named arguments?
            string? bindingAuthLevel = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 0, "anonymous");
            List<string>? bindingMethods = attribute.PositionalArguments.Count > 1 ? 
                                                WorkerIndexingHelper.ExtractOneOrMore(attribute.PositionalArguments[1]) : 
                                                new List<string>() { "GET", "POST" };
            string? route = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 2);
            if (bindingMethods == null)
            {
                bindingMethods = new List<string>() { "GET", "POST" };
            }
            bindingInformation.Direction = (int)BindingDirection;
            bindingInformation.Type = BindingType;
            if (bindingAuthLevel != null)
            {
                bindingInformation.otherInformation.Add("authLevel", bindingAuthLevel);
            }
            bindingInformation.otherInformation.Add("methods", bindingMethods);
            if (route != null)
            {
                bindingInformation.otherInformation.Add("route", route);
            }
            return bindingInformation;
        }
        
        public new bool ShouldUseDefaultOutputBindings(List<BindingInformation> existingOutputBindings)
        {
            return existingOutputBindings.Where(x => x.Type == "http" && x.Direction == (int)BindingInformation.Directions.Out).Count() == 0;
        }
    }
}
