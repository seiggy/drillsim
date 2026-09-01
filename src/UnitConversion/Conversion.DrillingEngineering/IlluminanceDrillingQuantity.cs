using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class IlluminanceDrillingQuantity : IlluminanceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-2;

        private static IlluminanceDrillingQuantity instance_ = null;
        public static new IlluminanceDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new IlluminanceDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public IlluminanceDrillingQuantity() : base()
        {
            Name = "IlluminanceDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "Illuminance (drilling)" };
            ID = new Guid("32a67147-7ae9-4cbe-8893-641dbb65b66d");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
