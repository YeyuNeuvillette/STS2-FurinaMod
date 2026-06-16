using Furina.Characters.Furina.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Furina.Characters.Furina.Powers;

public sealed class AdaptStrengthPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Adapt>();
    protected override bool IsVisibleInternal => false;
}