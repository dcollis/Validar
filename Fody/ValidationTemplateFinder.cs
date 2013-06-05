using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public class ValidationTemplateFinder
{
    public TypeDefinition TypeDefinition;
    public TypeReference TypeReference;
    
    public MethodDefinition TemplateConstructor;
    public MethodReference TemplateConstructorRef;

    public List<TypeDefinition> AllTypes;

    public ModuleDefinition ModuleDefinition { get; set; }

    public void Execute()
    {
        var typeDefinition = AllTypes.FirstOrDefault(x => x.Name == "ValidationTemplate" || x.Name == "ValidationTemplate`1");
        if (typeDefinition== null)
        {
            throw new WeavingException("Could not find a type named ValidationTemplate");
        }
        TypeReference = ModuleDefinition.Import(typeDefinition);
        TypeDefinition = TypeReference.Resolve();

        TemplateConstructorRef = ModuleDefinition.Import(TypeDefinition
            .Methods
            .FirstOrDefault(x =>
                            x.IsConstructor &&
                            x.Parameters.Count == 1 &&
                            x.Parameters.First().ParameterType.Name == "INotifyPropertyChanged"));
        if (TemplateConstructorRef == null)
        {
            throw new WeavingException("Found ValidationTemplate but it did not have a constructor that takes INotifyPropertyChanged as a parameter");
        }
        TemplateConstructor = TemplateConstructorRef.Resolve();
    }

}