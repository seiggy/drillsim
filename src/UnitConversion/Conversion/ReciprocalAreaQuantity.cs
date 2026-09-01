using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents reciprocal area.
    /// </summary>
    public partial class ReciprocalAreaQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "A^-1";
        public override string SIUnitName { get; } = "reciprocal square metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{1}{m^2}";
        public override double LengthDimension { get; } = -2;

        private static ReciprocalAreaQuantity instance_ = null;
        public static ReciprocalAreaQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ReciprocalAreaQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "reciprocal square metre",
                UnitLabel = "1/m^2",
                ID = new Guid("7d3ba499-70b9-43d0-b1eb-ddc2a52ca306"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "reciprocal square foot",
                UnitLabel = "1/ft^2",
                ID = new Guid("05d07310-74a0-4c76-be4b-729b9e40b822"),
                ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot",
            },
            new UnitChoice
            {
                UnitName = "reciprocal square inch",
                UnitLabel = "1/in^2",
                ID = new Guid("7207d426-917f-4c0a-8e97-bdb360392a62"),
                ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch",
            }
        };

        public ReciprocalAreaQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "reciprocal area" };
            ID = new Guid("1fff82b1-2e6b-4f74-b814-a655ac4db93c");
            DescriptionMD = "**reciprocal area** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is reciprocal square metre with unit label $\\frac{1}{m^2}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
