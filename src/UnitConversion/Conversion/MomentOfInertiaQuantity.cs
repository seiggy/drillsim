using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion
{
    public partial class MomentOfInertiaQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "I";
        public override string SIUnitName { get; } = "kilogram metre squared";
        public override string SIUnitLabelLatex { get; } = "kg \\cdot m^{2}";
        public override double MassDimension { get; } = 1;
        public override double LengthDimension { get; } = 2;
        private static MomentOfInertiaQuantity instance_ = null;

        public static MomentOfInertiaQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MomentOfInertiaQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                    UnitName = "kilogram metre squared",
                    UnitLabel = "kg.m²",
                    ID = new Guid("01c11147-677d-47d2-9167-59601d7961b2"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
            new UnitChoice
                {
                    UnitName = "gram centimetre squared",
                    UnitLabel = "g.cm²",
                    ID = new Guid("71e4e230-c611-4de9-b056-a1ef732b7fce"),
                    ConversionFactorFromSIFormula = "Factors.Kilo/(Factors.Centi*Factors.Centi)"
                },
            new UnitChoice
                {
                    UnitName = "pound foot squared",
                    UnitLabel = "lb.ft²",
                    ID = new Guid("103bd4aa-494a-4ec3-bf60-c3ce5bab364e"),
                    ConversionFactorFromSIFormula = "1.0/(Factors.Foot*Factors.Foot*Factors.Pound)"
                },
            new UnitChoice
                {
                    UnitName = "pound inch squared",
                    UnitLabel = "lb.in²",
                    ID = new Guid("ce8e3a4e-2cea-471a-a0dc-846523001be2"),
                    ConversionFactorFromSIFormula = "1.0/(Factors.Inch*Factors.Inch*Factors.Pound)"
                }
        };
        public MomentOfInertiaQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "moment of inertia" };
            ID = new Guid("cf7c69b0-b4d7-45d2-9d7d-4714996424c0");
            DescriptionMD = string.Empty;
            DescriptionMD += @"The moment of inertia is is a measure of an object's resistance to changes in its rotation rate. It is the rotational analog of mass for linear motion. The moment of inertia depends on the mass distribution of an object and the axis of rotation." + Environment.NewLine;
            DescriptionMD += @"The dimension of moment of inertia is:" + Environment.NewLine;
            DescriptionMD += "$" + GetDimensionsEnclosed() + "$." + Environment.NewLine;
            if (!string.IsNullOrEmpty(SIUnitLabelLatex) && !string.IsNullOrEmpty(SIUnitName) && UsualNames != null && UsualNames.Count > 0)
            {
                DescriptionMD += Environment.NewLine;
                DescriptionMD += @"The SI unit for **" + UsualNames.First() + "** is: " + SIUnitName + " with the associated unit label $" + SIUnitLabelLatex + "$" + Environment.NewLine;
            }
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
