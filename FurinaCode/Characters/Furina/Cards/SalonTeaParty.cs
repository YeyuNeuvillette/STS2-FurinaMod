using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Furina.Characters.Furina.Minions;
using Furina.Characters.Furina.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class SalonTeaParty : InviteCard
{
    protected override bool HasEnergyCostX => true;

    protected override Type SelectMinionType(PlayerChoiceContext choiceContext)
    {
        return InvitationManager.SelectRandomSalonMember(Owner);
    }

    public SalonTeaParty()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        int x = ResolveEnergyXValue();
        if (CurrentUpgradeLevel > 0)
            x++;

        var state = ArkheSystem.GetState(Owner.Creature, includeDualEffect: false);

        for (int i = 0; i < x; i++)
        {
            if (state == ArkheState.Ousia)
            {
                await HandleOusiaState(choiceContext);
            }
            else if (state == ArkheState.Pneuma)
            {
                await HandlePneumaState(choiceContext);
            }
        }
    }
}
