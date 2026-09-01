using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class PressurePerVolumeDrillingQuantity : PressurePerVolumeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1.0;

        private static PressurePerVolumeDrillingQuantity instance_ = null;
        public static new PressurePerVolumeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new PressurePerVolumeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public PressurePerVolumeDrillingQuantity() : base()
        {
            Name = "PressurePerVolumeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "PressurePerVolume (drilling)" };
            ID = new Guid("ab4635a5-2d0a-4c37-8a74-148c5db05d2d");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
