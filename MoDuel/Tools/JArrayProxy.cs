using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace MoDuel.Tools {

    public class JArrayProxy {
        
        /// <summary>
        /// Registers the proxy type to <see cref="UserData"/>.
        /// </summary>
        public static void Register() {
            UserData.RegisterProxyType<JArrayProxy, JArray>(r => new JArrayProxy(r));
        }

        /// <summary>
        /// The json array we are proxying.
        /// </summary>
        readonly JArray arr;

        [MoonSharpHidden]
        public JArrayProxy(JArray arr) {
            this.arr = arr;
        }

        /// <summary>
        /// Get only indexed accessor of the <see cref="JArray"/>.
        /// <para>Adjusted -1 to behave like lua indexing.</para>
        /// </summary>
        public object this[int pos] {
            get {
                return arr[pos - 1].ToObject<object>();
            }
        }

        public int Length => arr.Count;

        public bool Contains(object obj) {
            return arr.Contains(obj);
        }

    }

}
