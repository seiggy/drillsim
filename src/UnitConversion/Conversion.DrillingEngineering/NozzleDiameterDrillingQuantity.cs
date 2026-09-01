using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class NozzleDiameterDrillingQuantity : LengthSmallQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.0001;

        private static NozzleDiameterDrillingQuantity instance_ = null;

        public static new NozzleDiameterDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new NozzleDiameterDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public NozzleDiameterDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "nozzle diameter (drilling)" };
            ID = new Guid("1fbe2318-851d-4fd7-b711-9c23ffe544c7");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of nozzle diameter in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Metre).UnitLabel + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
