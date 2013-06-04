﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public partial class ModuleWeaver
{
    NotifyDataErrorInfoFinder notifyDataErrorInfoFinder;
    DataErrorInfoFinder dataErrorInfoFinder;
    ValidationTemplateFinder templateFinder;
    public Action<string> LogInfo { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public IAssemblyResolver AssemblyResolver { get; set; }

    public ModuleWeaver()
    {
        LogInfo = s => { };
    }

    public void Execute()
    {
        var allTypes = ModuleDefinition.GetTypes().Where(x => x.IsClass).ToList();
        try
        {
            templateFinder = new ValidationTemplateFinder
                                 {
                                     AllTypes = allTypes
                                 };
            templateFinder.Execute();
        }
        catch(WeavingException)
        {
            var refTypes = GetTypesFromAttributeAssembly(allTypes);
            templateFinder = new ValidationTemplateFinder
                                {
                                    AllTypes = refTypes
                                };
            templateFinder.Execute();
        }
        

        dataErrorInfoFinder = new DataErrorInfoFinder
                                  {
                                      ValidationTemplateFinder = templateFinder,
                                      ModuleDefinition = ModuleDefinition,
                                  };
        dataErrorInfoFinder.Execute();
        notifyDataErrorInfoFinder = new NotifyDataErrorInfoFinder
                                        {
                                            ValidationTemplateFinder = templateFinder,
                                            ModuleDefinition = ModuleDefinition,
                                        };
        notifyDataErrorInfoFinder.Execute();


        if (!dataErrorInfoFinder.Found && !notifyDataErrorInfoFinder.Found)
        {
            throw new WeavingException("Found ValidationTemplate but it did not implement INotifyDataErrorInfo or IDataErrorInfo");
        }
        ProcessTypes(allTypes);
        RemoveReference();
    }

    public List<TypeDefinition> GetTypesFromAttributeAssembly(List<TypeDefinition> allTypes)
    {
        int i = 0;
        TypeDefinition def = null;
        while (def == null && i < allTypes.Count)
        {
            if (allTypes[i].CustomAttributes.Any(x => x.Constructor.DeclaringType.Name == "InjectValidationAttribute"))
            {
                def = allTypes[i];
            }
            i++;
        }
        if(def != null)
        {
            var attribute = def.CustomAttributes.First(x => x.Constructor.DeclaringType.Name == "InjectValidationAttribute");
            return attribute.AttributeType.Module.GetTypes().Where(x => x.IsClass).ToList();
        }
        return null;
    }

    public void ProcessTypes(List<TypeDefinition> allTypes)
    {
        foreach (var type in allTypes)
        {
            if (!type.ImplementsINotify())
            {
                LogInfo(string.Format("Skipping '{0}' because it does not implement INotifyPropertyChanged.", type.Name));
                continue;
            }
            ProcessType(type);
        }
    }
    public void ProcessType(TypeDefinition typeDefinition)
    {
        if (!typeDefinition.CustomAttributes.ContainsValidationAttribute())
        {
            //TODO:log
            return;
        }

        if (dataErrorInfoFinder.Found)
        {
            var injector = new DataErrorInfoInjector
            {
                TypeDefinition = typeDefinition,
                TypeSystem = ModuleDefinition.TypeSystem,
                ValidationTemplateFinder = templateFinder,
                DataErrorInfoFinder = dataErrorInfoFinder,
                ModuleWeaver = this,
            };
            injector.Execute();
        }

        if (notifyDataErrorInfoFinder.Found)
        {
            var injector = new NotifyDataErrorInfoInjector
            {
                TypeDefinition = typeDefinition,
                NotifyDataErrorInfoFinder = notifyDataErrorInfoFinder,
                ValidationTemplateFinder = templateFinder,
                TypeSystem= ModuleDefinition.TypeSystem,
                ModuleWeaver = this,
            };
            injector.Execute();
        }
    }
}