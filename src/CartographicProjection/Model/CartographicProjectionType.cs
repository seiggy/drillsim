using System;
using System.Collections.Generic;

namespace NORCE.Drilling.CartographicProjection.Model
{
    public enum AxisType {None, x, y }

    public enum AreaNormalizationTransformType { None, Linear, Quadratic, Tangent }

    public enum ProjectionType
    {
        Unknown,
        UTM,
        LambertConformalConic,
        LambertConformalConicAlternative,
        LambertEqualAreaConic,
        TransverseMercator,
        Mercator,
        Polyconic,
        ModifiedStereographicAlaska,
        AlbersEqualArea,
        AzimuthalEquidistant,
        Aitoff,
        Bonne,
        CalCoopOceanFish,
        Cassini,
        CentralCylinder,
        EqualAreaCylindrical,
        EquidistantCylindrical,
        EquidistantConic,
        GeostationarySatelliteView,
        GeneralSinusoidalSeries,
        ModifiedStereographic48US,
        ModifiedStereographic50US,
        InternationalMapWorldPolyconic,
        Laborde,
        LambertAzimuthalEqualArea,
        Lagrange,
        LeeOblatedStereographic,
        SpaceObliqueLandsat,
        McBrydeThomasFlatPolarSinusoidal,
        MillerOblatedStereographic,
        SpaceObliqueMISR,
        NewZealandMapGrid,
        ObliqueMercator,
        Orthographic,
        QuadrilateralizedSphericalCube,
        RoussilheStereographic,
        S2,
        SphericalCrossTrackHeight,
        Sinusoidal,
        SwissObliqueMercator,
        Stereographic,
        ObliqueStereographicAlternative,
        UniversalPolarStereographic,
        UrmaevV,
        WebMercator
    }

    /// <summary>
    /// a base class other classes derive from
    /// derived classes derive from base class through aggregation and the use of a discriminating type
    /// C# formal inheritance is not used to preserve the simplicity of the serialization process
    /// ASSUMPTION: for each CartographicProjectionType instance, one, and only one, of the derived data should be instanciated, while others remain null
    /// </summary>
    public class CartographicProjectionType
    {
        private static Dictionary<ProjectionType, CartographicProjectionType>? dictionary_ = null;
        /// <summary>
        /// return the reference prototype for the requested projection
        /// </summary>
        /// <param name="projection"></param>
        /// <returns></returns>
        public static CartographicProjectionType? Get(ProjectionType projection)
        {
            if (dictionary_ == null)
            {
                Initialize();
            }
            CartographicProjectionType? proto = null;
            dictionary_!.TryGetValue(projection, out proto);
            return proto;
        }

        public static IEnumerable<ProjectionType> Get()
        {
            if (dictionary_ == null)
            {
                Initialize();
            }
            return dictionary_!.Keys;
        }

        public static IEnumerable<CartographicProjectionType> GetAll()
        {
            if (dictionary_ == null)
            {
                Initialize();
            }
            return dictionary_!.Values;
        }

