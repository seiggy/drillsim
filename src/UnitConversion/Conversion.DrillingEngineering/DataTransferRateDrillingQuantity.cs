using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class DataTransferRateDrillingQuantity : DataTransferRateQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1.0;

        private static DataTransferRateDrillingQuantity instance_ = null;
        public static new DataTransferRateDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new DataTransferRateDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public DataTransferRateDrillingQuantity() : base()
        {
            Name = "DataTransferRateDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "DataTransferRate (drilling)" };
            ID = new Guid("7d97ec45-85eb-4dc9-9207-fe945e6f945f");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
