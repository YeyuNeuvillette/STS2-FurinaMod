using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Furina.Characters.Base;
using Furina.Characters.Furina.Patches;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Cards;

[RegisterCard(typeof(FurinaCardPool))]
public sealed class BackupDancer : BaseCard
{
    public override bool GainsBlock => true;

    public override int MaxUpgradeLevel => int.MaxValue;

    private int _subscribedGeneration = -1;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new List<DynamicVar>
        {
            new BlockVar(3m, ValueProp.Move),
            new RepeatVar(1)
        };

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new List<CardKeyword> { CardKeyword.Exhaust };

    public BackupDancer()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }
    }

    public override void AfterCreated()
    {
        TrySubscribe();
    }

    internal void TrySubscribe()
    {
        if (_subscribedGeneration == CardUpgradeEventBus.Generation)
            return;

        _subscribedGeneration = CardUpgradeEventBus.Generation;
        CardUpgradeEventBus.AnyCardUpgraded += OnAnyCardUpgraded;
    }

    internal static void SubscribeAllForCombat(IReadOnlyList<Player> players)
    {
        foreach (var player in players)
        {
            foreach (var pile in player.Piles)
            {
                foreach (var card in pile.Cards)
                {
                    if (card is BackupDancer dancer)
                        dancer.TrySubscribe();
                }
            }
        }
    }

    private void OnAnyCardUpgraded(CardModel upgradedCard)
    {
        if (ReferenceEquals(upgradedCard, this))
            return;

        if (!IsUpgradable)
            return;

        if (IsInCombat != upgradedCard.IsInCombat)
            return;

        if (Owner != upgradedCard.Owner)
            return;

        CardUpgradeEventBus.PropagateUpgrade(() => CardCmd.Upgrade(this));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}