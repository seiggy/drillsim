using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class TimePerVolumeDrillingQuantity : TimePerVolumeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static TimePerVolumeDrillingQuantity instance_ = null;
        public static new TimePerVolumeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new TimePerVolumeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public TimePerVolumeDrillingQuantity() : base()
        {
            Name = "TimePerVolumeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "TimePerVolume (drilling)" };
            ID = new Guid("0c8154be-c4f0-4dae-a19b-deefc269d18a");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
