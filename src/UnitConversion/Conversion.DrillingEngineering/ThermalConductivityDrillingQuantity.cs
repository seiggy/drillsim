using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ThermalConductivityDrillingQuantity : ThermalConductivityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.01;
        private static ThermalConductivityDrillingQuantity instance_ = null;

        public static new ThermalConductivityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ThermalConductivityDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public ThermalConductivityDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "thermal conductivity (drilling)" };
            ID = new Guid("186eef6a-9da3-4b97-a6d0-d496bdf59321");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of thermal conductivity in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + ThermalConductivityQuantity.Instance.GetUnitChoice(ThermalConductivityQuantity.UnitChoicesEnum.WattPerMetreKelvin).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(ThermalConductivityQuantity.Instance.GetUnitChoice(ThermalConductivityQuantity.UnitChoicesEnum.WattPerMetreKelvin));
            this.UnitChoices.Add(ThermalConductivityQuantity.Instance.GetUnitChoice(ThermalConductivityQuantity.UnitChoicesEnum.CaloriePerMetreSecondDegreeCelsius));
            this.UnitChoices.Add(ThermalConductivityQuantity.Instance.GetUnitChoice(ThermalConductivityQuantity.UnitChoicesEnum.CaloriePerCentimetreSecondDegreeCelsius));
            this.UnitChoices.Add(ThermalConductivityQuantity.Instance.GetUnitChoice(ThermalConductivityQuantity.UnitChoicesEnum.BritishThermalUnitPerHourFootDegreeFahrenheit));
            this.UnitChoices.Add(ThermalConductivityQuantity.Instance.GetUnitChoice(ThermalConductivityQuantity.UnitChoicesEnum.BritishThermalUnitInchPerHourSquareFootDegreeFahrenheit));
            SemanticExample = GetSemanticExample();
        }
    }
}
