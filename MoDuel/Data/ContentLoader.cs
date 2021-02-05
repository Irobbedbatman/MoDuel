using MoDuel.Cards;
using MoDuel.Heroes;
using MoDuel.Mana;
using MoonSharp.Environment;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace MoDuel.Data {

    /// <summary>
    /// A static class to access various loaders that store the loaded values into <see cref="LoadedContent"/>
    /// <para>
    /// <list type="table"><listheader>Loadable Content: </listheader><para/>
    /// <item><see cref="Card"/>s</item>,
    /// <item><see cref="Hero"/>es</item>,
    /// <item>'GameActions' in the form <see cref="Closure"/> lua functions.</item>
    /// </list>
    /// </para>
    /// <para>Most methods require a <see cref="LuaEnvironment"/> to bake strings or files into <see cref="DynValue"/> lua functions.</para>
    /// <para>Most methods require a <see cref="LoadedContent"/> to store everything once it has been loaded.</para>
    /// </summary>
    public static class ContentLoader {

        /// <summary>
        /// Add the load methods to the <see cref="Script.Globals"/> in the provided <see cref="LuaEnvironment"/>.
        /// <para>These load methods are used in lua actions to load required content.</para>
        /// </summary>
        public static void RegisterLoads(LoadedContent content, LuaEnvironment luaEnv) {
            luaEnv.AsScript.Globals["LoadCard"] = (Func<string, Card>)((cardId) => { return LoadCard(cardId, content, luaEnv); });
            luaEnv.AsScript.Globals["LoadHero"] = (Func<string, Hero>)((heroId) => { return LoadHero(heroId, content, luaEnv); });
            luaEnv.AsScript.Globals["LoadAction"] = (Func<string, Closure>)((actionId) => { return LoadAction(actionId, content, luaEnv); });
            luaEnv.AsScript.Globals["LoadJsonFile"] = (Func<string, JObject>)((filename) => { return LoadJsonFile(filename, content); });
            luaEnv.AsScript.Globals["Include"] = (Func<string, Table>)((filename) => { return Include(filename, content, luaEnv); });
        }

        /// <summary>
        /// Removes the load methods from the <see cref="Script.Globals"/>
        /// <para>These are important to use as loading content should only happen before the game starts.</para>
        /// </summary>
        public static void DeregisterLoads(LuaEnvironment luaEnv) {
            luaEnv.AsScript.Globals.Remove("LoadCard");
            luaEnv.AsScript.Globals.Remove("LoadHero");
            luaEnv.AsScript.Globals.Remove("LoadAction");
            luaEnv.AsScript.Globals.Remove("LoadJsonFile");
            luaEnv.AsScript.Globals.Remove("Include");
        }

        /// <summary>
        /// The directory to search content from.
        /// <para>The search is recursive.</para>
        /// </summary>
        public static string ContentDirectory { get; private set; } = "";
        public static void SetContentDirectory(string dir) { ContentDirectory = dir; }

        /// <summary>
        /// Should a log be printed out each time something is loaded.
        /// </summary>
        public static bool LogLoads = false;

        /// <summary>
        /// Loads a card with the given card index from a json file.
        /// <para>Also bakes any lua code contained within the json file.</para>
        /// <para>Also loads other linked content listed in the json file.</para>
        /// </summary>
        /// <param name="cardId">The index of the card to load.</param>
        /// <param name="content">The container to store the file when loaded.</param>
        /// <param name="luaEnv">The lua environment used in the duel flow so code can be compiled now and run later.</param>
        /// <returns>A card that was loaded from the json file, or the card with same <paramref name="cardId"/> that had already been loaded.</returns>
        public static Card LoadCard(string cardId, LoadedContent content, LuaEnvironment luaEnv) {
            //If the card was already loaded no need to load it again.
            if (content.TryGetCard(cardId, out Card _card))
                return _card;

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
                GetTriggersFrom(cJson["Triggers"], content, luaEnv),
                GetTriggersFrom(cJson["Explicit"], content, luaEnv),
                GetParametersFrom(cJson["Parameters"], luaEnv)
            );

            //Add the loaded card to the loaded content.
            content.AddLoadedCard(cardId, card);
            LoadLinkedContent(cJson["LinkedContent"], content, luaEnv);
            return card;
        }

        /// <summary>
        /// Loads a hero with given hero index from a json file.
        /// <para>Also bakes any lua code contained within the json file.</para>
        /// <para>Also loads other linked content listen in the json file.</para>
        /// </summary>
        /// <param name="heroId">The index of the hero to load.</param>
        /// <param name="content">The container to store the file when loaded.</param>
        /// <param name="luaEnv">The lua environment used in the duel flow so code can be compiled now and run later.</param>
        /// <returns>A hero that was loaded from the json file, or a hero with same <paramref name="heroId"/> that had already been loaded.</returns>
        public static Hero LoadHero(string heroId, LoadedContent content, LuaEnvironment luaEnv) {
            //If the hero was already loaded no need to load it again.
            if (content.TryGetHero(heroId, out Hero _hero))
                return _hero;

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
                GetTriggersFrom(hJson["Triggers"], content, luaEnv),
                GetParametersFrom(hJson["Parameters"], luaEnv)
            );

            //Add the loaded hero to the loaded content.
            content.AddLoadedHero(heroId, hero);
            LoadLinkedContent(hJson["LinkedContent"], content, luaEnv);
            return hero;
        }

        /// <summary>
        /// Load a lua file for use of it's methods more than anything.
        /// <para>Returns a table containing all the global values in the provided file.</para>
        /// </summary>
        public static Table Include(string fileName, LoadedContent content, LuaEnvironment luaEnv) {

            // Don't need to load the file if has been loaded already.
            if (content.TryGetLuaFile(fileName, out Table table))
                return table;

            string fileNameFull = SearchForFile(fileName);

            if (fileNameFull == null) {
                Console.Write("File: " + fileName + " is missing.");
                return luaEnv.TemporaryTable(false);
            }
            // Construct a temporary table for the use for the function.
            var tempTable = luaEnv.TemporaryTable(true);

            try {
                // Retrieve all the globals from the file.
                luaEnv.AsScript.DoFile(fileNameFull, tempTable);
            }
            catch (Exception e) {
                // Display errors if the excution fails at any point.
                Console.Write(e.StackTrace);
                Console.WriteLine("LuaFunctions found in " + fileName + " failed excution.");
            }

            // Add the action to the list of loaded actions.
            content.AddLuaFile(fileName, tempTable);
            // And return it.
            return tempTable;

        }


        /// <summary>
        /// Loads an action coded in lua and bakes it into a <see cref="DynValue"/> function.
        /// </summary>
        /// <param name="actionId">The unqiue id of the action to load.</param>
        /// <param name="content">The container to store the file when loaded.</param>
        /// <param name="luaEnv">The script used in the duel flow so code can be compiled now and run later.</param>
        /// <returns>A function that was baked from the lua code, or a function with same <paramref name="actionId"/> that had already been loaded.</returns>
        public static Closure LoadAction(string actionId, LoadedContent content, LuaEnvironment luaEnv) {
            //If the action is alrady loaded no need to load it again.
            if (content.TryGetAction(actionId, out var _func))
                return _func;

            if (LogLoads)
                Console.WriteLine("Loading: " + actionId);

            // Get the file name of the action.
            string fileName = SearchForFile("Action_" + actionId + ".lua");

            // Ensure the file exists.
            if (fileName == null)
                throw new FileNotFoundException("Action " + actionId + " was not found. Content Directory: " + ContentDirectory + " ||| Looked for 'Action_" + actionId + ".lua'");

            // The function we retrieve from the file.
            Closure func = null;

            try {
                // Construct a temporary table for the use for the function.
                var tempTable = luaEnv.TemporaryTable(true);

                // Retrieve all the globals from the file.
                luaEnv.AsScript.DoFile(fileName, tempTable);
                // Try to find the function by name.
                func = tempTable.Get(actionId).Function;


            }
            catch (Exception e) {
                // Display errors if the excution fails at any point.
                Console.Write(e.StackTrace);
                Console.WriteLine("Action " + actionId + " failed excution. Ensure function name is the same as the actionId, that is, the name of the file without the 'Action_' or the '.lua'.");
            }

            // Add the action to the list of loaded actions.
            content.AddLoadedAction(actionId, func);
            // And return it.
            return func;

        }

        /// <summary>
        /// Loads a json file for use in lua.
        /// </summary>
        /// <param name="fileName">The name of the file to load.</param>
        /// <param name="content">The container to store the file when loaded.</param>
        public static JObject LoadJsonFile(string fileName, LoadedContent content) {
            // Get the file name of the action.
            string file = SearchForFile(fileName);
            if (file == null)
                return null;
            //Convert the file to a json object.
            var jFile = JObject.Parse(File.ReadAllText(file));
            // Add it to the list of loaded files.
            content.AddMiscFile(fileName, jFile);
            return jFile;
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
        /// <param name="content">The content the linked content will be stored in.</param>
        /// <param name="luaEnv">The <see cref="LuaEnvironment"/> to run any lua code in the linked content.</param>
        private static void LoadLinkedContent(JToken linkedContent, LoadedContent content, LuaEnvironment luaEnv) {
            if (linkedContent == null)
                return;

            JArray cards = (JArray)linkedContent["Cards"];
            JArray heroes = (JArray)linkedContent["Heros"];
            JArray actions = (JArray)linkedContent["Actions"];
            if (cards != null)
                foreach(string c in cards)
                    LoadCard(c, content, luaEnv);
            if (heroes != null)
                foreach (string h in heroes)
                    LoadCard(h, content, luaEnv);
            if (actions != null)
                foreach (string a in actions)
                    LoadCard(a, content, luaEnv);
        }

        /// <summary>
        /// Iterates over a Json Block <see cref="JToken"/> looking for trigger/reactions.
        /// <para>Compiles any code with <see cref="Script"/> for faster use later.</para>
        /// <para>Performs the action <paramref name="act"/> on each found trigger/reaction.</para>
        /// </summary>
        /// <param name="triggerBlock">The block of json data to iterate over.</param>
        /// <param name="content">The content the triggers will be stored in.</param>
        /// <param name="luaEnv">The lua environment that is used to compile any <see cref="DynValue"/> functions.</param>
        /// </summary>
        private static Dictionary<string, Closure> GetTriggersFrom(JToken triggerBlock, LoadedContent content, LuaEnvironment luaEnv) {
            if (triggerBlock == null)
                return null;

            Dictionary<string, Closure> triggers = new Dictionary<string, Closure>();
            foreach (JProperty trigger in triggerBlock) {
                if (!content.TryGetAction(trigger.Value.Value<string>(), out Closure func))
                    func = LoadAction(trigger.Value.Value<string>(), content, luaEnv);
                triggers.Add(trigger.Name, func);
            }
            return triggers;
        }

        /// <summary>
        /// Gets the paramaters of a <see cref="JToken"/> and returns them in a dictionay.
        /// </summary>
        /// <param name="paramToken">The parameters stored in json.</param>
        /// <param name="luaEnv">The lua environment that is used to retrieve any <see cref="DynValue"/> parameters.</param>
        private static Dictionary<string, DynValue> GetParametersFrom(JToken paramToken, LuaEnvironment luaEnv) {
            if (paramToken == null)
                return null;

            Dictionary<string, DynValue> parameters = new Dictionary<string, DynValue>();
            foreach (JProperty param in paramToken) {
                //Convert the json data and store it in the dictionary.
                //JArrayProxy.cs is used for Json arrays.
                parameters.Add(param.Name, DynValue.FromObject(luaEnv.AsScript, param.Value.ToObject<object>()));
            }
            return parameters;
        }


    }
}
