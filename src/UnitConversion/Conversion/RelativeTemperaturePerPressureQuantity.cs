using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class RelativeTemperaturePerPressureQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "dT/dp";
        public override string SIUnitName { get; } = "kelvin per pascal";
        public override string SIUnitLabelLatex { get; } = "\\frac{K}{Pa}";
        public override double LengthDimension { get; } = 1;
        public override double MassDimension { get; } = -1;
        public override double TimeDimension { get; } = 2;
        public override double TemperatureDimension { get; } = 1;

        private static RelativeTemperaturePerPressureQuantity instance_ = null;
        public static RelativeTemperaturePerPressureQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new RelativeTemperaturePerPressureQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new()
        {
            new UnitChoice { UnitName = "kelvin per pascal", UnitLabel = "K/Pa", ID = new Guid("edc02e4f-5fa4-4498-b54d-7f820f6e6c3b"), ConversionFactorFromSIFormula = "1.0/Factors.Unit", IsSI = true },
            new UnitChoice { UnitName = "kelvin per bar", UnitLabel = "K/bar", ID = new Guid("c3964170-0f1b-4510-b771-ca13f636a972"), ConversionFactorFromSIFormula = "Factors.Bar" },
            new UnitChoice { UnitName = "relative celsius per bar", UnitLabel = "°C/bar", ID = new Guid("46f08355-221a-4cdc-bdc9-fcaa6169aa1c"), ConversionFactorFromSIFormula = "Factors.Bar" },
            new UnitChoice { UnitName = "rankine per psi", UnitLabel = "°R/psi", ID = new Guid("1c7f0459-a0e9-4a2d-9368-fa5629681d7c"), ConversionFactorFromSIFormula = "Factors.PSI/Factors.FahrenheitSlope" }
        };

        public RelativeTemperaturePerPressureQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").First();
            UsualNames = new HashSet<string>() { "relative temperature per pressure", "RelativeTemperaturePerPressure" };
            ID = new Guid("bbaa29a4-af78-428d-895a-fc192d9ff1ca");
            DescriptionMD = "**Relative temperature per pressure** is a relative-temperature gradient with respect to pressure." + Environment.NewLine;
            DescriptionMD += "Its coherent SI unit is kelvin per pascal with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
