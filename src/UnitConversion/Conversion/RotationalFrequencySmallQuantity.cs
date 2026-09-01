using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion
{
    public partial class RotationalFrequencySmallQuantity : RotationalFrequencyQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1.6666e-4;

        private static RotationalFrequencySmallQuantity instance_ = null;

        public static new RotationalFrequencySmallQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new RotationalFrequencySmallQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public RotationalFrequencySmallQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "rotational frequency (small)", "rotary speed (small)" };
            ID = new Guid("b7ab1664-3d56-4ae5-842a-e4d6d0475ef9");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += @"The meaningful precision of small rotational frequency is typically: " + MeaningfulPrecisionInSI.ToString() + " " + FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.Hertz).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.Hertz));
            this.UnitChoices.Add(FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.Rpm));
            SemanticExample = GetSemanticExample();
        }

    }
}
