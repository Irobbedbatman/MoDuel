using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Mana {

    [MoonSharp.Interpreter.MoonSharpUserData]
    public readonly struct ManaType {

        public static ManaType Empty = new ManaType();

        private static HashSet<ManaType> LoadedTypes = new HashSet<ManaType>();

        public static bool TryGetMana(string name, out ManaType type) => LoadedTypes.TryGetValue(new ManaType(name), out type);

        public static implicit operator string(ManaType mt) {
            return mt.Name;
        }

        //END STATIC

        public readonly string Name;

        public ManaType(string name) {
            Name = name;
        }




    }
}
