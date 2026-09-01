using System;
using System.Collections.Generic;
using System.Linq;
using OSDC.UnitConversion.Conversion;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ShockRateQuantity : FrequencyQuantity
    {

        public override double? MeaningfulPrecisionInSI { get; } = 0.016666666666666666;
        private static ShockRateQuantity instance_ = null;

        public static new ShockRateQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ShockRateQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public ShockRateQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "shock rate" };
            ID = new Guid("0076d96f-bfc3-4f98-8541-4fd12e4bcbff");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A shock rate is the number of shocks per unit time." + Environment.NewLine;
            DescriptionMD += @"The meaningful precision of shock rate is typically: " + MeaningfulPrecisionInSI.ToString() + " " + FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.HertzPerSecond).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.Hertz));
            this.UnitChoices.Add(FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.ShockPerMinute));
            this.UnitChoices.Add(FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.ShockPerHour));
            SemanticExample = GetSemanticExample();
        }
    }
}
