using Furina.Characters.Furina.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Furina.Characters.Furina.Powers;

public sealed class WarmupStrengthPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Warmup>();
    protected override bool IsVisibleInternal => false;

}