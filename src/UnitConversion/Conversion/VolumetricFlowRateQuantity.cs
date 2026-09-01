using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class VolumetricFlowRateQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "Q";
        public override string SIUnitName { get; } = "cubic metre per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^{3}}{s}";
        public override double LengthDimension { get; } = 3;
        public override double TimeDimension { get; } = -1;
        private static VolumetricFlowRateQuantity instance_ = null;

        public static VolumetricFlowRateQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumetricFlowRateQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
        new UnitChoice
        {
          UnitName = "cubic metre per second",
          UnitLabel = "m³/s",
          ID = new Guid("fabe7a23-1ba2-4f5d-b3f5-d5729166c7a0"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit",
          IsSI = true
        },
        new UnitChoice
        {
          UnitName = "litre per second",
          UnitLabel = "L/s",
          ID = new Guid("6dafb683-0f61-42ba-a22c-7bd5c9cea4ae"),
          ConversionFactorFromSIFormula = "1.0/Factors.Milli"
        },
        new UnitChoice
        {
          UnitName = "cubic foot per second",
          UnitLabel = "ft³/s",
          ID = new Guid("a02fc355-34da-447b-ad1f-66cef4cc96d5"),
          ConversionFactorFromSIFormula = "1.0/(Factors.Foot*Factors.Foot*Factors.Foot)"
        },
        new UnitChoice
        {
          UnitName = "UK gallon per second",
          UnitLabel = "gpsUK",
          ID = new Guid("21ca44f5-ed4e-414d-b285-b38730600794"),
          ConversionFactorFromSIFormula = "1.0/Factors.GallonUK"
        },
        new UnitChoice
        {
          UnitName = "US gallon per second",
          UnitLabel = "gpsUS",
          ID = new Guid("21fe7e3a-bf92-4c94-b6d7-e2d30dfc4cd3"),
          ConversionFactorFromSIFormula = "1.0/Factors.GallonUS"
        },
        new UnitChoice
        {
          UnitName = "barrel per second",
          UnitLabel = "bbl/s",
          ID = new Guid("a73caac6-062e-4f79-8374-8fb2598b6842"),
          ConversionFactorFromSIFormula = "1.0/Factors.Barrel"
        },
        new UnitChoice
        {
          UnitName = "cubic metre per minute",
          UnitLabel = "m³/min",
          ID = new Guid("98eec130-224c-457a-a5dd-50b55a3a28ab"),
          ConversionFactorFromSIFormula = "Factors.Minute / Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "litre per minute",
          UnitLabel = "L/min",
          ID = new Guid("e1ff1684-02ed-41c0-a82c-40b4a6d348b5"),
          ConversionFactorFromSIFormula = "Factors.Minute / Factors.Milli"
        },
        new UnitChoice
        {
          UnitName = "cubic foot per minute",
          UnitLabel = "ft³/min",
          ID = new Guid("a81e08db-d8fd-4379-bc8e-c223c3fb71b3"),
          ConversionFactorFromSIFormula = "Factors.Minute / (Factors.Foot*Factors.Foot*Factors.Foot)"
        },
        new UnitChoice
        {
          UnitName = "UK gallon per minute",
          UnitLabel = "gpmUK",
          ID = new Guid("bb1d3fbc-efc1-4cf7-9c13-33c25a874224"),
          ConversionFactorFromSIFormula = "Factors.Minute / Factors.GallonUK"
        },
        new UnitChoice
        {
          UnitName = "US gallon per minute",
          UnitLabel = "gpmUS",
          ID = new Guid("ceca76e7-fd1e-4220-b072-9f40467a05e3"),
          ConversionFactorFromSIFormula = "Factors.Minute / Factors.GallonUS"
        },
        new UnitChoice
        {
          UnitName = "barrel per minute",
          UnitLabel = "bbl/m",
          ID = new Guid("3672c640-3924-4921-861b-d14c99643615"),
          ConversionFactorFromSIFormula = "Factors.Minute /Factors.Barrel"
        },
        new UnitChoice
        {
          UnitName = "cubic metre per hour",
          UnitLabel = "m³/h",
          ID = new Guid("d6607bed-0235-4838-87c8-2ff6c76be2ad"),
          ConversionFactorFromSIFormula = "Factors.Hour/ Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "litre per hour",
          UnitLabel = "L/h",
          ID = new Guid("fe2d8a01-fd2b-4652-80bd-f0da2ce990fd"),
          ConversionFactorFromSIFormula = "Factors.Hour/ Factors.Milli"
        },
        new UnitChoice
        {
          UnitName = "cubic foot per hour",
          UnitLabel = "ft³/h",
          ID = new Guid("54f30d1c-670a-4566-bfb2-3ce644d68878"),
          ConversionFactorFromSIFormula = "Factors.Hour / (Factors.Foot*Factors.Foot*Factors.Foot)"
        },
        new UnitChoice
        {
          UnitName = "UK gallon per hour",
          UnitLabel = "gphUK",
          ID = new Guid("85d47672-e1ce-4b33-be1d-eabc2ec1c4c1"),
          ConversionFactorFromSIFormula = "Factors.Hour/ Factors.GallonUK"
        },
        new UnitChoice
        {
          UnitName = "US gallon per hour",
          UnitLabel = "gphUS",
          ID = new Guid("36892d41-4a11-4d32-9a8c-1e43ac1f8c6d"),
          ConversionFactorFromSIFormula = "Factors.Hour/ Factors.GallonUS"
        },
        new UnitChoice
        {
          UnitName = "barrel per hour",
          UnitLabel = "bbl/h",
          ID = new Guid("af1bd583-3a13-4d3a-8f65-ede4f8fe9789"),
          ConversionFactorFromSIFormula = "Factors.Hour/ Factors.Barrel"
        },
        new UnitChoice
        {
          UnitName = "cubic metre per day",
          UnitLabel = "m³/d",
          ID = new Guid("f512755c-166c-4346-a0f7-74f09703410f"),
          ConversionFactorFromSIFormula = "Factors.Day / Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "million cubic metre per day",
          UnitLabel = "Mm³/d",
          ID = new Guid("d09b4db7-9fbe-4018-aeb7-15286ccff8d6"),
          ConversionFactorFromSIFormula = "Factors.Day / Factors.Mega"
        },
        new UnitChoice
        {
          UnitName = "UK gallon per day",
          UnitLabel = "gpdUK",
          ID = new Guid("334cf647-375e-4423-8ef4-e1171f938f9a"),
          ConversionFactorFromSIFormula = "Factors.Day / Factors.GallonUK"
        },
        new UnitChoice
        {
          UnitName = "US gallon per day",
          UnitLabel = "gpdUS",
          ID = new Guid("c0a47cc8-8ba1-47a1-ab94-d3e4361dd063"),
          ConversionFactorFromSIFormula = "Factors.Day / Factors.GallonUS"
        },
        new UnitChoice
        {
          UnitName = "million UK gallon per day",
          UnitLabel = "MillionUKGallonPerDay",
          ID = new Guid("f083576e-1d99-4243-bcfa-5ca6093b6931"),
          ConversionFactorFromSIFormula = "Factors.Day / (Factors.Mega*Factors.GallonUK)"
        },
        new UnitChoice
        {
          UnitName = "million US gallon per day",
          UnitLabel = "MillionUSGallonPerDay",
          ID = new Guid("ffe9958a-3daa-472d-87ab-0b1217dcb1c5"),
          ConversionFactorFromSIFormula = "Factors.Day / (Factors.Mega*Factors.GallonUS)"
        },
        new UnitChoice
        {
          UnitName = "litre per day",
          UnitLabel = "L/d",
          ID = new Guid("67628fe2-6a64-4ea6-94fe-5a34e7568b53"),
          ConversionFactorFromSIFormula = "Factors.Day / Factors.Milli"
        },
        new UnitChoice
        {
          UnitName = "million liter per day",
          UnitLabel = "MillionLiterPerDay",
          ID = new Guid("1889c1dd-5aa3-45fe-b6cb-79a760e44832"),
          ConversionFactorFromSIFormula = "Factors.Day / (Factors.Mega*Factors.Milli)"
        },
        new UnitChoice
        {
          UnitName = "cubic foot per day",
          UnitLabel = "ft³/d",
          ID = new Guid("6b9a886b-b96a-473b-8dea-e850583ad5d8"),
          ConversionFactorFromSIFormula = "Factors.Day / (Factors.Foot*Factors.Foot*Factors.Foot)"
        },
        new UnitChoice
        {
          UnitName = "thousand standard cubic foot per day",
          UnitLabel = "Mscf/d",
          ID = new Guid("627a75ed-2ab8-480a-96be-b9266d34ba5d"),
          ConversionFactorFromSIFormula = "Factors.Day / (1000.0*Factors.Foot*Factors.Foot*Factors.Foot)"
        },
        new UnitChoice
        {
          UnitName = "million standard cubic foot per day",
          UnitLabel = "MMscf/d",
          ID = new Guid("f1c76ab3-8e54-4ad1-a7fe-a09be2a61285"),
          ConversionFactorFromSIFormula = "Factors.Day / (Factors.Mega*Factors.Foot*Factors.Foot*Factors.Foot)"
        },
        new UnitChoice
        {
          UnitName = "barrel per day",
          UnitLabel = "bbl/d",
          ID = new Guid("9d36ce54-5bdc-4f4b-9948-dd65c58c9db3"),
          ConversionFactorFromSIFormula = "Factors.Day / Factors.Barrel"
        },
        new UnitChoice
        {
          UnitName = "cubic metre per year",
          UnitLabel = "m³/year",
          ID = new Guid("f0e95734-b6a0-4081-b022-8c5bc0e7dd64"),
          ConversionFactorFromSIFormula = "Factors.YearJulian/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "litre per year",
          UnitLabel = "L/year",
          ID = new Guid("d2b8abd2-cd97-4933-8e0a-54d8a4eef7ce"),
          ConversionFactorFromSIFormula = "Factors.YearJulian/Factors.Milli"
        },
        new UnitChoice
        {
          UnitName = "cubic foot per year",
          UnitLabel = "ft³/year",
          ID = new Guid("5ae7fcd3-fae0-4d81-abca-4c3d36d49e6d"),
          ConversionFactorFromSIFormula = "Factors.YearJulian / (Factors.Foot*Factors.Foot*Factors.Foot)"
        },
        new UnitChoice
        {
          UnitName = "UK gallon per year",
          UnitLabel = "gpyUK",
          ID = new Guid("b7f54c43-a2ee-4b27-bccb-4c0e752e4caf"),
          ConversionFactorFromSIFormula = "Factors.YearJulian /Factors.GallonUK"
        },
        new UnitChoice
        {
          UnitName = "US gallon per year",
          UnitLabel = "gpyUS",
          ID = new Guid("94558d1e-fbe2-4f06-a985-44210a1f0bc8"),
          ConversionFactorFromSIFormula = "Factors.YearJulian /Factors.GallonUS"
        },
        new UnitChoice
        {
          UnitName = "barrel per year",
          UnitLabel = "bbl/year",
          ID = new Guid("0360ea30-fbf5-4dda-bf99-670dd6983420"),
          ConversionFactorFromSIFormula = "Factors.YearJulian /Factors.Barrel"
        }
        };
        public VolumetricFlowRateQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volumetric flow rate", "flow rate (volumetric)" };
            ID = new Guid("9c4eb2bc-413f-456e-ae6b-b1055be8e839");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A volumetric flowrate is the volume of fluid that passes per unit time: $\frac{dV}{dt}$, where $V$ is a volume and $t$ is time." + Environment.NewLine;
            DescriptionMD += @"The dimension of volumetric flowrate is:" + Environment.NewLine;
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
