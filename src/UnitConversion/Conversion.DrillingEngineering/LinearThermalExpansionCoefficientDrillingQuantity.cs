using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class LinearThermalExpansionCoefficientDrillingQuantity : LinearThermalExpansionCoefficientQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static LinearThermalExpansionCoefficientDrillingQuantity instance_ = null;
        public static new LinearThermalExpansionCoefficientDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new LinearThermalExpansionCoefficientDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public LinearThermalExpansionCoefficientDrillingQuantity() : base()
        {
            Name = "LinearThermalExpansionCoefficientDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "LinearThermalExpansionCoefficient (drilling)" };
            ID = new Guid("86dc3231-9b24-40f5-874e-29c3e57cc5e8");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
