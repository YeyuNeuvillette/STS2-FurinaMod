using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using Furina.Characters.Furina;
using Furina.Characters.Furina.Powers;
using Furina.Characters.Furina.Targeting;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class Adapt : OusiaPneumaCard
{
    public override TargetType TargetType => ForceNoneState
        ? FurinaTargetTypes.AnyPlayer
        : base.TargetType;

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

        if (!ForceNoneState)
        {
            await ApplyLegacyNoArkheEffect(choiceContext, cardPlay);
            return;
        }

        var friendlyTarget = cardPlay.Target;
        if (friendlyTarget is not { IsAlive: true, IsPlayer: true, Side: CombatSide.Player })
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<AdaptStrengthPower>(choiceContext, friendlyTarget,
            DynamicVars["AdaptStrengthPower"].BaseValue, Owner.Creature, this);

        var enemyTarget = await ChooseEnemy(choiceContext);
        if (enemyTarget != null)
            await ApplyStrengthDown(choiceContext, enemyTarget);
    }

    private async Task ApplyLegacyNoArkheEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemyTarget = cardPlay.Target ?? CombatState?.HittableEnemies.FirstOrDefault();
        if (enemyTarget == null || Owner?.Creature == null)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await ApplyStrengthDown(choiceContext, enemyTarget);
        await PowerCmd.Apply<AdaptStrengthPower>(choiceContext, Owner.Creature,
            DynamicVars["AdaptStrengthPower"].BaseValue, Owner.Creature, this);
    }

    private async Task ApplyStrengthDown(PlayerChoiceContext choiceContext, Creature target)
    {
        if (target.HasPower<ArtifactPower>())
        {
            await PowerCmd.Apply<ArtifactPower>(choiceContext, target, -1m, Owner?.Creature, this);
            return;
        }

        await PowerCmd.Apply<AdaptStrengthDownPower>(choiceContext, target,
            DynamicVars["AdaptStrengthPower"].BaseValue, Owner?.Creature, this);
    }

    private async Task<Creature?> ChooseEnemy(PlayerChoiceContext choiceContext)
    {
        if (Owner == null || CombatState == null)
            return null;

        var candidates = CombatState.HittableEnemies
            .OrderBy(enemy => enemy.CombatId.GetValueOrDefault(uint.MaxValue))
            .ToList();
        if (candidates.Count == 0)
            return null;

        var synchronizer = RunManager.Instance.PlayerChoiceSynchronizer;
        uint choiceId = synchronizer.ReserveChoiceId(Owner);
        await choiceContext.SignalPlayerChoiceBegun(Owner, PlayerChoiceOptions.None);

        try
        {
            PlayerChoiceResult result;
            if (LocalContext.IsMe(Owner))
            {
                var ownerNode = NCombatRoom.Instance?.GetCreatureNode(Owner.Creature);
                if (ownerNode == null)
                {
                    result = PlayerChoiceResult.FromIndex(null);
                }
                else
                {
                    var startPosition = ownerNode.Hitbox.GlobalPosition + ownerNode.Hitbox.Size / 2f;
                    NTargetManager.Instance.StartTargeting(
                        TargetType.AnyEnemy,
                        startPosition,
                        TargetMode.ClickMouseToTarget,
                        () => CombatManager.Instance.IsOverOrEnding,
                        null);

                    var selectedNode = await NTargetManager.Instance.SelectionFinished();
                    var selected = (selectedNode as NCreature)?.Entity;
                    int index = selected == null ? -1 : candidates.IndexOf(selected);
                    result = PlayerChoiceResult.FromIndex(index >= 0 ? index : null);
                }

                synchronizer.SyncLocalChoice(Owner, choiceId, result);
            }
            else
            {
                result = await synchronizer.WaitForRemoteChoice(Owner, choiceId);
            }

            int? selectedIndex = result.AsIndexOrNull();
            return selectedIndex is >= 0 && selectedIndex < candidates.Count
                ? candidates[selectedIndex.Value]
                : null;
        }
        finally
        {
            await choiceContext.SignalPlayerChoiceEnded();
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AdaptStrengthPower"].UpgradeValueBy(2m);
    }
}
