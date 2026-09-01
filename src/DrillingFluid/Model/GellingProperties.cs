using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class GellingProperties
    {
        /// <summary>
        /// A List of the properties 
        /// </summary>
        public List<GelData>? GelPropertiesTable { get; set; }

        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public GellingProperties() : base()
        {
        }
    }
}
