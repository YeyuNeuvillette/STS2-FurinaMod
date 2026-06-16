using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;
using Furina.Characters.Furina.Minions;
using Furina.Characters.Furina.Powers;
using MinionLib.Minion;
using MinionLib.Commands;
using Godot;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Embrace : InviteCard
{
    public Embrace()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override Type SelectMinionType(PlayerChoiceContext choiceContext)
    {
        return typeof(SurintendanteChevalmarin);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        var ousiaPower = Owner.Creature.GetPower<OusiaPower>();
        var pneumaPower = Owner.Creature.GetPower<PneumaPower>();

        if (pneumaPower != null && pneumaPower.Amount > 0)
        {
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
        else
        {
            await base.OnPlay(choiceContext, cardPlay);
        }

        var drawAmount = CurrentUpgradeLevel > 0 ? 2m : 1m;
        await CardPileCmd.Draw(choiceContext, drawAmount, Owner);
    }

    private new async Task EnhanceSingerOfManyWaters(PlayerChoiceContext choiceContext, Creature singer)
    {
        if (Owner?.Creature == null)
            return;

        GD.Print($"[Embrace] Enhancing SingerOfManyWaters: {singer.Name}");
        GD.Print($"[Embrace] Before enhancement - MaxHp: {singer.MaxHp}, CurrentHp: {singer.CurrentHp}");

        var oldMaxHp = singer.MaxHp;
        var oldCurrentHp = singer.CurrentHp;
        var newMaxHp = oldMaxHp + 5;
        
        await CreatureCmd.SetMaxHp(singer, newMaxHp);
        
        var hpIncrease = newMaxHp - oldMaxHp;
        var newCurrentHp = Math.Min(oldCurrentHp + hpIncrease, newMaxHp);
        await CreatureCmd.SetCurrentHp(singer, newCurrentHp);
        
        GD.Print($"[Embrace] After HP enhancement - MaxHp: {singer.MaxHp}, CurrentHp: {singer.CurrentHp}");
    }

    protected override void OnUpgrade()
    {
    }
}