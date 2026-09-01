using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class RadiantIntensityDrillingQuantity : RadiantIntensityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-2;

        private static RadiantIntensityDrillingQuantity instance_ = null;
        public static new RadiantIntensityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new RadiantIntensityDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public RadiantIntensityDrillingQuantity() : base()
        {
            Name = "RadiantIntensityDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "RadiantIntensity (drilling)" };
            ID = new Guid("ba285b42-b0a1-457a-8b9f-b09b3f8c257c");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
