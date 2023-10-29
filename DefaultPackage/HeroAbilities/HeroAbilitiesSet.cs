using MoDuel.Client;
using MoDuel.Data;
using MoDuel.Heroes;
using MoDuel.Players;
using MoDuel.Resources;

namespace DefaultPackage.HeroAbilities;

/// <summary>
/// The set of actions that define hero abilities.
/// </summary>
public class HeroAbilitiesSet {

    /// <summary>
    /// The ability fot the Hooded Figure. 
    /// <para>Heals 1 when the player using the hero charges.</para>
    /// </summary>
    [ActionName("SoulCharge")]
    [Dependency(DependencyAttribute.DependencyTypes.Action, "Heal")]
    public static void SoulCharge(HeroInstance hero, Player player, ResourceType _, int _2) {
        if (player.Hero == hero) {
            HealActions.Heal(player, 1);
            player.Context.SendBlockingRequest(new ClientRequest("LogText", "Abc"), 0);
            Console.WriteLine("Soul Charge Activate!!!");
        }
    }

}
