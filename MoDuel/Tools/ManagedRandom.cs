using MoDuel.Serialization;

namespace MoDuel.Tools;

/// <summary>
/// A managed randomiser that can be used by multiple elements to have a shared random environment.
/// </summary>
[SerializeReference]
public class ManagedRandom : Random {

    /// <summary>
    /// The amount of times the random has called the internal sampler. 
    /// <para>TODO: Some methods increment more than once and need to be tested.</para>
    /// </summary>
    private ulong _advancements = 0;

    public ManagedRandom() { }

    public ManagedRandom(int seed) : base(seed) { }

    /// <summary>
    /// Retrieves a random object from a generic enumerable.
    /// </summary>
    public object? NextItem(IEnumerable<object?> items) => NextItem<object?>(items);

    /// <summary>
    /// Get the next item from a set of parameters.
    /// </summary>
    public T? NextItemParams<T>(params T[] items) => NextItem(items);

    /// <summary>
    /// Retrieves a random object from a generic enumerable.
    /// </summary>
    public T? NextItem<T>(IEnumerable<T> items) {
        int count = items.Count();
        // Check empty collection.
        if (count == 0)
            return default;
        // Check single item in collection.
        if (count == 1)
            return items.First();
        return items.ElementAt(Next(0, items.Count()));
    }

    /// <summary>
    /// Calls the internal sampler until it has been advanced to the provided position.
    /// </summary>
    public void PerformCatchup(ulong position) {
        if (position < _advancements)
            throw new ArgumentException($"{nameof(ArgumentException)}: expected {nameof(position)} to have position further in the cycle. Provided: {position}. Current: {_advancements}.");
        while (position > _advancements) {
            Next();
        }
    }

    public override int Next() {
        _advancements++;
        return base.Next();
    }

    public override long NextInt64() {
        _advancements++;
        return base.NextInt64();
    }

    public override int Next(int maxValue) {
        _advancements++;
        return base.Next(maxValue);
    }

    public override int Next(int minValue, int maxValue) {
        _advancements++;
        return base.Next(minValue, maxValue);
    }

    public override void NextBytes(byte[] buffer) {
        _advancements++;
        base.NextBytes(buffer);
    }

    public override void NextBytes(Span<byte> buffer) {
        _advancements++;
        base.NextBytes(buffer);
    }

    public override double NextDouble() {
        _advancements++;
        return base.NextDouble();
    }


    public override long NextInt64(long maxValue) {
        _advancements++;
        return base.NextInt64(maxValue);
    }

    public override long NextInt64(long minValue, long maxValue) {
        _advancements++;
        return base.NextInt64(minValue, maxValue);
    }

    public override float NextSingle() {
        _advancements++;
        return base.NextSingle();
    }

}
