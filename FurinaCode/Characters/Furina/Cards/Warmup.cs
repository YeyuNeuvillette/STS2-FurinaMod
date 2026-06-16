using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Furina.Powers;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Warmup : OusiaPneumaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
    { 
        new PowerVar<WarmupStrengthPower>(2m),
        new PowerVar<WarmupDexterityPower>(2m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        base.AdditionalHoverTips.Concat(
            [
                HoverTipFactory.FromPower<StrengthPower>(),
                HoverTipFactory.FromPower<DexterityPower>()
            ]
        );

    public Warmup()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnOusiaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<WarmupStrengthPower>(choiceContext, Owner.Creature, DynamicVars["WarmupStrengthPower"].BaseValue, Owner.Creature, this);
    }

    protected override async Task OnPneumaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<WarmupDexterityPower>(choiceContext, Owner.Creature, DynamicVars["WarmupDexterityPower"].BaseValue, Owner.Creature, this);
    }

    protected override async Task OnNoArkheEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<WarmupStrengthPower>(choiceContext, Owner.Creature, DynamicVars["WarmupStrengthPower"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<WarmupDexterityPower>(choiceContext, Owner.Creature, DynamicVars["WarmupDexterityPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WarmupStrengthPower"].UpgradeValueBy(1m);
        DynamicVars["WarmupDexterityPower"].UpgradeValueBy(1m);
    }
}