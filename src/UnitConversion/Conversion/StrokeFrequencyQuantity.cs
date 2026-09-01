using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;

namespace OSDC.UnitConversion.Conversion
{
    public partial class StrokeFrequencyQuantity : FrequencyQuantity
    {

        public override double? MeaningfulPrecisionInSI { get; } = 0.016666666666666666;
        private static StrokeFrequencyQuantity instance_ = null;

        public static new StrokeFrequencyQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new StrokeFrequencyQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public StrokeFrequencyQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "stroke frequency" };
            ID = new Guid("86fd37e4-3ebf-42ec-9eb2-1e65f7abf29e");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A stroke frequency is the number of stokes per unit time" + Environment.NewLine;
            DescriptionMD += @"The meaningful precision of a stroke frequency is typically: " + MeaningfulPrecisionInSI.ToString() + " " + FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.Hertz).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.Hertz));
            this.UnitChoices.Add(FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.Spm));
            SemanticExample = GetSemanticExample();
        }
    }
}
