using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;
using Furina.Characters.Furina.Minions;
using Furina.Characters.Furina.Powers;
using MegaCrit.Sts2.Core.Models.CardPools;
using MinionLib.Commands;
using MegaCrit.Sts2.Core.Models.Monsters;
using MinionLib.Minion;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class InviteSingerOfManyWaters : InviteCard, KnowledgeDemon.IChoosable
{
    public override bool CanBeGeneratedInCombat => false;

    public override int MaxUpgradeLevel => 0;

    public InviteSingerOfManyWaters()
        : base(-1, CardType.Skill, CardRarity.Status, TargetType.None, shouldShowInCardLibrary: false)
    {
    }

    protected override Type SelectMinionType(PlayerChoiceContext choiceContext)
    {
        return typeof(SingerOfManyWaters);
    }

    public async Task OnChosen()
    {
        if (Owner?.Creature == null)
            return;

        var existingSinger = Owner.PlayerCombatState?.GetPet<SingerOfManyWaters>();

        if (existingSinger != null)
        {
            await EnhanceSingerOfManyWaters(new ThrowingPlayerChoiceContext(), existingSinger);
        }
        else
        {
            await MinionCmd.AddMinion<SingerOfManyWaters>(new ThrowingPlayerChoiceContext(), Owner, new MinionSummonOptions(
                MaxHp: 3m,
                Source: this,
                Position: MinionPosition.Front));
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
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

    private new async Task EnhanceSingerOfManyWaters(PlayerChoiceContext choiceContext, Creature singer)
    {
        if (Owner?.Creature == null)
            return;

        var oldMaxHp = singer.MaxHp;
        var oldCurrentHp = singer.CurrentHp;
        var newMaxHp = oldMaxHp + 5;
        
        await CreatureCmd.SetMaxHp(singer, newMaxHp);
        
        var hpIncrease = newMaxHp - oldMaxHp;
        var newCurrentHp = Math.Min(oldCurrentHp + hpIncrease, newMaxHp);
        await CreatureCmd.SetCurrentHp(singer, newCurrentHp);
    }
}