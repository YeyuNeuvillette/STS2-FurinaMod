using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Furina;
using Furina.Characters.Furina.Powers;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Adapt : OusiaPneumaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new PowerVar<AdaptStrengthPower>(5m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            var tips = new List<IHoverTip>();
            tips.Add(HoverTipFactory.FromPower<StrengthPower>());
            return tips;
        }
    }

    public Adapt()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override TargetType GetOusiaTargetType()
    {
        return TargetType.AnyPlayer;
    }

    protected override TargetType GetPneumaTargetType()
    {
        return TargetType.AnyEnemy;
    }

    protected override async Task OnOusiaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        var target = cardPlay.Target ?? Owner.Creature;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<AdaptStrengthPower>(choiceContext, target, DynamicVars["AdaptStrengthPower"].BaseValue, Owner.Creature, this);
    }

    protected override async Task OnPneumaEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
            return;

        await CreatureCmd.TriggerAnim(Owner!.Creature, "Cast", Owner.Character.CastAnimDelay);
        if (cardPlay.Target.HasPower<ArtifactPower>())
        {
            await PowerCmd.Apply<ArtifactPower>(choiceContext, cardPlay.Target, -1, Owner?.Creature, this);
            return;
        }
        await PowerCmd.Apply<AdaptStrengthDownPower>(choiceContext, cardPlay.Target, DynamicVars["AdaptStrengthPower"].BaseValue, Owner?.Creature, this);
    }

    protected override async Task OnNoArkheEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
            return;

        var enemyTarget = cardPlay.Target ?? CombatState?.HittableEnemies.FirstOrDefault();
        if (enemyTarget == null)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        if (enemyTarget.HasPower<ArtifactPower>())
        {
            await PowerCmd.Apply<ArtifactPower>(choiceContext, enemyTarget, -1, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<AdaptStrengthDownPower>(choiceContext, enemyTarget, DynamicVars["AdaptStrengthPower"].BaseValue, Owner.Creature, this);
        }
        await PowerCmd.Apply<AdaptStrengthPower>(choiceContext, Owner.Creature, DynamicVars["AdaptStrengthPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AdaptStrengthPower"].UpgradeValueBy(2m);
    }
}