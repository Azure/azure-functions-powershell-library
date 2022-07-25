using System.Management.Automation.Language;

namespace AzureFunctionsSDK.BundledBindings
{
    public abstract class IInputBinding : IBinding
    {
        public abstract BindingInformation ExtractBinding(AttributeAst attribute, ParameterAst parameter);
        
        public const BindingInformation.Directions BindingDirection = BindingInformation.Directions.In;
    }
}
