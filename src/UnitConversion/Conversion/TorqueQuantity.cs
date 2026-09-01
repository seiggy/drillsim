using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class TorqueQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "newton metre";
        public override string SIUnitLabelLatex { get; } = "N \\cdot m";
        public override double LengthDimension { get; } = 2;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;
        private static TorqueQuantity instance_ = null;

        public static TorqueQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TorqueQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
               new UnitChoice
                {
                  UnitName = "newton metre",
                  UnitLabel = "N•m",
                  ID = new Guid("50b017fa-8d81-4076-a485-61de1d8301b5"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                  IsSI = true
                },
                new UnitChoice
                {
                  UnitName = "decanewton metre",
                  UnitLabel = "daN•m",
                  ID = new Guid("501ce02c-8a5d-46e6-9ab6-ce443df70402"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force metre",
                  UnitLabel = "kgf•m",
                  ID = new Guid("282f97a0-df2a-4016-9ab0-796db49ff384"),
                  ConversionFactorFromSIFormula = "1.0/ Factors.KilogramForce"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton metre",
                  UnitLabel = "kN•m",
                  ID = new Guid("2e417a6e-1acc-4901-8704-7dfeb3f67546"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "foot pound",
                  UnitLabel = "ft•lbf",
                  ID = new Guid("700d9fc7-17e1-4ad6-84f1-39cacbe5fe51"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "kilofoot pound",
                  UnitLabel = "kft•lbf",
                  ID = new Guid("ee9be6ed-df75-4915-be6a-e3941dacd6bd"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Kilo*Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "newton decimetre",
                  UnitLabel = "N•dm",
                  ID = new Guid("e70db590-c5fb-4ab9-a2c4-3ad611cb7f63"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Deci"
                },
                new UnitChoice
                {
                  UnitName = "newton centimetre",
                  UnitLabel = "N•cm",
                  ID = new Guid("4acf4542-8df0-4f57-a852-7c0184dbeec9"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Centi"
                },
                new UnitChoice
                {
                  UnitName = "newton millimetre",
                  UnitLabel = "N•mm",
                  ID = new Guid("a933225d-7c8e-4ce4-b0ea-4c1c6e9f7e34"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "inch pound",
                  UnitLabel = "in•lbf",
                  ID = new Guid("0d40553e-d8c4-4b75-ad05-61199b15a0a1"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.PoundForce*Factors.Inch)"
                }
        };
        public TorqueQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "torque" };
            ID = new Guid("3eb9e01e-48fa-430e-82c6-3aee4d359ac4");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A torque is a measure of the rotational force applied to a body around an axis." + Environment.NewLine;
            DescriptionMD += @"The dimension of torque is:" + Environment.NewLine;
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
