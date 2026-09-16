using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.DotnetLibraries.General.Statistics;
using GeologicalProperties.Model;

namespace GeologicalProperties.ModelTest
{
    public class Tests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
        }
        public Model.GeologicalProperties ConstructGeologicalProperties(MetaInfo metaInfo, Model.GeologicalPropertyEntry geologicalPropertyTableRow1, Model.GeologicalPropertyEntry geologicalPropertyTableRow2)
        {
            DateTimeOffset creationDate = DateTimeOffset.UtcNow;                     
            return new Model.GeologicalProperties()
            {
                MetaInfo = metaInfo,
                Name = "My test GeologicalProperties",
                Description = "My test GeologicalProperties",
                CreationDate = creationDate,
                LastModificationDate = creationDate,
                GeologicalPropertyTable = [geologicalPropertyTableRow1, geologicalPropertyTableRow2]
            };
        }
        public GaussianDrillingProperty ConstructGaussianDrillingProperty(double? val)
        {                        
            return new GaussianDrillingProperty
            {
                GaussianValue = new GaussianDistribution
                {
                    Mean = val
                }
            };
        }
        public InterpolationProperties ConstructInterpolationProperties()
        {
            return new InterpolationProperties
            {
                Interpolate = false,
                InterpolationStep = 1.0
            };
        }
        public ExtrapolationProperties ConstructExtrapolationProperties()
        {
            return new ExtrapolationProperties
            {
                Extrapolate = false,
                ExtrapolationStep = null,
                UsedDataRange = null,
                DepthRange = null
            };
        }
        public Model.GeologicalPropertyEntry ConstructGeologicalPropertiesTable()
        {
            return new Model.GeologicalPropertyEntry()
            {
                InternalFrictionAngle = ConstructGaussianDrillingProperty(null),
                UnconfinedCompressiveStrength = ConstructGaussianDrillingProperty(null),
                ConfinedCompressiveStrength = ConstructGaussianDrillingProperty(null),
                Porosity = ConstructGaussianDrillingProperty(null),
                Permeability = ConstructGaussianDrillingProperty(null),
                MeasuredDepth = ConstructGaussianDrillingProperty(null),
                PressureDifferential = ConstructGaussianDrillingProperty(null),
                DataType = GeologicalPropertyTableOrigin.Measured
            };
        }

        [Test]
        public void Petrophysics_PreservesDepthNullsAndFormationConfidence()
        {
            var artifactId = Guid.NewGuid();
            var geologicalProperties = new Model.GeologicalProperties
            {
                Petrophysics = new PetrophysicsDataset
                {
                    Provenance = new PetrophysicsDatasetProvenance
                    {
                        DatasetName = "FORCE 2020",
                        Classification = PetrophysicsDataClassification.Observed,
                        SourceArtifacts =
                        [
                            new PetrophysicsSourceArtifact
                            {
                                ID = artifactId,
                                License = "CC-BY-4.0",
                                Attribution = "FORCE 2020"
                            }
                        ]
                    },
                    LogRuns =
                    [
                        new PetrophysicsLogRun
                        {
                            ID = Guid.NewGuid(),
                            DepthAxis = new PetrophysicsDepthAxis
                            {
                                Reference = PetrophysicsDepthReference.MeasuredDepth,
                                CanonicalUnit = "m"
                            },
                            DepthValues = [1000, 1000.5, 1001],
                            Curves =
                            [
                                new PetrophysicsLogCurve
                                {
                                    ID = Guid.NewGuid(),
                                    CanonicalMnemonic = "RHOB",
                                    Values = [2.31, null, 2.33],
                                    NullFlags = [false, true, false],
                                    QualityFlags = [null, "source-null", null]
                                }
                            ]
                        }
                    ],
                    FormationTops =
                    [
                        new PetrophysicsFormationTop
                        {
                            ID = Guid.NewGuid(),
                            FormationName = "Example",
                            Confidence = 0.8,
                            Method = "agency reference pick",
                            SourceArtifactID = artifactId,
                            Classification = PetrophysicsDataClassification.Observed,
                            Depths =
                            [
                                new PetrophysicsDepthCoordinate
                                {
                                    Reference = PetrophysicsDepthReference.TrueVerticalDepthSubsea,
                                    OriginalValue = 3116.8,
                                    OriginalUnit = "ftUS",
                                    Value = 950,
                                    Unit = "m",
                                    Datum = "MSL",
                                    PositiveDown = true
                                }
                            ]
                        }
                    ]
                }
            };

            PetrophysicsLogCurve curve = geologicalProperties.Petrophysics.LogRuns![0].Curves![0];
            Assert.That(curve.Values![1], Is.Null);
            Assert.That(curve.NullFlags, Is.EqualTo(new[] { false, true, false }));
            Assert.That(geologicalProperties.Petrophysics.FormationTops![0].Confidence, Is.EqualTo(0.8));
            Assert.That(geologicalProperties.Petrophysics.FormationTops[0].Depths![0].Reference,
                Is.EqualTo(PetrophysicsDepthReference.TrueVerticalDepthSubsea));
            Assert.That(geologicalProperties.Petrophysics.FormationTops[0].Depths[0].OriginalUnit,
                Is.EqualTo("ftUS"));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }
    }
}