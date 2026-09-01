using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ForceRateOfChangeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "Newton per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{N}{s}";
        public override double MassDimension { get; } = 1;
        public override double LengthDimension { get; } = 1;
        public override double TimeDimension { get; } = -3;
        private static ForceRateOfChangeQuantity instance_ = null;

        public static ForceRateOfChangeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ForceRateOfChangeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                  UnitName = "newton per second",
                  UnitLabel = "N/s",
                  ID = new Guid("c766ff54-d778-4ee6-9c65-8467932efa60"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                  IsSI = true
                },
                new UnitChoice
                {
                  UnitName = "decanewton per second",
                  UnitLabel = "daN/s",
                  ID = new Guid("1b7c3f4d-30ec-4d50-8063-0d1452c88615"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton per second",
                  UnitLabel = "kN/s",
                  ID = new Guid("5c99b2ac-51b7-4f9c-b82b-036f0b02492d"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "kilodecanewton per second",
                  UnitLabel = "kdaN/s",
                  ID = new Guid("f0ad684b-827c-43f9-8f6e-c9097bc82dd3"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Kilo*Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force per second",
                  UnitLabel = "kgf/s",
                  ID = new Guid("e5dc01f1-09d4-4304-b065-8096026647e8"),
                  ConversionFactorFromSIFormula = "1.0/Factors.KilogramForce"
                },
                new UnitChoice
                {
                  UnitName = "pound force per second",
                  UnitLabel = "lbf/s",
                  ID = new Guid("92ed16d5-3ea8-4102-a1cb-89f527d2b4a0"),
                  ConversionFactorFromSIFormula = "1.0/Factors.PoundForce"
                },
                new UnitChoice
                {
                  UnitName = "kilopound force per second",
                  UnitLabel = "klbf/s",
                  ID = new Guid("1f45684c-4582-4d5f-a5c5-950e4c9dbff7"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Kilo*Factors.PoundForce)"
                },

                new UnitChoice
                {
                  UnitName = "newton per minute",
                  UnitLabel = "N/min",
                  ID = new Guid("58df085b-9f10-4148-ad09-cb05bbcfa920"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Unit"
                },
                new UnitChoice
                {
                  UnitName = "decanewton per minute",
                  UnitLabel = "daN/min",
                  ID = new Guid("130a7d93-a5d2-4d9b-bdb5-4c5784f61c79"),
                  ConversionFactorFromSIFormula = "Factors.Minute/(Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton per minute",
                  UnitLabel = "kN/min",
                  ID = new Guid("5841d94c-2349-4f51-a965-eb8cc3cc19d9"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "kilodecanewton per minute",
                  UnitLabel = "kdaN/min",
                  ID = new Guid("1a80c782-8438-43ac-ba6c-46b6b7abe761"),
                  ConversionFactorFromSIFormula = "Factors.Minute/(Factors.Kilo*Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force per minute",
                  UnitLabel = "kgf/min",
                  ID = new Guid("323c8871-2f8f-41bd-9df8-50e3b50bf093"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.KilogramForce"
                },
                new UnitChoice
                {
                  UnitName = "pound force per minute",
                  UnitLabel = "lbf/min",
                  ID = new Guid("924a79ab-743d-4c69-b5fb-b9a60bc70726"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.PoundForce"
                },
                new UnitChoice
                {
                  UnitName = "kilopound force per minute",
                  UnitLabel = "klbf/min",
                  ID = new Guid("1b5bc3fc-3784-4508-83d5-4a21b5e9fe84"),
                  ConversionFactorFromSIFormula = "Factors.Minute/(Factors.Kilo*Factors.PoundForce)"
                },

                new UnitChoice
                {
                  UnitName = "newton per hour",
                  UnitLabel = "N/h",
                  ID = new Guid("efa69e3c-b03b-4520-8ae8-e92ab6953141"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Unit"
                },
                new UnitChoice
                {
                  UnitName = "decanewton per hour",
                  UnitLabel = "daN/h",
                  ID = new Guid("1b0aaee4-9d74-4289-9f08-96c2d31a19f3"),
                  ConversionFactorFromSIFormula = "Factors.Hour/(Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton per hour",
                  UnitLabel = "kN/h",
                  ID = new Guid("edd7e626-f9a4-42c5-bce3-5b72c1f3ca55"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "kilodecanewton per hour",
                  UnitLabel = "kdaN/h",
                  ID = new Guid("ce5ff57d-e427-4e4f-aa11-c1f02118b3e1"),
                  ConversionFactorFromSIFormula = "Factors.Hour/(Factors.Kilo*Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force per hour",
                  UnitLabel = "kgf/h",
                  ID = new Guid("5c638813-fe28-47de-b7b5-a65760562b12"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.KilogramForce"
                },
                new UnitChoice
                {
                  UnitName = "pound force per hour",
                  UnitLabel = "lbf/h",
                  ID = new Guid("a1e5538f-a653-4a4b-8240-01b6a709a0d4"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.PoundForce"
                },
                new UnitChoice
                {
                  UnitName = "kilopound force per hour",
                  UnitLabel = "klbf/h",
                  ID = new Guid("6041cb2d-b49a-46b4-87ef-1f89ddd89758"),
                  ConversionFactorFromSIFormula = "Factors.Hour/(Factors.Kilo*Factors.PoundForce)"
                }
        };
        public ForceRateOfChangeQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "force rate of change" };
            ID = new Guid("2f28f6d5-5b01-4fd0-9924-bf84250f6092");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A force rate of change is the time derivative of a force: $\frac{dF}{dt}$, where $F$ is the mass density and $t$ is time." + Environment.NewLine;
            DescriptionMD += @"The dimension of force rate of change is:" + Environment.NewLine;
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
