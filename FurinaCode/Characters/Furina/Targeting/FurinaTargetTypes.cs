using MegaCrit.Sts2.Core.Entities.Cards;
using MinionLib.Targeting;

namespace Furina.Characters.Furina.Targeting;

public static class FurinaTargetTypes
{
    public static readonly TargetType AnyPlayer =
        CustomTargetTypeManager.Register(new AnyPlayerTargetType(), "Furina", nameof(AnyPlayer));
}