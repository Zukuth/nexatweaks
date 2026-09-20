namespace NexaTweaks.Core.Catalog;

/// <summary>The one-line count shown under every category title: how many options it has, how
/// many are already applied and how many don't apply to this machine.</summary>
public static class CategorySummary
{
    public static string Format(int total, int applied, int unavailable, bool isChecking)
    {
        var head = $"{total} {Plural(total, "ajuste", "ajustes")}";
        if (isChecking) return $"{head} · comprobando estado...";

        var text = $"{head} · {applied} {Plural(applied, "aplicado", "aplicados")}";
        if (unavailable > 0)
            text += $" · {unavailable} {Plural(unavailable, "no disponible", "no disponibles")} en este equipo";
        return text;
    }

    private static string Plural(int count, string singular, string plural) => count == 1 ? singular : plural;
}
