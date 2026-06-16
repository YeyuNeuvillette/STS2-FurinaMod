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

[RegisterCard(typeof(TokenCardPool))]
public sealed class InviteMademoiselleCrabaletta : InviteCard, KnowledgeDemon.IChoosable
{
    public override bool CanBeGeneratedInCombat => false;

    public override int MaxUpgradeLevel => 0;

    public InviteMademoiselleCrabaletta()
        : base(-1, CardType.Skill, CardRarity.Status, TargetType.None, shouldShowInCardLibrary: false)
    {
    }

    protected override Type SelectMinionType(PlayerChoiceContext choiceContext)
    {
        return typeof(MademoiselleCrabaletta);
    }

    public async Task OnChosen()
    {
        if (Owner?.Creature == null)
            return;

        var existingMinion = Owner.PlayerCombatState?.Pets.FirstOrDefault(p => p.Monster?.GetType() == typeof(MademoiselleCrabaletta));

        if (existingMinion != null)
        {
            await EnhanceExistingMinion(new ThrowingPlayerChoiceContext(), existingMinion, typeof(MademoiselleCrabaletta));
        }
        else
        {
            await MinionCmd.AddMinion<MademoiselleCrabaletta>(new ThrowingPlayerChoiceContext(), Owner, new MinionSummonOptions(
                MaxHp: 3m,
                Source: this,
                Position: MinionPosition.Front));
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        var existingMinion = Owner.PlayerCombatState?.Pets.FirstOrDefault(p => p.Monster?.GetType() == typeof(MademoiselleCrabaletta));

        if (existingMinion != null)
        {
            await EnhanceExistingMinion(choiceContext, existingMinion);
        }
        else
        {
            await MinionCmd.AddMinion<MademoiselleCrabaletta>(choiceContext, Owner, new MinionSummonOptions(
                MaxHp: 3m,
                Source: this,
                Position: MinionPosition.Front));
        }
    }

    private async Task EnhanceExistingMinion(PlayerChoiceContext choiceContext, Creature existingMinion)
    {
        if (Owner?.Creature == null)
            return;

        var oldMaxHp = existingMinion.MaxHp;
        var oldCurrentHp = existingMinion.CurrentHp;
        var newMaxHp = oldMaxHp + 3;
        
        await CreatureCmd.SetMaxHp(existingMinion, newMaxHp);
        
        var hpIncrease = newMaxHp - oldMaxHp;
        var newCurrentHp = Math.Min(oldCurrentHp + hpIncrease, newMaxHp);
        await CreatureCmd.SetCurrentHp(existingMinion, newCurrentHp);

        var crabShield = existingMinion.GetPower<Actions.CrabShield>();
        if (crabShield != null)
        {
            var enhancement = existingMinion.GetPower<Actions.CrabShieldEnhancement>();
            if (enhancement == null)
            {
                await PowerCmd.Apply<Actions.CrabShieldEnhancement>(choiceContext, existingMinion, 1m, Owner.Creature, this);
                enhancement = existingMinion.GetPower<Actions.CrabShieldEnhancement>();
            }
            
            if (enhancement != null)
            {
                enhancement.DynamicVars["BlockBonus"].BaseValue += 2m;
                crabShield.UpdateBlock();
            }
        }
    }
}