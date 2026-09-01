using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class MassDensityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "kilogram per cubic metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{kg}{m^{3}}";
        public override double MassDimension { get; } = 1;
        public override double LengthDimension { get; } = -3;
        private static MassDensityQuantity instance_ = null;

        public static MassDensityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassDensityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                    UnitName = "kilogram per cubic metre",
                    UnitLabel = "kg/m³",
                    ID = new Guid("8e175ca0-08f6-441d-afcf-a58bbe429abf"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic metre",
                    UnitLabel = "g/m³",
                    ID = new Guid("8c5b7fc3-0ade-4e85-9646-71ec5fcb869a"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity",
                    UnitLabel = "s.g.",
                    ID = new Guid("da94ba95-4494-45af-bf99-31f00031c680"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre",
                    UnitLabel = "g/cm³",
                    ID = new Guid("64f1b0d8-609f-4ed9-99da-52e18fe97450"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi / Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK)",
                    UnitLabel = "ppgUK",
                    ID = new Guid("75ecf787-8778-4d74-afc7-498e117d27bf"),
                    ConversionFactorFromSIFormula = "1.0/Factors.PPGUK"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US)",
                    UnitLabel = "ppgUS",
                    ID = new Guid("dcc01dd0-4610-42c7-9a55-2ddeb45ef6da"),
                    ConversionFactorFromSIFormula = "1.0/Factors.PPGUS"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot",
                    UnitLabel = "lb/ft³",
                    ID = new Guid("f7479c8c-8d03-460b-bfa3-2b68808be935"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch",
                    UnitLabel = "lb/in³",
                    ID = new Guid("d549658a-76ab-4507-8a9e-e62a5cf47e23"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard",
                    UnitLabel = "lb/yd³",
                    ID = new Guid("8199e187-5283-42cc-891e-b3887c3aa450"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard/Factors.Pound"
                }
        };
        public MassDensityQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass density" };
            ID = new Guid("5754358c-9359-4bb0-8eb4-08602d19c6af");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Mass density is the amount of mass per unit volume of a substance." + Environment.NewLine;
            DescriptionMD += @"The dimension of mass density is:" + Environment.NewLine;
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