using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Persistence;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Tests;

[TestFixture]
public sealed class WorldSetupTests
{
    private WorldSetupTestStore _store = null!;

    [SetUp]
    public async Task CreateStore()
    {
        _store = new WorldSetupTestStore();
        await _store.InitializeAsync();
    }

    [TearDown]
    public async Task DisposeStore() => await _store.DisposeAsync();

    [Test]
    public async Task Catalog_EmptyAndHistoricalScopes_DoNotInventOrMigrateModels()
    {
        WorldGenerationRequest request = TestData.ConditionedRequest();
        WorldSetupProfileCatalog empty = await _store.Manager.GetSetupProfilesAsync(
            request.FieldId, request.ReservoirName);
        ReservoirWorld world = await _store.Manager.CreateAsync(request);
        WorldSetupProfile profile = (await Catalog(request)).Profiles.Single();
        await _store.ExecuteAsync(
            "UPDATE ReservoirWorldSpecs SET ModelVersion = $version, CanonicalRequestJson = $json WHERE WorldId = $id;",
            ("$version", "reservoir-hidden-world-v2"), ("$json", "unsupported historical request"),
            ("$id", world.Summary.WorldId));
        PersistedWorldSpec? historical = await _store.Repository.LoadWorldSpecAsync(world.Summary.WorldId);

        WorldSetupProfileCatalog unsupported = await _store.Manager.GetSetupProfilesAsync(
            request.FieldId, request.ReservoirName);
        ReservoirWorld? unavailable = await _store.Manager.PrepareRealizationAsync(Setup(request, profile.ProfileId));
        PersistedWorldSpec? after = await _store.Repository.LoadWorldSpecAsync(world.Summary.WorldId);

        Assert.Multiple(() =>
        {
            Assert.That(empty.FieldId, Is.EqualTo(request.FieldId));
            Assert.That(empty.ReservoirName, Is.EqualTo(request.ReservoirName));
            Assert.That(empty.Profiles, Is.Empty);
            Assert.That(unsupported.Profiles, Is.Empty);
            Assert.That(unavailable, Is.Null);
            Assert.That(after, Is.EqualTo(historical));
        });
    }

    [Test]
    public async Task Catalog_DeduplicatesSeedAndGridVariants_AndRemainsStableAfterRestartAndSetup()
    {
        WorldGenerationRequest template = FullTemplate();
        await _store.Manager.CreateAsync(template);
        WorldSetupProfile original = (await Catalog(template)).Profiles.Single();
        await _store.Manager.CreateAsync(template with
        {
            Seed = template.Seed + 1,
            Grid = template.Grid with { CountX = 3, CountY = 5, CountZ = 2 },
            ConditioningPoints = template.ConditioningPoints.Reverse().ToArray(),
            StructuralConditioningPoints = template.StructuralConditioningPoints.Reverse().ToArray(),
            FluidContacts = template.FluidContacts with { GasWater = template.FluidContacts.GasWater.Reverse().ToArray() }
        });
        await _store.Manager.PrepareRealizationAsync(Setup(template, original.ProfileId, "Preview", 0));
        await _store.Manager.PrepareRealizationAsync(Setup(template, original.ProfileId, "Standard", int.MaxValue));

        WorldSetupProfileCatalog catalog = await Catalog(template);
        WorldSetupProfileCatalog restarted = await _store.NewManager().GetSetupProfilesAsync(
            template.FieldId, template.ReservoirName);

        Assert.Multiple(() =>
        {
            Assert.That(catalog.Profiles, Is.EqualTo(new[] { original }));
            Assert.That(restarted.Profiles, Is.EqualTo(catalog.Profiles));
            Assert.That(original.ProfileId, Does.Match("^rsp_[0-9a-f]{64}$"));
            Assert.That(original.WorldModelVersion, Is.EqualTo(ReservoirWorldFactory.ModelVersion));
        });
    }

