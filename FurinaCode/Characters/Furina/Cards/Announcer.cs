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
using MegaCrit.Sts2.Core.Localization.DynamicVars;
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

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new IntVar("CostReduction", 1)
    ];

    protected override async Task OnOusiaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            GD.Print("[Announcer] OnOusiaEffect early return: Owner or Creature is null");
            return;
        }

        GD.Print("[Announcer] OnOusiaEffect applying AnnouncerOusiaPower");
        var power = await PowerCmd.Apply<AnnouncerOusiaPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        if (power != null)
            power.CostReduction = DynamicVars["CostReduction"].BaseValue;
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
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
        var power = await PowerCmd.Apply<AnnouncerPneumaPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        if (power != null)
            power.CostReduction = DynamicVars["CostReduction"].BaseValue;
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
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
        var ousiaPower = await PowerCmd.Apply<AnnouncerOusiaPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        if (ousiaPower != null)
            ousiaPower.CostReduction = DynamicVars["CostReduction"].BaseValue;
        var pneumaPower = await PowerCmd.Apply<AnnouncerPneumaPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        if (pneumaPower != null)
            pneumaPower.CostReduction = DynamicVars["CostReduction"].BaseValue;
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        GD.Print("[Announcer] OnChorusEffect done");
    }

    protected override void OnUpgrade()
    {
        DynamicVars["CostReduction"].UpgradeValueBy(1);
    }
}