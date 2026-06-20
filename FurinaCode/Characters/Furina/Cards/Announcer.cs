using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
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
        {
            GD.Print("[Announcer] OnOusiaEffect early return: Owner or Creature is null");
            return;
        }

        GD.Print("[Announcer] OnOusiaEffect applying AnnouncerOusiaPower");
        await PowerCmd.Apply<AnnouncerOusiaPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        GD.Print("[Announcer] OnOusiaEffect done");
    }

    protected override async Task OnPneumaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            GD.Print("[Announcer] OnPneumaEffect early return: Owner or Creature is null");
            return;
        }

        GD.Print("[Announcer] OnPneumaEffect applying AnnouncerPneumaPower");
        await PowerCmd.Apply<AnnouncerPneumaPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        GD.Print("[Announcer] OnPneumaEffect done");
    }

    protected override async Task OnChorusEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            GD.Print("[Announcer] OnChorusEffect early return: Owner or Creature is null");
            return;
        }

        GD.Print("[Announcer] OnChorusEffect applying both powers");
        await PowerCmd.Apply<AnnouncerOusiaPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await PowerCmd.Apply<AnnouncerPneumaPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        GD.Print("[Announcer] OnChorusEffect done");
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}