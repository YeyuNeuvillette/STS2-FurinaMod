using Godot;
using STS2RitsuLib.Scaffolding.Content;
using Furina.Extensions;

namespace Furina.Characters.Furina;

public class FurinaPotionPool : TypeListPotionPoolModel
{
    public override string EnergyColorName => "furina";
    public override Color LabOutlineColor => Furina.Color;
    public override string BigEnergyIconPath => "charui/energy_furina_big.png".ImagePath();
    public override string TextEnergyIconPath => "charui/energy_furina.png".ImagePath();
}