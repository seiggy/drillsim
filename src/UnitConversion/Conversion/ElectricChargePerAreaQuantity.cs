using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents electric charge per area.
    /// </summary>
    public partial class ElectricChargePerAreaQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "sigma";
        public override string SIUnitName { get; } = "coulomb per square metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{C}{m^2}";
        public override double LengthDimension { get; } = -2;
        public override double TimeDimension { get; } = 1;
        public override double ElectricCurrentDimension { get; } = 1;

        private static ElectricChargePerAreaQuantity instance_ = null;
        public static ElectricChargePerAreaQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectricChargePerAreaQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "coulomb per square metre",
                UnitLabel = "C/m^2",
                ID = new Guid("7d5aa795-ed12-441d-9779-ad45a90a0158"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "microcoulomb per square centimetre",
                UnitLabel = "uC/cm^2",
                ID = new Guid("4bc8c041-8c49-44d3-9b67-760d123442cb"),
                ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi/Factors.Micro",
            }
        };

        public ElectricChargePerAreaQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electric charge per area", "surface charge density" };
            ID = new Guid("14a547f7-dfb7-4162-875d-656c2a24743d");
            DescriptionMD = "**electric charge per area** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is coulomb per square metre with unit label $\\frac{C}{m^2}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
