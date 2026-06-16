using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Entrust : OusiaPneumaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new BlockVar(20m, ValueProp.Move),
        new HpLossVar(5m)
    };

    public override bool GainsBlock => true;

    public override bool HasTurnEndInHandEffect => CurrentArkheState is ArkheState.Ousia or ArkheState.None;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            var tips = new List<IHoverTip> { HoverTipFactory.FromKeyword(CardKeyword.Exhaust) };
            if (CurrentArkheState is ArkheState.Pneuma)
            {
                tips.Add(HoverTipFactory.FromKeyword(CardKeyword.Ethereal));
            }
            return tips;
        }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        ModKeywordRegistry.GetCardKeyword("FURINA_KEYWORD_OUSIA_PNEUMA")
    };

    public Entrust()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        if (CurrentArkheState is ArkheState.Pneuma or ArkheState.None)
        {
            return PileType.Exhaust;
        }
        return base.GetResultPileTypeForCardPlay();
    }

    protected override async Task OnOusiaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
    }

    protected override async Task OnPneumaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
    }

    protected override async Task OnNoArkheEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
    }

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (CurrentArkheState is ArkheState.Pneuma && side == CombatSide.Player && Pile?.Type == PileType.Hand)
        {
            await CardCmd.Exhaust(choiceContext, this, causedByEthereal: true);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(10m);
    }
}