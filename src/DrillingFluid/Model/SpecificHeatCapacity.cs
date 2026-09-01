using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Dynamic;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class SpecificHeatCapacity
    {
        /// <summary>
        /// Slope of the linearized heat capacity
        /// </summary>
        public GaussianDrillingProperty Slope { get; set; }
        /// <summary>
        /// Linearized constant of T = 0 for the heat capacity
        /// </summary>
        public GaussianDrillingProperty HeatCapacityAtZeroKelvin { get; set; }
        /// <summary>
        /// Temperature range of the linearizations
        /// </summary>
        public double?[] TemperatureRange { get; set; }
        /// <summary>
        /// Specific heat coefficient range for the given temperatures
        /// </summary>
        public double?[] CoefficientRange { get; set; }
        
        public SpecificHeatCapacity() : base()
        {
        }
    }
}
