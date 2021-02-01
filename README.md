# MoDuel

An somewhat complete modular libary for handling the game flow and input of simple card games; inspired by Nexon & devCAT's Mabinogi Duel / Duel of Summoners without using any copyright or trademarked materials. As a I am not a lawyer and have not seeked legal consoltation on the topic; I can't specify the legal situation.

### Explanation
The foremost important part is the `DuelFlow` class. As titled it controls the flow of the duel, it takes input into the `DuelFlow.BufferedCommands`; each player can have one input buffered each. The input turns into a action that runs a lua file named after a provided 'cmdId'.

The effects these commands have are based on loaded content. Some of this is implied to be loaded e.g. The Charge action. While others are loaded based on the list of cards and hero each player is using; and other cards, heros, animations and actions that they link to. Only one isntance of any of those is stored at a time. Currently there is no unloading.

One action that cards can do is 'Summon Creature' this will create a `CreatureInstance` which is a child of a `CardInstance` as we need to restore the creature to the card when it dies. Also cards can transform storing their previous state in the case it need retrevial.

Everything that could be considered a target derives from the `Target` Class. This provides them with a unique `TargetIndex` this is sent across the network to determine context. Each 'Target' also store a dictionary of 'Values' that can be accessed through the 'GetValue' & 'SetValue' methods, you can individually clear a value using 'ClearValue'.

Moonsharp allows us to access the game state from lua code and call any functions on targets.

To send data back to the client there is two main methods in 'DuelFlow'. 'SendRequest' which allows you to provided the name of the and a paramter list of arguments; you can also use 'SendRequestTo' which will send it only to the specified player.

### Furthur Info
Hoping to a have a wiki for more detailed use cases of specific things.

### Prerequisites

JSON.NET: https://www.newtonsoft.com/json

MoonSharp : https://www.moonsharp.org/

## Authors

* **Todd O'Donnell** - [Irobbedbatman](https://github.com/Irobbedbatman)

## License

This project is licensed under the CC BY AU - https://creativecommons.org/licenses/by/3.0/au/
