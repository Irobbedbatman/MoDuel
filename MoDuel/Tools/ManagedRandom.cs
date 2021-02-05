using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Tools {

    [MoonSharp.Interpreter.MoonSharpUserData]
    public class ManagedRandom : Random {

        public ManagedRandom() { }

        public ManagedRandom(int seed) : base(seed) { }

        /// <summary>
        /// Returns a random number that is low inclusive but high exclusive.
        /// </summary>
        public int NextInt(int low, int high) => Next(low, high);

        /// <summary>
        /// Returns a random number that is low inclusive and high inclusive.
        /// <para>Doen't support high = <see cref="int.MaxValue"/></para>
        /// </summary>
        public int NextInc(int low, int high) => Next(low, high+1);

        /// <summary>
        /// Returns a random float between high and low inclusivly.
        /// </summary>
        public float NextFloat(float low, float high) {
            float val = Next();
            float percent = val / (int.MaxValue - 1);
            return (high - low) * percent + low;
        }

        /// <summary>
        /// Retrieves a random object from a collection.
        /// </summary>
        public T NextItem<T>(IEnumerable<T> collection) {
            var c = collection.Count();
            if (c > 0)
                return collection.ElementAt(Next(0, c));
            return default;
        }

        /// <summary>
        /// Retrieves a random object from a <see cref="MoonSharp.Interpreter.Table"/>
        /// </summary>
        public MoonSharp.Interpreter.DynValue NextItem(MoonSharp.Interpreter.Table table) => NextItem(table.Values);


    }
}
