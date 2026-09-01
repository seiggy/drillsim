using OSDC.UnitConversion.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class SpecificVolumeSquaredQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "cubic metre squared per kilogram squared";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^{6}}{kg^{2}}";
        public override double MassDimension { get; } = -2;
        public override double LengthDimension { get; } = 6;
        private static SpecificVolumeSquaredQuantity instance_ = null;

        public static SpecificVolumeSquaredQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new SpecificVolumeSquaredQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre squared per kilogram squared",
                UnitLabel = "m⁶/kg²",
                ID = new Guid("9c15bf12-237d-486f-bba4-a2fbfc4e8478"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true
            },
            new UnitChoice
            {
                UnitName = "cubic metre squared per gram squared",
                UnitLabel = "m⁶/g²",
                ID = new Guid("b3e7d7b9-5671-4a15-bb70-7885bd6540c5"),
                ConversionFactorFromSIFormula = "Factors.Milli*Factors.Milli/1.0"
            },
            new UnitChoice
            {
                UnitName = "cubic decimetre squared per gram squared",
                UnitLabel = "dm⁶/g²",
                ID = new Guid("f29c66ae-2cf0-459d-af24-34e62a350904"),
                ConversionFactorFromSIFormula = "Factors.Milli*Factors.Milli/(Factors.Deci*Factors.Deci*Factors.Deci*Factors.Deci*Factors.Deci*Factors.Deci)"
            },
            new UnitChoice
            {
                UnitName = "litre squared per gram squared",
                UnitLabel = "L²/g²",
                ID = new Guid("5e37afb2-6def-422c-963c-6d7377421a66"),
                ConversionFactorFromSIFormula = "Factors.Milli*Factors.Milli/(Factors.Litre*Factors.Litre)"
            },
            new UnitChoice
            {
                UnitName = "decilitre squared per gram squared",
                UnitLabel = "dL²/g²",
                ID = new Guid("7707bae3-e596-4d4f-9e52-c862236abe40"),
                ConversionFactorFromSIFormula = "Factors.Milli*Factors.Milli/(Factors.Litre*Factors.Deci*Factors.Litre*Factors.Deci)"
            },
            new UnitChoice
            {
                UnitName = "centilitre squared per gram squared",
                UnitLabel = "cL²/g²",
                ID = new Guid("7e344fe0-8487-4d24-99e8-8ef370e01c99"),
                ConversionFactorFromSIFormula = "Factors.Milli*Factors.Milli/(Factors.Litre*Factors.Centi*Factors.Litre*Factors.Centi)"
            },
            new UnitChoice
            {
                UnitName = "millilitre squared per gram squared",
                UnitLabel = "mL²/g²",
                ID = new Guid("119533f4-6a22-4d16-8927-6cf60b6919c1"),
                ConversionFactorFromSIFormula = "Factors.Milli*Factors.Milli/(Factors.Litre*Factors.Milli*Factors.Litre*Factors.Milli)"
            },
            new UnitChoice
            {
                UnitName = "cubic centimetre squared per gram squared",
                UnitLabel = "cm⁶/g²",
                ID = new Guid("d1331578-5ea9-4e15-a687-0eaef32d5197"),
                ConversionFactorFromSIFormula = "Factors.Milli*Factors.Milli/(Factors.Centi*Factors.Centi*Factors.Centi*Factors.Centi*Factors.Centi*Factors.Centi)"
            },
            new UnitChoice
            {
                UnitName = "cubic millimetre squared per gram squared",
                UnitLabel = "mm⁶/g²",
                ID = new Guid("16048716-100d-4089-af27-3ba731defa11"),
                ConversionFactorFromSIFormula = "Factors.Milli*Factors.Milli/(Factors.Milli*Factors.Milli*Factors.Milli*Factors.Milli*Factors.Milli*Factors.Milli)"
            },
            new UnitChoice
            {
                UnitName = "litre squared per kilogram squared",
                UnitLabel = "L²/kg²",
                ID = new Guid("0b1b47d5-fdb3-47af-9f80-ea9193a60fa4"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Litre*Factors.Litre)"
            },
            new UnitChoice
            {
                UnitName = "decilitre squared per kilogram squared",
                UnitLabel = "dL²/kg²",
                ID = new Guid("c4fe5cce-8106-4640-b70a-a915faf84317"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Litre*Factors.Deci*Factors.Litre*Factors.Deci)"
            },
            new UnitChoice
            {
                UnitName = "centilitre squared per kilogram squared",
                UnitLabel = "cL²/kg²",
                ID = new Guid("c9130aa9-0f78-4e56-91b4-e8c397e48f34"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Litre*Factors.Centi*Factors.Litre*Factors.Centi)"
            },
            new UnitChoice
            {
                UnitName = "millilitre squared per kilogram squared",
                UnitLabel = "mL²/kg²",
                ID = new Guid("7d9a4f75-0e77-4eb7-9690-dadad54690cb"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Litre*Factors.Milli*Factors.Litre*Factors.Milli)"
            },
            new UnitChoice
            {
                UnitName = "cubic centimetre squared per kilogram squared",
                UnitLabel = "cm⁶/kg²",
                ID = new Guid("037ab139-0665-489c-8b5c-8183e738059c"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Centi*Factors.Centi*Factors.Centi*Factors.Centi*Factors.Centi*Factors.Centi)"
            },
            new UnitChoice
            {
                UnitName = "cubic millimetre squared per kilogram squared",
                UnitLabel = "mm⁶/kg²",
                ID = new Guid("6c6b8e30-4a7f-496d-9884-7c65124fb09e"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Milli*Factors.Milli*Factors.Milli*Factors.Milli*Factors.Milli*Factors.Milli)"
            },
            new UnitChoice
            {
                UnitName = "cubic yard squared per pound squared",
                UnitLabel = "yd⁶/lb²",
                ID = new Guid("be485c8d-8b4f-451b-bf5a-bef3713bc4a8"),
                ConversionFactorFromSIFormula = "Factors.Pound*Factors.Pound/(Factors.Yard*Factors.Yard*Factors.Yard*Factors.Yard*Factors.Yard*Factors.Yard)"
            },
            new UnitChoice
            {
                UnitName = "cubic feet squared per pound squared",
                UnitLabel = "ft⁶/lb²",
                ID = new Guid("b8599a96-d1bf-4832-aafb-4e79a2f7aa2f"),
                ConversionFactorFromSIFormula = "Factors.Pound*Factors.Pound/(Factors.Foot*Factors.Foot*Factors.Foot*Factors.Foot*Factors.Foot*Factors.Foot)"
            },
            new UnitChoice
            {
                UnitName = "cubic inches squared per pound squared",
                UnitLabel = "in⁶/lb²",
                ID = new Guid("55d2064c-dc72-4c25-bbab-5d94b34dfada"),
                ConversionFactorFromSIFormula = "Factors.Pound*Factors.Pound/(Factors.Inch*Factors.Inch*Factors.Inch*Factors.Inch*Factors.Inch*Factors.Inch)"
            },
            new UnitChoice
            {
                UnitName = "cubic yard squared per ounce squared",
                UnitLabel = "yd⁶/oz²",
                ID = new Guid("41aa2db9-4162-4e89-ab3e-f516eee318fb"),
                ConversionFactorFromSIFormula = "Factors.Ounce*Factors.Ounce/(Factors.Yard*Factors.Yard*Factors.Yard*Factors.Yard*Factors.Yard*Factors.Yard)"
            },
            new UnitChoice
            {
                UnitName = "cubic feet squared per ounce squared",
                UnitLabel = "ft⁶/oz²",
                ID = new Guid("985c778c-2247-48cd-bf75-8658b0cce2e4"),
                ConversionFactorFromSIFormula = "Factors.Ounce*Factors.Ounce/(Factors.Foot*Factors.Foot*Factors.Foot*Factors.Foot*Factors.Foot*Factors.Foot)"
            },
            new UnitChoice
            {
                UnitName = "cubic inches squared per ounce squared",
                UnitLabel = "in⁶/oz²",
                ID = new Guid("ed7d6e1d-2948-4e36-a165-8bb207f320c4"),
                ConversionFactorFromSIFormula = "Factors.Ounce*Factors.Ounce/(Factors.Inch*Factors.Inch*Factors.Inch*Factors.Inch*Factors.Inch*Factors.Inch)"
            },
            new UnitChoice
            {
                UnitName = "gallon UK squared per ounce squared",
                UnitLabel = "galUK²/oz²",
                ID = new Guid("57b49ba7-1aaa-4746-a7dc-7e6578fa2ea3"),
                ConversionFactorFromSIFormula = "Factors.Ounce*Factors.Ounce/(Factors.GallonUK*Factors.GallonUK)"
            },
            new UnitChoice
            {
                UnitName = "gallon US squared per ounce squared",
                UnitLabel = "galUS²/oz²",
                ID = new Guid("1ddf2dc3-a5cd-457d-904d-cfead242bb05"),
                ConversionFactorFromSIFormula = "Factors.Ounce*Factors.Ounce/(Factors.GallonUS*Factors.GallonUS)"
            },
            new UnitChoice
            {
                UnitName = "gallon UK squared per pound squared",
                UnitLabel = "galUK²/lb²",
                ID = new Guid("b689bb7e-7334-41c6-b8de-afe74cec4dd0"),
                ConversionFactorFromSIFormula = "Factors.Pound*Factors.Pound/(Factors.GallonUK*Factors.GallonUK)"
            },
            new UnitChoice
            {
                UnitName = "gallon US squared per pound squared",
                UnitLabel = "galUS²/lb²",
                ID = new Guid("109d07d6-b035-420a-9cee-fb44524ad0fb"),
                ConversionFactorFromSIFormula = "Factors.Pound*Factors.Pound/(Factors.GallonUS*Factors.GallonUS)"
            }
        };
        public SpecificVolumeSquaredQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "specific volume squared" };
            ID = new Guid("a266621c-b583-443f-ae53-1ad46d90252b");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Specific volume squared is the square of the specific volume." + Environment.NewLine;
            DescriptionMD += @"The dimension of specific volume squared is:" + Environment.NewLine;
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

