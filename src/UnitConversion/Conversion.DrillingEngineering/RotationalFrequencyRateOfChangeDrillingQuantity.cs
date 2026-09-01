using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class RotationalFrequencyRateOfChangeDrillingQuantity : RotationalFrequencyRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.016666666666666666;

        private static RotationalFrequencyRateOfChangeDrillingQuantity instance_ = null;

        public static new RotationalFrequencyRateOfChangeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new RotationalFrequencyRateOfChangeDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public RotationalFrequencyRateOfChangeDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "rotational frequency rate of change (drilling)" };
            ID = new Guid("4950170a-7882-4673-9d27-3402dbbca2bb");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of rotational frequency rate of change in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + RotationalFrequencyRateOfChangeQuantity.Instance.GetUnitChoice(RotationalFrequencyRateOfChangeQuantity.UnitChoicesEnum.HertzPerSecond).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(RotationalFrequencyRateOfChangeQuantity.Instance.GetUnitChoice(RotationalFrequencyRateOfChangeQuantity.UnitChoicesEnum.HertzPerSecond));
            this.UnitChoices.Add(RotationalFrequencyRateOfChangeQuantity.Instance.GetUnitChoice(RotationalFrequencyRateOfChangeQuantity.UnitChoicesEnum.RpmPerSecond));
            SemanticExample = GetSemanticExample();
        }
    }
}
