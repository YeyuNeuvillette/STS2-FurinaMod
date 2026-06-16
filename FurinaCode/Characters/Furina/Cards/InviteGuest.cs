using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Monsters;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;
using Furina.Characters.Furina.Minions;
using Furina.Characters.Furina.Powers;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
[RegisterCharacterStarterCard(typeof(Furina), 1,Order=3)]
public sealed class InviteGuest : InviteCard
{
    public InviteGuest()
        : base(1, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
    }

    protected override Type SelectMinionType(PlayerChoiceContext choiceContext)
    {
        var minions = new[] { typeof(GentilhommeUsher), typeof(MademoiselleCrabaletta), typeof(SurintendanteChevalmarin) };
        return Owner.RunState.Rng.CombatCardSelection.NextItem(minions)!;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        if (CurrentUpgradeLevel > 0)
        {
            await HandleUpgradedEffect(choiceContext);
        }
        else
        {
            await base.OnPlay(choiceContext, cardPlay);
        }
    }

    private async Task HandleUpgradedEffect(PlayerChoiceContext choiceContext)
    {
        if (Owner?.Creature == null)
            return;

        var creature = Owner.Creature;
        if (creature.CombatState == null)
            return;

        var ousiaPower = creature.GetPower<OusiaPower>();
        var pneumaPower = creature.GetPower<PneumaPower>();

        List<CardModel> options;

        if (ousiaPower != null && ousiaPower.Amount > 0)
        {
            options = new List<CardModel>
            {
                creature.CombatState.CreateCard<InviteGentilhommeUsher>(Owner),
                creature.CombatState.CreateCard<InviteMademoiselleCrabaletta>(Owner),
                creature.CombatState.CreateCard<InviteSurintendanteChevalmarin>(Owner)
            };
        }
        else if (pneumaPower != null && pneumaPower.Amount > 0)
        {
            options = new List<CardModel>
            {
                creature.CombatState.CreateCard<InviteSingerOfManyWaters>(Owner)
            };
        }
        else
        {
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
            return;
        }

        var selectedCard = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), options, Owner);
        if (selectedCard != null)
        {
            await ((KnowledgeDemon.IChoosable)selectedCard).OnChosen();
        }
    }
}