using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AreaPerVolumeDrillingQuantity : AreaPerVolumeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static AreaPerVolumeDrillingQuantity instance_ = null;
        public static new AreaPerVolumeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new AreaPerVolumeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public AreaPerVolumeDrillingQuantity() : base()
        {
            Name = "AreaPerVolumeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "AreaPerVolume (drilling)" };
            ID = new Guid("ac7714ab-ad3f-40b6-96cc-4ec2445c6d85");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
