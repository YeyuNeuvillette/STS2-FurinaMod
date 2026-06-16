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

public sealed class JoyfulSinging : ModActionTemplate
{
    private const string _energyKey = "Energy";

    public override TargetType TargetType => TargetType.None;

    public override bool AutoRemoveAtTurnEnd => false;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new DynamicVar(_energyKey, 1m) };

    public override PowerAssetProfile AssetProfile => new PowerAssetProfile(
        "res://Furina/images/powers/furina_power_joyful_singing.png",
        "res://Furina/images/powers/big/furina_power_joyful_singing.png");

    public override bool DecrementAfterAct => false;

    public override bool CanAct(ICombatState combatState)
    {
        return false;
    }

    protected override async Task OnAct(PlayerChoiceContext choiceContext, Creature? target)
    {
        await Task.CompletedTask;
    }

    public void UpdateEnergy()
    {
        base.DynamicVars[_energyKey].BaseValue = 1m;
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature == Owner && Owner.PetOwner != null)
        {
            UpdateEnergy();
            var energyAmount = (int)base.DynamicVars[_energyKey].BaseValue;
            
            await PlayerCmd.GainEnergy(energyAmount, Owner.PetOwner);
        }
    }
}