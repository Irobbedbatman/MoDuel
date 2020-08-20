using MoDuel.Animation;
using MoDuel.Cards;
using MoDuel.Heroes;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Data {
    /// <summary>
    /// A static singleton that holds all the currently loaded content
    /// </summary>
    public class LoadedContent {

        private LoadedContent() { }

        /// <summary>
        /// Accessor for the <see cref="LoadedContent"/> Singleton.
        /// </summary>
        public static LoadedContent Instance { get; private set; } = new LoadedContent();

        #region Card Loading and Accessors.
        private static Dictionary<string, Card> LoadedCards = new Dictionary<string, Card>();
        public void AddLoadedCard(string cardID, Card card) => LoadedCards.Add(cardID, card);
        public bool IsCardLoaded(string cardID) => LoadedCards.ContainsKey(cardID);
        public Card GetCard(string cardID) => LoadedCards[cardID];
        #endregion

        #region Action Loading and Accessors
        private static HashSet<string> LoadedActions = new HashSet<string>();
        public void AddLoadedAction(string actionID) => LoadedActions.Add(actionID);
        public bool IsActionLoaded(string actionID) => LoadedActions.Contains(actionID);
        #endregion

        #region Hero Loading and Accessors
        private static Dictionary<string, Hero> LoadedHeroes = new Dictionary<string, Hero>();
        public void AddLoadedHero(string heroID, Hero hero) => LoadedHeroes.Add(heroID, hero);
        public bool IsHeroLoaded(string heroID) => LoadedHeroes.ContainsKey(heroID);
        public Hero GetHero(string heroID) => LoadedHeroes[heroID];
        #endregion

        /// <summary>
        /// Get the amount of Loaded Items from each Dictionary Combined.
        /// <para>Could be used for debugging and getting an idea of memory usage.</para>
        /// </summary>
        public int Count => LoadedCards.Count + LoadedActions.Count + LoadedHeroes.Count;

        /// <summary>
        /// Gets the keys of all of the loaded items from each dictionary combined.
        /// <para>Could be used to ensure that content was linked properly and other debugging situations.</para>
        /// <para>Each key comes with a prefix stating what type of content it is. Split with the character '|'.</para>
        /// </summary>
        /// <returns></returns>
        public string[] GetAllKeysSummary() {
            List<string> keys = new List<string>();
            foreach (var k in LoadedCards.Keys)
                keys.Add("Card|" + k.ToString());
            foreach (var k in LoadedActions)
                keys.Add("Action|" + k);
            foreach (var k in LoadedHeroes.Keys)
                keys.Add("Hero|" + k.ToString());
            return keys.ToArray();
        }
    }
}
