using System.Collections.Generic;
using System.Threading.Tasks;
using Furina.Characters.Furina.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Revision : FurinaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new EnergyVar(1), new PowerVar<RevisionPower>(1m)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        HoverTipFactory.FromCardWithCardHoverTips<Script>();

    public Revision()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<RevisionPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["RevisionPower"].BaseValue,
            Owner.Creature,
            this);

        Script.RefreshAll(Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Energy"].UpgradeValueBy(1m);
        DynamicVars["RevisionPower"].UpgradeValueBy(1m);
    }
}
