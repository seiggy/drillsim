using OSDC.DotnetLibraries.General.DataManagement;
using System;

namespace NORCE.Drilling.CartographicProjection.Model
{
    /// <summary>
    /// Light weight version of a CartographicConversionSet
    /// Used to avoid loading the complete CartographicConversionSet (heavy weight data) each time we only need contextual info on the data
    /// Typically used for listing, sorting and filtering purposes
    /// </summary>
    public class CartographicConversionSetLight
    {
        /// <summary>
        /// a MetaInfo for the CartographicConversionSetLight
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
        /// the name of the reference cartographic projection
        /// </summary>
        public string? CartographicProjectionName { get; set; }
        /// <summary>
        /// the description of the reference cartographic projection
        /// </summary>
        public string? CartographicProjectionDescription { get; set; }
        /// <summary>
        /// default constructor required for parsing the data model as a json file
        /// </summary>
        public CartographicConversionSetLight() : base()
        {
        }

        /// <summary>
        /// base constructor
        /// </summary>
        public CartographicConversionSetLight(MetaInfo? metaInfo, string? name, string? descr, DateTimeOffset? creationDate, DateTimeOffset? modifDate, string? cartographicProjectionName, string? cartographicProjectionDescr)
        {
            MetaInfo = metaInfo;
            Name = name;
            Description = descr;
            CreationDate = creationDate;
            LastModificationDate = modifDate;
            CartographicProjectionName = cartographicProjectionName;
            CartographicProjectionDescription = cartographicProjectionDescr;
        }
    }
}
