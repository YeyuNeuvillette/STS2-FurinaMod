using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MinionLib.Minion;
using Furina.Characters.Furina.Minions;
using Furina.Characters.Furina.Powers;
using MinionLib.Commands;
using Godot;
using STS2RitsuLib.Keywords;

namespace Furina.Characters.Furina.Cards;

public abstract class InviteCard(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType target,
    bool shouldShowInCardLibrary = true)
    : FurinaCard(energyCost, type, rarity, target, shouldShowInCardLibrary)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_INVITE")
    };

    protected abstract Type SelectMinionType(PlayerChoiceContext choiceContext);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        var ousiaPower = Owner.Creature.GetPower<OusiaPower>();
        var pneumaPower = Owner.Creature.GetPower<PneumaPower>();

        if (ousiaPower != null && ousiaPower.Amount > 0)
        {
            await HandleOusiaState(choiceContext);
        }
        else if (pneumaPower != null && pneumaPower.Amount > 0)
        {
            await HandlePneumaState(choiceContext);
        }
    }

    protected async Task HandleOusiaState(PlayerChoiceContext choiceContext)
    {
        if (Owner?.Creature == null)
            return;

        var minionType = SelectMinionType(choiceContext);
        var existingMinion = Owner.PlayerCombatState?.Pets.FirstOrDefault(p => p.Monster?.GetType() == minionType);

        if (existingMinion != null)
        {
            await EnhanceExistingMinion(choiceContext, existingMinion, minionType);
        }
        else
        {
            await SummonMinion(choiceContext, minionType);
        }
    }

    protected async Task HandlePneumaState(PlayerChoiceContext choiceContext)
    {
        if (Owner?.Creature == null)
            return;

        var existingSinger = Owner.PlayerCombatState?.GetPet<SingerOfManyWaters>();

        if (existingSinger != null)
        {
            await EnhanceSingerOfManyWaters(choiceContext, existingSinger);
        }
        else
        {
            await MinionCmd.AddMinion<SingerOfManyWaters>(choiceContext, Owner, new MinionSummonOptions(
                MaxHp: 3m,
                Source: this,
                Position: MinionPosition.Front));
        }
    }

    protected async Task SummonMinion(PlayerChoiceContext choiceContext, Type minionType)
    {
        if (Owner?.Creature == null)
            return;

        if (minionType == typeof(GentilhommeUsher))
        {
            await MinionCmd.AddMinion<GentilhommeUsher>(choiceContext, Owner, new MinionSummonOptions(
                MaxHp: 3m,
                Source: this,
                Position: MinionPosition.Front));
        }
        else if (minionType == typeof(MademoiselleCrabaletta))
        {
            await MinionCmd.AddMinion<MademoiselleCrabaletta>(choiceContext, Owner, new MinionSummonOptions(
                MaxHp: 3m,
                Source: this,
                Position: MinionPosition.Front));
        }
        else if (minionType == typeof(SurintendanteChevalmarin))
        {
            await MinionCmd.AddMinion<SurintendanteChevalmarin>(choiceContext, Owner, new MinionSummonOptions(
                MaxHp: 3m,
                Source: this,
                Position: MinionPosition.Front));
        }
    }

    protected async Task EnhanceExistingMinion(PlayerChoiceContext choiceContext, Creature existingMinion, Type minionType)
    {
        if (Owner?.Creature == null)
            return;

        if (existingMinion == null || existingMinion.PetOwner == null)
        {
            GD.PrintErr("[InviteCard] Existing minion is null or has no owner");
            return;
        }

        GD.Print($"[InviteCard] Enhancing existing minion: {existingMinion.Name}, Type: {minionType.Name}");
        GD.Print($"[InviteCard] Before enhancement - MaxHp: {existingMinion.MaxHp}, CurrentHp: {existingMinion.CurrentHp}");

        var oldMaxHp = existingMinion.MaxHp;
        var oldCurrentHp = existingMinion.CurrentHp;
        var newMaxHp = oldMaxHp + 3;
        
        await CreatureCmd.SetMaxHp(existingMinion, newMaxHp);
        
        var hpIncrease = newMaxHp - oldMaxHp;
        var newCurrentHp = Math.Min(oldCurrentHp + hpIncrease, newMaxHp);
        await CreatureCmd.SetCurrentHp(existingMinion, newCurrentHp);
        
        GD.Print($"[InviteCard] After HP enhancement - MaxHp: {existingMinion.MaxHp}, CurrentHp: {existingMinion.CurrentHp}");

        if (minionType == typeof(GentilhommeUsher))
        {
            GD.Print("[InviteCard] Enhancing HydroBarrage");
            await EnhanceHydroBarrage(choiceContext, existingMinion);
        }
        else if (minionType == typeof(MademoiselleCrabaletta))
        {
            GD.Print("[InviteCard] Enhancing CrabShield");
            await EnhanceCrabShield(choiceContext, existingMinion);
        }
        else if (minionType == typeof(SurintendanteChevalmarin))
        {
            GD.Print("[InviteCard] Enhancing PureWaterNote");
            await EnhancePureWaterNote(choiceContext, existingMinion);
        }
    }

    protected async Task EnhanceSingerOfManyWaters(PlayerChoiceContext choiceContext, Creature singer)
    {
        if (Owner?.Creature == null)
            return;

        GD.Print($"[InviteCard] Enhancing SingerOfManyWaters: {singer.Name}");
        GD.Print($"[InviteCard] Before enhancement - MaxHp: {singer.MaxHp}, CurrentHp: {singer.CurrentHp}");

        var oldMaxHp = singer.MaxHp;
        var oldCurrentHp = singer.CurrentHp;
        var newMaxHp = oldMaxHp + 5;
        
        await CreatureCmd.SetMaxHp(singer, newMaxHp);
        
        var hpIncrease = newMaxHp - oldMaxHp;
        var newCurrentHp = Math.Min(oldCurrentHp + hpIncrease, newMaxHp);
        await CreatureCmd.SetCurrentHp(singer, newCurrentHp);
        
        GD.Print($"[InviteCard] After HP enhancement - MaxHp: {singer.MaxHp}, CurrentHp: {singer.CurrentHp}");
    }

    private async Task EnhanceHydroBarrage(PlayerChoiceContext choiceContext, Creature minion)
    {
        GD.Print("[InviteCard] EnhanceHydroBarrage called");
        var hydroBarrage = minion.GetPower<Actions.HydroBarrage>();
        if (hydroBarrage != null)
        {
            GD.Print($"[InviteCard] HydroBarrage found, applying enhancement");
            await PowerCmd.Apply<Actions.HydroBarrageEnhancement>(choiceContext, minion, 1m, Owner.Creature, this);
            hydroBarrage.UpdateDamage();
            GD.Print($"[InviteCard] UpdateDamage called");
        }
        else
        {
            GD.PrintErr("[InviteCard] HydroBarrage power not found");
        }
    }

    private async Task EnhanceCrabShield(PlayerChoiceContext choiceContext, Creature minion)
    {
        GD.Print("[InviteCard] EnhanceCrabShield called");
        var crabShield = minion.GetPower<Actions.CrabShield>();
        if (crabShield != null)
        {
            GD.Print($"[InviteCard] CrabShield found, applying enhancement");
            await PowerCmd.Apply<Actions.CrabShieldEnhancement>(choiceContext, minion, 1m, Owner.Creature, this);
            crabShield.UpdateBlock();
            GD.Print($"[InviteCard] UpdateBlock called");
        }
        else
        {
            GD.PrintErr("[InviteCard] CrabShield power not found");
        }
    }

    private async Task EnhancePureWaterNote(PlayerChoiceContext choiceContext, Creature minion)
    {
        GD.Print("[InviteCard] EnhancePureWaterNote called");
        var pureWaterNote = minion.GetPower<Actions.PureWaterNote>();
        if (pureWaterNote != null)
        {
            GD.Print($"[InviteCard] PureWaterNote found, applying enhancement");
            await PowerCmd.Apply<Actions.PureWaterNoteEnhancement>(choiceContext, minion, 1m, Owner.Creature, this);
            pureWaterNote.UpdateDraw();
            GD.Print($"[InviteCard] UpdateDraw called");
        }
        else
        {
            GD.PrintErr("[InviteCard] PureWaterNote power not found");
        }
    }
}