using MoDuel.Abilities;
using MoDuel.State;
using System.Linq.Expressions;
using System.Reflection;

namespace MoDuel.Data.Assembled;

/// <summary>
/// The base class used by packages to details their code and automates the loading of such code.
/// </summary>
public abstract class PackagedCode {

    /// <summary>
    /// Create an empty fall back <see cref="PackagedCode"/> to use when one could not be loaded.
    /// </summary>
    public static PackagedCode CreateEmptyPackagedCode(Package package) => new NoPackagedCode(package);

    /// <summary>
    /// The <see cref="PackagedCode"/> used by <see cref="CreateEmptyPackagedCode(Package)"/>.
    /// <para>Doesn't load or require anything.</para>
    /// </summary>
    private class NoPackagedCode(Package package) : PackagedCode(package) {
        public override ICollection<Delegate> GetAllActions() => Array.Empty<Delegate>();
        public override void OnPackageLoaded(Package package) { }
        public override void OnDuelLoaded(DuelState state) { }
    }

    /// <summary>
    /// The <see cref="Data.Package"/> that contains this <see cref="PackagedCode"/>.
    /// </summary>
    public readonly Package Package;

    /// <summary>
    /// The collection of actions held within the <see cref="PackagedCode"/>.
    /// <para>The keys are defined through the <see cref="ActionNameAttribute"/> or the method name.</para>
    /// </summary>
    public readonly Dictionary<string, ActionFunction> Actions = [];

    /// <summary>
    /// The collection of abilities held within the <see cref="PackagedCode"/>.
    /// <para>The keys are defined through the type name of the ability.</para>
    /// </summary>
    public readonly Dictionary<string, Ability> Abilities = [];

    /// <summary>
    /// The dependencies that are required at load time.
    /// </summary>
    private readonly HashSet<DependencyAttribute> Dependencies = [];

    /// <summary>
    /// Flag that is set when the packed code has been loaded.
    /// </summary>
    public bool IsLoaded { get; private set; } = false;

    public PackagedCode(Package package) {
        Package = package;
        RegisterActions();
        RegisterAbilities();
    }

    /// <summary>
    /// Performs all the loading steps after creation.
    /// </summary>
    internal void Load() {
        // Ensure the code is only loaded once.
        if (IsLoaded) return;
        IsLoaded = true;
        LoadAllDependencies();
        OnPackageLoaded(Package);
    }
    /// <summary>
    /// Searches via reflection for all the <see cref="ActionNameAttribute"/>s and build delegates so they can be returned in <see cref="GetAllActions"/>.
    /// </summary>
    /// <returns>All the <see cref="ActionFunction"/> delegates that could be found in the package.</returns>
    protected ICollection<Delegate> GetAllActionsViaTag() {
        var types = GetType().Assembly.GetTypes();
        var methods = types.SelectMany(t => t.GetMethods());
        var actions = methods.Where(m => m.GetCustomAttribute<ActionNameAttribute>() != null);
        var delegates = actions.Select(a => BuildDelegate(a));
        return delegates.ToList();
    }

    /// <summary>
    /// Builds a <see cref="Delegate"/> from the provided <see cref="MethodInfo"/>.
    /// <para>The <paramref name="instance"/> is required for non-static calls.</para>
    /// </summary>
    private Delegate BuildDelegate(MethodInfo methodInfo, object? instance = null) {

        // Get the types of the parameters.
        IEnumerable<Type> parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType);

        // Start building the expression tree.
        var parameters = parameterTypes.Select(Expression.Parameter).ToArray();

        // The expression should call the method with the parameters.

        MethodCallExpression body;

        if (methodInfo.IsStatic) {
            body = Expression.Call(methodInfo, parameters);
        }
        else {
            Expression instanceExpression = Expression.Constant(instance);
            body = Expression.Call(instanceExpression, methodInfo, parameters);
        }

        // The name of the lambda also the name that the action will be searched by.
        var name = methodInfo.GetCustomAttribute<ActionNameAttribute>()?.ActionName ?? methodInfo.Name;

        // Record every dependency that needs to be loaded.
        var dependencies = methodInfo.GetCustomAttributes<DependencyAttribute>();
        foreach (var dependency in dependencies) {
            Dependencies.Add(dependency);
        }

        // Build and return the delegate.
        var lambda = Expression.Lambda(body, name, true, parameters);
        return lambda.Compile(false);
    }

    /// <summary>
    /// Registers all the actions from the package and requests a load of any dependencies.
    /// </summary>
    private void RegisterActions() {

        foreach (var action in GetAllActions()) {

            var methodInfo = action.Method;
            // Actions cam use an attribute to define their name or fall back to the method name.
            var actionName = methodInfo.GetCustomAttribute<ActionNameAttribute>()?.ActionName ?? methodInfo.Name;

            // Get the full item path so that it can be reloaded.
            var actionPath = PackageCatalogue.GetFullItemPath(Package, actionName);

            // Register and assign the action function.
            ActionFunction func = new(actionPath, action);
            Actions.Add(actionName, func);

            // Record every dependency that needs to be loaded.
            var dependencies = methodInfo.GetCustomAttributes<DependencyAttribute>();
            foreach (var dependency in dependencies) {
                Dependencies.Add(dependency);
            }

        }
    }

    /// <summary>
    /// Registers all the abilities from the package and requests a load of any dependencies.
    /// </summary>
    private void RegisterAbilities() {

        var abilityTypes = GetType().Assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(Ability)));
        foreach (var abilityType in abilityTypes) {

            // TODO PERFORMANCE: Consider other ways to call the constructor that may be more performant.
            Ability? ability = (Ability?)Activator.CreateInstance(abilityType);
            if (ability == null) {
                continue;
            }

            // Register the ability and provide the source package.
            ability.SourcePackage = Package;
            Abilities.Add(abilityType.Name, ability);

            // Record every dependency that needs to be loaded.
            var dependencies = abilityType.GetCustomAttributes<DependencyAttribute>();
            foreach (var dependency in dependencies) {
                Dependencies.Add(dependency);
            }
        }
    }


    /// <summary>
    /// Load each of the <see cref="Dependencies"/>.
    /// <para>This should be done once all packages are loaded to handle recursion.</para>
    /// </summary>
    private void LoadAllDependencies() {
        foreach (var dependency in Dependencies) {
            dependency.LoadDependency(Package.Catalogue, Package);
        }
    }

    /// <summary>
    /// Gets a set of all the actions that exist within the package.
    /// <para>Can use <see cref="GetAllActionsViaTag"/> to see what should be returned.</para>
    /// </summary>
    public abstract ICollection<Delegate> GetAllActions();

    /// <summary>
    /// The call back once the <see cref="PackagedCode"/> has been fully loaded allow for custom code execution.
    /// </summary>
    public abstract void OnPackageLoaded(Package package);

    /// <summary>
    /// The call back once a new <see cref="DuelState"/> has been started.
    /// </summary>
    public abstract void OnDuelLoaded(DuelState state);

}
