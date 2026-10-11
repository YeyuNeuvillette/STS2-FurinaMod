using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Furina.Characters.Furina.Powers;

[RegisterPower]
public sealed class NeverEndingPower : FurinaPower
{
    private sealed class Data
    {
        public decimal BlockBeforeClear;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override object InitInternalData() => new Data();

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.Static(StaticHoverTip.Block), AgilityPowerRef.HoverTip()];

    public override bool ShouldClearBlock(Creature creature)
    {
        if (creature == Owner)
            GetInternalData<Data>().BlockBeforeClear = creature.Block;

        return true;
    }

    public override async Task AfterBlockCleared(Creature creature)
    {
        if (creature != Owner)
            return;

        var data = GetInternalData<Data>();
        var lostBlock = Math.Max(0m, data.BlockBeforeClear - creature.Block);
        data.BlockBeforeClear = 0m;
        if (lostBlock <= 0m)
            return;

        Flash();
        await AgilityPowerRef.Apply(
            new MegaCrit.Sts2.Core.GameActions.Multiplayer.ThrowingPlayerChoiceContext(),
            Owner,
            lostBlock,
            Owner,
            null);
    }
}
