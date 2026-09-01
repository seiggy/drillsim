using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents electric conductivity.
    /// </summary>
    public partial class ElectricConductivityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "sigma";
        public override string SIUnitName { get; } = "siemens per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{S}{m}";
        public override double LengthDimension { get; } = -3;
        public override double TimeDimension { get; } = 3;
        public override double MassDimension { get; } = -1;
        public override double ElectricCurrentDimension { get; } = 2;

        private static ElectricConductivityQuantity instance_ = null;
        public static ElectricConductivityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectricConductivityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "siemens per metre",
                UnitLabel = "S/m",
                ID = new Guid("ff411d9f-fba7-408c-bc74-e4eeb62f2e55"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "millisiemens per metre",
                UnitLabel = "mS/m",
                ID = new Guid("97dc8b32-f052-4721-ada2-ae0ead9270c5"),
                ConversionFactorFromSIFormula = "1.0/Factors.Milli",
            },
            new UnitChoice
            {
                UnitName = "microsiemens per centimetre",
                UnitLabel = "uS/cm",
                ID = new Guid("1bbf59c3-3b41-4082-8cff-3c842653b5db"),
                ConversionFactorFromSIFormula = "Factors.Centi/Factors.Micro",
            }
        };

        public ElectricConductivityQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electric conductivity" };
            ID = new Guid("17cc6e0e-aa7f-4d35-bd42-bf23c2bcb41b");
            DescriptionMD = "**electric conductivity** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is siemens per metre with unit label $\\frac{S}{m}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
