using MoDuel.Cards;
using MoDuel.Heroes;
using MoDuel.Resources;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace MoDuel.Data.Assembled;

/// <summary>
/// Attribute attached to <see cref="ActionFunction"/> methods that inform the loader of a dependency it has.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class DependencyAttribute : Attribute {

    /// <summary>
    /// The type of dependency that can be required to load.
    /// </summary>
    public enum DependencyTypes {
        Card,
        Hero,
        Action,
        Json,
        ResourceType,
        /// <summary>
        /// The type used when a <see cref="Type"/> was passed to the constructor.
        /// </summary>
        RawType
    }

    /// <summary>
    /// The type of dependency to load.
    /// </summary>
    public readonly DependencyTypes DependencyType;

    /// <summary>
    /// Gets a <see cref="DependencyType"/> regardless of if a clr type was provided.
    /// </summary>
    public DependencyTypes FullDependencyType {
        get {
            if (DependencyType != DependencyTypes.RawType) {
                return DependencyType;
            }
            return GetDependencyTypesFromClrType(DependencyRawType);

        }
    }

    /// <summary>
    /// The name and path of the dependency to load.
    /// </summary>
    public readonly string DependencyName;

    /// <summary>
    /// The <see cref="Type"/> of decencies to load when using a specific type.
    /// </summary>
    public readonly Type? DependencyRawType;

    public DependencyAttribute(DependencyTypes type, string name) {
        DependencyType = type;
        DependencyName = name;
    }

    public DependencyAttribute(Type type, string name) {
        DependencyType = DependencyTypes.RawType;
        DependencyName = name;
        DependencyRawType = type;
    }

    /// <inheritdoc/>
    public override int GetHashCode() {
        HashCode hashCode = new();
        hashCode.Add(FullDependencyType);
        hashCode.Add(DependencyName);
        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is not DependencyAttribute dep) {
            return false;
        }
        return FullDependencyType == dep.FullDependencyType &&
            DependencyName == dep.DependencyName;
    }

    /// <inheritdoc/>
    public override bool Match(object? obj) {
        return Equals(obj);
    }

    /// <summary>
    /// Converts a clr type to a <see cref="DependencyTypes"/> value.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public DependencyTypes GetDependencyTypesFromClrType(Type? type) {
        return (type?.Name) switch {
            nameof(Card) => DependencyTypes.Card,
            nameof(Hero) => DependencyTypes.Hero,
            nameof(ActionFunction) or nameof(Action) => DependencyTypes.Hero,
            nameof(ResourceType) => DependencyTypes.ResourceType,
            nameof(JsonObject) or nameof(JsonNode) => DependencyTypes.Json,
            _ => throw new NotImplementedException($"Dependency loading for type: {type}; Item Path: {DependencyName}."),
        };
    }

    /// <summary>
    /// Loads the dependency that this references.
    /// </summary>
    /// <param name="catalogue">The list of all packages as the dependency can be external.</param>
    /// <param name="package">The package the item this attribute is attached to is within for local addressing.</param>
    /// <exception cref="NotImplementedException"></exception>
    public void LoadDependency(PackageCatalogue catalogue, Package package) {
        switch (FullDependencyType) {
            case DependencyTypes.Card:
                catalogue.LoadCard(DependencyName, package);
                break;
            case DependencyTypes.Hero:
                catalogue.LoadHero(DependencyName, package);
                break;
            case DependencyTypes.Action:
                catalogue.LoadAction(DependencyName, package);
                break;
            case DependencyTypes.Json:
                catalogue.LoadJson(DependencyName, package);
                break;
            case DependencyTypes.ResourceType:
                catalogue.LoadResourceType(DependencyName, package);
                break;
            default:
                throw new NotImplementedException($"Dependency loading for type: {DependencyType}; Item Path: {DependencyName}.");
        }
    }

    /// <summary>
    /// Loads the dependency that this references.
    /// </summary>
    /// <param name="type">The <see cref="DependencyRawType"/> that was provided.</param>
    /// <param name="catalogue">The list of all packages as the dependency can be external.</param>
    /// <param name="package">The package the item this attribute is attached to is within for local addressing.</param>
    /// <exception cref="NotImplementedException"></exception>
    private void LoadDependency(Type? type, PackageCatalogue catalogue, Package package) {
        if (type == null)
            return;
        switch (type.Name) {
            case nameof(Card):
                catalogue.LoadCard(DependencyName, package);
                break;
            case nameof(Hero):
                catalogue.LoadHero(DependencyName, package);
                break;
            case nameof(ActionFunction):
            case nameof(Action):
                catalogue.LoadAction(DependencyName, package);
                break;
            case nameof(ResourceType):
                catalogue.LoadResourceType(DependencyName, package);
                break;
            case nameof(JsonNode):
            case nameof(JsonObject):
                catalogue.LoadJson(DependencyName, package);
                break;
            default:
                throw new NotImplementedException($"Dependency loading for type: {type}; Item Path: {DependencyName}.");
        }

    }

}
