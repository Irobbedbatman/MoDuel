using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoDuel.Mana;

namespace MoDuel.Cards {

    /// <summary>
    /// Whether a card is a creature or a spell.
    /// <para>Data files may refer to them literally and need to parse them. Using <see cref="Enum.Parse(CardType, string)"/>.</para>
    /// </summary>
    public enum CardType { Creature, Spell };

    /// <summary>
    /// A card that has been created or loaded from a file.
    /// <para>Stores all values and functions in <see cref="DynValue"/> lua functions.</para>
    /// </summary>
    [MoonSharpUserData]
    public class Card {

        /// <summary>
        /// The unique string identifier given to each card.
        /// </summary>
        public readonly string CardID;
        /// <summary>
        /// The mana this card uses to deduct from a <see cref="PlayerManaPool"/>
        /// </summary>
        public readonly ManaType ManaType;
        /// <summary>
        /// If the card is a spell or creature.
        /// </summary>
        public readonly CardType CardType;

        public bool IsCreature => CardType == CardType.Creature;
        public bool IsSpell => CardType == CardType.Spell;

        /// <summary>
        /// Fixed values that were listed in the json file.
        /// <para>Also contains information like attack, cost, etc.</para>
        /// </summary>  
        public readonly IReadOnlyDictionary<string, DynValue> Parameters;

        /// <summary>
        /// Checks to see if <see cref="Parameters"/>["Tags"] exists and contains <paramref name="tag"/>.
        /// </summary>
        /// <param name="tag">The tag to check for.</param>
        /// <returns></returns>
        public bool HasTag(string tag) {
            if (Parameters?.ContainsKey("Tags") ?? false) {
                var arr = Parameters["Tags"].ToObject<Newtonsoft.Json.Linq.JArray>().ToArray();
                return arr.Contains(tag);
            }
            return false;
        }

        /// <summary>
        /// One of the dictionary of triggers that are called on this card.
        /// <para>Reaction is a <see cref="DynValue"/> lua function.</para>
        /// </summary>
        public readonly IReadOnlyDictionary<string, DynValue> TriggerReactions, ExplicitTriggerReactions;

        public Card(string cardID, ManaType manaType, CardType cardType, Dictionary<string, DynValue> triggers, Dictionary<string, DynValue> exTriggers, Dictionary<string, DynValue> parameters) {
            CardID = cardID;
            ManaType = manaType;
            CardType = cardType;
            TriggerReactions = triggers;
            ExplicitTriggerReactions = exTriggers;
            Parameters = parameters;
        }

    }
}
