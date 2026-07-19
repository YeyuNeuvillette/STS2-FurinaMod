using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MinionLib.Action;
using Furina.Characters.Furina.Minions;
using Furina.Characters.Furina.RightClick.GameActions;
using STS2RitsuLib;
using STS2RitsuLib.RuntimeInput;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils;
using STS2RitsuLib.Utils.Persistence;

namespace Furina.Characters.Furina.Systems;

public sealed class FurinaHotkeySettings
{
    public string ChevalmarinKey { get; set; } = "C";
    public string UsherKey { get; set; } = "V";
    public string CrabalettaKey { get; set; } = "B";
}

public static class FurinaHotkeyService
{
    private const string SettingsKey = "hotkey_settings";
    private const string SettingsFileName = "hotkey_settings.json";

    private static IRuntimeHotkeyHandle? _chevalmarinHandle;
    private static IRuntimeHotkeyHandle? _usherHandle;
    private static IRuntimeHotkeyHandle? _crabalettaHandle;
    private static bool _initialized;

    private static string _currentChevalmarinKey = "C";
    private static string _currentUsherKey = "V";
    private static string _currentCrabalettaKey = "B";

    private static readonly I18N I18N = RitsuLibFramework.CreateLocalization(
        $"{MainFile.ModId}-Settings",
        pckFolders: [$"{MainFile.ResPath}/localization"]);

    public static void RegisterData()
    {
        var store = RitsuLibFramework.GetDataStore(MainFile.ModId);
        store.Register(
            key: SettingsKey,
            fileName: SettingsFileName,
            scope: SaveScope.Global,
            defaultFactory: () => new FurinaHotkeySettings(),
            autoCreateIfMissing: true);
    }

    public static void RegisterSettingsPage()
    {
        try
        {
            var chevalmarinBinding = new ModSettingsValueBinding<FurinaHotkeySettings, string>(
                MainFile.ModId,
                SettingsKey,
                SaveScope.Global,
                s => s.ChevalmarinKey,
                (s, v) =>
                {
                    s.ChevalmarinKey = v;
                    RebindChevalmarin(v);
                });

            var usherBinding = new ModSettingsValueBinding<FurinaHotkeySettings, string>(
                MainFile.ModId,
                SettingsKey,
                SaveScope.Global,
                s => s.UsherKey,
                (s, v) =>
                {
                    s.UsherKey = v;
                    RebindUsher(v);
                });

            var crabalettaBinding = new ModSettingsValueBinding<FurinaHotkeySettings, string>(
                MainFile.ModId,
                SettingsKey,
                SaveScope.Global,
                s => s.CrabalettaKey,
                (s, v) =>
                {
                    s.CrabalettaKey = v;
                    RebindCrabaletta(v);
                });

            RitsuLibFramework.RegisterModSettings(
                MainFile.ModId,
                page => page
                    .WithTitle(ModSettingsText.Literal("Furina"))
                    .WithModDisplayName(ModSettingsText.Literal("Furina"))
                    .AddSection("hotkeys", section => section
                        .WithTitle(ModSettingsText.Literal("Salon Member Hotkeys"))
                        .WithDescription(ModSettingsText.Literal(
                            "Customize hotkeys for triggering salon member actions during combat."))
                        .AddKeyBinding(
                            "chevalmarin_hotkey",
                            ModSettingsText.Literal("Surintendante Chevalmarin"),
                            chevalmarinBinding,
                            description: ModSettingsText.Literal(
                                "Hotkey for Chevalmarin's Pure Water Note action."))
                        .AddKeyBinding(
                            "usher_hotkey",
                            ModSettingsText.Literal("Gentilhomme Usher"),
                            usherBinding,
                            description: ModSettingsText.Literal(
                                "Hotkey for Usher's Hydro Barrage action."))
                        .AddKeyBinding(
                            "crabaletta_hotkey",
                            ModSettingsText.Literal("Mademoiselle Crabaletta"),
                            crabalettaBinding,
                            description: ModSettingsText.Literal(
                                "Hotkey for Crabaletta's Crab Shield action."))));

            MainFile.Logger.Info("[FurinaHotkey] Settings page registered successfully.");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[FurinaHotkey] Failed to register settings page: {ex}");
        }
    }

