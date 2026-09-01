using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ElectricConductanceDrillingQuantity : ElectricConductanceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ElectricConductanceDrillingQuantity instance_ = null;
        public static new ElectricConductanceDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ElectricConductanceDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ElectricConductanceDrillingQuantity() : base()
        {
            Name = "ElectricConductanceDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ElectricConductance (drilling)" };
            ID = new Guid("4b6f7948-4698-4ad0-b0ba-b39b49bde8f7");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
