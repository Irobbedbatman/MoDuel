using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Mana {

    [MoonSharp.Interpreter.MoonSharpUserData]
    public class ManaPool : IEnumerable<Mana> {

        /// <summary>
        /// The hidden dictionary that ties <see cref="ManaType"/> to a <see cref="Mana"/>.
        /// </summary>
        private Dictionary<ManaType, Mana> _pool = new Dictionary<ManaType, Mana>();

        /// <summary>
        /// Constructor that converts an array of <see cref="ManaType"/> and turns them into a <see cref="ManaPool"/>
        /// </summary>
        /// <param name="manatypes">Array of required <see cref="ManaType"/>s for this <see cref="ManaPool"/></param>
        public ManaPool(ManaType[] manatypes) {
            foreach (var manatype in manatypes)
                _pool.Add(manatype, new Mana(manatype));
        }

        /// <summary>
        /// Implicit operator that converts a <see cref="ManaPool"/> into the an array of <see cref="Mana"/>
        /// <para>Only <see cref="IEnumerable{T}"/> public accessor of <see cref="ManaPool"/>.</para>
        /// </summary>
        /// <param name="mp">The <see cref="ManaPool"/> to convet.</param>
        public static implicit operator Mana[] (ManaPool mp) { return mp._pool.Values.ToArray(); }
        /// <summary>
        /// Retrieve from <see cref="ManaPool"/> using an int instead of a <see cref="ManaType"/>
        /// <para>Retrieves from that position in the mana pool, rather than converting <paramref name="index"/> to a <see cref="ManaType"/>.</para>
        /// </summary>
        /// <param name="index">The index in the array of <see cref="ManaPool"/> values.</param>
        /// <returns>The Mana attached to the given index."/></returns>
        public Mana this[int index] => _pool.Values.ToArray()[index-1];

        /// <summary>
        /// Retrieve from <see cref="ManaPool"/> using a <see cref="ManaType"/> parsed from a string.
        /// </summary>
        /// <param name="manatypestring">The string to use <see cref="Enum.Parse(Type, string)"/> on.</param>
        /// <returns>A Mana tied to a <see cref="CostType"/></returns>
        public Mana this[string manatypestring] => _pool[new ManaType(manatypestring)];

        /// <summary>
        /// Retrieve from <see cref="ManaPool"/> using a <see cref="ManaType"/>.
        /// </summary>
        /// <param name="type">The type of <see cref="Mana"/> we want to access.</param>
        /// <returns></returns>
        public Mana this[ManaType type] => _pool[type];

        public int Length => _pool.Count;

        IEnumerator<Mana> IEnumerable<Mana>.GetEnumerator() {
            return new ManaEnumerator(_pool.Values.ToArray());
        }

        public IEnumerator GetEnumerator() {
            return new ManaEnumerator(_pool.Values.ToArray());
        }
    }
}
