using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class BlockVelocityDrillingQuantity : VelocityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.01;
        private static BlockVelocityDrillingQuantity instance_ = null;

        public static new BlockVelocityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new BlockVelocityDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public BlockVelocityDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "block velocity (drilling)" };
            ID = new Guid("82a2b5fc-9edd-45ea-80cb-1cd46d516fdb");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of block velocity in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MetrePerSecond).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MetrePerSecond));
            this.UnitChoices.Add(VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MetrePerMinute));
            this.UnitChoices.Add(VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MetrePerHour));
            this.UnitChoices.Add(VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.FootPerHour));
            this.UnitChoices.Add(VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.FootPerMinute));
            this.UnitChoices.Add(VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.FootPerSecond));
            SemanticExample = GetSemanticExample();
        }
    }
}
