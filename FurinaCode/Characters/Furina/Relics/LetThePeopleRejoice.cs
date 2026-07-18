using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;
using Furina.Characters.Furina.Cards;
using Furina.Characters.Furina.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace Furina.Characters.Furina.Relics;

[RegisterRelic(typeof(FurinaRelicPool))]
public sealed class LetThePeopleRejoice : BaseRelic
{
    public override MegaCrit.Sts2.Core.Entities.Relics.RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<string> RegisteredKeywordIds =>
        new List<string> { "FURINA_KEYWORD_ARKHE" };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        HoverTipFactory.FromCardWithCardHoverTips<SeatsSacredAndSecular>();

    public override async Task BeforeCombatStart()
    {
        await ArkheSystem.SetState(new ThrowingPlayerChoiceContext(), Owner, ArkheState.Ousia);
    }

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner)
            return;

        if (Owner.Creature?.CombatState == null)
            return;

        var card = Owner.Creature.CombatState.CreateCard<SeatsSacredAndSecular>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
        if (!ArkheSystem.IsArkhePower(power) || power.Owner != Owner?.Creature || amount <= 0m)
            return;

        await PlayerCmd.GainEnergy(1m, Owner);
    }
}
