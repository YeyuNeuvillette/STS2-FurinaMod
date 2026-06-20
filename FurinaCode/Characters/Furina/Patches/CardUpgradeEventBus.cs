using System;
using MegaCrit.Sts2.Core.Models;

namespace Furina.Characters.Furina.Patches;

public static class CardUpgradeEventBus
{
    private static event Action<CardModel>? _anyCardUpgraded;
    private static bool _isPropagating;

    internal static int Generation { get; private set; }

    public static event Action<CardModel> AnyCardUpgraded
    {
        add => _anyCardUpgraded += value;
        remove => _anyCardUpgraded -= value;
    }

    internal static void NotifyCardUpgraded(CardModel card)
    {
        if (_isPropagating)
            return;
        _anyCardUpgraded?.Invoke(card);
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
        _anyCardUpgraded = null;
        _isPropagating = false;
        Generation++;
    }
}