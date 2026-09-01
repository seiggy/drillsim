using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class ThermalConductivity
    {  
        /// <summary>
        /// Slope of the linearized thermal conductivity
        /// </summary>
        public GaussianDrillingProperty Slope { get; set; }
        /// <summary>
        /// Linearized constant of T = 0 for the thermal conductivity
        /// </summary>
        public GaussianDrillingProperty ThermalConductivityAtZeroKelvin { get; set; }
        /// <summary>
        /// Temperature range of the linearizations
        /// </summary>
        public double?[] TemperatureRange { get; set; }
        /// <summary>
        /// Thermal conductivity range for the given temperatures
        /// </summary>
        public double?[] CoefficientRange { get; set; }
        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public ThermalConductivity() : base()
        {
        }
    }
}
