using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Furina.Characters.Base;
using Furina.Characters.Furina.Minions;
using Furina.Characters.Furina.Powers;
using Furina.RitsuAdapters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MinionLib.Action;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class EveryoneOut : InviteCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new PowerVar<EveryoneOutPower>(1m)
    };

    protected override Type SelectMinionType(PlayerChoiceContext choiceContext)
    {
        return typeof(GentilhommeUsher);
    }

    public EveryoneOut()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        if (ArkheSystem.GetState(Owner.Creature, includeDualEffect: false) == ArkheState.Pneuma)
        {
            for (int i = 0; i < 3; i++)
            {
                await HandlePneumaState(choiceContext);
            }
        }
        else
        {
            foreach (var minionType in InvitationManager.GetSalonMemberTypes())
            {
                await HandleOusiaState(choiceContext, minionType);
            }
        }

        await AddExtraUsesToSalonMembers(1);
        await PowerCmd.Apply<EveryoneOutPower>(choiceContext, Owner.Creature, DynamicVars["EveryoneOutPower"].BaseValue, Owner.Creature, this);
    }

    private async Task AddExtraUsesToSalonMembers(decimal extraAmount)
    {
        var salonPets = Owner!.PlayerCombatState?.Pets
            .Where(InvitationManager.IsSalonMember)
            .ToList();

        if (salonPets == null)
            return;

        foreach (var pet in salonPets)
        {
            var action = pet.GetPower<ModActionTemplate>();
            if (action == null) continue;

            if (!IsOnActiveTurn(action)) continue;

            if (action.DynamicVars.TryGetValue("Uses", out var usesVar))
            {
                usesVar.BaseValue += extraAmount;
                action.InvokeDisplayAmountChanged();
            }

            await Task.CompletedTask;
        }
    }

    private static bool IsOnActiveTurn(ModActionTemplate action)
    {
        if (action.DynamicVars.TryGetValue("TurnCounter", out var turnCounterVar))
        {
            return turnCounterVar.BaseValue == 0m;
        }
        return true;
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
