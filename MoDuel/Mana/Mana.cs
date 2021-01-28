using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Mana {

    [MoonSharp.Interpreter.MoonSharpUserData]
    public class Mana {

        /// <summary>
        /// The type of mana this <see cref="PlayerMana"/> is
        /// </summary>
        public ManaType ManaType;

        /// <summary>
        /// How much of this mana is currently available to use.
        /// <para>Should be bound between 0 and <see cref="ManaCap"/></para>
        /// </summary>
        public int ManaCount { get; set; }

        public Mana(ManaType manatype) {
            ManaType = manatype;
            ManaCount = 0;
        }

    }
}
