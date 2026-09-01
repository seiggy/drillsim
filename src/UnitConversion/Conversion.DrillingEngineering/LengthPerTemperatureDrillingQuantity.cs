using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class LengthPerTemperatureDrillingQuantity : LengthPerTemperatureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static LengthPerTemperatureDrillingQuantity instance_ = null;
        public static new LengthPerTemperatureDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new LengthPerTemperatureDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public LengthPerTemperatureDrillingQuantity() : base()
        {
            Name = "LengthPerTemperatureDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "LengthPerTemperature (drilling)" };
            ID = new Guid("181308b0-d102-4276-91a3-b6c08223b204");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
