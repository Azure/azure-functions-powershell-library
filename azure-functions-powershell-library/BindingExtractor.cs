//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using AzureFunctions.PowerShell.SDK.BundledBindings;
using Common;
using System.Management.Automation.Language;

namespace AzureFunctions.PowerShell.SDK
{
    internal static class BindingExtractor
    {
        static BindingExtractor()
        {
            // We want this behavior to be extensible by third parties who may want to add binding definitions. 
            // To accomplish this, we parse through all loaded assemblies to find any class which implements 
            //   IBinding, and load it manually into our supportedBindings

            // Find all types that implement IBinding
            // Apparently, there are scenarios where this throws a ReflectionTypeLoadException - filtering it seems to help
            IEnumerable<Type>? types = AppDomain.CurrentDomain.GetAssemblies()
                                                              .Where(ass => ass.IsDynamic == false)
                                                              .Where(x => !(x.FullName ?? "").StartsWith("Microsoft.GeneratedCode"))
                                                              .SelectMany(assembly => assembly.GetTypes())
                                                              .Where(type => type.IsSubclassOf(typeof(IBinding)));
            foreach (Type type in types)
            {
                try
                {
                    // Instantiate an object of this type, and call its method to load it into supportedBindings for use later
                    object? obj = Activator.CreateInstance(type);
                    if (obj is not null)
                    {
                        ((IBinding)obj).AddToExtractor();
                    }
                }
                catch (MissingMethodException)
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

        public static BindingInformation? ExtractInputBinding(AttributeAst attribute, ParameterAst parameter)
        {
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

        public static BindingInformation? ExtractOutputBinding(AttributeAst attribute)
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

        public static List<BindingInformation> GetDefaultBindings(List<BindingInformation> existingInputBindings, List<BindingInformation> existingOutputBindings)
        {
            List<BindingInformation> defaultBindings = new List<BindingInformation>();
            foreach (BindingInformation existingInputBinding in existingInputBindings)
            {
                // Try to figure out which IBininding class was used to create the BindingInformation 
                //   Might be worth changing BindingInformation to refer to the instance of IBinding that created it
                //   Would need to avoid serializing this information when returning to the worker. 
                List<IBinding> matchingSupportedBindings = supportedBindings.Where(x => x.BindingType == existingInputBinding.Type).ToList();

                if (matchingSupportedBindings.Count() > 0)
                {
                    foreach (IBinding matchingSupportedBinding in matchingSupportedBindings)
                    {
                        // Each IBinding is allowed to define its own list of default output bindings. It is also 
                        // given the responsibility of determining whether these bindings should be used, based on 
                        // the output bindings that have been explicitly declared 
                        if (matchingSupportedBinding.ShouldUseDefaultOutputBindings(existingOutputBindings))
                        {
                            defaultBindings.AddRange(matchingSupportedBinding.defaultOutputBindings);
                        }
                    }
                }
            }
            return defaultBindings;
        }
    }
}
