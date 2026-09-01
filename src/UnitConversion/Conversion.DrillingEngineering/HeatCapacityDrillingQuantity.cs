using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class HeatCapacityDrillingQuantity : HeatCapacityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-1;

        private static HeatCapacityDrillingQuantity instance_ = null;
        public static new HeatCapacityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new HeatCapacityDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public HeatCapacityDrillingQuantity() : base()
        {
            Name = "HeatCapacityDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "HeatCapacity (drilling)" };
            ID = new Guid("49a2d7ff-64bb-4269-94f7-5999f517feb5");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