    public static void InitializeHotkeys()
    {
        if (_initialized)
        {
            MainFile.Logger.Info("[FurinaHotkey] Already initialized, skipping.");
            return;
        }

        try
        {
            var store = RitsuLibFramework.GetDataStore(MainFile.ModId);
            var settings = store.Get<FurinaHotkeySettings>(SettingsKey);

            _currentChevalmarinKey = settings.ChevalmarinKey;
            _currentUsherKey = settings.UsherKey;
            _currentCrabalettaKey = settings.CrabalettaKey;

            MainFile.Logger.Info(
                $"[FurinaHotkey] Initializing hotkeys: Chevalmarin={settings.ChevalmarinKey}, Usher={settings.UsherKey}, Crabaletta={settings.CrabalettaKey}");

            _chevalmarinHandle = RuntimeHotkeyService.Register(
                settings.ChevalmarinKey,
                () => TryTriggerMinionAction(typeof(SurintendanteChevalmarin)),
                new RuntimeHotkeyOptions
                {
                    Id = "furina_chevalmarin",
                    DisplayName = ModSettingsText.I18N(I18N, "furina.settings.hotkey.chevalmarin.label", "Surintendante Chevalmarin"),
                    Description = ModSettingsText.I18N(I18N, "furina.settings.hotkey.chevalmarin.description", "Hotkey for Chevalmarin's Pure Water Note action."),
                    Category = ModSettingsText.I18N(I18N, "furina.settings.page.title", "Furina"),
                    Purpose = "MinionAction",
                    MarkInputHandled = true,
                    SuppressWhenTextInputFocused = true,
                    SuppressWhenDevConsoleVisible = true,
                    DebugName = "Chevalmarin",
                });

            _usherHandle = RuntimeHotkeyService.Register(
                settings.UsherKey,
                () => TryTriggerMinionAction(typeof(GentilhommeUsher)),
                new RuntimeHotkeyOptions
                {
                    Id = "furina_usher",
                    DisplayName = ModSettingsText.I18N(I18N, "furina.settings.hotkey.usher.label", "Gentilhomme Usher"),
                    Description = ModSettingsText.I18N(I18N, "furina.settings.hotkey.usher.description", "Hotkey for Usher's Hydro Barrage action."),
                    Category = ModSettingsText.I18N(I18N, "furina.settings.page.title", "Furina"),
                    Purpose = "MinionAction",
                    MarkInputHandled = true,
                    SuppressWhenTextInputFocused = true,
                    SuppressWhenDevConsoleVisible = true,
                    DebugName = "Usher",
                });

            _crabalettaHandle = RuntimeHotkeyService.Register(
                settings.CrabalettaKey,
                () => TryTriggerMinionAction(typeof(MademoiselleCrabaletta)),
                new RuntimeHotkeyOptions
                {
                    Id = "furina_crabaletta",
                    DisplayName = ModSettingsText.I18N(I18N, "furina.settings.hotkey.crabaletta.label", "Mademoiselle Crabaletta"),
                    Description = ModSettingsText.I18N(I18N, "furina.settings.hotkey.crabaletta.description", "Hotkey for Crabaletta's Crab Shield action."),
                    Category = ModSettingsText.I18N(I18N, "furina.settings.page.title", "Furina"),
                    Purpose = "MinionAction",
                    MarkInputHandled = true,
                    SuppressWhenTextInputFocused = true,
                    SuppressWhenDevConsoleVisible = true,
                    DebugName = "Crabaletta",
                });

            _initialized = true;
            MainFile.Logger.Info("[FurinaHotkey] Hotkeys initialized successfully.");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[FurinaHotkey] Failed to initialize hotkeys: {ex}");
        }
    }

    public static void RebindChevalmarin(string bindingText)
    {
        _currentChevalmarinKey = bindingText;
        if (_chevalmarinHandle?.TryRebind(bindingText, out var normalized) == true)
        {
            _currentChevalmarinKey = normalized;
            MainFile.Logger.Info($"[FurinaHotkey] Rebound Chevalmarin to '{normalized}'.");
        }
    }

