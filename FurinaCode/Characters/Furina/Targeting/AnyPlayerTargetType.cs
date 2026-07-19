using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Action;
using MinionLib.Targeting;

namespace Furina.Characters.Furina.Targeting;

public sealed class AnyPlayerTargetType : ICustomTargetType
{
    public bool IsSingleTarget => true;

    public bool IsValidTargetPreview(Creature target)
    {
        return IsValidTarget(target);
    }

    public bool IsValidTarget(CardModel card, Creature target)
    {
        return IsValidTarget(target);
    }

    public bool IsValidTarget(PotionModel potion, Creature target)
    {
        return IsValidTarget(target);
    }

    public bool IsValidTarget(ActionModel action, Creature target)
    {
        return IsValidTarget(target);
    }

    private static bool IsValidTarget(Creature target)
    {
        return target is { IsAlive: true, IsPlayer: true, Side: CombatSide.Player };
    }
}
