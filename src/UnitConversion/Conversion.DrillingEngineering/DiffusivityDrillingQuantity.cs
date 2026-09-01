using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class DiffusivityDrillingQuantity : DiffusivityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static DiffusivityDrillingQuantity instance_ = null;
        public static new DiffusivityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new DiffusivityDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public DiffusivityDrillingQuantity() : base()
        {
            Name = "DiffusivityDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "Diffusivity (drilling)" };
            ID = new Guid("b7fa0754-5a2a-4e1f-a01a-2d36adc5d2c9");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