    [TestCase("padding")]
    [TestCase("heterogeneity")]
    [TestCase("structural")]
    [TestCase("property")]
    [TestCase("contacts")]
    [TestCase("calibration-id")]
    [TestCase("calibration-sha")]
    public async Task Catalog_DistinctBlueprintControls_ProduceDistinctProfiles(string change)
    {
        WorldGenerationRequest original = FullTemplate();
        WorldGenerationRequest changed = change switch
        {
            "padding" => original with { Grid = original.Grid with { HorizontalPaddingM = 125 } },
            "heterogeneity" => original with
            {
                Heterogeneity = original.Heterogeneity with { CorrelationLengthXM = 325 }
            },
            "structural" => original with
            {
                StructuralConditioningPoints =
                [
                    original.StructuralConditioningPoints[0] with { ReservoirTopDepthM = 995 },
                    original.StructuralConditioningPoints[1]
                ]
            },
            "property" => original with
            {
                ConditioningPoints =
                [
                    original.ConditioningPoints[0] with { Porosity = 0.21 },
                    original.ConditioningPoints[1]
                ]
            },
            "contacts" => original with
            {
                FluidContacts = original.FluidContacts with { TransitionThicknessM = 7 }
            },
            "calibration-id" => original with
            {
                CalibrationArtifact = original.CalibrationArtifact with { Id = "another-calibration" }
            },
            "calibration-sha" => original with
            {
                CalibrationArtifact = original.CalibrationArtifact with { Sha256 = new string('b', 64) }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(change))
        };
        await _store.Manager.CreateAsync(original);
        WorldSetupProfile before = (await Catalog(original)).Profiles.Single();
        await _store.Manager.CreateAsync(changed);
        WorldSetupProfileCatalog catalog = await Catalog(original);

        Assert.Multiple(() =>
        {
            Assert.That(catalog.Profiles, Has.Count.EqualTo(2));
            Assert.That(catalog.Profiles.Select(profile => profile.ProfileId), Is.Unique);
            Assert.That(catalog.Profiles, Does.Contain(before));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Setup_ChangesOnlySeedAndGridCounts_AndIsByteIdenticalOnRetryAndRestart(bool oilRim)
    {
        WorldGenerationRequest template = FullTemplate();
        if (oilRim)
            template = template with
            {
                FluidContacts = template.FluidContacts with
                {
                    GasWater = [],
                    GasOil = template.FluidContacts.GasWater.Select(point => point with
                    {
                        ContactDepthTvdM = point.ContactDepthTvdM - 5
                    }).ToArray(),
                    OilWater = template.FluidContacts.GasWater.Select(point => point with
                    {
                        ContactDepthTvdM = point.ContactDepthTvdM + 5
                    }).ToArray()
                }
            };
        ReservoirWorld original = await _store.Manager.CreateAsync(template);
        PersistedWorldSpec? originalSpec = await _store.Repository.LoadWorldSpecAsync(original.Summary.WorldId);
        WorldSetupProfile profile = (await Catalog(template)).Profiles.Single();
        WorldSetupRequest request = Setup(template, profile.ProfileId, "Preview", 27);

        ReservoirWorld first = (await _store.Manager.PrepareRealizationAsync(request))!;
        PersistedWorldSpec? firstSpec = await _store.Repository.LoadWorldSpecAsync(first.Summary.WorldId);
        ReservoirWorld retry = (await _store.Manager.PrepareRealizationAsync(request))!;
        ReservoirWorld restarted = (await _store.NewManager().PrepareRealizationAsync(request))!;
        ReservoirWorld otherSeed = (await _store.Manager.PrepareRealizationAsync(
            request with { RealizationSeed = 28 }))!;
        ReservoirWorld standard = (await _store.Manager.PrepareRealizationAsync(
            request with { Resolution = "Standard" }))!;
        PersistedWorldSpec? standardSpec = await _store.Repository.LoadWorldSpecAsync(standard.Summary.WorldId);
        PersistedWorldSpec? originalAfter = await _store.Repository.LoadWorldSpecAsync(original.Summary.WorldId);
        PersistedWorldSpec? firstAfter = await _store.Repository.LoadWorldSpecAsync(first.Summary.WorldId);

        Assert.Multiple(() =>
        {
            Assert.That(first.Summary.Grid, Is.EqualTo(new WorldGridSummary(16, 16, 8, 2_048)));
            Assert.That(standard.Summary.Grid, Is.EqualTo(new WorldGridSummary(64, 64, 20, 81_920)));
            Assert.That(firstSpec!.CanonicalRequestJson, Is.EqualTo(ReservoirWorldFactory.CanonicalRequestJson(
                template with
                {
                    Seed = 27, Grid = template.Grid with { CountX = 16, CountY = 16, CountZ = 8 }
                })));
            Assert.That(standardSpec!.CanonicalRequestJson, Is.EqualTo(ReservoirWorldFactory.CanonicalRequestJson(
                template with
                {
                    Seed = 27, Grid = template.Grid with { CountX = 64, CountY = 64, CountZ = 20 }
                })));
            Assert.That(retry.Summary, Is.EqualTo(first.Summary));
            Assert.That(restarted.Summary, Is.EqualTo(first.Summary));
            Assert.That(TruthBytes(retry), Is.EqualTo(TruthBytes(first)));
            Assert.That(TruthBytes(restarted), Is.EqualTo(TruthBytes(first)));
            Assert.That(otherSeed.Summary.WorldId, Is.Not.EqualTo(first.Summary.WorldId));
            Assert.That(TruthBytes(otherSeed), Is.Not.EqualTo(TruthBytes(first)));
            Assert.That(standard.Summary.WorldId, Is.Not.EqualTo(first.Summary.WorldId));
            Assert.That(originalAfter, Is.EqualTo(originalSpec));
            Assert.That(firstAfter, Is.EqualTo(firstSpec));
        });
    }

    [Test]
    public async Task Setup_PreservesOriginalWorldAndPinnedState_AndCreatesNoSimulationState()
    {
        WorldGenerationRequest template = TestData.ConditionedRequest();
        ReservoirWorld world = await _store.Manager.CreateAsync(template);
        var states = new PersistentSimulationStateStore(_store.Repository, TimeProvider.System);
        var run = new SimulationRequest
        {
            DurationSeconds = 1,
            InitialTimeStepSeconds = 1,
            Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
            Wells = []
        };
        SimulationExecution execution = new ReservoirSimulator().Run(world, run);
        string stateId = await states.SaveAsync(world, null, run, execution);
        await _store.Repository.PinStateAsync(new PersistedStatePin(
            stateId, "test-approved-run", "immutable-run", DateTimeOffset.UtcNow));
        PersistedWorldSpec? originalSpec = await _store.Repository.LoadWorldSpecAsync(world.Summary.WorldId);
        PersistedState before = (await _store.Repository.LoadStateAsync(world.Summary.WorldId, stateId))!;
        WorldSetupProfile profile = (await Catalog(template)).Profiles.Single();

        ReservoirWorld prepared = (await _store.Manager.PrepareRealizationAsync(
            Setup(template, profile.ProfileId, "Preview", 42)))!;
        PersistedState after = (await _store.Repository.LoadStateAsync(world.Summary.WorldId, stateId))!;
        PersistedWorldSpec? afterSpec = await _store.Repository.LoadWorldSpecAsync(world.Summary.WorldId);
        int originalStates = await _store.Repository.CountStatesAsync(world.Summary.WorldId);
        int preparedStates = await _store.Repository.CountStatesAsync(prepared.Summary.WorldId);
        int pinCount = await _store.Repository.CountStatePinsAsync(stateId);
        ReservoirWorld restored = (await _store.NewManager().GetAsync(world.Summary.WorldId))!;

        Assert.Multiple(() =>
        {
            Assert.That(afterSpec, Is.EqualTo(originalSpec));
            Assert.That(TruthBytes(restored), Is.EqualTo(TruthBytes(world)));
            Assert.That(after with
            {
                PressureBlob = before.PressureBlob, OilBlob = before.OilBlob,
                WaterBlob = before.WaterBlob, GasBlob = before.GasBlob
            }, Is.EqualTo(before));
            Assert.That(after.PressureBlob, Is.EqualTo(before.PressureBlob));
            Assert.That(after.OilBlob, Is.EqualTo(before.OilBlob));
            Assert.That(after.WaterBlob, Is.EqualTo(before.WaterBlob));
            Assert.That(after.GasBlob, Is.EqualTo(before.GasBlob));
            Assert.That(originalStates, Is.EqualTo(1));
            Assert.That(preparedStates, Is.Zero);
            Assert.That(pinCount, Is.EqualTo(1));
        });
    }

    [TestCase("field")]
    [TestCase("reservoir")]
    [TestCase("profile")]
    public async Task Setup_ProfileMustBelongToExactFieldReservoirAndModel(string mismatch)
    {
        WorldGenerationRequest template = TestData.ConditionedRequest();
        await _store.Manager.CreateAsync(template);
        WorldSetupProfile profile = (await Catalog(template)).Profiles.Single();
        WorldSetupRequest request = Setup(template, profile.ProfileId);
        request = mismatch switch
        {
            "field" => request with { FieldId = Guid.NewGuid() },
            "reservoir" => request with { ReservoirName = template.ReservoirName.ToUpperInvariant() },
            "profile" => request with { ProfileId = "rsp_" + new string('0', 64) },
            _ => throw new ArgumentOutOfRangeException(nameof(mismatch))
        };

        Assert.That(await _store.Manager.PrepareRealizationAsync(request), Is.Null);
    }

    [TestCase("checksum")]
    [TestCase("canonical")]
    [TestCase("identity")]
    [TestCase("invalid-request")]
    [TestCase("invalid-json")]
    public async Task CatalogAndSetup_RejectCorruptionEvenWithWarmWorldCache(string corruption)
    {
        WorldGenerationRequest template = TestData.ConditionedRequest();
        ReservoirWorld world = await _store.Manager.CreateAsync(template);
        WorldSetupProfile profile = (await Catalog(template)).Profiles.Single();
        await _store.CorruptAsync(world.Summary.WorldId, corruption);

        Assert.ThrowsAsync<PersistenceIntegrityException>(async () => await Catalog(template));
        Assert.ThrowsAsync<PersistenceIntegrityException>(async () =>
            await _store.Manager.PrepareRealizationAsync(Setup(template, profile.ProfileId)));
        Assert.That(await _store.Repository.CountStatesAsync(world.Summary.WorldId), Is.Zero);
    }

    [Test]
    public async Task CatalogAndSetup_DoNotHideCorruptDuplicateRepresentatives()
    {
        WorldGenerationRequest template = TestData.ConditionedRequest();
        await _store.Manager.CreateAsync(template);
        WorldSetupProfile profile = (await Catalog(template)).Profiles.Single();
        ReservoirWorld duplicate = await _store.Manager.CreateAsync(template with { Seed = template.Seed + 1 });
        await _store.CorruptAsync(duplicate.Summary.WorldId, "checksum");

        Assert.ThrowsAsync<PersistenceIntegrityException>(async () => await Catalog(template));
        Assert.ThrowsAsync<PersistenceIntegrityException>(async () =>
            await _store.Manager.PrepareRealizationAsync(Setup(template, profile.ProfileId)));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Setup_ModelGenerationValidationIsAnIntegrityFailure_NotPrivateInputFeedback(
        bool rejectTemplate)
    {
        WorldGenerationRequest template = TestData.ConditionedRequest();
        await _store.Manager.CreateAsync(template);
        WorldSetupProfile profile = (await Catalog(template)).Profiles.Single();
        var manager = new PersistentWorldManager(
            new RejectingWorldFactory(rejectTemplate), new InMemoryWorldStore(1),
            _store.Repository, TimeProvider.System);

        PersistenceIntegrityException? failure = rejectTemplate
            ? Assert.ThrowsAsync<PersistenceIntegrityException>(async () =>
                await manager.GetSetupProfilesAsync(template.FieldId, template.ReservoirName))
            : Assert.ThrowsAsync<PersistenceIntegrityException>(async () =>
                await manager.PrepareRealizationAsync(Setup(template, profile.ProfileId)));
        IReadOnlyList<PersistedWorldSpec> specs = await _store.Repository.ListWorldSpecsAsync(
            template.FieldId, template.ReservoirName, ReservoirWorldFactory.ModelVersion);

        Assert.Multiple(() =>
        {
            Assert.That(failure!.InnerException, Is.TypeOf<ReservoirValidationException>());
            Assert.That(failure.Message, Does.Not.Contain("private-conditioning-value"));
            Assert.That(specs, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Repository_PagesAreBoundedScopedParameterizedAndDeterministicallyOrdered()
    {
        WorldGenerationRequest request = TestData.UniformWorldRequest(1, 1, 1) with
        {
            ReservoirName = "Sand' OR 1=1 --"
        };
        var specs = new List<PersistedWorldSpec>();
        for (int seed = 0; seed < 67; seed++)
        {
            ReservoirWorld world = await _store.Manager.CreateAsync(request with { Seed = seed });
            await _store.ExecuteAsync("UPDATE ReservoirWorldSpecs SET CreatedUtc = $created WHERE WorldId = $id;",
                ("$created", seed % 2 == 0 ? "2026-09-16T00:00:00.0000000+00:00" : "2026-09-15T00:00:00.0000000+00:00"),
                ("$id", world.Summary.WorldId));
            specs.Add((await _store.Repository.LoadWorldSpecAsync(world.Summary.WorldId))!);
        }
        await _store.Manager.CreateAsync(request with { FieldId = Guid.NewGuid() });
        await _store.Manager.CreateAsync(request with { ReservoirName = "Other sand" });

        IReadOnlyList<PersistedWorldSpec> first = await _store.Repository.ListWorldSpecsAsync(
            request.FieldId, request.ReservoirName, ReservoirWorldFactory.ModelVersion);
        IReadOnlyList<PersistedWorldSpec> second = await _store.Repository.ListWorldSpecsAsync(
            request.FieldId, request.ReservoirName, ReservoirWorldFactory.ModelVersion, first[^1]);
        IReadOnlyList<PersistedWorldSpec> end = await _store.Repository.ListWorldSpecsAsync(
            request.FieldId, request.ReservoirName, ReservoirWorldFactory.ModelVersion, second[^1]);
        WorldSetupProfileCatalog catalog = await Catalog(request);

        Assert.Multiple(() =>
        {
            Assert.That(first, Has.Count.EqualTo(64));
            Assert.That(second, Has.Count.EqualTo(3));
            Assert.That(end, Is.Empty);
            Assert.That(first.Concat(second), Is.EqualTo(specs.OrderBy(spec => spec.CreatedUtc)
                .ThenBy(spec => spec.WorldId, StringComparer.Ordinal)));
            Assert.That(catalog.Profiles, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task CatalogAndSetup_HonorCancelledRequestsWithoutCreatingRealizations()
    {
        WorldGenerationRequest template = TestData.ConditionedRequest();
        await _store.Manager.CreateAsync(template);
        WorldSetupProfile profile = (await Catalog(template)).Profiles.Single();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        Assert.ThrowsAsync<OperationCanceledException>(async () => await _store.Manager.GetSetupProfilesAsync(
            template.FieldId, template.ReservoirName, cancellation.Token));
        Assert.ThrowsAsync<OperationCanceledException>(async () => await _store.Manager.PrepareRealizationAsync(
            Setup(template, profile.ProfileId), cancellation.Token));
        IReadOnlyList<PersistedWorldSpec> specs = await _store.Repository.ListWorldSpecsAsync(
            template.FieldId, template.ReservoirName, ReservoirWorldFactory.ModelVersion);
        Assert.That(specs, Has.Count.EqualTo(1));
    }

    private Task<WorldSetupProfileCatalog> Catalog(WorldGenerationRequest request) =>
        _store.Manager.GetSetupProfilesAsync(request.FieldId, request.ReservoirName);

    internal static WorldSetupRequest Setup(
        WorldGenerationRequest template, string profileId, string resolution = "Preview", int seed = 0) => new()
        {
            FieldId = template.FieldId, ReservoirName = template.ReservoirName, ProfileId = profileId,
            Resolution = resolution, RealizationSeed = seed
        };

    private static byte[] TruthBytes(ReservoirWorld world) => new[]
        {
            world.TopDepthM, world.BaseDepthM, world.CellDepthM, world.CellThicknessM, world.NetToGross,
            world.Porosity, world.LogPermeability, world.PressurePa, world.OilSaturation,
            world.WaterSaturation, world.GasSaturation
        }
        .SelectMany(values => values.SelectMany(BitConverter.GetBytes)).ToArray();

    private static WorldGenerationRequest FullTemplate() => TestData.ConditionedRequest() with
    {
        Grid = new GridOptions { CountX = 8, CountY = 8, CountZ = 3, HorizontalPaddingM = 75 },
        Heterogeneity = TestData.ConditionedRequest().Heterogeneity with
        {
            NetToGrossStdDev = 0.09, ShalePorosity = 0.07, ShalePermeabilityM2 = 2e-20
        },
        StructuralConditioningPoints =
        [
            new() { EastingM = -20, NorthingM = -20, ReservoirTopDepthM = 990, ReservoirBaseDepthM = 1_050 },
            new() { EastingM = 720, NorthingM = 720, ReservoirTopDepthM = 1_010, ReservoirBaseDepthM = 1_070 }
        ],
        FluidContacts = new FluidContactOptions
        {
            TransitionThicknessM = 6,
            GasWater =
            [
                new() { EastingM = 0, NorthingM = 0, ContactDepthTvdM = 1_015 },
                new() { EastingM = 700, NorthingM = 700, ContactDepthTvdM = 1_025 }
            ]
        }
    };

    private sealed class RejectingWorldFactory(bool rejectTemplate) : IReservoirWorldFactory
    {
        public ReservoirWorld Create(WorldGenerationRequest request)
        {
            if (rejectTemplate || request.Grid.CountX != 8)
                throw new ReservoirValidationException(new Dictionary<string, string[]>
                {
                    ["fluidContacts"] = ["private-conditioning-value"]
                });
            return new ReservoirWorldFactory().Create(request);
        }
    }
}