    public static void RebindUsher(string bindingText)
    {
        _currentUsherKey = bindingText;
        if (_usherHandle?.TryRebind(bindingText, out var normalized) == true)
        {
            _currentUsherKey = normalized;
            MainFile.Logger.Info($"[FurinaHotkey] Rebound Usher to '{normalized}'.");
        }
    }

    public static void RebindCrabaletta(string bindingText)
    {
        _currentCrabalettaKey = bindingText;
        if (_crabalettaHandle?.TryRebind(bindingText, out var normalized) == true)
        {
            _currentCrabalettaKey = normalized;
            MainFile.Logger.Info($"[FurinaHotkey] Rebound Crabaletta to '{normalized}'.");
        }
    }

    public static string? GetHotkeyForLocEntry(string locEntryKey)
    {
        if (locEntryKey.StartsWith("HYDRO_BARRAGE")) return _currentUsherKey;
        if (locEntryKey.StartsWith("CRAB_SHIELD")) return _currentCrabalettaKey;
        if (locEntryKey.StartsWith("PURE_WATER_NOTE")) return _currentChevalmarinKey;
        return null;
    }

    private static void TryTriggerMinionAction(Type minionType)
    {
        try
        {
            if (!CombatManager.Instance.IsInProgress || CombatManager.Instance.PlayerActionsDisabled)
                return;

            var queueSynchronizer = RunManager.Instance.ActionQueueSynchronizer;
            if (queueSynchronizer.CombatState != ActionSynchronizerCombatState.PlayPhase)
                return;

            var me = LocalContext.GetMe(CombatManager.Instance.DebugOnlyGetState());
            if (me == null) return;

            var pet = me.PlayerCombatState?.Pets
                .FirstOrDefault(p => p.Monster?.GetType() == minionType);
            if (pet == null || !pet.IsAlive) return;

            var combatState = pet.CombatState;
            if (combatState == null) return;

            var actionPower = pet.Powers
                .OfType<ActionModel>()
                .FirstOrDefault(a => !a.OnlyRespondIconClick && a.CanAct(combatState));
            if (actionPower == null) return;

            var creatureNode = NCombatRoom.Instance?.GetCreatureNode(pet);
            if (creatureNode == null) return;

            TaskHelper.RunSafely(TriggerActionAsync(creatureNode, actionPower));
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[FurinaHotkey] Error in TryTriggerMinionAction({minionType.Name}): {ex.Message}");
        }
    }

    private static async Task TriggerActionAsync(NCreature actorNode, ActionModel actionPower)
    {
        var actor = actorNode.Entity;
        var combatState = actor.CombatState;
        if (combatState == null) return;

        var targetType = actionPower.TargetType;

        if (targetType == TargetType.None)
        {
            EnqueueMinionAction(actor, null);
            return;
        }

        var validTargets = actionPower.GetValidTargets(combatState);
        if (validTargets.Count == 0) return;

        if (!targetType.IsSingleTarget())
        {
            EnqueueMinionAction(actor, null);
            return;
        }

        if (targetType == TargetType.Self)
        {
            EnqueueMinionAction(actor, null);
            return;
        }

        actionPower.StartPulsing();

        try
        {
            var startPosition = actorNode.Hitbox.GlobalPosition + actorNode.Hitbox.Size / 2f;
            NTargetManager.Instance.StartTargeting(targetType, startPosition, TargetMode.ClickMouseToTarget,
                () => !GodotObject.IsInstanceValid(actorNode) || !actor.IsAlive, null);

            var selectedNode = await NTargetManager.Instance.SelectionFinished();
            if (selectedNode is not NCreature targetNode) return;

            var target = targetNode.Entity;
            if (!actionPower.IsValidTarget(combatState, target)) return;

            EnqueueMinionAction(actor, target);
        }
        finally
        {
            actionPower.StopPulsing();
        }
    }

    private static void EnqueueMinionAction(Creature minion, Creature? target)
    {
        if (minion.PetOwner is not { } owner)
            return;

        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
            new MinionActGameAction(owner, minion, target));
    }
}
