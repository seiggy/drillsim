using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class VolumePerPressureDrillingQuantity : VolumePerPressureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static VolumePerPressureDrillingQuantity instance_ = null;
        public static new VolumePerPressureDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new VolumePerPressureDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public VolumePerPressureDrillingQuantity() : base()
        {
            Name = "VolumePerPressureDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "VolumePerPressure (drilling)" };
            ID = new Guid("6f4a2e50-e836-4bf0-9ed9-d3e8d0097b74");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
