using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ForceQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "newton";
        public override string SIUnitLabelLatex { get; } = "N";
        public override double LengthDimension { get; } = 1;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;
        private static ForceQuantity instance_ = null;

        public static ForceQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ForceQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                  UnitName = "newton",
                  UnitLabel = "N",
                  ID = new Guid("2e6b218c-0f85-4e8d-b9c5-73b78d207ef8"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                  IsSI = true
                },
                new UnitChoice
                {
                  UnitName = "decanewton",
                  UnitLabel = "daN",
                  ID = new Guid("fc48e3a8-deb9-4cf6-aaad-5b18f7e37972"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton",
                  UnitLabel = "kN",
                  ID = new Guid("e30d6f94-7887-4746-8d4f-eb720239b702"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "kilodecanewton",
                  UnitLabel = "kdaN",
                  ID = new Guid("8ad7ae81-602b-4694-bc6d-c18f520f1266"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Kilo*Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force",
                  UnitLabel = "kgf",
                  ID = new Guid("ea771c51-4078-4aa6-b2df-db6f77a140ad"),
                  ConversionFactorFromSIFormula = "1.0/Factors.KilogramForce"
                },
                new UnitChoice
                {
                  UnitName = "tonne force",
                  UnitLabel = "tf",
                  ID = new Guid("6d7771d3-01cf-40f0-b5bc-3165cd0e6bea"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Kilo*Factors.KilogramForce)"
                },
                new UnitChoice
                {
                  UnitName = "pound force",
                  UnitLabel = "lbf",
                  ID = new Guid("c738ced5-1c99-42ec-9c47-59e7d6455ffa"),
                  ConversionFactorFromSIFormula = "1.0/Factors.PoundForce"
                },
                new UnitChoice
                {
                  UnitName = "kilopound force",
                  UnitLabel = "klbf",
                  ID = new Guid("fa385f22-3ed9-4f34-ab0c-193e3ac79375"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Kilo*Factors.PoundForce)"
                }
        };
        public ForceQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "force" };
            ID = new Guid("af9fd237-14d8-4b75-8d0b-34ea8961c20b");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A force is an influence that can cause an object to change its velocity unless counterbalanced by other forces." + Environment.NewLine;
            DescriptionMD += @"The dimension of force is:" + Environment.NewLine;
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
