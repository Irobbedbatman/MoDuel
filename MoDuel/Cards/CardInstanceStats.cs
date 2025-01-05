namespace MoDuel.Cards;

// See CardInstance.cs for documentation.
public partial class CardInstance {

    /// <summary>
    /// One of the current stats of this card.
    /// </summary>
    public int Level, Life, Attack, Armour, Defence, MaxLife;

    /// <summary>
    /// The card this level should be treated as instead of it;s <see cref="Owner"/>s.
    /// <para>Should not affect the level of <see cref="CardInstance"/s.></para>
    /// </summary>
    public int? FixedLevel;

}
