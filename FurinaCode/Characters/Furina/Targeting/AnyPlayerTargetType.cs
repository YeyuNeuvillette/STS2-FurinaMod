using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
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
        return IsValidTarget(target) && LocalContext.IsMe(target);
    }

    public bool IsValidTarget(CardModel card, Creature target)
    {
        return IsValidTarget(target) && target.Player == card.Owner;
    }

    public bool IsValidTarget(PotionModel potion, Creature target)
    {
        return IsValidTarget(target) && target.Player == potion.Owner;
    }

    public bool IsValidTarget(ActionModel action, Creature target)
    {
        return IsValidTarget(target) && target.Player == action.Owner.PetOwner;
    }

    private static bool IsValidTarget(Creature target)
    {
        return target is { IsAlive: true, IsPlayer: true, Side: CombatSide.Player };
    }
}