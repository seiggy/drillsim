using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents force area.
    /// </summary>
    public partial class ForceAreaQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "FA";
        public override string SIUnitName { get; } = "newton square metre";
        public override string SIUnitLabelLatex { get; } = "N\\cdot m^2";
        public override double LengthDimension { get; } = 3;
        public override double TimeDimension { get; } = -2;
        public override double MassDimension { get; } = 1;

        private static ForceAreaQuantity instance_ = null;
        public static ForceAreaQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ForceAreaQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "newton square metre",
                UnitLabel = "N*m^2",
                ID = new Guid("ca399da7-f37a-4845-a70b-734155df938f"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilonewton square metre",
                UnitLabel = "kN*m^2",
                ID = new Guid("c7a62913-295b-465b-8869-5c604b8da539"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "pound force square foot",
                UnitLabel = "lbf*ft^2",
                ID = new Guid("6966fa90-dbdb-451a-92cd-283db921d295"),
                ConversionFactorFromSIFormula = "1.0/(Factors.PoundForce*Factors.Foot*Factors.Foot)",
            }
        };

        public ForceAreaQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "force area" };
            ID = new Guid("b08fb64a-9b44-4585-a5cc-5f8d3fcacb08");
            DescriptionMD = "**force area** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is newton square metre with unit label $N\\cdot m^2$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
