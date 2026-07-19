using STS2RitsuLib.Scaffolding.Content;
using Furina.Extensions;

namespace Furina.Characters.Base;

public abstract class BaseRelic : ModRelicTemplate
{
    private string ArtFileName => $"{Id.Entry.ToRelicArtFileName()}.png";
    private string LegacyArtFileName => $"{Id.Entry.ToLegacyCompactFileName()}.png";
    private string OutlineFileName => $"{Id.Entry.ToRelicArtFileName()}_outline.png";
    private string LegacyOutlineFileName => $"{Id.Entry.ToLegacyCompactFileName()}_outline.png";

    public override string? CustomIconPath =>
        AssetPathResolver.ResolveExisting(
            ArtFileName.RelicImagePath(),
            LegacyArtFileName.RelicImagePath(),
            "relic.png".RelicImagePath());

    public override string? CustomIconOutlinePath =>
        AssetPathResolver.ResolveExisting(
            OutlineFileName.RelicImagePath(),
            LegacyOutlineFileName.RelicImagePath(),
            "relic_outline.png".RelicImagePath());

    public override string? CustomBigIconPath =>
        AssetPathResolver.ResolveExisting(
            ArtFileName.BigRelicImagePath(),
            ArtFileName.RelicImagePath(),
            LegacyArtFileName.BigRelicImagePath(),
            LegacyArtFileName.RelicImagePath(),
            "relic.png".BigRelicImagePath(),
            "relic.png".RelicImagePath());
}
