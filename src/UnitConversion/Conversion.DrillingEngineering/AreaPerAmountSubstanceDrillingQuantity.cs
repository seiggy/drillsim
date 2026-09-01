using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AreaPerAmountSubstanceDrillingQuantity : AreaPerAmountSubstanceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static AreaPerAmountSubstanceDrillingQuantity instance_ = null;
        public static new AreaPerAmountSubstanceDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new AreaPerAmountSubstanceDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public AreaPerAmountSubstanceDrillingQuantity() : base()
        {
            Name = "AreaPerAmountSubstanceDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "AreaPerAmountSubstance (drilling)" };
            ID = new Guid("b171b695-832d-4be4-893d-fc9064c2c727");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
