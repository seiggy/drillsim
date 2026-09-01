using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;


namespace NORCE.Drilling.GeothermalProperties.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class GeothermalProperties
    {
        /// <summary>
        /// a MetaInfo for the GeothermalProperties
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
        /// ID of the WellBore
        /// </summary>
        public Guid? WellBoreID { get; set; } = null;

        /// <summary>
        /// ID of the Trajectory
        /// </summary>
        public Guid? TrajectoryID { get; set; } = null;

        /// <summary>
        /// the date when the data was created
        /// </summary>
        public DateTimeOffset? CreationDate { get; set; }

        /// <summary>
        /// the date when the data was last modified
        /// </summary>
        public DateTimeOffset? LastModificationDate { get; set; }

        /// <summary>
        /// Type of the geothermal properties table. Raw data or processed.
        /// </summary>
        public TableType TableType { get; set; }

        /// <summary>
        /// Defines a gethermal table  
        /// </summary>
        public List<GeothermalData> GeothermalDataList { get; set; }
     
        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public GeothermalProperties() : base()
        {
        }
    }
}
