using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using Furina.Characters.Base;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class DaughterOfWaterFirstMovement : MovementCard
{
    protected override int[] TargetSequence => new[] { 0, 1, 2 };

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new PowerVar<IntangiblePower>(1m)
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_MOVEMENT"),
        CardKeyword.Exhaust
    };

    public DaughterOfWaterFirstMovement()
        : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner.Creature, DynamicVars["IntangiblePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}