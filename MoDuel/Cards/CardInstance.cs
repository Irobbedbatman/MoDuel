using MoDuel.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Cards {

    [MoonSharp.Interpreter.MoonSharpUserData]
    public class CardInstance : Target {

        /// <summary>
        /// The loaded <see cref="Card"/> this <see cref="CardInstance"/> derives from.
        /// </summary>
        public readonly Card Imprint;

        public readonly CardInstanceActivator Activator;

        /// <summary>
        /// A function that gets this cards cost from it's imprint's <see cref="Card.Parameters"/> and <see cref="Level"/>.
        /// <para>If <see cref="Level"/> is too high returns highest value int the <see cref="JArrayProxy"/>.</para>
        /// </summary>
        public int ImprintCost =>  (int)GetLeveledParamter<long>("Cost", 0);

        /// <summary>
        /// A function that gets this cards attack from it's imprint's <see cref="Card.Parameters"/> and <see cref="Level"/>.
        /// <para>If <see cref="Level"/> is too high returns highest value int the <see cref="JArrayProxy"/>.</para>
        /// </summary>
        public int ImprintAttack => (int)GetLeveledParamter<long>("Attack", 0);

        /// <summary>
        /// A function that gets this cards armor from it's imprint's <see cref="Card.Parameters"/> and <see cref="Level"/>.
        /// <para>If <see cref="Level"/> is too high returns highest value int the <see cref="JArrayProxy"/>.</para>
        /// </summary>
        public int ImprintArmor => (int)GetLeveledParamter<long>("Armor", 0);

        /// <summary>
        /// A function that gets this cards life from it's imprint's <see cref="Card.Parameters"/> and <see cref="Level"/>.
        /// <para>If <see cref="Level"/> is too high returns highest value int the <see cref="JArrayProxy"/>.</para>
        /// </summary>
        public int ImprintLife => (int)GetLeveledParamter<long>("Life", 1);

        /// <summary>   
        /// Gets a parameter from <see cref="Imprint.Parameters"/> that's type is an <see cref="JArrayProxy"/>.
        /// <para>Uses <see cref="Level"/> as the index to retrieve from.</para>
        /// <para>If <see cref="Level"/> is too high returns highest value int the <see cref="JArrayProxy"/>.</para>
        /// </summary>
        /// <param name="parameter">The paramater key in <see cref="Card.Parameters"/>.</param>
        /// <param name="fallback">The value to use if the parameter can't be found.</param>
        public object GetLeveledParamter(string parameter, object fallback = null) {
            if (Imprint.Parameters?.ContainsKey(parameter) ?? false) {
                var arr = new JArrayProxy(Imprint.Parameters[parameter].ToObject<JArray>());
                int index = Math.Max(1, Math.Min(arr.Length, Level));
                return arr[index];
            }
            return fallback;
        }

        /// <summary>   
        /// Gets a parameter from <see cref="Imprint.Parameters"/> that's type is an <see cref="JArrayProxy"/>.
        /// <para>Uses <see cref="Level"/> as the index to retrieve from.</para>
        /// <para>If <see cref="Level"/> is too high returns highest value int the <see cref="JArrayProxy"/>.</para>
        /// </summary>
        /// <param name="parameter">The paramater key in <see cref="Card.Parameters"/>.</param>
        /// <param name="fallback">The value to use if the parameter can't be found.</param>
        public T GetLeveledParamter<T>(string parameter, T fallback = default) {
            if (Imprint.Parameters?.ContainsKey(parameter) ?? false) {
                try {
                    var num = Imprint.Parameters[parameter].ToObject<T>();
                    if (num != null)
                        return num;
                }
                catch (Exception) { }
                var arr = new JArrayProxy(Imprint.Parameters[parameter].ToObject<JArray>());
                int index = Math.Max(1, Math.Min(arr.Length, Level));
                return (T)arr[index];
            }
            return fallback;
        }

        /// <summary>
        /// The form this card was in previously.
        /// <para><see cref="null"/> if this is the original form.</para>
        /// </summary>
        public readonly CardInstance PreviousState = null;

        /// <summary>
        /// The form this card was when all of its <see cref="PreviousState"/>s are gone through.
        /// 
        /// </summary>
        public CardInstance OriginalState => PreviousState ?? this;

        /// <summary>
        /// The current owner of this card.
        /// </summary>
        public Player Owner;

        /// <summary>
        /// The owner of this card originally.
        /// </summary>
        public readonly Player OriginalOwner = null;

        /// <summary>
        /// The level this card is currently at.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// The cost this card is currently at.
        /// </summary>
        public int CurrentCost { get; set; }

        /// <summary>
        /// The grave cost of this card.
        /// </summary>
        public int GraveCost {get; set; }

        /// <summary>
        /// Constructor that creates a new card instance for a card with a given owner.
        /// </summary>
        /// <param name="imprint">The card this card instance copies from.</param>
        public CardInstance(Card imprint, CardInstanceActivator activator) {
            Imprint = imprint;
            Activator = activator;
            activator.Enable(this);
        }

        /// <summary>
        /// Constructor that specifies for ownership.
        /// </summary>
        /// <param name="originalOwner">The owner of this card, both the original and original owner.</param>
        public CardInstance(Card imprint, CardInstanceActivator activator, Player originalOwner) : this(imprint, activator) {
            Owner = originalOwner;
            OriginalOwner = originalOwner;
        }

        /// <summary>
        /// Constructor that creates a new card over an old one, useful for transformation.
        /// </summary>
        /// <param name="previousState">The card instance to pull original state and previous state from.</param>
        public CardInstance(Card imprint, CardInstanceActivator activator, CardInstance previousState) : this(imprint, activator) {
            Owner = previousState.Owner;
            OriginalOwner = previousState.OriginalOwner;
            PreviousState = previousState;
        }

        public CardInstance SetOwner(Player owner) {
            Owner = owner;
            return this;
        }

        /// <summary>
        /// Is this card in it's owners Hand.
        /// </summary>
        public bool InHand => Owner.IsCardInHand(this);

        /// <summary>
        /// Is this card in it's owners Grave.
        /// </summary>
        public bool InGrave => Owner.IsCardInGrave(this);

        /// <summary>
        /// Checks to see if this card is imprinted off of a card with a given id.
        /// </summary>
        /// <param name="cardId"><see cref="Card.CardID"/></param>
        /// <returns>True if the card imprints id is equivalent to the given id.</returns>
        public bool MatchesId(string cardId) => Imprint.CardID == cardId;

        /// <summary>
        /// Check to see if this card instance is alive. Always false for card instances but for creatures it can vary.
        /// </summary>
        public bool IsAlive => false;


        public void Enable() => Activator.Enable(this);
        public void Disable() => Activator.Disable(this);
        public bool IsEnabled => Activator.IsEnabled(this);


    }
}
