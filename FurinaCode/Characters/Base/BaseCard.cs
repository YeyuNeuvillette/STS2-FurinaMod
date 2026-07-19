using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;
using Furina.Extensions;

namespace Furina.Characters.Base;

public abstract class BaseCard(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true)
    : ModCardTemplate(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    private string ArtFileName => $"{Id.Entry.ToCardArtFileName()}.png";
    private string LegacyArtFileName => $"{Id.Entry.ToLegacyCompactFileName()}.png";
    private string LegacyPrefixedArtFileName => $"{MainFile.ModId.ToLowerInvariant()}_{Id.Entry.ToLegacyCompactFileName()}.png";

    public override string CustomPortraitPath =>
        AssetPathResolver.ResolveExisting(
            ArtFileName.BigCardsImagePath(),
            ArtFileName.CardsImagePath(),
            LegacyPrefixedArtFileName.BigCardsImagePath(),
            LegacyPrefixedArtFileName.CardsImagePath(),
            LegacyArtFileName.BigCardsImagePath(),
            LegacyArtFileName.CardsImagePath(),
            "card.png".BigCardsImagePath(),
            "card.png".CardsImagePath());

    public override string PortraitPath =>
        AssetPathResolver.ResolveExisting(
            ArtFileName.CardsImagePath(),
            ArtFileName.BigCardsImagePath(),
            LegacyPrefixedArtFileName.CardsImagePath(),
            LegacyPrefixedArtFileName.BigCardsImagePath(),
            LegacyArtFileName.CardsImagePath(),
            LegacyArtFileName.BigCardsImagePath(),
            "card.png".CardsImagePath(),
            "card.png".BigCardsImagePath());

    public override string? CustomBetaPortraitPath =>
        AssetPathResolver.ResolveExisting(
            ArtFileName.CardBetaImagePath(),
            LegacyPrefixedArtFileName.CardBetaImagePath(),
            LegacyArtFileName.CardBetaImagePath(),
            PortraitPath);
}
