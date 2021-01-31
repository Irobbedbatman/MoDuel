using MoDuel.Cards;
using MoDuel.Heroes;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Data {
    /// <summary>
    /// A container that holds collections of loaded content.
    /// </summary>
    public class LoadedContent {

        public LoadedContent() { }

        #region Card Loading and Accessors.
        private readonly Dictionary<string, Card> LoadedCards = new Dictionary<string, Card>();
        public void AddLoadedCard(string cardID, Card card) => LoadedCards.Add(cardID, card);
        public bool IsCardLoaded(string cardID) => LoadedCards.ContainsKey(cardID);
        public Card GetCard(string cardID) => LoadedCards[cardID];
        public bool TryGetCard(string cardID, out Card card) => LoadedCards.TryGetValue(cardID, out card);
        #endregion

        #region Action Loading and Accessors
        private readonly Dictionary<string, Closure> LoadedActions = new Dictionary<string, Closure>();
        public void AddLoadedAction(string actionID, Closure func) => LoadedActions.Add(actionID, func);
        public bool IsActionLoaded(string actionID) => LoadedActions.ContainsKey(actionID);
        public Closure GetAction(string actionID) => LoadedActions[actionID];
        public bool TryGetAction(string actionID, out Closure func) => LoadedActions.TryGetValue(actionID, out func);
        #endregion

        #region Hero Loading and Accessors
        private readonly Dictionary<string, Hero> LoadedHeroes = new Dictionary<string, Hero>();
        public void AddLoadedHero(string heroID, Hero hero) => LoadedHeroes.Add(heroID, hero);
        public bool IsHeroLoaded(string heroID) => LoadedHeroes.ContainsKey(heroID);
        public Hero GetHero(string heroID) => LoadedHeroes[heroID];
        public bool TryGetHero(string heroID, out Hero hero) => LoadedHeroes.TryGetValue(heroID, out hero);
        #endregion

        #region Misc File Loading and Accessors
        private readonly Dictionary<string, JObject> LoadedFiles = new Dictionary<string, JObject>();
        public void AddMiscFile(string filename, JObject file) => LoadedFiles.Add(filename, file);
        public bool IsFileLoaded(string filename) => LoadedFiles.ContainsKey(filename);
        public JObject GetFile(string filename) => LoadedFiles[filename];
        public bool TryGetFile(string filename, out JObject file) => LoadedFiles.TryGetValue(filename, out file);
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
