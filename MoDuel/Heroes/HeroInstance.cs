using MoonSharp.Interpreter;

namespace MoDuel.Heroes {

    /// <summary>
    /// An instanced version of <see cref="Hero"/> stored in a <see cref="Player"/>.
    /// </summary>
    [MoonSharpUserData]
    public class HeroInstance {

        /// <summary>
        /// The <see cref="Hero"/> this <see cref="HeroInstance"/> is taken from.
        /// </summary>
        public readonly Hero Imprint;

        /// <summary>
        /// The <see cref="Player"/> that is using this <see cref="HeroInstance"/> as their <see cref="Player.CurrentHero"/>.
        /// </summary>
        public Player Owner;

        public HeroInstance(Hero hero) {
            Imprint = hero;
        }
    }
}
