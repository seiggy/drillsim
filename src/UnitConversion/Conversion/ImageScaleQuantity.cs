using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ImageScaleQuantity : DerivedBasePhysicalQuantity
    {
        public override string SIUnitName { get; } = "dot per metre";
        public override string SIUnitLabelLatex { get; } = "dpm";
        public override double? MeaningfulPrecisionInSI { get; } = 0.001;

        public override double LengthDimension { get; } = -1;

        private static ImageScaleQuantity instance_ = null;

        public static ImageScaleQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ImageScaleQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "dot per metre",
                UnitLabel = "dpm",
                ID = new Guid("acc723b8-083c-49f3-a208-184d2da3347d"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true
            },
            new UnitChoice
            {
                UnitName = "dot per inch",
                UnitLabel = "dpi",
                ID = new Guid("e042b571-b7d0-477d-abf6-8b8998e5ba6c"),
                ConversionFactorFromSIFormula = "Factors.Inch"
            },
            new UnitChoice
            {
                UnitName = "dot per millimetre",
                UnitLabel = "dpmm",
                ID = new Guid("6d4d5f26-8812-4002-a2bf-27ec7871c1f4"),
                ConversionFactorFromSIFormula = "Factors.Milli"
            },
            new UnitChoice
            {
                UnitName = "dot per micrometre",
                UnitLabel = "dpµm",
                ID = new Guid("76e21d1d-54f5-4bbb-81c6-1b92b8b30bfe"),
                ConversionFactorFromSIFormula = "Factors.Micro"
            }
        };

        public ImageScaleQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "image scale" };
            ID = new Guid("a3f230b0-a70b-40dd-9305-39e63bb1821b");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Image scale refers to the resolution or level of detail in an image." + Environment.NewLine;
            DescriptionMD += @"The dimension of image scale is:" + Environment.NewLine;
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
