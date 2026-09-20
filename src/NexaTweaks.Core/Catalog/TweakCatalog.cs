namespace NexaTweaks.Core.Catalog;

/// <summary>
/// Static, declarative catalog of every built-in tweak, grouped by category.
/// Split into one partial file per category (TweakCatalog.Windows.cs, TweakCatalog.Network.cs, ...)
/// to keep each category small enough to review on its own.
/// </summary>
public static partial class TweakCatalog
{
    private static IReadOnlyList<Tweaks.ITweak>? _all;

    /// <summary>
    /// Every built-in tweak, in the order the app shows the categories. Callers that need to look
    /// a tweak up by id (restoring a backup, for instance) use this instead of remembering to
    /// concatenate each category by hand.
    /// Built on first use, never in a static initializer: the category lists live in other files
    /// of this partial class and C# does not guarantee the order those initializers run in - doing
    /// it eagerly read them as null.
    /// </summary>
    public static IReadOnlyList<Tweaks.ITweak> All => _all ??= Windows
        .Concat(Services)
        .Concat(Privacy)
        .Concat(Interface)
        .Concat(Network)
        .Concat(Input)
        .Concat(Gpu)
        .Concat(Cleanup)
        .Concat(Advanced)
        .ToList();
}
