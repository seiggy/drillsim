using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ForcePerVolumeDrillingQuantity : ForcePerVolumeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ForcePerVolumeDrillingQuantity instance_ = null;
        public static new ForcePerVolumeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ForcePerVolumeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ForcePerVolumeDrillingQuantity() : base()
        {
            Name = "ForcePerVolumeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ForcePerVolume (drilling)" };
            ID = new Guid("662fe22f-7ad0-4f82-ac6f-ecddb3361603");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
