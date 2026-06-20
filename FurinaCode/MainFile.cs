using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Audio;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Keywords;
using MinionLib.RightClick;
using Furina.Characters.Base;
using Furina.Characters.Furina.Cards;
using Furina.Characters.Furina.Patches;
using Furina.Characters.Furina.Powers;
using Furina.Characters.Furina.RightClick;
using Furina.Characters.Furina.Relics;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;
using MegaCrit.Sts2.Core.Rooms;

namespace Furina;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "Furina";
    public const string ResPath = $"res://{ModId}";

    private const string FurinaFmodBankPath = "res://Furina/audios/Furina.bank";
    private const string FurinaFmodGuidsPath = "res://Furina/audios/GUIDs.txt";

    public static Logger Logger { get; } = new(ModId, LogType.Generic);

    private static IDisposable? _fmodBankDeferredInitSubscription;

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        Harmony harmony = new(ModId);
        harmony.PatchAll();

        using (RitsuLibFramework.BeginModDataRegistration(ModId))
        {
            FurinaRunData.Register();
        }

        RegisterKeywords();
        RegisterRightClickHandlers();
        RegisterConditionalPowers();
        ScriptPileRegistration.Register();
        SubscribeToCombatEvents();
        SufferingHolySonManager.Register();
        RitsuLibFramework.SubscribeLifecycle<CombatStartingEvent>(evt =>
        {
            MovementCard.ResetCombatState();
            MinionStorage.ClearAll();
            if (evt.CombatState is CombatState combatState)
                BackupDancer.SubscribeAllForCombat(combatState.Players);
        });
        RitsuLibFramework.SubscribeLifecycle<SideTurnStartingEvent>(evt =>
        {
            if (evt.Side == CombatSide.Player)
            {
                MinionStorage.IncrementTurnCount();
                MinionStorage.ResetSalonSolitaire();
            }
        });
        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<Announcer, WaterAndJustice>(ModId);
        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<SoloistsSolicitation, LetThePeopleRejoice>(ModId);

        Logger.Info("Furina mod initialized successfully");
    }

    private static void SubscribeToCombatEvents()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.CombatEnded += OnCombatEnded;
        }
    }

    private static void OnCombatEnded(CombatRoom _)
    {
        ScriptPileRegistration.OnCombatEnd();
        CardUpgradeEventBus.Clear();
    }

    private static void RegisterKeywords()
    {
        var keywords = RitsuLibFramework.GetKeywordRegistry(ModId);
        keywords.RegisterCardKeywordOwnedByLocNamespace(
            "arkhe",
            iconPath: null,
            cardDescriptionPlacement: ModKeywordCardDescriptionPlacement.None,
            includeInCardHoverTip: true);
        keywords.RegisterCardKeywordOwnedByLocNamespace(
            "invite",
            iconPath: null,
            cardDescriptionPlacement: ModKeywordCardDescriptionPlacement.None,
            includeInCardHoverTip: true);
        keywords.RegisterCardKeywordOwnedByLocNamespace(
            "ousia_pneuma",
            iconPath: null,
            cardDescriptionPlacement: ModKeywordCardDescriptionPlacement.BeforeCardDescription,
            includeInCardHoverTip: true);
        keywords.RegisterCardKeywordOwnedByLocNamespace(
            "trope",
            iconPath: null,
            cardDescriptionPlacement: ModKeywordCardDescriptionPlacement.BeforeCardDescription,
            includeInCardHoverTip: true);
        keywords.RegisterCardKeywordOwnedByLocNamespace(
            "movement",
            iconPath: null,
            cardDescriptionPlacement: ModKeywordCardDescriptionPlacement.None,
            includeInCardHoverTip: true);
        keywords.RegisterCardKeywordOwnedByLocNamespace(
            "script",
            iconPath: null,
            cardDescriptionPlacement: ModKeywordCardDescriptionPlacement.None,
            includeInCardHoverTip: true);
    }

    private static void RegisterConditionalPowers()
    {
        if (!AgilityPowerRef.IsNeuvilletteLoaded)
        {
            RitsuLibFramework.GetContentRegistry(ModId).RegisterPower<AgilityPower>();
            Logger.Info("Registered Furina's AgilityPower (Neuvillette not detected)");
        }
        else
        {
            Logger.Info("Neuvillette detected - using its AgilityPower implementation");
        }
    }

    private static void RegisterRightClickHandlers()
    {
    }

    private static void QueueFurinaFmodAfterDeferredInitialization()
    {
        if (_fmodBankDeferredInitSubscription != null)
            return;

        _fmodBankDeferredInitSubscription =
            RitsuLibFramework.SubscribeLifecycle<DeferredInitializationCompletedEvent>(_ =>
            {
                try
                {
                    if (FmodStudioServer.TryGet() is null)
                    {
                        Logger.Warn("FmodServer singleton missing; skipped Furina FMOD bank load.");
                        return;
                    }

                    if (!FmodStudioServer.TryLoadBank(FurinaFmodBankPath))
                    {
                        Logger.Warn($"Failed to load FMOD bank: {FurinaFmodBankPath}");
                        return;
                    }

                    FmodStudioServer.TryWaitForAllLoads();

                    if (!FmodStudioServer.TryLoadStudioGuidMappings(FurinaFmodGuidsPath))
                        Logger.Warn($"Failed to load FMOD guid map: {FurinaFmodGuidsPath}");
                }
                finally
                {
                    _fmodBankDeferredInitSubscription?.Dispose();
                    _fmodBankDeferredInitSubscription = null;
                }
            });
    }
}