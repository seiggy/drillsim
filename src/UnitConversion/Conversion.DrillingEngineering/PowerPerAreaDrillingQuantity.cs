using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class PowerPerAreaDrillingQuantity : PowerPerAreaQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-1;

        private static PowerPerAreaDrillingQuantity instance_ = null;
        public static new PowerPerAreaDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new PowerPerAreaDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public PowerPerAreaDrillingQuantity() : base()
        {
            Name = "PowerPerAreaDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "PowerPerArea (drilling)" };
            ID = new Guid("1aa19612-81c3-48db-a794-757a497af235");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
