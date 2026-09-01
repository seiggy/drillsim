using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents section modulus.
    /// </summary>
    public partial class SectionModulusQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "Z";
        public override string SIUnitName { get; } = "cubic metre";
        public override string SIUnitLabelLatex { get; } = "m^3";
        public override double LengthDimension { get; } = 3;

        private static SectionModulusQuantity instance_ = null;
        public static SectionModulusQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new SectionModulusQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre",
                UnitLabel = "m^3",
                ID = new Guid("cda6b200-50aa-4a4e-bde4-3de9d54d88f3"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "cubic centimetre",
                UnitLabel = "cm^3",
                ID = new Guid("b3d3c73b-8250-4341-9d67-cbab9012e3a5"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Centi*Factors.Centi*Factors.Centi)",
            },
            new UnitChoice
            {
                UnitName = "cubic inch",
                UnitLabel = "in^3",
                ID = new Guid("11cdcc79-d2ac-4088-a5c9-e0a69ca4a2fe"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Inch*Factors.Inch*Factors.Inch)",
            }
        };

        public SectionModulusQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "section modulus" };
            ID = new Guid("1f785970-5655-40b3-b9eb-0721ab42b452");
            DescriptionMD = "**section modulus** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is cubic metre with unit label $m^3$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