        private static void Initialize()
        {
            dictionary_ = new Dictionary<ProjectionType, CartographicProjectionType>();
            foreach (ProjectionType proj in Enum.GetValues(typeof(ProjectionType)))
            {
                CartographicProjectionType prototype = new CartographicProjectionType(proj);
                dictionary_.Add(proj, prototype);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ProjectionType Projection
        {
            get; private set;
        }
        /// <summary>
        /// indicate whether the latitude  origin should be used by the projection or not
        /// </summary>
        public bool UseLatitudeOrigin { get; private set; } = false;
        /// <summary>
        /// indicate whether latitude 1 shall be used or not
        /// </summary>
        public bool UseLatitude1 { get; private set; } = false;
        /// <summary>
        /// indicate whether latitude 2 shall be used or not
        /// </summary>
        public bool UseLatitude2 { get; private set; } = false;
        /// <summary>
        /// indicate whether the latitude of true scale shall be used or not
        /// </summary>
        public bool UseLatitudeTrueScale { get; private set; }
        /// <summary>
        /// indicate whether the longitude  origin should be used by the projection or not
        /// </summary>
        public bool UseLongitudeOrigin { get; private set; } = false;
        /// <summary>
        /// indicate whether a scaling shall be used or not
        /// </summary>
        public bool UseScaling { get; private set; } = false;
        /// <summary>
        /// indicate whether the false northing and easting should be used the projection or not
        /// </summary>
        public bool UseFalseEastingNorthing { get; private set; } = false;
        /// <summary>
        /// indicate whether the UTM zone shall be used or not
        /// </summary>
        public bool UseZone { get; private set; } = false;
        /// <summary>
        /// indicate whether the south hemisphere parameter shall be used or not
        /// </summary>
        public bool UseSouth { get; private set; } = false;
        /// <summary>
        /// indicate whether the hyperbolic flag shall be used or not
        /// </summary>
        public bool UseHyperbolic { get; private set; } = false;
        /// <summary>
        /// indicate whether the projection height shall be used or not
        /// </summary>
        public bool UseProjectionHeight { get; private set; } = false;
        /// <summary>
        /// indicate whether the height view point shall be used or not
        /// </summary>
        public bool UseHeightViewPoint { get; private set; } = false;
        /// <summary>
        /// indicate whether the axis sweep shall be used 
        /// </summary>
        public bool UseSweep { get; private set; } = false;
        /// <summary>
        /// indicate whether the azimuth of central line shall be used or not
        /// </summary>
        public bool UseAzimuthCentralLine { get; private set; } = false;
        /// <summary>
        /// indicate whether the weight parameter shall be used
        /// </summary>
        public bool UseWeight { get; private set; } = false;
        /// <summary>
        /// indicate whether the landsat satellite parameter shall be used or not
        /// </summary>
        public bool UseLandsat { get; private set; } = false;
        /// <summary>
        /// indicate whether the path parameter shall be used or not
        /// </summary>
        public bool UsePath { get; private set; } = false;
        /// <summary>
        /// indicate whether the azimuth centerline at the center point of the line shall be used or not
        /// </summary>
        public bool UseAlpha { get; private set; } = false;
        /// <summary>
        /// indicate whether the azimuth centerline of the rectified bearing of centre line
        /// </summary>
        public bool UseGamma { get; private set; } = false;
        /// <summary>
        /// indicate whether the Longitude 1 shall be used or not
        /// </summary>
        public bool UseLongitude1 { get; private set; } = false;
        /// <summary>
        /// indicate whether the Longitude 2 shall be used or not
        /// </summary>
        public bool UseLongitude2 { get; private set; } = false;
        /// <summary>
        /// indicate whether the Longitude of central point shall be used or not
        /// </summary>
        public bool UseLongitudeCentralPoint { get; private set; } = false;
        /// <summary>
        /// indicate if the no offset option shall be used or not
        /// </summary>
        public bool UseNoOffset { get; private set; } = false;
        /// <summary>
        /// indicate whether the no rotation option shall be used or not
        /// </summary>
        public bool UseNoRotation { get; private set; } = false;
        /// <summary>
        /// indicate
        /// </summary>
        public bool UseAreaNormalizationTransform { get; private set; } = false;
        /// <summary>
        /// indicate whethere the peg latitude shall be used or not
        /// </summary>
        public bool UsePegLatitude { get; private set; } = false;
        /// <summary>
        /// indicate whethere the peg longitude shall be used or not
        /// </summary>
        public bool UsePegLongitude { get; private set; } = false;
        /// <summary>
        /// indicate whethere the peg heading shall be used or not
        /// </summary>
        public bool UsePegHeading { get; private set; } = false;
        /// <summary>
        /// indicate whether the n parameter shall be used or not
        /// </summary>
        public bool UseN { get; private set; } = false;
        /// <summary>
        /// indicate whether the q parameter shall be used or not
        /// </summary>
        public bool UseQ { get; private set; } = false;
        /// <summary>
        /// default constructor: it should not be used
        /// However it is necessary for the compatibility with json serialization
        /// </summary>
        public CartographicProjectionType()
        {
        }
        /// <summary>
        /// private initialization constructor
        /// </summary>
        /// <param name="projectionType"></param>
        private CartographicProjectionType(ProjectionType projectionType)
        {
            Projection = projectionType;
            switch (projectionType)
            {
                case ProjectionType.UTM:
                    UseZone = true;
                    UseSouth = true;
                    break;
                case ProjectionType.LambertConformalConic:
                    UseLatitudeOrigin = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    UseLatitude1 = true;
                    UseLatitude2 = true;
                    UseScaling = true;
                    break;
                case ProjectionType.LambertConformalConicAlternative:
                    UseLatitudeOrigin = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.LambertEqualAreaConic:
                    UseLatitude1 = true;
                    UseSouth = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.TransverseMercator:
                    UseLongitudeOrigin = true;
                    UseLatitudeOrigin = true;
                    UseScaling = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.Mercator:
                    UseLatitudeTrueScale = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    UseScaling = true;
                    break;
                case ProjectionType.Polyconic:
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.ModifiedStereographicAlaska:
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.AlbersEqualArea:
                    UseLatitude1 = true;
                    UseLatitude2 = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.AzimuthalEquidistant:
                    UseLatitudeOrigin = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.Aitoff:
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.Bonne:
                    UseLatitude1 = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.CalCoopOceanFish:
                    break;
                case ProjectionType.Cassini:
                    UseLatitudeOrigin = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    UseHyperbolic = true;
                    break;
                case ProjectionType.CentralCylinder:
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.EqualAreaCylindrical:
                    UseLatitudeTrueScale = true;
                    UseLongitudeOrigin = true;
                    UseScaling = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.EquidistantCylindrical:
                    UseLongitudeOrigin = true;
                    UseLatitudeOrigin = true;
                    UseLatitudeTrueScale = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.EquidistantConic:
                    UseLatitude1 = true;
                    UseLatitude2 = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.GeostationarySatelliteView:
                    UseHeightViewPoint = true;
                    UseSweep = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.GeneralSinusoidalSeries:
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.ModifiedStereographic48US:
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.ModifiedStereographic50US:
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.InternationalMapWorldPolyconic:
                    UseLatitude1 = true;
                    UseLatitude2 = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.Laborde:
                    UseLongitudeOrigin = true;
                    UseLatitudeOrigin = true;
                    UseAzimuthCentralLine = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.LambertAzimuthalEqualArea:
                    UseLongitudeOrigin = true;
                    UseLatitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.Lagrange:
                    UseWeight = true;
                    UseLongitudeOrigin = true;
                    UseLatitude1 = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.LeeOblatedStereographic:
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.SpaceObliqueLandsat:
                    UseLandsat = true;
                    UsePath = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.McBrydeThomasFlatPolarSinusoidal:
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.MillerOblatedStereographic:
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.SpaceObliqueMISR:
                    UsePath = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.NewZealandMapGrid:
                    break;
                case ProjectionType.ObliqueMercator:
                    UseAlpha = true;
                    UseGamma = true;
                    UseLongitudeCentralPoint = true;
                    UseLatitudeOrigin = true;
                    UseLongitude1 = true;
                    UseLatitude1 = true;
                    UseLongitude2 = true;
                    UseLatitude2 = true;
                    UseNoRotation = true;
                    UseNoOffset = true;
                    UseScaling = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.Orthographic:
                    UseLongitudeOrigin = true;
                    UseLatitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.QuadrilateralizedSphericalCube:
                    UseLongitudeOrigin = true;
                    UseLatitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.RoussilheStereographic:
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.S2:
                    UseLongitudeOrigin = true;
                    UseLatitudeOrigin = true;
                    UseAreaNormalizationTransform = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.SphericalCrossTrackHeight:
                    UsePegLatitude = true;
                    UsePegLongitude = true;
                    UsePegHeading = true;
                    UseProjectionHeight = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.Sinusoidal:
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.SwissObliqueMercator:
                    UseLongitudeOrigin = true;
                    UseScaling = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.Stereographic:
                    UseLatitudeOrigin = true;
                    UseLatitudeTrueScale = true;
                    UseScaling = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.ObliqueStereographicAlternative:
                    UseLongitudeOrigin = true;
                    UseLatitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.UniversalPolarStereographic:
                    UseSouth = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.UrmaevV:
                    UseN = true;
                    UseQ = true;
                    UseAlpha = true;
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                case ProjectionType.WebMercator:
                    UseLongitudeOrigin = true;
                    UseFalseEastingNorthing = true;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// return the projection type argument for Proj4
        /// </summary>
        /// <returns></returns>
        public string GetProj4String()
        {
            string sval = "";
            switch (Projection)
            {
                case ProjectionType.UTM:
                    sval += " +proj=utm";
                    break;
                case ProjectionType.LambertConformalConic:
                    sval += " +proj=lcc";
                    break;
                case ProjectionType.LambertConformalConicAlternative:
                    sval += " +proj=lcca";
                    break;
                case ProjectionType.LambertEqualAreaConic:
                    sval += " +proj=leac";
                    break;
                case ProjectionType.TransverseMercator:
                    sval += " +proj=tmerc";
                    break;
                case ProjectionType.Mercator:
                    sval += " +proj=merc";
                    break;
                case ProjectionType.Polyconic:
                    sval += " +proj=poly";
                    break;
                case ProjectionType.ModifiedStereographicAlaska:
                    sval += " +proj=alsk";
                    break;
                case ProjectionType.AlbersEqualArea:
                    sval += " +proj=aea";
                    break;
                case ProjectionType.AzimuthalEquidistant:
                    sval += " +proj=aeqd";
                    break;
                case ProjectionType.Aitoff:
                    sval += " +proj=aitoff";
                    break;
                case ProjectionType.Bonne:
                    sval += " +proj=bonne";
                    break;
                case ProjectionType.CalCoopOceanFish:
                    sval += " +proj=calcofi";
                    break;
                case ProjectionType.Cassini:
                    sval += " +proj=cass";
                    break;
                case ProjectionType.CentralCylinder:
                    sval += " +proj=cc";
                    break;
                case ProjectionType.EqualAreaCylindrical:
                    sval += " +proj=cea";
                    break;
                case ProjectionType.EquidistantCylindrical:
                    sval += " +proj=eqc";
                    break;
                case ProjectionType.EquidistantConic:
                    sval += " +proj=eqdc";
                    break;
                case ProjectionType.GeostationarySatelliteView:
                    sval += " +proj=geos";
                    break;
                case ProjectionType.GeneralSinusoidalSeries:
                    sval += " +proj=gn_sinu";
                    break;
                case ProjectionType.ModifiedStereographic48US:
                    sval += " +proj=gs48";
                    break;
                case ProjectionType.ModifiedStereographic50US:
                    sval += " +proj=gs50";
                    break;
                case ProjectionType.InternationalMapWorldPolyconic:
                    sval += " +proj=imw_p";
                    break;
                case ProjectionType.Laborde:
                    sval += " +proj=labrd";
                    break;
                case ProjectionType.LambertAzimuthalEqualArea:
                    sval += " +proj=laea";
                    break;
                case ProjectionType.Lagrange:
                    sval += " +proj=lagrng";
                    break;
                case ProjectionType.LeeOblatedStereographic:
                    sval += " +proj=lee_as";
                    break;
                case ProjectionType.SpaceObliqueLandsat:
                    sval += " +proj=lsat";
                    break;
                case ProjectionType.McBrydeThomasFlatPolarSinusoidal:
                    sval += " +proj=mbtfps";
                    break;
                case ProjectionType.MillerOblatedStereographic:
                    sval += " +proj=mil_os";
                    break;
                case ProjectionType.SpaceObliqueMISR:
                    sval += " +proj=misrson";
                    break;
                case ProjectionType.NewZealandMapGrid:
                    sval += " +proj=nzmg";
                    break;
                case ProjectionType.ObliqueMercator:
                    sval += " +proj=omerc";
                    break;
                case ProjectionType.Orthographic:
                    sval += " +proj=ortho";
                    break;
                case ProjectionType.QuadrilateralizedSphericalCube:
                    sval += " +proj=qsc";
                    break;
                case ProjectionType.RoussilheStereographic:
                    sval += " +proj=rouss";
                    break;
                case ProjectionType.S2:
                    sval += " +proj=s2";
                    break;
                case ProjectionType.SphericalCrossTrackHeight:
                    sval += " +proj=sch";
                    break;
                case ProjectionType.Sinusoidal:
                    sval += " +proj=sinu";
                    break;
                case ProjectionType.SwissObliqueMercator:
                    sval += " +proj=somerc";
                    break;
                case ProjectionType.Stereographic:
                    sval += " +proj=stere";
                    break;
                case ProjectionType.ObliqueStereographicAlternative:
                    sval += " +proj=sterea";
                    break;
                case ProjectionType.UniversalPolarStereographic:
                    sval += " +proj=ups";
                    break;
                case ProjectionType.UrmaevV:
                    sval += " +proj=urm5";
                    break;
                case ProjectionType.WebMercator:
                    sval += " +proj=webmerc";
                    break;
                default:
                    break;
            }
            return sval;
        }
    }

}
