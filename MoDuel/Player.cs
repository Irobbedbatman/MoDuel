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
        /// Invokes the <see cref="OutBoundDelegate"/> with the given data.
        /// </summary>
        /// <param name="data"></param>
        public void SendRequest (ClientRequest data) => OutBoundDelegate?.Invoke(this, data);

        /// <summary>
        /// The <see cref="HeroInstance"/> this player is currently playing as.
        /// </summary
        public HeroInstance CurrentHero { get; private set; }

        public void ChangeHero(Hero newHero) {
            CurrentHero = new HeroInstance(newHero, this);
        }

        /// <summary>
        /// The <see cref="SubField"/> on this players side.
        /// </summary>
        public readonly SubField Field;

        /// <summary>
        /// One of the stats for this player.
        /// </summary>
        public int Level = 1, Exp = 0, Hp = 25, MaxHp = 25;

        /// <summary>
        /// The Hand of the player currently.
        /// </summary>
        private readonly HashSet<CardInstance> _currentHand = new HashSet<CardInstance>();
        /// <summary>
        /// The Hand of the player currently; readonly.
        /// </summary>
        public IReadOnlyList<CardInstance> CurrentHand => _currentHand.ToList().AsReadOnly();

        /// <summary>
        /// The Grave of the player currently.
        /// </summary>
        private readonly HashSet<CardInstance> _graveCards = new HashSet<CardInstance>();

        /// <summary>
        /// The Grave of the player currently; readonly.
        /// </summary>
        public IReadOnlyList<CardInstance> GraveCards => _graveCards.ToList().AsReadOnly();

        public HashSet<CardInstance> GetHandCards() {
            return new HashSet<CardInstance>(_currentHand);
        }

        public HashSet<CardInstance> GetGraveCards() {
            return new HashSet<CardInstance>(_graveCards);
        }

        public Player(string userId, Hero hero, ManaPool manaPool) {
            UserId = userId;
            ManaPool = manaPool;
            Field = new SubField(this);
            CurrentHero = new HeroInstance(hero, this);
        }

        /// <summary>
        /// Adds a card to this player's hand.
        /// </summary>
        /// <param name="card"></param>
        public void AddCardToHand(CardInstance card) => _currentHand.Add(card.SetOwner(this));

        /// <summary>
        /// Removes a card from this player's hand if it currently in there.
        /// </summary>
        /// <param name="card"></param>
        public void RemoveCardFromHand(CardInstance card) {
            if (_currentHand.Contains(card))
                _currentHand.Remove(card.SetOwner(null));
        }

        /// <summary>
        /// Adds a card to this player's grave.
        /// </summary>
        /// <param name="card"></param>
        public void AddCardToGrave(CardInstance card) => _graveCards.Add(card.SetOwner(this));

        /// <summary>
        /// Removes a card from this player's grave if it currently in there.
        /// </summary>
        /// <param name="card"></param>
        public void RemoveCardFromGrave(CardInstance card) {
            if (_graveCards.Contains(card))
                _graveCards.Remove(card.SetOwner(null));
        }

        /// <summary>
        /// Check to see if player has a certain card in their hand.
        /// </summary>
        public bool IsCardInHand(CardInstance card) => _currentHand.Contains(card);

        /// <summary>
        /// Check to see if player has a certain card in their grave.
        /// </summary>
        public bool IsCardInGrave(CardInstance card) => _graveCards.Contains(card);

        /// <summary>
        /// Check to see if player had a certain card at the start of the game using <see cref="Card.CardID"/>.
        /// </summary>
        /// TODO: Decide if this function is still relevant.
        //public bool BroughtCard(string cardId) => InitialHand.Any((ci) => ci.Imprint.CardID == cardId);

    }
}
