using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Rehearsal : BaseCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { CardKeyword.Exhaust };

    public Rehearsal()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var targetCard = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(new LocString("card_selection", "FURINA-REHEARSAL.selectionScreenPrompt"), 1),
            card => true,
            this)).FirstOrDefault();

        if (targetCard == null)
            return;

        GD.Print($"[Rehearsal] Selected card: {targetCard.Id.Entry}");
        
        var baseCost = targetCard.EnergyCost.GetWithModifiers(CostModifiers.None);
        GD.Print($"[Rehearsal] Base cost (without global modifiers): {baseCost}");
        
        var newCost = Math.Max(0, baseCost - 1);
        targetCard.EnergyCost.SetCustomBaseCost(newCost);
        GD.Print($"[Rehearsal] Card cost modified to: {newCost}");
    }

    protected override void OnUpgrade()
    {
        var baseCost = EnergyCost.GetWithModifiers(CostModifiers.None);
        var newCost = Math.Max(0, baseCost - 1);
        EnergyCost.SetCustomBaseCost(newCost);
    }
}