using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;

namespace NORCE.Drilling.GeothermalProperties.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class GeothermalData
    {
        public GeothermalPropertiesType RegionType {get; set;} = GeothermalPropertiesType.Air;
        /// <summary>
        /// Vertical Depth
        /// </summary>
        public double? VerticalDepth { get; set; }
        /// <summary>
        /// Temperature in Kelvin
        /// </summary>
        public double? Temperature { get; set; }
        /// <summary>
        /// Temperature gradient in Kelvin/meter
        /// </summary>
        public double? TemperatureGradient { get; set; }
     
        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public GeothermalData() : base()
        {
        }
    }
}
