using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class IsobaricSpecificHeatCapacityDrillingQuantity : IsobaricSpecificHeatCapacityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.01;
        private static IsobaricSpecificHeatCapacityDrillingQuantity instance_ = null;

        public static new IsobaricSpecificHeatCapacityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new IsobaricSpecificHeatCapacityDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public IsobaricSpecificHeatCapacityDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "specific heat capacity (drilling)" };
            ID = new Guid("05c59293-4e3b-4fc0-b579-12c241109610");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of specific heat capacity in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.JoulePerKilogramKelvin).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.JoulePerKilogramKelvin));
            this.UnitChoices.Add(IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.JoulePerGramKelvin));
            this.UnitChoices.Add(IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.JoulePerGramDegreeCelsius));
            this.UnitChoices.Add(IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.CaloriePerGramDegreeCelsius));
            this.UnitChoices.Add(IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.BritishThermalUnitPerPoundDegreeFahrenheit));
            SemanticExample = GetSemanticExample();
        }
    }
}
