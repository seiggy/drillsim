using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class VolumePerAngleDrillingQuantity : VolumePerAngleQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static VolumePerAngleDrillingQuantity instance_ = null;
        public static new VolumePerAngleDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new VolumePerAngleDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public VolumePerAngleDrillingQuantity() : base()
        {
            Name = "VolumePerAngleDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "VolumePerAngle (drilling)" };
            ID = new Guid("5c5a6df0-3657-4d0e-acb5-0bbdec7699af");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
