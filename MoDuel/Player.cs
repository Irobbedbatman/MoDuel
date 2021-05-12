using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoDuel.Heroes;
using MoDuel.Cards;
using MoDuel.Field;
using MoDuel.Mana;
using MoDuel.Tools;

namespace MoDuel {

    [MoonSharpUserData]
    public class Player : Target {

        /// <summary>
        /// The uniqueid, name, username of this player.
        /// </summary>
        public readonly string UserId;

        /// <summary>
        /// The <see cref="Mana.ManaPool"/> that this player has.
        /// </summary>
        public readonly ManaPool ManaPool;

        /// <summary>
        /// The animations that are sent out for this specific player.
        /// </summary>
        [MoonSharpHidden]
        public EventHandler<ClientRequest> OutBoundDelegate;

        /// <summary>
        /// Invokes <see cref="OutBoundDelegate"/> with a request for the client to do. By calling s
        /// <para>Call <see cref="BlockPlayback(double)"/> afterward if the request should stop other things from happening.</para>
        /// /// </summary>
        /// <param name="requestId">The request for the target to do.</param>
        /// <param name="arguments">Arguments sent outwards for the animation.</param>
        public void SendRequest(string requestId, params DynValue[] arguments) {
            var request = new ClientRequest(requestId, arguments);
            OutBoundDelegate?.Invoke(this, request);
        }

        /// <summary>
        /// The <see cref="HeroInstance"/> this player is currently playing as.
        /// </summary
        public HeroInstance Hero { get; private set; }

        public void ChangeHero(Hero newHero) {
            Hero = new HeroInstance(newHero, this);
        }

        /// <summary>
        /// The <see cref="SubField"/> on this players side.
        /// </summary>
        public readonly SubField Field;

        /// <summary>
        /// One of the stats for this player.
        /// </summary>
        public int Level = 1, Exp = 0, Life = 25, MaxLife = 25;

        /// <summary>
        /// The Hand of the player currently.
        /// </summary>
        private readonly HashSet<CardInstance> _hand = new HashSet<CardInstance>();
        /// <summary>
        /// The Hand of the player currently; readonly.
        /// </summary>
        public IReadOnlyList<CardInstance> Hand => _hand.ToList().AsReadOnly();

        /// <summary>
        /// The Grave of the player currently.
        /// </summary>
        private readonly HashSet<CardInstance> _grave = new HashSet<CardInstance>();

        /// <summary>
        /// The Grave of the player currently; readonly.
        /// </summary>
        public IReadOnlyList<CardInstance> Grave => _grave.ToList().AsReadOnly();

        public HashSet<CardInstance> GetHandCards() {
            return new HashSet<CardInstance>(_hand);
        }

        public HashSet<CardInstance> GetGraveCards() {
            return new HashSet<CardInstance>(_grave);
        }

        public Player(string userId, Hero hero, ManaPool manaPool) {
            UserId = userId;
            ManaPool = manaPool;
            Field = new SubField(this);
            Hero = new HeroInstance(hero, this);
        }

        /// <summary>
        /// Adds a card to this player's hand.
        /// </summary>
        /// <param name="card"></param>
        public void AddCardToHand(CardInstance card) => _hand.Add(card.SetOwner(this));

        /// <summary>
        /// Removes a card from this player's hand if it currently in there.
        /// </summary>
        /// <param name="card"></param>
        public void RemoveCardFromHand(CardInstance card) {
            if (_hand.Contains(card))
                _hand.Remove(card.SetOwner(null));
        }

        /// <summary>
        /// Adds a card to this player's grave.
        /// </summary>
        /// <param name="card"></param>
        public void AddCardToGrave(CardInstance card) => _grave.Add(card.SetOwner(this));

        /// <summary>
        /// Removes a card from this player's grave if it currently in there.
        /// </summary>
        /// <param name="card"></param>
        public void RemoveCardFromGrave(CardInstance card) {
            if (_grave.Contains(card))
                _grave.Remove(card.SetOwner(null));
        }

        /// <summary>
        /// Check to see if player has a certain card in their hand.
        /// </summary>
        public bool IsCardInHand(CardInstance card) => _hand.Contains(card);

        /// <summary>
        /// Check to see if player has a certain card in their grave.
        /// </summary>
        public bool IsCardInGrave(CardInstance card) => _grave.Contains(card);

        /// <summary>
        /// Check to see if player had a certain card at the start of the game using <see cref="Card.CardID"/>.
        /// </summary>
        /// TODO: Decide if this function is still relevant.
        //public bool BroughtCard(string cardId) => InitialHand.Any((ci) => ci.Imprint.CardID == cardId);

    }
}
