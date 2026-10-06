namespace TokenVampire.Domain;

public enum Provenance { Unknown = 0, Measured, Assumed }

public readonly record struct Measured<T>(T? Value, Provenance Provenance) where T : struct
{
    public static Measured<T> Unknown => default;
    public static Measured<T> Of(T v) => new(v, Provenance.Measured);
    public static Measured<T> Assume(T v) => new(v, Provenance.Assumed);
    public bool IsKnown => Provenance != Provenance.Unknown && Value.HasValue;
}
