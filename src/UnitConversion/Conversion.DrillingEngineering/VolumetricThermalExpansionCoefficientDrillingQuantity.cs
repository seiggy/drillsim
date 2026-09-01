using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class VolumetricThermalExpansionCoefficientDrillingQuantity : VolumetricThermalExpansionCoefficientQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static VolumetricThermalExpansionCoefficientDrillingQuantity instance_ = null;
        public static new VolumetricThermalExpansionCoefficientDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new VolumetricThermalExpansionCoefficientDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public VolumetricThermalExpansionCoefficientDrillingQuantity() : base()
        {
            Name = "VolumetricThermalExpansionCoefficientDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "VolumetricThermalExpansionCoefficient (drilling)" };
            ID = new Guid("42000cd2-8794-4531-bf7c-18921bb87cd3");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
