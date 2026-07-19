using System;
using MegaCrit.Sts2.Core.Models;

namespace Furina.Api.Cards;

public static class CardUpgradeEventBus
{
    private static event Action<CardModel>? AnyCardUpgradedInternal;
    private static bool _isPropagating;

    public static int Generation { get; private set; }

    public static bool IsPropagating => _isPropagating;

    public static event Action<CardModel> AnyCardUpgraded
    {
        add => AnyCardUpgradedInternal += value;
        remove => AnyCardUpgradedInternal -= value;
    }

    public static IDisposable Subscribe(Action<CardModel> handler)
    {
        AnyCardUpgraded += handler;
        return new CardUpgradeSubscription(handler);
    }

    internal static void NotifyCardUpgraded(CardModel card)
    {
        if (_isPropagating)
            return;

        AnyCardUpgradedInternal?.Invoke(card);
    }

    internal static void PropagateUpgrade(Action upgradeAction)
    {
        if (_isPropagating)
        {
            upgradeAction();
            return;
        }

        _isPropagating = true;
        try
        {
            upgradeAction();
        }
        finally
        {
            _isPropagating = false;
        }
    }

    internal static void Clear()
    {
        AnyCardUpgradedInternal = null;
        _isPropagating = false;
        Generation++;
    }

    private sealed class CardUpgradeSubscription(Action<CardModel> handler) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            AnyCardUpgraded -= handler;
        }
    }
}
