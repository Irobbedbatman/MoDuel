using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Mana {

    /// <summary>
    /// A struct thats only stores the name of a <see cref="Mana"/> and staticly stores all the loaded mana types.
    /// </summary>
    [MoonSharp.Interpreter.MoonSharpUserData]
    public readonly struct ManaType {

        /// <summary>
        /// A static <see cref="ManaType"/> for refrence purposes.
        /// <para>Important to note that <see cref="Name"/> will be <c>null</c>.</para>
        /// </summary>
        public static readonly ManaType Invalid = new ManaType();

        /// <summary>
        /// The types of mana that have been created and thus loaded.
        /// </summary>
        private static readonly HashSet<ManaType> LoadedTypes = new HashSet<ManaType>();

        /// <summary>
        /// Gets a <see cref="ManaType"/> if it has been loaded.
        /// </summary>
        public static bool TryGetMana(string name, out ManaType type) => LoadedTypes.TryGetValue(new ManaType(name), out type);

        /// <summary>
        /// Gets a <see cref="ManaType"/> if has been loadded otherwise returns <see cref="Invalid"/>.
        /// </summary>
        public static ManaType GetMana(string name) {
            if (TryGetMana(name, out var mana))
                return mana;
            else
                return Invalid;
        }

        /// <summary>
        /// Convertor of <see cref="ManaType"/>'s to string as this is all they really are.
        /// </summary>
        public static implicit operator string(ManaType mt) {
            return mt.Name;
        }

        //END STATIC

        /// <summary>
        /// The name and whole identity of this see <see cref="ManaType"/>.
        /// </summary>
        public readonly string Name;

        public ManaType(string name) {
            Name = name;
            LoadedTypes.Add(this);
        }




    }
}
