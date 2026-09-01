using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class GammaRayIndexDrillingQuantity : DimensionlessQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "GammaAPI";
        public override string SIUnitLabelLatex { get; } = "API";
        public override double? MeaningfulPrecisionInSI { get; } = 0.001;
        private static GammaRayIndexDrillingQuantity instance_ = null;

        public static new GammaRayIndexDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new GammaRayIndexDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public GammaRayIndexDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "gamma ray (drilling)" };
            ID = new Guid("15280017-0ac6-4dad-a6e1-1efdb05e64c5");
            DescriptionMD = string.Empty;
            DescriptionMD += "The Gamma Ray API is a unit used in the petroleum industry to measure the natural gamma radiation of formations. Specifically, the Gamma Ray Index (GRI) is used in well logging to measure the gamma ray activity of the rock surrounding the borehole." + Environment.NewLine;
            DescriptionMD += "Gamma rays are a form of electromagnetic radiation, and in well logging, they are used to assess the radioactive content of the formations, which helps in identifying rock types (shales, sandstones, etc.)." + Environment.NewLine;
            DescriptionMD += "Since the Gamma Ray API is a measure of gamma radiation intensity, let's break down its physical nature:" + Environment.NewLine;
            DescriptionMD += "- Gamma rays are photons, and their intensity is measured in counts per second, which can be related to energy or radiation dose." + Environment.NewLine;
            DescriptionMD += "- The Gamma Ray API is a unit-less scale used to compare different gamma ray intensity levels, and therefore the Gamma Ray API is fundamentally dimensionless." + Environment.NewLine;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of gamma ray in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + DimensionlessQuantity.Instance.GetUnitChoice(DimensionlessQuantity.UnitChoicesEnum.Dimensionless).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(DimensionlessQuantity.Instance.GetUnitChoice(DimensionlessQuantity.UnitChoicesEnum.Dimensionless));
            SemanticExample = GetSemanticExample();
        }
    }
}
