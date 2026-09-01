using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ElectricChargePerVolumeDrillingQuantity : ElectricChargePerVolumeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ElectricChargePerVolumeDrillingQuantity instance_ = null;
        public static new ElectricChargePerVolumeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ElectricChargePerVolumeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ElectricChargePerVolumeDrillingQuantity() : base()
        {
            Name = "ElectricChargePerVolumeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ElectricChargePerVolume (drilling)" };
            ID = new Guid("f19f63a8-49f0-4266-8643-a938d752764d");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
