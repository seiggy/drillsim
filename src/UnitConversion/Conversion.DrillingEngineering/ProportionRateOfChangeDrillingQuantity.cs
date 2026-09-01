using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ProportionRateOfChangeDrillingQuantity : ProportionRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ProportionRateOfChangeDrillingQuantity instance_ = null;
        public static new ProportionRateOfChangeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ProportionRateOfChangeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ProportionRateOfChangeDrillingQuantity() : base()
        {
            Name = "ProportionRateOfChangeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ProportionRateOfChange (drilling)" };
            ID = new Guid("1b775021-a7fa-4440-a2a1-ed7e1f64d237");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
