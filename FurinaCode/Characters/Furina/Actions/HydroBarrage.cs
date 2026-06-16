using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
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

public sealed class HydroBarrage : ModActionTemplate
{
    private const string _usesKey = "Uses";
    private const string _damageKey = "Damage";

    public override TargetType TargetType => TargetType.AnyEnemy;

    public override bool AutoRemoveAtTurnEnd => false;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => base.DynamicVars[_usesKey].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
    { 
        new DynamicVar(_usesKey, 1m),
        new DynamicVar(_damageKey, 2m)
    };

    public override PowerAssetProfile AssetProfile => new PowerAssetProfile(
        "res://Furina/images/powers/furina_power_hydro_barrage.png",
        "res://Furina/images/powers/big/furina_power_hydro_barrage.png");

    public override bool DecrementAfterAct => false;

    public override bool CanAct(ICombatState combatState)
    {
        var actor = Owner;
        var usesValue = base.DynamicVars[_usesKey].BaseValue;
        var result = usesValue > 0m && actor.IsAlive && actor.CombatState == combatState;
        GD.Print($"[MinionAction] HydroBarrage.CanAct: Uses={usesValue}, Owner.IsAlive={actor.IsAlive}, Owner.CombatState==combatState={actor.CombatState == combatState}, result={result}");
        return result;
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side)
        {
            base.DynamicVars[_usesKey].BaseValue = 1m;
            InvokeDisplayAmountChanged();
            UpdateDamage();
        }
        return Task.CompletedTask;
    }

    public void UpdateDamage()
    {
        var enhancement = Owner.GetPower<HydroBarrageEnhancement>();
        var damageBonus = enhancement?.GetDamageBonus() ?? 0m;
        base.DynamicVars[_damageKey].BaseValue = 2m + damageBonus;
    }

    protected override async Task OnAct(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target == null) return;
        if (base.DynamicVars[_usesKey].BaseValue <= 0m) return;
        
        base.DynamicVars[_usesKey].BaseValue--;
        InvokeDisplayAmountChanged();
        
        UpdateDamage();
        var damage = base.DynamicVars[_damageKey].BaseValue;
        var actor = Owner;
        
        await MinionAnimCmd.PlayBumpAttackAsync(actor, target,
            () => CreatureCmd.Damage(choiceContext, target, damage, ValueProp.Move, actor, null));
    }
}