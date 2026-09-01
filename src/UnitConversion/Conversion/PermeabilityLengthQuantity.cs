using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents permeability length.
    /// </summary>
    public partial class PermeabilityLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "kL";
        public override string SIUnitName { get; } = "square metre metre";
        public override string SIUnitLabelLatex { get; } = "m^2\\cdot m";
        public override double LengthDimension { get; } = 3;

        private static PermeabilityLengthQuantity instance_ = null;
        public static PermeabilityLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PermeabilityLengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "square metre metre",
                UnitLabel = "m^2*m",
                ID = new Guid("a4af5ef4-9b29-4ecc-8901-d056115a95f9"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "darcy metre",
                UnitLabel = "D*m",
                ID = new Guid("62cbe1a7-4677-4453-a54c-76dea6ab2be1"),
                ConversionFactorFromSIFormula = "1.0/Factors.Darcy",
            },
            new UnitChoice
            {
                UnitName = "millidarcy foot",
                UnitLabel = "mD*ft",
                ID = new Guid("fa208496-9604-4190-9da5-6ca9d8742bbc"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Milli*Factors.Darcy*Factors.Foot)",
            }
        };

        public PermeabilityLengthQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "permeability length" };
            ID = new Guid("6306fee6-0383-4ab9-9044-ee72c21f14bd");
            DescriptionMD = "**permeability length** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is " + SIUnitName + " with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
