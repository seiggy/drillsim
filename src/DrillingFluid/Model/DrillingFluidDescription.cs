using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class DrillingFluidDescription
    {
        /// <summary>
        /// a MetaInfo for the MyBaseData
        /// </summary>
        public MetaInfo? MetaInfo { get; set; }

        /// <summary>
        /// name of the data
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// a description of the data
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// the date when the data was created
        /// </summary>
        public DateTimeOffset? CreationDate { get; set; }

        /// <summary>
        /// the date when the data was last modified
        /// </summary>
        public DateTimeOffset? LastModificationDate { get; set; }

        /// <summary>
        /// a parameter defined as a Gaussian distribution 
        /// </summary>
        public DrillingFluidComposition DrillingFluidComposition { get; set; }

        /// <summary>
        /// PVT parameters for the drilling fluid mix
        /// </summary>
        public FluidPVTParameters? FluidPVTParameters { get; set; }

        /// <summary>
        /// Drilling fluid mix Density
        /// </summary>
        public GaussianDrillingProperty? FluidMassDensity { get; set; }
        
        /// <summary>
        /// Drilling fluid mix Density
        /// </summary>
        public GaussianDrillingProperty ReferenceTemperature { get; set; } = new GaussianDrillingProperty{Mean = 283};

        /// <summary>
        /// an input list of GellingProperties data
        /// </summary>
        public GellingProperties GellingProperties { get; set; }
        /// <summary>
        /// a parameter defined as a Gaussian distribution 
        /// </summary>
        public FlowCurve FlowCurve { get; set; }
        public SpecificHeatCapacity? SpecificHeatCapacity { get; set; }
        public ThermalConductivity? ThermalConductivity { get; set; }
        public DrillingFluidDescription() : base()
        {
        }
    }
}
