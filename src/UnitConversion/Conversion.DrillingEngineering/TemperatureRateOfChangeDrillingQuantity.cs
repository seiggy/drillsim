using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class TemperatureRateOfChangeDrillingQuantity : TemperatureRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-4;

        private static TemperatureRateOfChangeDrillingQuantity instance_ = null;
        public static new TemperatureRateOfChangeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new TemperatureRateOfChangeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public TemperatureRateOfChangeDrillingQuantity() : base()
        {
            Name = "TemperatureRateOfChangeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "TemperatureRateOfChange (drilling)" };
            ID = new Guid("5ee25fd2-92c9-49df-b461-32c7ae99a5bd");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
