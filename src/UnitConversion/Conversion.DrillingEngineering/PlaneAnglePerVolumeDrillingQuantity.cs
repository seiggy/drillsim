using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class PlaneAnglePerVolumeDrillingQuantity : PlaneAnglePerVolumeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static PlaneAnglePerVolumeDrillingQuantity instance_ = null;
        public static new PlaneAnglePerVolumeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new PlaneAnglePerVolumeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public PlaneAnglePerVolumeDrillingQuantity() : base()
        {
            Name = "PlaneAnglePerVolumeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "PlaneAnglePerVolume (drilling)" };
            ID = new Guid("1840a92b-6d09-4b2d-8073-39cd425dcc99");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
