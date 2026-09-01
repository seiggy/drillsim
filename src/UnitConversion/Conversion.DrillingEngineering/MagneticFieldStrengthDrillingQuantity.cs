using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MagneticFieldStrengthDrillingQuantity : MagneticFieldStrengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-2;

        private static MagneticFieldStrengthDrillingQuantity instance_ = null;
        public static new MagneticFieldStrengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MagneticFieldStrengthDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MagneticFieldStrengthDrillingQuantity() : base()
        {
            Name = "MagneticFieldStrengthDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "MagneticFieldStrength (drilling)" };
            ID = new Guid("f434e1a6-9f3b-40f8-b943-a46bf2f8511a");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
