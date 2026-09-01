using OSDC.DotnetLibraries.Drilling.DrillingProperties;
namespace NORCE.Drilling.DrillingFluid.Model
{
    public class LostCirculationMaterial : BaseCompositionProperties
    {
        /// <summary>     
        /// Type od added grain (fine, coarse...)
        /// </summary>  
        public LostCiruclationMaterialGrainType GrainSize { get; set; }
        /// Specific heat capactiy thermophysical property
        /// </summary>
        /// <summary>
        /// constructor for schema
        /// </summary>
        public LostCirculationMaterial()
        {

        }        
    }
}