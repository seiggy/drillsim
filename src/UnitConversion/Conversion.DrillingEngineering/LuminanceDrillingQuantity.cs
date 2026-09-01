using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class LuminanceDrillingQuantity : LuminanceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-2;

        private static LuminanceDrillingQuantity instance_ = null;
        public static new LuminanceDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new LuminanceDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public LuminanceDrillingQuantity() : base()
        {
            Name = "LuminanceDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "Luminance (drilling)" };
            ID = new Guid("fe8f0404-af85-48f2-b818-efed9a1d1478");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
