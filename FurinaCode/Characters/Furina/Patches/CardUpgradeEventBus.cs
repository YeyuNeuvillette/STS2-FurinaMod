using System;
using MegaCrit.Sts2.Core.Models;

namespace Furina.Characters.Furina.Patches;

[Obsolete("Use Furina.Api.Cards.CardUpgradeEventBus instead.")]
public static class CardUpgradeEventBus
{
    internal static int Generation => global::Furina.Api.Cards.CardUpgradeEventBus.Generation;

    public static event Action<CardModel> AnyCardUpgraded
    {
        add => global::Furina.Api.Cards.CardUpgradeEventBus.AnyCardUpgraded += value;
        remove => global::Furina.Api.Cards.CardUpgradeEventBus.AnyCardUpgraded -= value;
    }

    internal static void NotifyCardUpgraded(CardModel card)
    {
        global::Furina.Api.Cards.CardUpgradeEventBus.NotifyCardUpgraded(card);
    }

    internal static void PropagateUpgrade(Action upgradeAction)
    {
        global::Furina.Api.Cards.CardUpgradeEventBus.PropagateUpgrade(upgradeAction);
    }

    internal static void Clear()
    {
        global::Furina.Api.Cards.CardUpgradeEventBus.Clear();
    }
}
