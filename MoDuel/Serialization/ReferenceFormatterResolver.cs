using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using MoDuel.Data;
using MoDuel.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace MoDuel.Serialization;

/// <summary>
/// A <see cref="IFormatterResolver"/> that also allows for refernces to be stored.
/// </summary>
public class ReferenceFormatterResolver : IFormatterResolver {

    /// <summary>
    /// Converts a <see cref="MethodInfo"/> into a string such that the <see cref="MethodInfo"/> can be retreieved using <see cref="DeserializeMethod(string)"/>
    /// </summary>
    public static string SerializeMethod(MethodInfo info) {
        string typeName = info.DeclaringType?.GetSimpleQualifiedName() ?? "";
        return string.Concat(typeName, "|", info.Name);
    }

    /// <summary>
    /// Parses the provided <paramref name="path"/> to get the <see cref="MethodInfo"/> that was serialzied usign <see cref="SerializeMethod(MethodInfo)"/>.
    /// </summary>
    public static MethodInfo? DeserializeMethod(string path) {
        string[] complonents = path.Split('|');
        Type? type = Type.GetType(complonents[0]);
        if (type == null)
            return null;
        MethodInfo? method = type.GetMethod(complonents[1]);
        return method;
    }


#nullable disable

    /// <summary>
    /// The formatter that handle object references.
    /// </summary>
    private class ReferenceFormatter<T> : IMessagePackFormatter<T> {
        /// <summary>
        /// The container for serialized references.
        /// </summary>
        private readonly ReferencePool Pool;
        public ReferenceFormatter(ReferencePool pool) { Pool = pool; }
        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => (T)Pool.ReadReferncedObject(reader.ReadInt32());
        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options) => writer.WriteInt32(Pool.Serialize(value, options));

    }

    /// <summary>
    /// The formatter that handles reloadable files.
    /// </summary>
    private class ReloadFromatter<T> : IMessagePackFormatter<T> {

        /// <summary>
        /// The resolver that created this formatter. Used to call back to the resolver if the object should not be treated as a reloable.
        /// </summary>
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ReferenceFormatterResolver Resolver;
#pragma warning restore IDE0052 // Remove unread private members
        /// <summary>
        /// The <see cref="PackageCatalogue"/> that items will be reloaded from.
        /// </summary>
        private readonly PackageCatalogue Catalogue;

        public ReloadFromatter(ReferenceFormatterResolver resolver, PackageCatalogue catalogue) {
            Resolver = resolver; Catalogue = catalogue;
        }

        /// <inheritdoc/>
        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {

            // Check to see if the PackageCatalouge should be reloaded.
            if (typeof(T) == typeof(PackageCatalogue)) {
                // Read the dummy value.
                reader.ReadBoolean();
                return (T)(object)Catalogue;
            }

            // Get the item path of the object,
            var itemPath = reader.ReadString();

            return itemPath == null ? throw new NotImplementedException() : Catalogue.Load<T>(itemPath);
        }

        /// <inheritdoc/>
        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options) {

            // Package Catalouge needs to write a dummy value.
            if (value is PackageCatalogue) {
                // THis is a dummy value and need not be read.
                writer.Write(true);
                return;
            }

            if (value is not IReloadable reloadable) {
                return;
            }

            writer.Write(reloadable.GetItemPath());

        }
    }

    /// <summary>
    /// The formatter that handles <see cref="JToken"/> files.
    /// </summary>
    private class JTokenFormatter<T> : IMessagePackFormatter<T> {

        /// <summary>
        /// The <see cref="PackageCatalogue"/> that items will be reloaded from.
        /// </summary>
        private readonly PackageCatalogue Catalogue;
        /// <summary>
        /// The container for serialized references.
        /// </summary>
        private readonly ReferencePool Pool;

        public JTokenFormatter(ReferencePool pool, PackageCatalogue catalogue) {
            Catalogue = catalogue;
            Pool = pool;
        }

        /// <inheritdoc/>
        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {

            reader.ReadArrayHeader();

            // Check to see if the jso data can be reloaded.
            if (reader.ReadBoolean()) {
                string itemPath = reader.ReadString();
                // If the itemPath was null then it is the dead token.
                if (itemPath == null) {
                    return (T)(object)DeadToken.Instance;
                }
                return Catalogue.Load<T>(itemPath);
            }
            // If it was not it has been stored with as a refernce type.
            else {
                int id = reader.ReadInt32();
                return (T)Pool.ReadReferncedObject(id);
            }

        }

        /// <inheritdoc/>
        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options) {

            var token = value as JToken;

            // Need to write if the json data was reloadable.
            writer.WriteArrayHeader(2);

            // Check the jtoken can be reloaded.
            var reloadPath = token.GetReloadItemPath();
            if (token == DeadToken.Instance || reloadPath != null) {
                writer.Write(true);
                writer.Write(reloadPath);
                return;
            }
            // If the json can not be reloaded it is instead to be stored as a refernce.
            else {
                writer.Write(false);
                writer.Write(Pool.SerializeJson(token, options));
            }

        }

    }


#nullable enable

    /// <summary>
    /// The <see cref="ReferencePool"/> that references will be read and written to.
    /// </summary>
    private readonly ReferencePool Pool;

    /// <summary>
    /// The <see cref="PackageCatalogue"/> that items will be reloaded from.
    /// </summary>
    private readonly PackageCatalogue Catalogue;

    public bool SkipReload = false;

    public ReferenceFormatterResolver(ReferencePool pool, PackageCatalogue catalogue) {
        Pool = pool;
        Catalogue = catalogue;
    }

    /// <summary>
    /// Behaviour of <see cref="GetFormatter{T}()"/> but allows avoiding the <see cref="IReloadable"/> formatter.
    /// </summary>
    public IMessagePackFormatter<T>? GetFormatter<T>(bool skipReload) {

        SkipReload = false;

        // If the type is a json token serialize it specially.
        if (typeof(T).IsAssignableTo(typeof(JToken))) {
            return new JTokenFormatter<T>(Pool, Catalogue);
        }

        // If the type is reloadable use it's special formatter.
        if (!skipReload && typeof(T).IsAssignableTo(typeof(IReloadable))) {
            return new ReloadFromatter<T>(this, Catalogue);
        }

        // If the type is marked for reference recording use special formatter.
        if (typeof(T).GetCustomAttribute<SerializeReferenceAttribute>() != null) {
            return new ReferenceFormatter<T>(Pool);
        }

        // Fallback to MessagePacks next formatter.
        return TypelessContractlessStandardResolver.Instance.GetFormatter<T>();
    }

    /// <inheritdoc/>
    public IMessagePackFormatter<T>? GetFormatter<T>() => GetFormatter<T>(SkipReload);

}
