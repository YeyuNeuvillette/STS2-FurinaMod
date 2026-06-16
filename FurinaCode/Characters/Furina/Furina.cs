using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Characters.Visuals.Definition;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Scaffolding.Visuals.Definition;
using Furina.Extensions;

namespace Furina.Characters.Furina;

[RegisterCharacter]
public class Furina : ModCharacterTemplate<FurinaCardPool, FurinaRelicPool, FurinaPotionPool>
{
    public override bool RequiresEpochAndTimeline => false;
    public const string CharacterId = "Furina";
    public static readonly Color Color = new("2D3C81");

    public override Color NameColor => Color;
    public override Color MapDrawingColor => new("#2D3C81");
    public override Color EnergyLabelOutlineColor => new(0.176f, 0.235f, 0.506f);
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 70;
    public override int StartingGold => 99;

    public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
        CharacterAssetProfiles.Ironclad(),
        new(
            Scenes: new(
                VisualsPath: "furina.tscn".CharacterScenePath(CharacterId),
                EnergyCounterPath: "furina_energy_counter.tscn".CharacterScenePath(CharacterId),
                MerchantAnimPath: "furina_merchant.tscn".CharacterScenePath(CharacterId),
                RestSiteAnimPath: null
            ),
            Ui: new(
                IconTexturePath: "furina_icon.png".CharacterImgPath(CharacterId),
                IconOutlineTexturePath: null,
                IconPath: "furina_icon.tscn".CharacterScenePath(CharacterId),
                CharacterSelectBgPath: "furina_bg.tscn".CharacterScenePath(CharacterId),
                CharacterSelectIconPath: "furina_char_select.png".CharacterImgPath(CharacterId),
                CharacterSelectLockedIconPath: "furina_char_select_locked.png".CharacterImgPath(CharacterId),
                CharacterSelectTransitionPath: null,
                MapMarkerPath: "furina_map.png".CharacterImgPath(CharacterId)
            ),
            Vfx: null,
            Audio: new(
                CharacterSelectSfx: null
            ),
            Multiplayer: new CharacterMultiplayerAssetSet(
                "multiplayer_hand_point.png".CharacterImgPath(CharacterId),
                "multiplayer_hand_rock.png".CharacterImgPath(CharacterId),
                "multiplayer_hand_paper.png".CharacterImgPath(CharacterId),
                "multiplayer_hand_scissors.png".CharacterImgPath(CharacterId))
        ));

    public override CharacterWorldProceduralVisualSet? WorldProceduralVisuals =>
        CharacterWorldProceduralVisualSetBuilder.Create()
            .RestSite(builder => builder
                .Single("overgrowth_loop", "furina_rest_site.png".CharacterImgPath(CharacterId))
                .Single("hive_loop", "furina_rest_site.png".CharacterImgPath(CharacterId))
                .Single("glory_loop", "furina_rest_site.png".CharacterImgPath(CharacterId)))
            .Build();

    public override string? PlaceholderCharacterId => "ironclad";
    public override float AttackAnimDelay => 0.15f;
    public override float CastAnimDelay => 0.25f;

    public override List<string> GetArchitectAttackVfx()
    {
        return
        [
            "vfx/vfx_attack_blunt",
            "vfx/vfx_heavy_blunt",
            "vfx/vfx_attack_slash",
            "vfx/vfx_bloody_impact",
            "vfx/vfx_rock_shatter"
        ];
     }

    protected override NCreatureVisuals? TryCreateCreatureVisuals()
    {
        return null;
    }
}