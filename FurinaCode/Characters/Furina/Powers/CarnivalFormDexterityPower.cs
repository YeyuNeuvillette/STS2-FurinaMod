using Furina.Characters.Furina.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Furina.Characters.Furina.Powers;

public sealed class CarnivalFormDexterityPower : TemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Card<CarnivalForm>();
    protected override bool IsVisibleInternal => false;
}