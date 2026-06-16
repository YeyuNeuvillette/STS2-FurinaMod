using System.Linq;
using System.Threading.Tasks;
using Furina.Characters.Furina.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;

namespace Furina;

public static class SufferingHolySonManager
{
    public static void Register()
    {
        RitsuLibFramework.SubscribeLifecycle<CombatStartingEvent>(OnCombatStarting);
        RitsuLibFramework.SubscribeLifecycle<ActEnteredEvent>(OnActEntered);
    }

    private static async void OnCombatStarting(CombatStartingEvent evt)
    {
        if (evt.CombatState is not CombatState combatState)
            return;

        if (evt.RunState is not RunState runState)
            return;

        if (evt.RunState.CurrentRoom?.RoomType != RoomType.Boss)
            return;

        foreach (var player in combatState.Players)
        {
            if (!FurinaRunData.SufferingHolySon.TryGet(runState, player.NetId, out var data))
                continue;

            if (data.BossMaxHpReduction <= 0)
                continue;

            var enemies = combatState.Enemies.ToList();
            foreach (var enemy in enemies)
            {
                int targetMaxHp = Math.Max(1, enemy.MaxHp - data.BossMaxHpReduction);
                await CreatureCmd.SetMaxHp(enemy, targetMaxHp);
            }
        }
    }

    private static void OnActEntered(ActEnteredEvent evt)
    {
        if (evt.RunState is not RunState runState)
            return;

        foreach (var player in runState.Players)
        {
            FurinaRunData.SufferingHolySon.Modify(runState, player.NetId, data =>
            {
                data.BossMaxHpReduction = 0;
            });
        }
    }
}