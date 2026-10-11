using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Editing : FurinaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new List<DynamicVar> { new DamageVar(9m, ValueProp.Move) };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        base.AdditionalHoverTips.Concat(HoverTipFactory.FromCardWithCardHoverTips<Script>());

    public Editing()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || cardPlay.Target == null)
            return;

        var playCount = 1;
        var scriptPile = TropeSystem.GetPile(Owner);
        if (scriptPile.Cards.Count > 0)
        {
            var selectedCard = (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                scriptPile.Cards,
                Owner,
                new CardSelectorPrefs(
                    new LocString("card_selection", "FURINA-EDITING.selectionScreenPrompt"),
                    1))).FirstOrDefault();

            if (selectedCard != null)
            {
                await CardPileCmd.Add(selectedCard, PileType.Hand);
                playCount = 2;
            }
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitCount(playCount)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
