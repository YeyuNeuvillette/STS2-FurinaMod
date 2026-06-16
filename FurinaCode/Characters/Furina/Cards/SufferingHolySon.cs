using System.Collections.Generic;
using System.Threading.Tasks;
using Furina.Characters.Base;
using Furina.Characters.Furina.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class SufferingHolySon : FurinaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new HpLossVar(2m),
        new DynamicVar("MaxHpLoss", 20m)
    };

    public SufferingHolySon()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);

        var reduction = (int)DynamicVars["MaxHpLoss"].BaseValue;

        FurinaRunData.SufferingHolySon.Modify(Owner, data =>
        {
            data.BossMaxHpReduction += reduction;
        });

        await PowerCmd.Apply<SufferingHolySonPower>(choiceContext, Owner.Creature, reduction, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxHpLoss"].UpgradeValueBy(10m);
    }
}