using STS2RitsuLib.Scaffolding.Content;
using Furina.Extensions;

namespace Furina.Characters.Base;

public abstract class BasePotion : ModPotionTemplate
{
    private string ArtFileName => $"{Id.Entry.ToPotionArtFileName()}.png";
    private string LegacyArtFileName => $"{Id.Entry.ToLegacyCompactFileName()}.png";
    private string PrefixedArtFileName => $"{MainFile.ModId.ToLowerInvariant()}_potion_{Id.Entry.ToLegacyCompactFileName()}.png";
    private string OutlineFileName => $"{Id.Entry.ToPotionArtFileName()}_outline.png";
    private string LegacyOutlineFileName => $"{Id.Entry.ToLegacyCompactFileName()}_outline.png";
    private string PrefixedOutlineFileName => $"{MainFile.ModId.ToLowerInvariant()}_potion_{Id.Entry.ToLegacyCompactFileName()}_outline.png";

    public override string? CustomImagePath =>
        AssetPathResolver.ResolveExisting(
            ArtFileName.PotionImagePath(),
            PrefixedArtFileName.PotionImagePath(),
            LegacyArtFileName.PotionImagePath(),
            "potion.png".PotionImagePath());

    public override string? CustomOutlinePath =>
        AssetPathResolver.ResolveExisting(
            OutlineFileName.PotionImagePath(),
            PrefixedOutlineFileName.PotionImagePath(),
            LegacyOutlineFileName.PotionImagePath(),
            "potion_outline.png".PotionImagePath());
}
