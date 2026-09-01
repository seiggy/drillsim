using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MolarVolumeDrillingQuantity : MolarVolumeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static MolarVolumeDrillingQuantity instance_ = null;
        public static new MolarVolumeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MolarVolumeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MolarVolumeDrillingQuantity() : base()
        {
            Name = "MolarVolumeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "MolarVolume (drilling)" };
            ID = new Guid("cd16afde-4301-4836-a1de-83f2af94f57a");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
