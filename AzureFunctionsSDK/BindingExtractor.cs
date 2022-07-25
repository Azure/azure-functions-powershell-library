using AzureFunctionsSDK.BundledBindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsSDK
{
    internal static class BindingExtractor
    {
        static BindingExtractor()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                                                .SelectMany(assembly => assembly.GetTypes())
                                                .Where(type => type.IsSubclassOf(typeof(IBinding)));
            foreach (Type type in types)
            {
                Console.WriteLine(type.Name);
                try
                {
                    var obj = Activator.CreateInstance(type);
                    if (obj is not null)
                    {
                        ((IBinding)obj).AddToExtractor();
                    }
                }
                catch (MissingMethodException ex)
                {
                    //Do nothing, it's an abstract class or improperly declared. 
                }
            }
        }
        private static List<IBinding> supportedBindings = new List<IBinding>();
        public static void addSupportedBinding(IBinding binding)
        {
            supportedBindings.Add(binding);
        }
        
        public static bool hasSupportedBinding(Type bindingType)
        {
            if (supportedBindings.Where(x => x.GetType() == bindingType).Count() > 0)
            {
                return true;
            }
            return false;
        }

        public static BindingInformation? extractInputBinding(AttributeAst attribute, ParameterAst parameter)
        {
            Console.WriteLine($"Parsing attribute {attribute.TypeName.Name}");
            List<IInputBinding> inputBindings = supportedBindings.Where(x => x is IInputBinding).Cast<IInputBinding>().ToList();
            List<BindingInformation> bindings = new List<BindingInformation>();
            foreach (IInputBinding binding in inputBindings)
            {
                if (binding.BindingMatches(attribute)) 
                {
                    return binding.ExtractBinding(attribute, parameter);
                }
            }
            return null;
        }

        public static BindingInformation? extractOutputBinding(AttributeAst attribute)
        {
            List<IOutputBinding> outputBindings = supportedBindings.Where(x => x is IOutputBinding).Cast<IOutputBinding>().ToList();
            List<BindingInformation> bindings = new List<BindingInformation>();
            foreach (IOutputBinding binding in outputBindings)
            {
                if (binding.BindingMatches(attribute))
                {
                    return binding.ExtractBinding(attribute);
                }
            }
            return null;
        }
    }
}
