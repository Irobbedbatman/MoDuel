using MoDuel.Cards;
using MoDuel.Field;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Environment;

namespace MoDuel {

    [MoonSharpUserData]
    public abstract class Target {
        /// <summary>
        /// The <see cref="Tools.Indexer"/> used to generate the unique <see cref="TargetIndex"/>
        /// </summary>
        private static Tools.Indexer _Indexer = new Tools.Indexer();

        /// <summary>
        /// The dictionary of all targets so that one can retreived by index.
        /// </summary>
        private static Dictionary<int,Target> Targets = new Dictionary<int, Target>();

        /// <summary>
        /// Get a target with the given index.
        /// </summary>
        public static Target GetTarget(int targetIndex) => Targets[targetIndex];

        //END STATIC

        /// <summary>
        /// The unique index of this target. Can be set in a constructor; for instance when playing as a client to sync data with other connected instances.
        /// </summary>
        public readonly int Index = -1;

        public Target (int Index = -1) {
            if (Index == -1)
                Index = _Indexer.GetNext();
            this.Index = Index;
            if (!Targets.ContainsKey(Index))
                Targets.Add(Index, this);
        }

        /// <summary>
        /// Deconstructor of a Target. Frees up the unqiue index to use elsewhere.
        /// </summary>
        ~Target() { _Indexer.Free(Index); Targets.Remove(Index); }

        /// <summary>
        /// A MoonSharp lua table.
        /// <para>Use this to store instance values in lua code on any <see cref="Target"/>.</para>
        /// </summary>
        public Table Values = new Table(null); //TODO: Target correct table value.


        /// <summary>
        /// Is the target a creature instance.
        /// </summary>
        public bool IsCreature => GetType() == typeof(CreatureInstance);
        /// <summary>
        /// Is the target the whole field or a sub field.
        /// </summary>
        public bool IsField => GetType() == typeof(FullField) || GetType() == typeof(SubField);
        /// <summary>
        /// Is the target the whole field.
        /// </summary>
        public bool IsWholeField => GetType() == typeof(FullField);
        /// <summary>
        /// Is the target a sub field.
        /// </summary>
        public bool IsSubField => GetType() == typeof(SubField);
        /// <summary>
        /// Is the target a field slot.
        /// </summary>
        public bool IsSlot => GetType() == typeof(FieldSlot);
        /// <summary>
        /// Is the target a card instance.
        /// </summary>
        public bool IsCard => GetType() == typeof(CardInstance);
        /// <summary>
        /// Is the target a player.
        /// </summary>
        public bool IsPlayer => GetType() == typeof(Player);
        /// <summary>
        /// Is the target an effect.
        /// </summary>
        public bool IsEffect => GetType() == typeof(OngoingEffect);


    }
}
