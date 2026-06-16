using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MinionLib.Action;
using Furina.RitsuAdapters;
using MinionLib.Commands;
using MinionLib.Targeting;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Models;

namespace Furina.Characters.Furina.Actions;

public sealed class CrabShield : ModActionTemplate
{
    private const string _usesKey = "Uses";
    private const string _blockKey = "Block";

    public override TargetType TargetType => MinionTargetTypes.AnyMinionOrOwner;

    public override bool AutoRemoveAtTurnEnd => false;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => base.DynamicVars[_usesKey].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
    { 
        new DynamicVar(_usesKey, 1m),
        new DynamicVar(_blockKey, 2m)
    };

    public override PowerAssetProfile AssetProfile => new PowerAssetProfile(
        "res://Furina/images/powers/furina_power_crab_shield.png",
        "res://Furina/images/powers/big/furina_power_crab_shield.png");

    public override bool DecrementAfterAct => false;

    public override bool CanAct(ICombatState combatState)
    {
        var actor = Owner;
        return base.DynamicVars[_usesKey].BaseValue > 0m && actor.IsAlive && actor.CombatState == combatState;
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        Owner.PowerApplied += OnPowerApplied;
        UpdateBlock();
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        oldOwner.PowerApplied -= OnPowerApplied;
    }

    private void OnPowerApplied(PowerModel power)
    {
        if (power is CrabShieldEnhancement)
        {
            UpdateBlock();
        }
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side)
        {
            base.DynamicVars[_usesKey].BaseValue = 1m;
            InvokeDisplayAmountChanged();
            UpdateBlock();
        }
        return Task.CompletedTask;
    }

    public void UpdateBlock()
    {
        var enhancement = Owner.GetPower<CrabShieldEnhancement>();
        var blockBonus = enhancement?.GetBlockBonus() ?? 0m;
        base.DynamicVars[_blockKey].BaseValue = 2m + blockBonus;
    }

    protected override async Task OnAct(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target == null) return;
        if (base.DynamicVars[_usesKey].BaseValue <= 0m) return;
        
        base.DynamicVars[_usesKey].BaseValue--;
        InvokeDisplayAmountChanged();
        
        UpdateBlock();
        var block = (int)base.DynamicVars[_blockKey].BaseValue;
        
        await CreatureCmd.GainBlock(target, block, ValueProp.Move, null);
    }
}