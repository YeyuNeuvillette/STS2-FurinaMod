using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class ReadThrough : FurinaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
    { 
        new BlockVar(4m, ValueProp.Move)
    };

    public override bool GainsBlock => true;

    public ReadThrough()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        var hand = Owner.PlayerCombatState?.Hand;
        var totalCardsCount = hand != null ? hand.Cards.Count : 0;
        var baseBlock = DynamicVars.Block.BaseValue;
        var totalBlock = baseBlock + totalCardsCount;

        GD.Print($"[ReadThrough] Total cards in hand: {totalCardsCount}");
        GD.Print($"[ReadThrough] Base block: {baseBlock}");
        GD.Print($"[ReadThrough] Total block to gain: {totalBlock}");

        await CreatureCmd.GainBlock(Owner.Creature, totalBlock, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}