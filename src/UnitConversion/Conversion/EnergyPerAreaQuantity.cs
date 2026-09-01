using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class EnergyPerAreaQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "E/A";
        public override string SIUnitName { get; } = "joule per square metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{J}{m^2}";
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;

        private static EnergyPerAreaQuantity instance_ = null;
        public static EnergyPerAreaQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new EnergyPerAreaQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new()
        {
            new UnitChoice { UnitName = "joule per square metre", UnitLabel = "J/m²", ID = new Guid("79a7abbb-9c8c-499a-aab1-c2a9cde0f9a1"), ConversionFactorFromSIFormula = "1.0/Factors.Unit", IsSI = true },
            new UnitChoice { UnitName = "kilojoule per square metre", UnitLabel = "kJ/m²", ID = new Guid("b01266c8-4711-4f15-9be6-36a07adbaffa"), ConversionFactorFromSIFormula = "1.0/Factors.Kilo" },
            new UnitChoice { UnitName = "joule per square centimetre", UnitLabel = "J/cm²", ID = new Guid("ab9081db-72a5-4536-b133-cb64158e9fcf"), ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi" },
            new UnitChoice { UnitName = "foot pound force per square foot", UnitLabel = "ft•lbf/ft²", ID = new Guid("4f0c36c7-f1d6-48ee-a097-591e606ef1f3"), ConversionFactorFromSIFormula = "Factors.Foot/Factors.PoundForce" }
        };

        public EnergyPerAreaQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").First();
            UsualNames = new HashSet<string>() { "energy per area", "specific impact energy", "EnergyPerArea", "SpecificImpactEnergy" };
            ID = new Guid("31fb561e-bda5-4459-b7aa-5701a8cc93e2");
            DescriptionMD = "**Energy per area** retains the energy-over-area meaning even though its dimensions equal force per length." + Environment.NewLine;
            DescriptionMD += "Its coherent SI unit is joule per square metre with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
