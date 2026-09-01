using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents length per pressure.
    /// </summary>
    public partial class LengthPerPressureQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "L/p";
        public override string SIUnitName { get; } = "metre per pascal";
        public override string SIUnitLabelLatex { get; } = "\\frac{m}{Pa}";
        public override double LengthDimension { get; } = 2;
        public override double TimeDimension { get; } = 2;
        public override double MassDimension { get; } = -1;

        private static LengthPerPressureQuantity instance_ = null;
        public static LengthPerPressureQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new LengthPerPressureQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "metre per pascal",
                UnitLabel = "m/Pa",
                ID = new Guid("22af19e8-1809-461b-a8a1-db4f803938d7"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "millimetre per bar",
                UnitLabel = "mm/bar",
                ID = new Guid("8bfdf072-db03-4604-a6d9-39f9b99eee9f"),
                ConversionFactorFromSIFormula = "Factors.Bar/Factors.Milli",
            },
            new UnitChoice
            {
                UnitName = "inch per psi",
                UnitLabel = "in/psi",
                ID = new Guid("8dcd3d94-db24-4573-8e67-bf8fbf3a4e13"),
                ConversionFactorFromSIFormula = "Factors.PSI/Factors.Inch",
            }
        };

        public LengthPerPressureQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "length per pressure", "pressure compliance length" };
            ID = new Guid("3dbc03bb-db8b-49cb-b261-1e4dd657f594");
            DescriptionMD = "**length per pressure** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is metre per pascal with unit label $\\frac{m}{Pa}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
