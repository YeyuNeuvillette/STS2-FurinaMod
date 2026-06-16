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
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace Furina.Characters.Furina.Actions;

public sealed class PureWaterNote : ModActionTemplate
{
    private const string _usesKey = "Uses";
    private const string _turnCounterKey = "TurnCounter";
    private const string _drawKey = "Draw";

    public override TargetType TargetType => TargetType.None;

    public override bool AutoRemoveAtTurnEnd => false;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => base.DynamicVars[_usesKey].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
    { 
        new DynamicVar(_usesKey, 1m),
        new DynamicVar(_turnCounterKey, 0m),
        new DynamicVar(_drawKey, 1m)
    };

    public override PowerAssetProfile AssetProfile => new PowerAssetProfile(
        "res://Furina/images/powers/furina_power_pure_water_note.png",
        "res://Furina/images/powers/big/furina_power_pure_water_note.png");

    public override bool DecrementAfterAct => false;

    public override bool CanAct(ICombatState combatState)
    {
        var actor = Owner;
        return base.DynamicVars[_usesKey].BaseValue > 0m && actor.IsAlive && actor.CombatState == combatState;
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side)
        {
            base.DynamicVars[_turnCounterKey].BaseValue++;
            
            if (base.DynamicVars[_turnCounterKey].BaseValue >= 2m)
            {
                base.DynamicVars[_turnCounterKey].BaseValue = 0m;
                base.DynamicVars[_usesKey].BaseValue = 1m;
                InvokeDisplayAmountChanged();
                UpdateDraw();
            }
        }
        return Task.CompletedTask;
    }

    public void UpdateDraw()
    {
        var enhancement = Owner.GetPower<PureWaterNoteEnhancement>();
        var drawBonus = enhancement?.GetDrawBonus() ?? 0m;
        base.DynamicVars[_drawKey].BaseValue = 1m + drawBonus;
    }

    protected override async Task OnAct(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (base.DynamicVars[_usesKey].BaseValue <= 0m) return;
        
        base.DynamicVars[_usesKey].BaseValue--;
        InvokeDisplayAmountChanged();
        
        UpdateDraw();
        var drawAmount = (int)base.DynamicVars[_drawKey].BaseValue;
        
        if (Owner.PetOwner != null)
        {
            await CardPileCmd.Draw(choiceContext, drawAmount, Owner.PetOwner);
        }
    }
}