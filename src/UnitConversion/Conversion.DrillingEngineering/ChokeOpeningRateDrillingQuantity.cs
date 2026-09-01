using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    /// <summary>
    /// The rate of change of the normalized opening of a drilling choke.
    /// </summary>
    public partial class ChokeOpeningRateDrillingQuantity : ProportionRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.0001;

        private static ChokeOpeningRateDrillingQuantity instance_ = null;
        public static new ChokeOpeningRateDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ChokeOpeningRateDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public ChokeOpeningRateDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "choke opening rate (drilling)", "choke opening speed", "choke rate", "ChokeRate" };
            ID = new Guid("0f7754c1-31cf-4312-a2fd-b25608da33da");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of choke opening rate in the drilling context is typically: " + MeaningfulPrecisionInSI + " 1/s (0.01 %/s)." + Environment.NewLine;
            Reset();
            UnitChoices.Add(ProportionRateOfChangeQuantity.Instance.GetUnitChoice(ProportionRateOfChangeQuantity.UnitChoicesEnum.ProportionPerSecond));
            UnitChoices.Add(ProportionRateOfChangeQuantity.Instance.GetUnitChoice(ProportionRateOfChangeQuantity.UnitChoicesEnum.PercentPerSecond));
            SemanticExample = GetSemanticExample();
        }
    }
}
