using MegaCrit.Sts2.Core.Entities.Cards;
using Furina.Characters.Base;

namespace Furina.Characters.Furina.Cards;

public abstract class FurinaCard(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true)
    : BaseCard(energyCost, type, rarity, targetType, shouldShowInCardLibrary);