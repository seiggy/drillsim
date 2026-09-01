using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class LightExposureDrillingQuantity : LightExposureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-2;

        private static LightExposureDrillingQuantity instance_ = null;
        public static new LightExposureDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new LightExposureDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public LightExposureDrillingQuantity() : base()
        {
            Name = "LightExposureDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "LightExposure (drilling)" };
            ID = new Guid("eecc107e-1b47-4fd3-9bec-87c4c6cedffa");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
