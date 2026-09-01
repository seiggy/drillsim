using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ReciprocalElectricTensionDrillingQuantity : ReciprocalElectricTensionQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ReciprocalElectricTensionDrillingQuantity instance_ = null;
        public static new ReciprocalElectricTensionDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ReciprocalElectricTensionDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ReciprocalElectricTensionDrillingQuantity() : base()
        {
            Name = "ReciprocalElectricTensionDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ReciprocalElectricTension (drilling)" };
            ID = new Guid("b9b651ce-4330-495d-8db3-ad7f47e98f9e");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
