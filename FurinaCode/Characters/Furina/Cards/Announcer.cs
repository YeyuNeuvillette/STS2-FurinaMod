using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;
using Furina.Characters.Furina.Powers;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
[RegisterCharacterStarterCard(typeof(Furina), 1,Order=4)]
public sealed class Announcer : OusiaPneumaCard
{
    public Announcer()
        : base(0, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
    }

    protected override async Task OnOusiaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await PowerCmd.Apply<AnnouncerOusiaPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override async Task OnPneumaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await PowerCmd.Apply<AnnouncerPneumaPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override async Task OnChorusEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await PowerCmd.Apply<AnnouncerOusiaPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await PowerCmd.Apply<AnnouncerPneumaPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}