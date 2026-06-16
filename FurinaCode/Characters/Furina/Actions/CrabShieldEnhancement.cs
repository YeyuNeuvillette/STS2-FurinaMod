using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MinionLib.Action;
using Furina.RitsuAdapters;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace Furina.Characters.Furina.Actions;

public sealed class CrabShieldEnhancement : ModActionTemplate
{
    private const string _blockBonusKey = "BlockBonus";

    public override TargetType TargetType => TargetType.None;

    public override bool AutoRemoveAtTurnEnd => false;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false;

    public override int DisplayAmount => (int)GetBlockBonus();

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new DynamicVar(_blockBonusKey, 0m) };

    public override PowerAssetProfile AssetProfile => new PowerAssetProfile(
        "res://Furina/images/powers/crab_shield_enhancement.png",
        "res://Furina/images/powers/big/crab_shield_enhancement.png");

    public override bool DecrementAfterAct => false;

    public override bool CanAct(ICombatState combatState)
    {
        return false;
    }

    protected override async Task OnAct(PlayerChoiceContext choiceContext, Creature? target)
    {
        await Task.CompletedTask;
    }

    public decimal GetBlockBonus()
    {
        return Amount * 2m;
    }
}