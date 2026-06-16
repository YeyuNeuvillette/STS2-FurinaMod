using Godot;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;
using Furina.Extensions;

namespace Furina.Characters.Furina;

public class FurinaCardPool : TypeListCardPoolModel
{
    public override string Title => Furina.CharacterId;
    public override string EnergyColorName => "furina";
    public override string BigEnergyIconPath => "charui/energy_furina_big.png".ImagePath();
    public override string TextEnergyIconPath => "charui/energy_furina.png".ImagePath();
    public override Material? PoolFrameMaterial => MaterialUtils.CreateHsvShaderMaterial(0.637f, 0.651f, 0.506f);
    public override Color DeckEntryCardColor => Furina.Color;
    public override bool IsColorless => false;
}