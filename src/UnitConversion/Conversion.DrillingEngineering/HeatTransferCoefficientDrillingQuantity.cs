using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class HeatTransferCoefficientDrillingQuantity : HeatTransferCoefficientQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.0001;
        private static HeatTransferCoefficientDrillingQuantity instance_ = null;

        public static new HeatTransferCoefficientDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new HeatTransferCoefficientDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public HeatTransferCoefficientDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "heat transfer coefficient (drilling)" };
            ID = new Guid("c99547c5-b545-4060-bd82-eadc13772493");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of heat transfer coefficient in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + HeatTransferCoefficientQuantity.Instance.GetUnitChoice(HeatTransferCoefficientQuantity.UnitChoicesEnum.WattPerSquareMetrePerKelvin).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(HeatTransferCoefficientQuantity.Instance.GetUnitChoice(HeatTransferCoefficientQuantity.UnitChoicesEnum.WattPerSquareMetrePerKelvin));
            this.UnitChoices.Add(HeatTransferCoefficientQuantity.Instance.GetUnitChoice(HeatTransferCoefficientQuantity.UnitChoicesEnum.BritishThermalUnitPerHourPerSquareFootPerDegreeFahrenheit));
            SemanticExample = GetSemanticExample();
        }
    }
}
