using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class CompletedDrillingFluid
    {
        
        public GellingProperties GellingProperties { get; set; }
        public SpecificHeatCapacity SpecificHeatCapacity { get; set; }
        public ThermalConductivity ThermalConductivity { get; set; }
        public GaussianDrillingProperty MassDensity { get; set;  }     
        public FlowCurve PostProcessedFlowCurve { get; set; }
        public FluidPVTParameters FluidPVTParameters { get; set; }
        public List<GenericRheologicalModel> RheologicalModelProperties { get; set; }
        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public CompletedDrillingFluid() : base()
        {
        }
    }
}
