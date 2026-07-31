using System.Collections.Generic;
using System.Threading.Tasks;
using Furina.Characters.Base;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class DaughterOfWaterSecondMovement : MovementCard
{
    private static readonly int[] Sequence = [1, 2];

    protected override IReadOnlyList<int> TargetSequence => Sequence;

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new EnergyVar(3)
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_MOVEMENT"),
        CardKeyword.Retain,
        CardKeyword.Exhaust
    };

    public DaughterOfWaterSecondMovement()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}
