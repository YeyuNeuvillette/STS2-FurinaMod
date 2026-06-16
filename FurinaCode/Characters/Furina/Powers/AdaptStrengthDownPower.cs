using Furina.Characters.Furina.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Furina.Characters.Furina.Powers;

public sealed class AdaptStrengthDownPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Adapt>();
    protected override bool IsPositive => false;
    protected override bool IsVisibleInternal => false;
}