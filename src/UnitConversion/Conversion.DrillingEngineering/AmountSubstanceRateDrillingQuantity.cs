using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AmountSubstanceRateDrillingQuantity : AmountSubstanceRateQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static AmountSubstanceRateDrillingQuantity instance_ = null;
        public static new AmountSubstanceRateDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new AmountSubstanceRateDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public AmountSubstanceRateDrillingQuantity() : base()
        {
            Name = "AmountSubstanceRateDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "AmountSubstanceRate (drilling)" };
            ID = new Guid("ad80aa26-8878-44cf-be2c-2354de33adb0");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
