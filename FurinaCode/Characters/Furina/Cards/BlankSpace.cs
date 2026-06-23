using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.ValueProps;
using Furina.Characters.Base;
using Furina.Characters.Furina.Patches;
using Furina.Characters.Furina.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class BlankSpace : BaseCard
{
    public override bool GainsBlock => true;

    private static readonly Dictionary<ulong, bool> _suppressCardRewardByPlayer = new();

    private static readonly RewardModifierDelegate _rewardModifier = ModifyRewards;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new List<DynamicVar>
        {
            new BlockVar(25m, ValueProp.Move)
        };

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new List<CardKeyword> { CardKeyword.Retain };

    public BlankSpace()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<BlankSpacePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);

        _suppressCardRewardByPlayer[Owner.NetId] = true;
        RewardModifierBus.Register(_rewardModifier);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(10m);
    }

    public override Task BeforeCombatStart()
    {
        if (Owner != null)
            _suppressCardRewardByPlayer.Remove(Owner.NetId);
        return Task.CompletedTask;
    }

    private static void ModifyRewards(in RewardModifyContext context)
    {
        if (!_suppressCardRewardByPlayer.TryGetValue(context.Player.NetId, out bool suppress) || !suppress)
            return;

        int removed = context.Rewards.RemoveAll(r => r is CardReward);
        if (removed > 0)
        {
            _suppressCardRewardByPlayer.Remove(context.Player.NetId);
            GD.Print($"[BlankSpace] ModifyRewards: removed {removed} card rewards for player NetId={context.Player.NetId}");
        }
    }
}