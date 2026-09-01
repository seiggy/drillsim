using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class RadianceDrillingQuantity : RadianceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-2;

        private static RadianceDrillingQuantity instance_ = null;
        public static new RadianceDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new RadianceDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public RadianceDrillingQuantity() : base()
        {
            Name = "RadianceDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "Radiance (drilling)" };
            ID = new Guid("adb860b7-aaf4-4e2f-bdee-632d07c7afb7");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
