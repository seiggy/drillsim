using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AmountSubstancePerAreaRateOfChangeDrillingQuantity : AmountSubstancePerAreaRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-9;

        private static AmountSubstancePerAreaRateOfChangeDrillingQuantity instance_ = null;
        public static new AmountSubstancePerAreaRateOfChangeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new AmountSubstancePerAreaRateOfChangeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public AmountSubstancePerAreaRateOfChangeDrillingQuantity() : base()
        {
            Name = "AmountSubstancePerAreaRateOfChangeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "AmountSubstancePerAreaRateOfChange (drilling)" };
            ID = new Guid("792fdeed-b8d7-49cf-aa2b-c372f63fccea");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
