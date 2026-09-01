using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class MassDensityRateOfChangeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "kilogram per cubic metre per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{\\frac{kg}{m^{3}}}{s}";
        public override double MassDimension { get; } = 1;
        public override double LengthDimension { get; } = -3;
        public override double TimeDimension { get; } = -1;
        private static MassDensityRateOfChangeQuantity instance_ = null;

        public static MassDensityRateOfChangeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassDensityRateOfChangeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per second",
                    UnitLabel = "kg/m³/s",
                    ID = new Guid("d80197aa-f0b2-4a26-a5a4-b132a248c377"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per second",
                    UnitLabel = "sg/s",
                    ID = new Guid("dec0a290-ffd8-4fc0-ae11-3a6068469791"),
                    ConversionFactorFromSIFormula = "1.0/Factors.WaterDensity4degC1Atm"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per minute",
                    UnitLabel = "sg/min",
                    ID = new Guid("9c314f49-3297-4f7b-9cf3-5da32ba2f1cc"),
                    ConversionFactorFromSIFormula = "Factors.Minute / Factors.WaterDensity4degC1Atm"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per hour",
                    UnitLabel = "sg/h",
                    ID = new Guid("ce5d34b0-d7ab-48a4-87c1-9a43fabf5c06"),
                    ConversionFactorFromSIFormula = "Factors.Hour / Factors.WaterDensity4degC1Atm"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per second",
                    UnitLabel = "g/cm³/s",
                    ID = new Guid("e26f57a2-9659-40fd-a670-38a3b83fd36f"),
                    ConversionFactorFromSIFormula = "Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per minute",
                    UnitLabel = "g/cm³/min",
                    ID = new Guid("93777f41-0e47-46aa-9ab6-413987553817"),
                    ConversionFactorFromSIFormula = "Factors.Minute * Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per hour",
                    UnitLabel = "g/cm³/h",
                    ID = new Guid("c8d6a682-00ca-4d0f-b603-bf2d755f4b31"),
                    ConversionFactorFromSIFormula = "Factors.Hour *Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per second",
                    UnitLabel = "ppgUK/s",
                    ID = new Guid("e5a712d2-b874-4e7a-873e-a4f4f3ec7a67"),
                    ConversionFactorFromSIFormula = "1.0 / Factors.PPGUK"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per minute",
                    UnitLabel = "ppgUK/min",
                    ID = new Guid("e79c74b9-774d-4695-81d5-75042f268b96"),
                    ConversionFactorFromSIFormula = "Factors.Minute / Factors.PPGUK"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per hour",
                    UnitLabel = "ppgUK/h",
                    ID = new Guid("5b461e39-d632-4f5e-b7e7-ef30745e5070"),
                    ConversionFactorFromSIFormula = "Factors.Hour / Factors.PPGUK"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per second",
                    UnitLabel = "ppgUS/s",
                    ID = new Guid("eee6814a-d701-4cd8-b392-ebfedde20e11"),
                    ConversionFactorFromSIFormula = "1.0 /Factors.PPGUS"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per minute",
                    UnitLabel = "ppgUS/min",
                    ID = new Guid("c047d53d-38f3-4dce-b590-1c9ab700a3ea"),
                    ConversionFactorFromSIFormula = "Factors.Minute / Factors.PPGUS"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per hour",
                    UnitLabel = "ppgUS/h",
                    ID = new Guid("822327d4-9f7c-4e91-9528-4eff5dfc9643"),
                    ConversionFactorFromSIFormula = "Factors.Hour / Factors.PPGUS"
                }
        };
        public MassDensityRateOfChangeQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass density rate of change" };
            ID = new Guid("be272506-8c7a-4eff-9a05-ad4d07f36e11");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A mass density rate of change is the time derivative of a mass density: $\frac{d\rho}{dt}$, where $\rho$ is the mass density and $t$ is time." + Environment.NewLine;
            DescriptionMD += @"The dimension of mass density rate of change is:" + Environment.NewLine;
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