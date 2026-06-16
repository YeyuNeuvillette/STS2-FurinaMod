using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.CardPiles;

namespace Furina;

public static class ScriptPileRegistration
{
    private static bool _scriptCardCreatedThisCombat;
    private static PileType? _scriptPileType;
    
    public static void Register()
    {
        var registry = ModCardPileRegistry.For(MainFile.ModId);
        
        var spec = new ModCardPileSpec
        {
            Scope = ModCardPileScope.CombatOnly,
            Style = ModCardPileUiStyle.BottomLeft,
            Anchor = ModCardPileAnchor.Default,
            IconPath = "res://Furina/images/ui/script_pile_registration.png",
            VisibleWhen = context => 
            {
                if (context.Player?.PlayerCombatState == null)
                    return false;
                    
                return _scriptCardCreatedThisCombat;
            }
        };
        
        registry.RegisterOwned("SCRIPT", spec);
    }
    
    public static void OnScriptCardCreated()
    {
        _scriptCardCreatedThisCombat = true;
        GD.Print($"[ScriptPileRegistration] Script card created, visible: {_scriptCardCreatedThisCombat}");
    }
    
    public static void OnCombatEnd()
    {
        _scriptCardCreatedThisCombat = false;
        GD.Print($"[ScriptPileRegistration] Combat ended, visible: {_scriptCardCreatedThisCombat}");
    }
    
    public static PileType GetScriptPileType()
    {
        if (!_scriptPileType.HasValue)
        {
            if (ModCardPileRegistry.TryGet("FURINA_CARDPILE_SCRIPT", out var definition))
            {
                _scriptPileType = definition.PileType;
                GD.Print($"[ScriptPileRegistration] Script pile type: {_scriptPileType.Value}");
            }
            else
            {
                _scriptPileType = PileType.Discard;
                GD.Print($"[ScriptPileRegistration] Script pile type not found, defaulting to Discard");
            }
        }
        return _scriptPileType.Value;
    }
}