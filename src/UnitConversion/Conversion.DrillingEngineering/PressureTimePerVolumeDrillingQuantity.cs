using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class PressureTimePerVolumeDrillingQuantity : PressureTimePerVolumeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1.0;

        private static PressureTimePerVolumeDrillingQuantity instance_ = null;
        public static new PressureTimePerVolumeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new PressureTimePerVolumeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public PressureTimePerVolumeDrillingQuantity() : base()
        {
            Name = "PressureTimePerVolumeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "PressureTimePerVolume (drilling)" };
            ID = new Guid("448db949-aa69-4cd0-aa54-4a790b2685a3");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
