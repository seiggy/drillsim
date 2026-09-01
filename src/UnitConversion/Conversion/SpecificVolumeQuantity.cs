using OSDC.UnitConversion.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class SpecificVolumeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "cubic metre per kilogram";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^{3}}{kg}";
        public override double MassDimension { get; } = -1;
        public override double LengthDimension { get; } = 3;
        private static SpecificVolumeQuantity instance_ = null;

        public static SpecificVolumeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new SpecificVolumeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre per kilogram",
                UnitLabel = "m³/kg",
                ID = new Guid("0c321874-be1d-4ca7-8bfe-eac3c2b6e2f4"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true
            },
            new UnitChoice
            {
                UnitName = "cubic metre per gram",
                UnitLabel = "m³/g",
                ID = new Guid("72a258a9-bb34-46b2-8eb7-3123fb054669"),
                ConversionFactorFromSIFormula = "Factors.Milli/1.0"
            },
            new UnitChoice
            {
                UnitName = "cubic decimetre per gram",
                UnitLabel = "dm³/g",
                ID = new Guid("563b1574-04df-4285-a953-20d7da6b528f"),
                ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Deci*Factors.Deci*Factors.Deci)"
            },
            new UnitChoice
            {
                UnitName = "litre per gram",
                UnitLabel = "L/g",
                ID = new Guid("d9206161-2a7a-45e8-9306-f5f9714bea84"),
                ConversionFactorFromSIFormula = "Factors.Milli/Factors.Litre"
            },
            new UnitChoice
            {
                UnitName = "decilitre per gram",
                UnitLabel = "dL/g",
                ID = new Guid("65c84088-e307-4ce2-adfa-bb380e639484"),
                ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Litre*Factors.Deci)"
            },
            new UnitChoice
            {
                UnitName = "centilitre per gram",
                UnitLabel = "cL/g",
                ID = new Guid("e608ef34-e8b9-4134-8692-b72273ddd0af"),
                ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Litre*Factors.Centi)"
            },
            new UnitChoice
            {
                UnitName = "millilitre per gram",
                UnitLabel = "mL/g",
                ID = new Guid("9cc34144-ca23-4d82-b725-c39e1607c356"),
                ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Litre*Factors.Milli)"
            },
            new UnitChoice
            {
                UnitName = "cubic centimetre per gram",
                UnitLabel = "cm³/g",
                ID = new Guid("5ac98a68-f85b-40b9-84d7-f6409bc79944"),
                ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Centi*Factors.Centi*Factors.Centi)"
            },
            new UnitChoice
            {
                UnitName = "cubic millimetre per gram",
                UnitLabel = "mm³/g",
                ID = new Guid("a31a14cf-66d9-4789-8ea5-cca1f874a3f1"),
                ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Milli*Factors.Milli*Factors.Milli)"
            },
            new UnitChoice
            {
                UnitName = "litre per kilogram",
                UnitLabel = "L/kg",
                ID = new Guid("f4551a81-3856-434b-9247-4215d5782052"),
                ConversionFactorFromSIFormula = "1.0/Factors.Litre"
            },
            new UnitChoice
            {
                UnitName = "decilitre per kilogram",
                UnitLabel = "dL/kg",
                ID = new Guid("07e7e24d-6538-4dd2-8c5d-d09e7cf2f006"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Litre*Factors.Deci)"
            },
            new UnitChoice
            {
                UnitName = "centilitre per kilogram",
                UnitLabel = "cL/kg",
                ID = new Guid("f0702462-ab52-4675-ba1e-0d7f86c8c3ea"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Litre*Factors.Centi)"
            },
            new UnitChoice
            {
                UnitName = "millilitre per kilogram",
                UnitLabel = "mL/kg",
                ID = new Guid("b87d48ef-91c1-48a8-8bac-41e774e3c3f0"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Litre*Factors.Milli)"
            },
            new UnitChoice
            {
                UnitName = "cubic centimetre per kilogram",
                UnitLabel = "cm³/kg",
                ID = new Guid("7e258609-fb1f-4ad9-b712-9277427adaa5"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Centi*Factors.Centi*Factors.Centi)"
            },
            new UnitChoice
            {
                UnitName = "cubic millimetre per kilogram",
                UnitLabel = "mm³/kg",
                ID = new Guid("e72a515f-8edc-4049-973c-b9516aba6b61"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Milli*Factors.Milli*Factors.Milli)"
            },
            new UnitChoice
            {
                UnitName = "cubic yard per pound",
                UnitLabel = "yd³/lb",
                ID = new Guid("cb0950fd-ebbb-459a-b542-f041b9388b1b"),
                ConversionFactorFromSIFormula = "Factors.Pound/(Factors.Yard*Factors.Yard*Factors.Yard)"
            },
            new UnitChoice
            {
                UnitName = "cubic feet per pound",
                UnitLabel = "ft³/lb",
                ID = new Guid("7b5fe09f-170e-4a4f-bf30-77d4ffbbbd28"),
                ConversionFactorFromSIFormula = "Factors.Pound/(Factors.Foot*Factors.Foot*Factors.Foot)"
            },
            new UnitChoice
            {
                UnitName = "cubic inches per pound",
                UnitLabel = "in³/lb",
                ID = new Guid("8bf7c8f0-64fe-4785-be5f-5928906aea6a"),
                ConversionFactorFromSIFormula = "Factors.Pound/(Factors.Inch*Factors.Inch*Factors.Inch)"
            },
            new UnitChoice
            {
                UnitName = "cubic yard per ounce",
                UnitLabel = "yd³/oz",
                ID = new Guid("7027900f-a654-482a-8fc0-548f6d68c470"),
                ConversionFactorFromSIFormula = "Factors.Ounce/(Factors.Yard*Factors.Yard*Factors.Yard)"
            },
            new UnitChoice
            {
                UnitName = "cubic feet per ounce",
                UnitLabel = "ft³/oz",
                ID = new Guid("ce834ba1-6ca8-4651-8fbd-1d008113ea1e"),
                ConversionFactorFromSIFormula = "Factors.Ounce/(Factors.Foot*Factors.Foot*Factors.Foot)"
            },
            new UnitChoice
            {
                UnitName = "cubic inches per ounce",
                UnitLabel = "in³/oz",
                ID = new Guid("a23d16bd-f9a2-475d-a76c-e6025940d631"),
                ConversionFactorFromSIFormula = "Factors.Ounce/(Factors.Inch*Factors.Inch*Factors.Inch)"
            },
            new UnitChoice
            {
                UnitName = "gallon UK per ounce",
                UnitLabel = "galUK/oz",
                ID = new Guid("cfe6a1db-5f38-4eb1-8a43-83dd959ee91c"),
                ConversionFactorFromSIFormula = "Factors.Ounce/Factors.GallonUK"
            },
            new UnitChoice
            {
                UnitName = "gallon US per ounce",
                UnitLabel = "galUS/oz",
                ID = new Guid("216ba9e0-5c75-427a-8179-be65050bb773"),
                ConversionFactorFromSIFormula = "Factors.Ounce/Factors.GallonUS"
            },
            new UnitChoice
            {
                UnitName = "gallon UK per pound",
                UnitLabel = "galUK/lb",
                ID = new Guid("2a509c09-5584-4df3-9b03-ef6afa56a0d3"),
                ConversionFactorFromSIFormula = "Factors.Pound/Factors.GallonUK"
            },
            new UnitChoice
            {
                UnitName = "gallon US per pound",
                UnitLabel = "galUS/lb",
                ID = new Guid("8f6021d8-130a-4496-8a88-38b3b30aadfc"),
                ConversionFactorFromSIFormula = "Factors.Pound/Factors.GallonUS"
            }
        };
        public SpecificVolumeQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "specific volume" };
            ID = new Guid("ad0b041e-4bfe-4e4a-9c9f-1b800d2332ba");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Specific volume is the volume occupied by a unit mass of a substance, equal to the reciprocal of its density." + Environment.NewLine;
            DescriptionMD += @"The dimension of specific volume is:" + Environment.NewLine;
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

