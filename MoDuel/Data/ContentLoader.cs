using MoDuel.Cards;
using MoDuel.Heroes;
using MoonSharp.Interpreter;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using MoDuel.Mana;
using System.Collections.Generic;

namespace MoDuel.Data {

    /// <summary>
    /// A static class to access various loaders that store the loaded values into <see cref="LoadedContent"/>
    /// <para>
    /// <list type="table"><listheader>Loadable Content: </listheader><para/>
    /// <item><see cref="Card"/>s</item>,
    /// <item><see cref="Hero"/>es</item>,
    /// <item>'GameActions' in the form <see cref="DynValue"/> lua functions.</item>
    /// </list>
    /// </para>
    /// <para>Some methods require a <see cref="Script"/> to bake strings or files into <see cref="DynValue"/> lua functions.</para>
    /// </summary>
    public static class ContentLoader {

        /// <summary>
        /// Add the load methods to the <see cref="Script.Globals"/>.
        /// <para>These load methods are used in lua actions to load required content.</para>
        /// </summary>
        public static void RegisterLoads(Script script) {
            script.Globals["LoadCard"] = (Func<string, Card>)((cardId) => { return LoadCard(script, cardId); });
            script.Globals["LoadHero"] = (Func<string, Hero>)((heroId) => { return LoadHero(script, heroId); });
            script.Globals["LoadAction"] = (Action<string>)((actionId) => { LoadAction(script, actionId); });
        }

        /// <summary>
        /// Removes the load methods from the <see cref="Script.Globals"/>
        /// <para>These are important to use as loading content should only happen before the game starts.</para>
        /// </summary>
        /// <param name="script"></param>
        public static void DeregisterLoads(Script script) {
            script.Globals.Remove("LoadCard");
            script.Globals.Remove("LoadHero");
            script.Globals.Remove("LoadAction");
        }


        /// <summary>
        /// Shorthand accessor for <see cref="LoadedContent.Instance"/>
        /// </summary>
        private static readonly LoadedContent LC = LoadedContent.Instance;

        /// <summary>
        /// The directory to search content from.
        /// <para>The search is recursive.</para>
        /// </summary>
        public static string ContentDirectory { get; private set; } = "";
        public static void SetContentDirectory(string dir) { ContentDirectory = dir; }

        public static bool LogLoads = false;

        /// <summary>
        /// Loads a card with the given card index from a json file.
        /// <para>Also bakes any lua code contained within the json file.</para>
        /// <para>Also loads other linked content listed in the json file.</para>
        /// </summary>
        /// <param name="script">The script used in the duel flow so code can be compiled now and run later.</param>
        /// <param name="cardIndex">The index of the card to load.</param>
        /// <returns>A card that was loaded from the json file, or the card with same <paramref name="cardIndex"/> that had already been loaded.</returns>
        public static Card LoadCard(Script script, string cardId) {
            //If the card was already loaded no need to load it again.
            if (LC.IsCardLoaded(cardId))
                return LC.GetCard(cardId);

            if (LogLoads)
                Console.WriteLine("Loading: " + cardId);

            string fileName = SearchForFile("Card_" + cardId + ".json");

            if (fileName == null)
                throw new FileNotFoundException("Card " + cardId + " was not found. Content Directory: " + ContentDirectory + " ||| Looked for 'Card_" + cardId + ".json'");

            //Convert the file to a json object.
            JObject cJson = JObject.Parse(File.ReadAllText(fileName));

            Card card = new Card(
                cJson["ID"]?.Value<string>(),
                new ManaType(cJson["Mana"]?.Value<string>() ?? ""),
                (CardType)Enum.Parse(typeof(CardType), cJson["Type"]?.Value<string>() ?? "Creature"),
                GetTriggersFrom(cJson["Triggers"], script),
                GetTriggersFrom(cJson["Explicit"], script),
                GetParametersFrom(cJson["Parameters"], script)
            );

            //Add the loaded card to the loaded content.
            LC.AddLoadedCard(cardId, card);
            LoadLinkedContent(cJson["LinkedContent"], script);
            return card;
        }

        /// <summary>
        /// Loads a hero with given hero index from a json file.
        /// <para>Also bakes any lua code contained within the json file.</para>
        /// <para>Also loads other linked content listen in the json file.</para>
        /// </summary>
        /// <param name="script">The script used in the duel flow so code can be compiled now and run later.</param>
        /// <param name="heroId">The index of the hero to load.</param>
        /// <returns>A hero that was loaded from the json file, or a hero with same <paramref name="heroId"/> that had already been loaded.</returns>
        public static Hero LoadHero(Script script, string heroId) {
            //If the hero is already loaded no need to load it again.
            if (LC.IsHeroLoaded(heroId))
                return LC.GetHero(heroId);

            if (LogLoads)
                Console.WriteLine("Loading: " + heroId);

            string fileName = SearchForFile("Hero_" + heroId + ".json");

            if (fileName == null)
                throw new FileNotFoundException("Hero " + heroId + " was not found. Content Directory: " + ContentDirectory + " ||| Looked for 'Hero_" + heroId + ".json'");

            //Convert the file to a json object.
            JObject hJson = JObject.Parse(File.ReadAllText(fileName));

            //Get the static hero data.
            Hero hero = new Hero(
                hJson["ID"].Value<string>(),
                GetTriggersFrom(hJson["Triggers"], script),
                GetParametersFrom(hJson["Parameters"], script)
            );

            //Add the loaded hero to the loaded content.
            LC.AddLoadedHero(heroId, hero);
            LoadLinkedContent(hJson["LinkedContent"], script);
            return hero;
        }

