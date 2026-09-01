using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ElectricConductivityDrillingQuantity : ElectricConductivityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ElectricConductivityDrillingQuantity instance_ = null;
        public static new ElectricConductivityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ElectricConductivityDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ElectricConductivityDrillingQuantity() : base()
        {
            Name = "ElectricConductivityDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ElectricConductivity (drilling)" };
            ID = new Guid("0c975e7f-442f-426d-9f0a-5f85dc9f281e");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
