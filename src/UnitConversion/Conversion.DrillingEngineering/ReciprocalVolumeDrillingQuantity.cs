using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ReciprocalVolumeDrillingQuantity : ReciprocalVolumeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ReciprocalVolumeDrillingQuantity instance_ = null;
        public static new ReciprocalVolumeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ReciprocalVolumeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ReciprocalVolumeDrillingQuantity() : base()
        {
            Name = "ReciprocalVolumeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ReciprocalVolume (drilling)" };
            ID = new Guid("48684b06-7e68-49e3-b81d-95d7600dd4ea");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
