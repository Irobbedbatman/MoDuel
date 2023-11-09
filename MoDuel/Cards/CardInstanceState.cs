using MoDuel.Field;
using System.Diagnostics.CodeAnalysis;

namespace MoDuel.Cards;

// See CardInstance.cs for documentation.
public partial class CardInstance {

    /// <summary>
    /// The form this card was in previously.
    /// <para><see cref="null"/> if this is the original form.</para>
    /// </summary>
    public readonly CardInstance? PreviousState = null;

    /// <summary>
    /// The form this card was when all of its <see cref="PreviousState"/>s are gone through.
    /// </summary>
    public CardInstance OriginalState => PreviousState ?? this;


    /// <summary>
    /// The internal position value used sync or desync with a <see cref="ILocation"/> value.
    /// </summary>
    internal ILocation? _position;

    /// <summary>
    /// The location this card is in.
    /// </summary>
    [MemberNotNullWhen(true, nameof(_position))]
    public ILocation? Position {
        get {
            return _position;
        }
        set {
            // Don't bother setting into the same location.
            if (value == _position) return;
            // Clear the new value ready for use.
            if (value != null) {
                value.RemoveCurrentOccupant();
                value.JustAdd(this);
            }
            //If the new slot is null we still need to clear the position.
            _position?.JustRemove(this);
            _position = value;
            Owner = value?.GetOwner();
        }
    }

    /// <summary>
    /// The field slot this card is in. If it is not on the field this value is null.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Position))]
    public FieldSlot? FieldPosition {
        get {
            if (Position is FieldSlot fs)
                return fs;
            return null;
        }
        set => Position = value;
    }

    /// <summary>
    /// Shared active state of <see cref="ExplicitTriggerActive"/> and <see cref="ImplicitTriggerActive"/>.
    /// <para>Used to simplify disabling both.</para>
    /// </summary>
    public bool Active {
        get => ExplicitTriggerActive && ImplicitTriggerActive;
        set {
            ExplicitTriggerActive = value;
            ImplicitTriggerActive = false;
        }
    }

    /// <summary>
    /// If the explicit triggering should be used externally.
    /// </summary>
    public bool ExplicitTriggerActive = true;

    /// <summary>
    /// If the implicit triggering should be used externally
    /// </summary>
    public bool ImplicitTriggerActive = true;

    /// <summary>
    /// Check to see if this card instance is alive. This is true if it is on the field.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Position), nameof(FieldPosition))]
    public bool IsAlive => FieldPosition != null;

    /// <summary>
    /// Is this card in it's owners Hand.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Owner))]
    public bool InHand => Owner?.IsCardInHand(this) ?? false;

    /// <summary>
    /// Is this card in it's owners Grave.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Owner))]
    public bool InGrave => Owner?.IsCardInGrave(this) ?? false;

    /// <summary>
    /// Inline setter for <see cref="Position"/> that returns the same <see cref="CardInstance"/>.
    /// </summary>
    public CardInstance SetPosition(ILocation position) {
        Position = position;
        return this;
    }

}
