using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class RelativeTemperaturePerPressureDrillingQuantity : RelativeTemperaturePerPressureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-9;

        private static RelativeTemperaturePerPressureDrillingQuantity instance_ = null;
        public static new RelativeTemperaturePerPressureDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new RelativeTemperaturePerPressureDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public RelativeTemperaturePerPressureDrillingQuantity() : base()
        {
            Name = "RelativeTemperaturePerPressureDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "RelativeTemperaturePerPressure (drilling)" };
            ID = new Guid("8ea41d0d-070a-4704-a83a-e30b3ac21282");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
