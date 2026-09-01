using NORCE.Drilling.DrillingFluid.ModelShared;
using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class FlowCurve
    {

        /// <summary>
        /// Rheometer uses rheometer data
        /// </summary>
        public CouetteRheometer CouetteRheometer { get; set; }
        /// <summary>
        /// a table with measurements compatible with OSDC microservice YPL 
        /// </summary>        
        public List<FlowCurveTable> TableValues { get; set; }
        public FlowCurve() : base()
        {
        }
    }
}
