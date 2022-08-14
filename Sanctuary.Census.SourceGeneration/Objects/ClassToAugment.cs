using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Sanctuary.Census.SourceGeneration.Objects;

public class ClassToAugment
{
    public string Namespace { get; }
    public string Name { get; }
    public ImmutableArray<Location> Locations { get; }
    public IReadOnlyList<IPropertySymbol> Properties { get; }
    public IReadOnlyList<IFieldSymbol> Constants { get; }
    public ImmutableArray<AttributeData> Attributes { get; }
    public ImmutableArray<IMethodSymbol> Constructors { get;  }

    public ClassToAugment
    (
        string @namespace,
        string name,
        ImmutableArray<Location> locations,
        IReadOnlyList<IPropertySymbol> properties,
        IReadOnlyList<IFieldSymbol> constants,
        ImmutableArray<AttributeData> attributes,
        ImmutableArray<IMethodSymbol> constructors
    )
    {
        Namespace = @namespace;
        Name = name;
        Locations = locations;
        Properties = properties;
        Constants = constants;
        Attributes = attributes;
        Constructors = constructors;
    }

    public static ClassToAugment FromNamedTypeSymbol(INamedTypeSymbol classSymbol)
    {
        // Get the full type name of the class, including the namespace
        string fullName = classSymbol.ToString();

        ImmutableArray<ISymbol> classMembers = classSymbol.GetMembers();
        List<IPropertySymbol> properties = new(classMembers.Length);
        List<IFieldSymbol> constants = new();

        foreach (ISymbol member in classMembers)
        {
            switch (member)
            {
                case IPropertySymbol property:
                    properties.Add(property);
                    break;
                case IFieldSymbol { IsConst: true } field:
                    constants.Add(field);
                    break;
            }
        }

        int index = fullName.LastIndexOf('.');
        string @namespace = fullName.Substring(0, index);
        string name = fullName.Substring(index + 1);
        ImmutableArray<AttributeData> attributes = classSymbol.GetAttributes();

        return new ClassToAugment
        (
            @namespace,
            name,
            classSymbol.Locations,
            properties,
            constants,
            attributes,
            classSymbol.Constructors
        );
    }
}
