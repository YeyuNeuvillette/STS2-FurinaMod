using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class AllWatersKindredsPeoples : FurinaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { CardKeyword.Exhaust };

    public AllWatersKindredsPeoples()
        : base(3, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
            return;

        var affectedPiles = new[] { PileType.Draw, PileType.Hand, PileType.Discard };
        foreach (var pileType in affectedPiles)
        {
            foreach (var card in pileType.GetPile(Owner).Cards)
            {
                if (CurrentUpgradeLevel > 0)
                    card.EnergyCost.AddUntilPlayed(-1);
                else
                    card.EnergyCost.AddThisTurn(-1);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
