using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class MaterialStrengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "pascal";
        public override string SIUnitLabelLatex { get; } = "Pa";
        public override double MassDimension { get; } = 1;
        public override double LengthDimension { get; } = -1;
        public override double TimeDimension { get; } = -2;
        private static MaterialStrengthQuantity instance_ = null;

        public static MaterialStrengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MaterialStrengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
               new UnitChoice
                {
                    UnitName = "pascal",
                    UnitLabel = "Pa",
                    ID = new Guid("159e99d3-c79d-4dc6-974f-05cc38af001e"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "megapascal",
                    UnitLabel = "MPa",
                    ID = new Guid("38b95b61-a825-4393-a0e8-ecd686575735"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Mega"
                },
                new UnitChoice
                {
                    UnitName = "gigapascal",
                    UnitLabel = "GPa",
                    ID = new Guid("c9aa0a18-02ac-42a0-9afe-8a08b4f03331"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Giga"
                },
                new UnitChoice
                {
                    UnitName = "psi",
                    UnitLabel = "psi",
                    ID = new Guid("4adf2a33-05c3-49bb-ba61-59dd76f4621e"),
                    ConversionFactorFromSIFormula = "1.0/Factors.PSI"
                },
                new UnitChoice
                {
                    UnitName = "pound per 100 square foot",
                    UnitLabel = "lbf/100ft²",
                    ID = new Guid("eb1e2a52-3de3-4338-ad4d-40e8ce90e40b"),
                    ConversionFactorFromSIFormula = "100.0*Factors.Foot*Factors.Foot/(Factors.PoundForce)"
                },
                new UnitChoice
                {
                    UnitName = "megapound per square inch",
                    UnitLabel = "Mpsi",
                    ID = new Guid("197a8b98-190d-4d45-91d7-85af12deab02"),
                    ConversionFactorFromSIFormula = "1.0/(Factors.Mega*Factors.PSI)"
                }
        };
        public MaterialStrengthQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "material strength" };
            ID = new Guid("d9ca8230-a07a-45c0-ba67-051b70607c40");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Material strength refers to the ability of a material to withstand an applied force or load without failing or deforming. It measures how much stress a material can endure before it breaks, bends, or permanently deforms, often categorized into types like tensile, compressive, and shear strength." + Environment.NewLine;
            DescriptionMD += @"The dimension of material strength is:" + Environment.NewLine;
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
