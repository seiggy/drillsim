using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AmountSubstancePerAreaDrillingQuantity : AmountSubstancePerAreaQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static AmountSubstancePerAreaDrillingQuantity instance_ = null;
        public static new AmountSubstancePerAreaDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new AmountSubstancePerAreaDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public AmountSubstancePerAreaDrillingQuantity() : base()
        {
            Name = "AmountSubstancePerAreaDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "AmountSubstancePerArea (drilling)" };
            ID = new Guid("26cae8a6-16e5-44a7-9bad-dd141ac93740");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
