using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MolarHeatCapacityDrillingQuantity : MolarHeatCapacityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-1;

        private static MolarHeatCapacityDrillingQuantity instance_ = null;
        public static new MolarHeatCapacityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MolarHeatCapacityDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MolarHeatCapacityDrillingQuantity() : base()
        {
            Name = "MolarHeatCapacityDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "MolarHeatCapacity (drilling)" };
            ID = new Guid("79a9762b-6859-4ab8-a65f-09d41a811ad5");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
