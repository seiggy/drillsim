using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion
{
    public partial class RotationalFrequencyRateOfChangeQuantity : FrequencyRateOfChangeQuantity
    {
        public override string TypicalSymbol { get; } = "null";
        private static RotationalFrequencyRateOfChangeQuantity instance_ = null;

        public static new RotationalFrequencyRateOfChangeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new RotationalFrequencyRateOfChangeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public RotationalFrequencyRateOfChangeQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "rotational frequency rate of change" };
            ID = new Guid("ed24a9f7-b70d-4f39-a992-241f25e1a77e");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += @"A rotation frequency rate of change is the time derivative of a rotation frequency." + Environment.NewLine;
            DescriptionMD += @"The meaningful precision of rotational frequency is typically: " + MeaningfulPrecisionInSI.ToString() + " " + FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.HertzPerSecond).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.HertzPerSecond));
            this.UnitChoices.Add(FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.RpmPerSecond));
            SemanticExample = GetSemanticExample();
        }

    }
}
