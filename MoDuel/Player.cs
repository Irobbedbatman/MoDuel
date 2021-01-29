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
using MoDuel.Animation;

namespace MoDuel {

    [MoonSharpUserData]
    public class Player : Target {

        /// <summary>
        /// The amount of experience it takes to go from the previous level to this level.
        /// </summary>
        public static readonly int LEVEL_2_XP = 6, LEVEL_3_XP = 14, INVALID_XP = -1;

        /// <summary>
        /// The uniqueid, name, username of this player.
        /// </summary>
        public readonly string UserId;

        /// <summary>
        /// The <see cref="Mana.ManaPool"/> that this player has.
        /// </summary>
        public readonly ManaPool ManaPool;

        private HeroInstance _currenthero;

        /// <summary>
        /// The animations that are sent out for this specific player.
        /// </summary>
        [MoonSharpHidden]
        public EventHandler<AnimationData> OutBoundDelegate;

        /// <summary>
        /// Invokes the <see cref="OutBoundDelegate"/> with the given data.
        /// </summary>
        /// <param name="data"></param>
        public void SendAnimation (AnimationData data) => OutBoundDelegate?.Invoke(this, data);

        /// <summary>
        /// The hero this player is currently.
        /// </summary
        public HeroInstance CurrentHero {
            get { return _currenthero; }
            set {
                value.Owner = this;
                _currenthero = value;
            }
        }

        /// <summary>
        /// The <see cref="SubField"/> on this players side.
        /// </summary>
        public readonly SubField Field;

        /// <summary>
        /// One of the stats for this player.
        /// </summary>
        public int Level = 1, CurrentExp = 0, CurrentHp = 25, MaxHp = 25;

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
            CurrentHero = new HeroInstance(hero);
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


        public int ExpNextLevel() {
            switch (Level) {
                case 1:
                    return LEVEL_2_XP;
                case 2:
                    return LEVEL_3_XP;
                default:
                    return INVALID_XP;
            }

        }

    }
}
