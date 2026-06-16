using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Base;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Reunion : TropeCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new CardsVar(2),
        new BlockVar("BonusBlock", 6m, ValueProp.Move)
    };

    public override bool GainsBlock => true;

    public Reunion()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.OnPlay(choiceContext, cardPlay);

        if (Owner == null)
            return;

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

        if (Owner.PlayerCombatState?.Hand != null)
        {
            var handCount = Owner.PlayerCombatState.Hand.Cards.Count();
            if (handCount >= CardPile.MaxCardsInHand)
            {
                if (Owner.Creature != null)
                {
                    await CreatureCmd.GainBlock(Owner.Creature, DynamicVars["BonusBlock"].BaseValue, ValueProp.Move, cardPlay);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}