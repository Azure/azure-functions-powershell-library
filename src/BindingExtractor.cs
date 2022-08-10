//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings;
using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System.Management.Automation.Language;
using System.Reflection;

namespace Microsoft.Azure.Functions.PowerShell.SDK
{
    internal static class BindingExtractor
    {
        static BindingExtractor()
        {
            // We want this behavior to be extensible by third parties who may want to add binding definitions. 
            // To accomplish this, we parse through all loaded assemblies to find any class which implements 
            //   IBinding, and load it manually into our supportedBindings

            // Find all types that implement IBinding
            IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(IBinding))));
                }
                catch (ReflectionTypeLoadException)
                {
                    // Do nothing, ReflectionTypeLoadExceptions seem to be thrown in the virtualized PowerShell environments used 
                    // by GitHub in the test environment. So far, ignoring them hasn't changed the behavior of the app, they seem
                    // to be thrown only by assemblies that come packaged with .NET. 
                }
            }
            foreach (Type type in types)
            {
                try
                {
                    // Instantiate an object of this type, and call its method to load it into supportedBindings for use later
                    object? obj = Activator.CreateInstance(type);
                    if (!BindingExtractor.hasSupportedBinding(type) && obj is not null)
                    {
                        BindingExtractor.addSupportedBinding((IBinding)obj);
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
            IEnumerable<IInputBinding> inputBindings = supportedBindings.Where(x => x is IInputBinding).Cast<IInputBinding>();
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
            IEnumerable<IOutputBinding> outputBindings = supportedBindings.Where(x => x is IOutputBinding).Cast<IOutputBinding>();
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
                // Try to figure out which IBinding class was used to create the BindingInformation 
                //   Might be worth changing BindingInformation to refer to the instance of IBinding that created it
                //   Would need to avoid serializing this information when returning to the worker. 
                IEnumerable<IBinding> matchingSupportedBindings = supportedBindings.Where(x => x.BindingType == existingInputBinding.Type);

                if (matchingSupportedBindings.Count() > 0)
                {
                    foreach (IInputBinding matchingSupportedBinding in matchingSupportedBindings)
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
