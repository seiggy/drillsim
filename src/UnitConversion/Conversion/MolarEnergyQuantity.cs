using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents molar energy.
    /// </summary>
    public partial class MolarEnergyQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "E_m";
        public override string SIUnitName { get; } = "joule per mole";
        public override string SIUnitLabelLatex { get; } = "\\frac{J}{mol}";
        public override double LengthDimension { get; } = 2;
        public override double TimeDimension { get; } = -2;
        public override double MassDimension { get; } = 1;
        public override double AmountSubstanceDimension { get; } = -1;

        private static MolarEnergyQuantity instance_ = null;
        public static MolarEnergyQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MolarEnergyQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "joule per mole",
                UnitLabel = "J/mol",
                ID = new Guid("641ee110-a322-4f46-be92-1a9bda7a294e"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilojoule per mole",
                UnitLabel = "kJ/mol",
                ID = new Guid("b1feb189-8ed5-4002-9b3c-8dc1ea28198b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "electronvolt per particle",
                UnitLabel = "eV/particle",
                ID = new Guid("de2026ff-c51d-4d01-a22f-1214f15adc2b"),
                ConversionFactorFromSIFormula = "1.0/(Factors.ElectronCharge*Factors.AvogadroConstant)",
            }
        };

        public MolarEnergyQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "molar energy", "energy per amount of substance" };
            ID = new Guid("f839e9da-af77-4cea-95b2-40deffc6b1db");
            DescriptionMD = "**molar energy** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is joule per mole with unit label $\\frac{J}{mol}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
