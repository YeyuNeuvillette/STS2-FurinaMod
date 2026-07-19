using Godot;

namespace Furina.Characters.Base;

internal static class AssetPathResolver
{
    public static string ResolveExisting(params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (ResourceLoader.Exists(candidate))
                return candidate;
        }

        return candidates[^1];
    }
}
