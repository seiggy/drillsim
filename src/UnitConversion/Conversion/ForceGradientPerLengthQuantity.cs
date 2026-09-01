using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ForceGradientPerLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "newton per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{N}{m}";
        public override double LengthDimension { get; } = 0;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;
        private static ForceGradientPerLengthQuantity instance_ = null;

        public static ForceGradientPerLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ForceGradientPerLengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
        {
          UnitName = "newton per metre",
          UnitLabel = "N/m",
          ID = new Guid("e503a1d3-1815-4321-8087-6e3d6dc641c8"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit",
          IsSI = true
        },
        new UnitChoice
        {
          UnitName = "newton per 30 metre",
          UnitLabel = "N/30m",
          ID = new Guid("be16e271-5ce7-445b-a8db-9014a6acc22b"),
          ConversionFactorFromSIFormula = "3*Factors.Deca/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "newton per 10 metre",
          UnitLabel = "N/10m",
          ID = new Guid("46defe0c-4a00-45d1-83bb-f898e00a78c5"),
          ConversionFactorFromSIFormula = "Factors.Deca/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "newton per decimetre",
          UnitLabel = "N/dm",
          ID = new Guid("dcd21076-ecb6-481e-8b8b-1cd5ccc68915"),
          ConversionFactorFromSIFormula = "Factors.Deci/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "newton per centimetre",
          UnitLabel = "N/cm",
          ID = new Guid("739cb2cd-2c9f-4efc-ad17-306b8f09de57"),
          ConversionFactorFromSIFormula = "Factors.Centi/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "newton per millimetre",
          UnitLabel = "N/mm",
          ID = new Guid("9375f700-72fb-4212-a51d-0f4500e7b13c"),
          ConversionFactorFromSIFormula = "Factors.Milli/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "decanewton per metre",
          UnitLabel = "daN/m",
          ID = new Guid("2566918f-f1b1-4ffb-906b-adb3680812e1"),
          ConversionFactorFromSIFormula = "1.0/Factors.Deca"
        },
        new UnitChoice
        {
          UnitName = "decanewton per 30 metre",
          UnitLabel = "daN/30m",
          ID = new Guid("20de7177-2099-4f86-89da-fdfa68bf67ed"),
          ConversionFactorFromSIFormula = "3*Factors.Deca/Factors.Deca"
        },
        new UnitChoice
        {
          UnitName = "decanewton per 10 metre",
          UnitLabel = "daN/10m",
          ID = new Guid("4f30206a-b381-4a28-9e2d-fafc026e71d5"),
          ConversionFactorFromSIFormula = "Factors.Deca/Factors.Deca"
        },
        new UnitChoice
        {
          UnitName = "decanewton per decimetre",
          UnitLabel = "daN/dm",
          ID = new Guid("cf20b9bb-aab1-4f1a-832c-1cfbe8ffc825"),
          ConversionFactorFromSIFormula = "Factors.Deci/Factors.Deca"
        },
        new UnitChoice
        {
          UnitName = "decanewton per centimetre",
          UnitLabel = "daN/cm",
          ID = new Guid("47704d55-35cc-4bfc-9f93-7cf7f29c81ac"),
          ConversionFactorFromSIFormula = "Factors.Centi/Factors.Deca"
        },
        new UnitChoice
        {
          UnitName = "decanewton per millimetre",
          UnitLabel = "daN/mm",
          ID = new Guid("1f418c90-f2e6-4bc8-8c06-f281e56ef6cc"),
          ConversionFactorFromSIFormula = "Factors.Milli/Factors.Deca"
        },
        new UnitChoice
        {
          UnitName = "kilonewton per metre",
          UnitLabel = "kN/m",
          ID = new Guid("9ec7912e-9506-43ce-9089-80000d7ddd3f"),
          ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
        },
        new UnitChoice
        {
          UnitName = "kilonewton per 30 metre",
          UnitLabel = "kN/30m",
          ID = new Guid("b08fae49-fdc3-409e-8b0f-3349ab189dc9"),
          ConversionFactorFromSIFormula = "3*Factors.Deca/Factors.Kilo"
        },
        new UnitChoice
        {
          UnitName = "kilonewton per 10 metre",
          UnitLabel = "kN/10m",
          ID = new Guid("f57cb3e9-4da5-4960-aff6-a27167276e4a"),
          ConversionFactorFromSIFormula = "Factors.Deca/Factors.Kilo"
        },
        new UnitChoice
        {
          UnitName = "kilonewton per decimetre",
          UnitLabel = "kN/dm",
          ID = new Guid("f3033c1b-1be8-4110-832a-4b60c31043e6"),
          ConversionFactorFromSIFormula = "Factors.Deci/Factors.Kilo"
        },
        new UnitChoice
        {
          UnitName = "kilonewton per centimetre",
          UnitLabel = "kN/cm",
          ID = new Guid("4db740c5-df92-4f65-b0da-2119ad80cbfc"),
          ConversionFactorFromSIFormula = "Factors.Centi/Factors.Kilo"
        },
        new UnitChoice
        {
          UnitName = "kilonewton per millimetre",
          UnitLabel = "kN/mm",
          ID = new Guid("14578d1b-6d43-441a-8f1b-aa77ab10a9bf"),
          ConversionFactorFromSIFormula = "Factors.Milli/Factors.Kilo"
        },
        new UnitChoice
        {
          UnitName = "pound per foot",
          UnitLabel = "lbf/ft",
          ID = new Guid("516e4b02-2f1a-49a7-8cd9-3fa4e28c8fce"),
          ConversionFactorFromSIFormula = "Factors.Foot/Factors.PoundForce"
        },
        new UnitChoice
        {
          UnitName = "pound per inch",
          UnitLabel = "lbf/in",
          ID = new Guid("8a5772d2-1253-4269-958a-af9f779aecc6"),
          ConversionFactorFromSIFormula = "Factors.Inch/Factors.PoundForce"
        },
        new UnitChoice
        {
          UnitName = "kilopound per foot",
          UnitLabel = "klbf/ft",
          ID = new Guid("bf63e80f-97df-48d1-afbf-c83415654e44"),
          ConversionFactorFromSIFormula = "Factors.Foot/(Factors.Kilo*Factors.PoundForce)"
        },
        new UnitChoice
        {
          UnitName = "kilopound per inch",
          UnitLabel = "klbf/in",
          ID = new Guid("fa6a4a38-b070-48d1-a747-be22ab0e57b6"),
          ConversionFactorFromSIFormula = "Factors.Inch/(Factors.Kilo*Factors.PoundForce)"
        },
        new UnitChoice
        {
          UnitName = "pound per 30 foot",
          UnitLabel = "lbf/30ft",
          ID = new Guid("0d0926be-19fa-4687-88d1-35f1acc58717"),
          ConversionFactorFromSIFormula = "30.0*Factors.Foot/Factors.PoundForce"
        },
        new UnitChoice
        {
          UnitName = "pound per 100 foot",
          UnitLabel = "lbf/100ft",
          ID = new Guid("dcaa5f41-da2f-49d2-be41-80fb6f0a06ec"),
          ConversionFactorFromSIFormula = "100.0*Factors.Foot/Factors.PoundForce"
        },
        new UnitChoice
        {
          UnitName = "kilopound per 30 foot",
          UnitLabel = "klbf/30ft",
          ID = new Guid("27a355cf-36ae-458d-acbd-2a5ad931bbab"),
          ConversionFactorFromSIFormula = "30.0*Factors.Foot/(Factors.Kilo*Factors.PoundForce)"
        },
        new UnitChoice
        {
          UnitName = "kilopound per 100 foot",
          UnitLabel = "klbf/100ft",
          ID = new Guid("0d5c841e-b259-4fdf-93d7-e39cca391adb"),
          ConversionFactorFromSIFormula = "100.0*Factors.Foot/(Factors.Kilo*Factors.PoundForce)"
        }
        };
        public ForceGradientPerLengthQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "force gradient per length" };
            ID = new Guid("e5212340-1147-4cad-9f71-5cd9d4208ffd");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A force gradient per length is the first derivative compared to a distance of a force: $\frac{dF}{ds}$ where $F$ is a force and $s$ is a distance." + Environment.NewLine;
            DescriptionMD += @"The dimension of force gradient per length is:" + Environment.NewLine;
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
