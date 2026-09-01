using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class PowerPerVolumeDrillingQuantity : PowerPerVolumeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-1;

        private static PowerPerVolumeDrillingQuantity instance_ = null;
        public static new PowerPerVolumeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new PowerPerVolumeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public PowerPerVolumeDrillingQuantity() : base()
        {
            Name = "PowerPerVolumeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "PowerPerVolume (drilling)" };
            ID = new Guid("7c60e428-55a4-4934-9dc9-6eb4fe1170be");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
