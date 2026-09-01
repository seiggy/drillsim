using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class RateOfPenetrationDrillingQuantity : VelocityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 2.77778E-05;
        private static RateOfPenetrationDrillingQuantity instance_ = null;

        public static new RateOfPenetrationDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new RateOfPenetrationDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public RateOfPenetrationDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "rate of penetration (drilling)", "ROP" };
            ID = new Guid("c2581b41-944c-410b-9805-62c4b54de510");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of rate of penetration in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MetrePerMinute).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MetrePerSecond));
            this.UnitChoices.Add(VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MetrePerHour));
            this.UnitChoices.Add(VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.FootPerSecond));
            this.UnitChoices.Add(VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.FootPerHour));
            SemanticExample = GetSemanticExample();
        }
    }
}
