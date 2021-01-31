using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace MoDuel.Tools {

    public class JObjectProxy {

        /// <summary>
        /// Registers the proxy type to <see cref="UserData"/>.
        /// </summary>
        public static void Register() {
            UserData.RegisterProxyType<JObjectProxy, JObject>(r => new JObjectProxy(r));
        }

        /// <summary>
        /// The json object we are proxying.
        /// </summary>
        readonly JObject obj;

        [MoonSharpHidden]
        public JObjectProxy(JObject obj) {
            this.obj = obj;
        }

        /// <summary>
        /// Retrieves a value from the json object with the given key.
        /// <para>Returns <c>null</c> if the key wasn't found.</para>
        /// </summary>
        public object GetValue(string key) {
            if (!obj.ContainsKey(key))
                return null;
            return obj[key];
        }

        public Table ToTable(Table table) {
            foreach (var pair in obj) {
                table[pair.Key] = pair.Value;
            }
            return table;
        }


    }
}
