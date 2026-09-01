using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class VolumePerAreaDrillingQuantity : VolumePerAreaQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static VolumePerAreaDrillingQuantity instance_ = null;
        public static new VolumePerAreaDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new VolumePerAreaDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public VolumePerAreaDrillingQuantity() : base()
        {
            Name = "VolumePerAreaDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "VolumePerArea (drilling)" };
            ID = new Guid("bb3019dd-fc37-4d62-87bf-21d175f80eed");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