        /// <summary>
        /// Loads an action coded in lua and bakes it into a <see cref="DynValue"/> function.
        /// </summary>
        /// <param name="script">The script used in the duel flow so code can be compiled now and run later.</param>
        /// <param name="actionId">The unqiue id of the action to load.</param>
        /// <returns>A function that was baked from the lua code, or a function with same <paramref name="actionId"/> that had already been loaded.</returns>
        public static void LoadAction(Script script, string actionId) {
            //If the action is alrady loaded no need to load it again.
            if (LC.IsActionLoaded(actionId))
                return;

            if (LogLoads)
                Console.WriteLine("Loading: " + actionId);

            string fileName = SearchForFile("Action_" + actionId + ".lua");

            if (fileName == null)
                throw new FileNotFoundException("Action " + actionId + " was not found. Content Directory: "+ ContentDirectory + " ||| Looked for 'Action_" + actionId + ".lua'");

            script.DoFile(fileName);

            //Add the loaded action to the loaded content.
            LC.AddLoadedAction(actionId);
        }


        /// <summary>
        /// Searches for a file with the given key in the <see cref="ContentDirectory"/>.
        /// <para>Searches subdirectories to.</para>
        /// <para>Returns null if the fill wasn't found.</para>
        /// </summary>
        private static string SearchForFile(string key) {
            //Search for the given action using the given key
            string[] files = Directory.GetFiles(ContentDirectory, key, SearchOption.AllDirectories);
            //If the action was found we return null.
            if (files.Length == 0)
                return null;
            //If we get more than one file we simply ignore the rest.
            return files[0];
        }

        /// <summary>
        /// Loads any cards/heroes/actions that are mentioned in linked content.
        /// <para>The format of the token should be arrays refrenced by the content type (Cards | Heroes | Actions).</para>
        /// </summary>
        /// <param name="linkedContent">The json token that has all the linked content arrays.</param>
        /// <param name="script">The <see cref="Script"/> to run any lua code.</param>
        private static void LoadLinkedContent(JToken linkedContent, Script script) {
            if (linkedContent == null)
                return;

            JArray cards = (JArray)linkedContent["Cards"];
            JArray heroes = (JArray)linkedContent["Heros"];
            JArray actions = (JArray)linkedContent["Actions"];
            if (cards != null)
                foreach(string c in cards)
                    LoadCard(script, c);
            if (heroes != null)
                foreach (string h in heroes)
                    LoadCard(script, h);
            if (actions != null)
                foreach (string a in actions)
                    LoadCard(script, a);
        }

        /// <summary>
        /// Iterates over a Json Block <see cref="JToken"/> looking for trigger/reactions.
        /// <para>Compiles any code with <see cref="Script"/> for faster use later.</para>
        /// <para>Performs the action <paramref name="act"/> on each found trigger/reaction.</para>
        /// </summary>
        /// <param name="triggerBlock">The block of json data to iterate over.</param>
        /// <param name="script">The script used to compile any text to <see cref="DynValue"/> functions.</param>
        /// <param name="act">The action performed on any found trigger/reaction. String is the trigger key and <see cref="DynValue"/> is the reaction function.</param>
        private static Dictionary<string, DynValue> GetTriggersFrom(JToken triggerBlock, Script script) {
            if (triggerBlock == null)
                return null;

            Dictionary<string, DynValue> triggers = new Dictionary<string, DynValue>();
            foreach (JProperty trigger in triggerBlock) {
                if (LC.IsActionLoaded(trigger.Value.Value<string>())) 
                    triggers.Add(trigger.Name,script.Globals.Get(trigger.Value.Value<string>()));
                else {
                    LoadAction(script, trigger.Value.Value<string>());
                    triggers.Add(trigger.Name, script.Globals.Get(trigger.Value.Value<string>()));
                }
            }
            return triggers;
        }

        /// <summary>
        /// Gets the paramaters of a <see cref="JToken"/> and returns them in a dictionay.
        /// </summary>
        private static Dictionary<string, DynValue> GetParametersFrom(JToken _parameters, Script script) {
            if (_parameters == null)
                return null;

            Dictionary<string, DynValue> parameters = new Dictionary<string, DynValue>();
            foreach (JProperty param in _parameters) {
                //Convert the json data and store it in the dictionary.
                //JArrayProxy.cs is used for Json arrays.
                parameters.Add(param.Name, DynValue.FromObject(script, param.Value.ToObject<object>()));
            }
            return parameters;
        }


    }
}
