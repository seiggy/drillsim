using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents magnetic vector potential.
    /// </summary>
    public partial class MagneticVectorPotentialQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "A";
        public override string SIUnitName { get; } = "tesla metre";
        public override string SIUnitLabelLatex { get; } = "T\\cdot m";
        public override double LengthDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;
        public override double MassDimension { get; } = 1;
        public override double ElectricCurrentDimension { get; } = -1;

        private static MagneticVectorPotentialQuantity instance_ = null;
        public static MagneticVectorPotentialQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MagneticVectorPotentialQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "tesla metre",
                UnitLabel = "T*m",
                ID = new Guid("bb7d89a9-8058-44b7-b8e6-85ee98ec071c"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "weber per metre",
                UnitLabel = "Wb/m",
                ID = new Guid("9e0dbfa9-bfb8-4f80-a42d-cf893d72f04d"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
            },
            new UnitChoice
            {
                UnitName = "gauss centimetre",
                UnitLabel = "G*cm",
                ID = new Guid("4efec503-b750-47c0-8b39-e7dc000157f9"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Gauss*Factors.Centi)",
            }
        };

        public MagneticVectorPotentialQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "magnetic vector potential" };
            ID = new Guid("b8845077-919a-4d8b-a856-b97f8cd8d74b");
            DescriptionMD = "**magnetic vector potential** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is tesla metre with unit label $T\\cdot m$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
