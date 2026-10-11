using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Utils.HarmonyIl;

namespace Furina.Characters.Furina.Cards;

/// <summary>
/// Identifies cards whose play effect invokes the game's card-draw commands.
/// This is based on behavior rather than localized description text, so it also
/// works for vanilla cards and cards supplied by other mods in any language.
/// </summary>
internal static class CardDrawEffectDetector
{
    private static readonly ConcurrentDictionary<Type, bool> Cache = new();

    public static bool HasCardDrawEffect(CardModel card)
    {
        return Cache.GetOrAdd(card.GetType(), AnalyzeCardType);
    }

    private static bool AnalyzeCardType(Type cardType)
    {
        var onPlay = cardType.GetMethod(
            "OnPlay",
            BindingFlags.Instance | BindingFlags.NonPublic);

        return CallsCardDraw(onPlay, cardType, new HashSet<MethodBase>());
    }

    private static bool CallsCardDraw(MethodBase? method, Type cardType, HashSet<MethodBase> visited)
    {
        if (method == null || !visited.Add(method))
            return false;

        if (method is MethodInfo methodInfo)
        {
            var asyncStateMachine = methodInfo.GetCustomAttribute<AsyncStateMachineAttribute>();
            var moveNext = asyncStateMachine?.StateMachineType.GetMethod(
                nameof(IAsyncStateMachine.MoveNext),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (moveNext != null && CallsCardDraw(moveNext, cardType, visited))
                return true;
        }

        foreach (var calledMethod in GetCalledMethods(method))
        {
            if (IsCardDrawCommand(calledMethod))
                return true;

            var resolvedMethod = ResolveCardOverride(calledMethod, cardType);
            if (resolvedMethod != null && CallsCardDraw(resolvedMethod, cardType, visited))
                return true;

            if (IsCardOwnedHelper(calledMethod, cardType)
                && CallsCardDraw(calledMethod, cardType, visited))
                return true;
        }

        return false;
    }

    private static bool IsCardDrawCommand(MethodBase method)
    {
        return (method.DeclaringType == typeof(CardPileCmd)
                && (method.Name == nameof(CardPileCmd.Draw)
                    || method.Name == nameof(CardPileCmd.DrawWithoutBlockingOnOtherPlayers)))
            || (method.DeclaringType == typeof(CardCmd)
                && method.Name == nameof(CardCmd.DiscardAndDraw));
    }

    private static MethodInfo? ResolveCardOverride(MethodBase method, Type cardType)
    {
        if (method is not MethodInfo methodInfo
            || !methodInfo.IsVirtual
            || methodInfo.DeclaringType == null
            || !methodInfo.DeclaringType.IsAssignableFrom(cardType))
            return null;

        var parameterTypes = methodInfo.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
        var overrideMethod = cardType.GetMethod(
            methodInfo.Name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: parameterTypes,
            modifiers: null);

        return overrideMethod != methodInfo ? overrideMethod : null;
    }

    private static bool IsCardOwnedHelper(MethodBase method, Type cardType)
    {
        var declaringType = method.DeclaringType;
        if (declaringType == null || declaringType.Assembly != cardType.Assembly)
            return false;

        return declaringType == cardType
            || declaringType.DeclaringType == cardType
            || declaringType.IsAssignableFrom(cardType);
    }

    private static IEnumerable<MethodBase> GetCalledMethods(MethodBase method)
    {
        foreach (var instruction in PatchProcessor.GetCurrentInstructions(method))
        {
            if (HarmonyIl.TryGetCalledMethod(instruction, out var calledMethod))
                yield return calledMethod;
        }
    }
}
