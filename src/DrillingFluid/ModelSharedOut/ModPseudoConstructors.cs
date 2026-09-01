namespace NORCE.Drilling.DrillingFluid.ModelShared
{
	public class PseudoConstructors
	{
		public static MetaInfo ConstructMetaInfo()
			{
				return new MetaInfo 
				{
					ID = Guid.NewGuid(),
					HttpHostName = "https://dev.digiwells.no/",
					HttpHostBasePath = "DrillingFluid/api/",
					HttpEndPoint = "DrillingFluid/",
				};
			}

		public static MetaInfo ConstructMetaInfo(Guid id)
			{
				return new MetaInfo 
				{
					ID = id,
					HttpHostName = "https://dev.digiwells.no/",
					HttpHostBasePath = "DrillingFluid/api/",
					HttpEndPoint = "DrillingFluid/",
				};
			}
		public static DrillString ConstructDrillString()
		{
			return new DrillString
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				WellBoreID = null, 
				DrillStringSectionList = new List<DrillStringSection>
					{
						ConstructDrillStringSection(),
					},
				SensorsList = new List<DrillStringSensor>
					{
						ConstructDrillStringSensor(),
					},
			};
		}
		public static DrillStringComponent ConstructDrillStringComponent()
		{
			return new DrillStringComponent
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				Description = "Default Description",
				FieldID = null, 
				Type = (DrillStringComponentTypes)0,
				PartList = new List<DrillStringComponentPart>
					{
						ConstructDrillStringComponentPart(),
					},
				Length = 0.0, 
			};
		}
		public static DrillStringComponentPart ConstructDrillStringComponentPart()
		{
			return new DrillStringComponentPart
			{
				ID = new Guid(),
				Name = "Default Name",
				TotalLength = 0.0, 
				OuterDiameter = 0.0, 
				InnerDiameter = 0.0, 
				OuterDiameterState2 = null, 
				OuterDiameterStateBoolean = null, 
				FrictionFactorRotation = null, 
				FrictionFactorAxialDisplacement = null, 
				PressureLossConstantLowFlowRate = null, 
				PressureLossConstantHighFlowRate = null, 
				EccentricityDistance = null, 
				EccentricityAngle = null, 
				TotalFlowAreaCondition1 = null, 
				TotalFlowAreaCondition2 = null, 
				TotalFlowAreaConditionBoolean = null, 
				FlowrateThresholdValue = null, 
				InnerCoatingThickness = null, 
				InnerCoatingDensity = null, 
				InnerCoatingThermalCondutivity = null, 
				InnerCoatingHeatCapacity = null, 
				OuterCoatingThickness = null, 
				OuterCoatingDensity = null, 
				OuterCoatingThermalCondutivity = null, 
				OuterCoatingHeatCapacity = null, 
				YieldStrength = null, 
				UltimateStrength = null, 
				SecondCrossSectionTorsionalInertia = null, 
				FirstCrossSectionTorsionalInertia = 0.0, 
				CrossSectionArea = 0.0, 
				YoungModulus = 0.0, 
				PoissonRatio = 0.0, 
				MaterialDensity = 0.0, 
				AveragePartDensity = 0.0, 
				Mass = 0.0, 
				HeatCapacity = 0.0, 
				ThermalCondutivity = 0.0, 
			};
		}
		public static DrillStringSection ConstructDrillStringSection()
		{
			return new DrillStringSection
			{
				Name = "Default Name",
				Count = 0, 
				SectionComponentList = new List<DrillStringComponent>
					{
						ConstructDrillStringComponent(),
					},
			};
		}
		public static DrillStringSensor ConstructDrillStringSensor()
		{
			return new DrillStringSensor
			{
				DistanceFromBit = null, 
				MeasurementRange = null, 
				SensorAngles = new List<double>
					{
						0.0, 
					},
				SensorTypeInt = 0, 
				SensorType = (DrillStringSensorTypes)0,
			};
		}
		public static CountPerDay ConstructCountPerDay()
		{
			return new CountPerDay
			{
				Date = DateTimeOffset.UtcNow,
				Count = 0, 
			};
		}
		public static History ConstructHistory()
		{
			return new History
			{
				Data = new List<CountPerDay>
					{
						ConstructCountPerDay(),
					},
			};
		}
		public static UsageStatisticsCartographicProjection ConstructUsageStatisticsCartographicProjection()
		{
			return new UsageStatisticsCartographicProjection
			{
				LastSaved = DateTimeOffset.UtcNow,
				BackUpInterval = "Default BackUpInterval",
				GetAllCartographicProjectionTypeIdPerDay = ConstructHistory(),
				GetCartographicProjectionTypeByIdPerDay = ConstructHistory(),
				GetAllCartographicProjectionTypePerDay = ConstructHistory(),
				GetAllCartographicProjectionIdPerDay = ConstructHistory(),
				GetAllCartographicProjectionMetaInfoPerDay = ConstructHistory(),
				GetCartographicProjectionByIdPerDay = ConstructHistory(),
				GetAllCartographicProjectionLightPerDay = ConstructHistory(),
				GetAllCartographicProjectionPerDay = ConstructHistory(),
				PostCartographicProjectionPerDay = ConstructHistory(),
				PutCartographicProjectionByIdPerDay = ConstructHistory(),
				DeleteCartographicProjectionByIdPerDay = ConstructHistory(),
				GetAllCartographicConversionSetIdPerDay = ConstructHistory(),
				GetAllCartographicConversionSetMetaInfoPerDay = ConstructHistory(),
				GetCartographicConversionSetByIdPerDay = ConstructHistory(),
				GetAllCartographicConversionSetLightPerDay = ConstructHistory(),
				GetAllCartographicConversionSetPerDay = ConstructHistory(),
				PostCartographicConversionSetPerDay = ConstructHistory(),
				PutCartographicConversionSetByIdPerDay = ConstructHistory(),
				DeleteCartographicConversionSetByIdPerDay = ConstructHistory(),
			};
		}
		public static CartographicConversionSet ConstructCartographicConversionSet()
		{
			return new CartographicConversionSet
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				CartographicProjectionID = null, 
				CartographicCoordinateList = new List<CartographicCoordinate>
					{
						ConstructCartographicCoordinate(),
					},
			};
		}
		public static CartographicCoordinate ConstructCartographicCoordinate()
		{
			return new CartographicCoordinate
			{
				Northing = null, 
				Easting = null, 
				VerticalDepth = null, 
				GeodeticCoordinate = ConstructGeodeticCoordinate(),
				GridConvergenceDatum = null, 
			};
		}
		public static CartographicProjection ConstructCartographicProjection()
		{
			return new CartographicProjection
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				ProjectionType = (ProjectionType)0,
				GeodeticDatumID = null, 
				LatitudeOrigin = null, 
				Latitude1 = null, 
				Latitude2 = null, 
				LatitudeTrueScale = null, 
				LongitudeOrigin = null, 
				Scaling = null, 
				FalseEasting = null, 
				FalseNorthing = null, 
				Zone = 0, 
				IsSouth = false, 
				IsHyperbolic = false, 
				ProjectionHeight = null, 
				HeightViewPoint = null, 
				Sweep = (AxisType)0,
				AzimuthCentralLine = null, 
				Weight = null, 
				Landsat = null, 
				Path = null, 
				Alpha = null, 
				Gamma = null, 
				Longitude1 = null, 
				Longitude2 = null, 
				LongitudeCentralPoint = null, 
				NoOffset = false, 
				NoRotation = false, 
				AreaNormalizationTransform = (AreaNormalizationTransformType)0,
				PegLatitude = null, 
				PegLongitude = null, 
				PegHeading = null, 
				N = null, 
				Q = null, 
			};
		}
		public static CartographicProjectionType ConstructCartographicProjectionType()
		{
			return new CartographicProjectionType
			{
				Projection = (ProjectionType)0,
				UseLatitudeOrigin = false, 
				UseLatitude1 = false, 
				UseLatitude2 = false, 
				UseLatitudeTrueScale = false, 
				UseLongitudeOrigin = false, 
				UseScaling = false, 
				UseFalseEastingNorthing = false, 
				UseZone = false, 
				UseSouth = false, 
				UseHyperbolic = false, 
				UseProjectionHeight = false, 
				UseHeightViewPoint = false, 
				UseSweep = false, 
				UseAzimuthCentralLine = false, 
				UseWeight = false, 
				UseLandsat = false, 
				UsePath = false, 
				UseAlpha = false, 
				UseGamma = false, 
				UseLongitude1 = false, 
				UseLongitude2 = false, 
				UseLongitudeCentralPoint = false, 
				UseNoOffset = false, 
				UseNoRotation = false, 
				UseAreaNormalizationTransform = false, 
				UsePegLatitude = false, 
				UsePegLongitude = false, 
				UsePegHeading = false, 
				UseN = false, 
				UseQ = false, 
			};
		}
		public static GeodeticCoordinate ConstructGeodeticCoordinate()
		{
			return new GeodeticCoordinate
			{
				LatitudeWGS84 = null, 
				LongitudeWGS84 = null, 
				VerticalDepthWGS84 = null, 
				LatitudeDatum = null, 
				LongitudeDatum = null, 
				VerticalDepthDatum = null, 
				OctreeDepth = 0, 
				OctreeCode = ConstructOctreeCodeLong(),
			};
		}
		public static OctreeCodeLong ConstructOctreeCodeLong()
		{
			return new OctreeCodeLong
			{
				Depth = 0, 
				CodeHigh = 0, 
				CodeLow = 0, 
			};
		}
		public static UsageStatisticsGeodeticDatum ConstructUsageStatisticsGeodeticDatum()
		{
			return new UsageStatisticsGeodeticDatum
			{
				LastSaved = DateTimeOffset.UtcNow,
				BackUpInterval = "Default BackUpInterval",
				GetAllSpheroidIdPerDay = ConstructHistory(),
				GetAllSpheroidMetaInfoPerDay = ConstructHistory(),
				GetSpheroidByIdPerDay = ConstructHistory(),
				GetAllSpheroidPerDay = ConstructHistory(),
				PostSpheroidPerDay = ConstructHistory(),
				PutSpheroidByIdPerDay = ConstructHistory(),
				DeleteSpheroidByIdPerDay = ConstructHistory(),
				GetAllGeodeticDatumIdPerDay = ConstructHistory(),
				GetAllGeodeticDatumMetaInfoPerDay = ConstructHistory(),
				GetGeodeticDatumByIdPerDay = ConstructHistory(),
				GetAllGeodeticDatumLightPerDay = ConstructHistory(),
				GetAllGeodeticDatumPerDay = ConstructHistory(),
				PostGeodeticDatumPerDay = ConstructHistory(),
				PutGeodeticDatumByIdPerDay = ConstructHistory(),
				DeleteGeodeticDatumByIdPerDay = ConstructHistory(),
				GetAllGeodeticConversionSetIdPerDay = ConstructHistory(),
				GetAllGeodeticConversionSetMetaInfoPerDay = ConstructHistory(),
				GetGeodeticConversionSetByIdPerDay = ConstructHistory(),
				GetAllGeodeticConversionSetLightPerDay = ConstructHistory(),
				GetAllGeodeticConversionSetPerDay = ConstructHistory(),
				PostGeodeticConversionSetPerDay = ConstructHistory(),
				PutGeodeticConversionSetByIdPerDay = ConstructHistory(),
				DeleteGeodeticConversionSetByIdPerDay = ConstructHistory(),
			};
		}
		public static GeodeticConversionSet ConstructGeodeticConversionSet()
		{
			return new GeodeticConversionSet
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				GeodeticDatum = ConstructGeodeticDatum(),
				OctreeBounds = ConstructBounds(),
				GeodeticCoordinates = new List<GeodeticCoordinate>
					{
						ConstructGeodeticCoordinate(),
					},
			};
		}
		public static GeodeticDatum ConstructGeodeticDatum()
		{
			return new GeodeticDatum
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				IsDefault = false, 
				Spheroid = ConstructSpheroid(),
				DeltaX = ConstructScalarDrillingProperty(),
				DeltaY = ConstructScalarDrillingProperty(),
				DeltaZ = ConstructScalarDrillingProperty(),
				RotationX = ConstructScalarDrillingProperty(),
				RotationY = ConstructScalarDrillingProperty(),
				RotationZ = ConstructScalarDrillingProperty(),
				ScaleFactor = ConstructScalarDrillingProperty(),
			};
		}
		public static Spheroid ConstructSpheroid()
		{
			return new Spheroid
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				IsDefault = false, 
				SemiMajorAxis = ConstructScalarDrillingProperty(),
				IsSemiMajorAxisSet = false, 
				SemiMinorAxis = ConstructScalarDrillingProperty(),
				IsSemiMinorAxisSet = false, 
				Eccentricity = ConstructScalarDrillingProperty(),
				IsEccentricitySet = false, 
				SquaredEccentricity = ConstructScalarDrillingProperty(),
				IsSquaredEccentricitySet = false, 
				Flattening = ConstructScalarDrillingProperty(),
				IsFlatteningSet = false, 
				InverseFlattening = ConstructScalarDrillingProperty(),
				IsInverseFlatteningSet = false, 
			};
		}
		public static ScalarDrillingProperty ConstructScalarDrillingProperty()
		{
			return new ScalarDrillingProperty
			{
				DiracDistributionValue = ConstructDiracDistribution(),
			};
		}
		public static Point3D ConstructPoint3D()
		{
			return new Point3D
			{
				X = null, 
				Y = null, 
				Z = null, 
			};
		}
		public static Bounds ConstructBounds()
		{
			return new Bounds
			{
				MinX = null, 
				MaxX = null, 
				MinY = null, 
				MaxY = null, 
				MinZ = null, 
				MaxZ = null, 
				MiddleX = null, 
				MiddleY = null, 
				MiddleZ = null, 
				IntervalX = null, 
				IntervalY = null, 
				IntervalZ = null, 
				Center = ConstructPoint3D(),
			};
		}
		public static DiracDistribution ConstructDiracDistribution()
		{
			return new DiracDistribution
			{
				MinValue = 0.0, 
				MaxValue = 0.0, 
				Value = null, 
			};
		}
		public static Field ConstructField()
		{
			return new Field
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				CartographicProjectionID = null, 
			};
		}
		public static FieldCartographicConversionSet ConstructFieldCartographicConversionSet()
		{
			return new FieldCartographicConversionSet
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				FieldID = null, 
				CartographicCoordinateList = new List<CartographicCoordinate>
					{
						ConstructCartographicCoordinate(),
					},
			};
		}
		public static UsageStatisticsField ConstructUsageStatisticsField()
		{
			return new UsageStatisticsField
			{
				LastSaved = DateTimeOffset.UtcNow,
				BackUpInterval = "Default BackUpInterval",
				GetAllFieldIdPerDay = ConstructHistory(),
				GetAllFieldMetaInfoPerDay = ConstructHistory(),
				GetFieldByIdPerDay = ConstructHistory(),
				GetAllFieldLightPerDay = ConstructHistory(),
				GetAllFieldPerDay = ConstructHistory(),
				PostFieldPerDay = ConstructHistory(),
				PutFieldByIdPerDay = ConstructHistory(),
				DeleteFieldByIdPerDay = ConstructHistory(),
				GetAllFieldCartographicConversionSetIdPerDay = ConstructHistory(),
				GetAllFieldCartographicConversionSetMetaInfoPerDay = ConstructHistory(),
				GetFieldCartographicConversionSetByIdPerDay = ConstructHistory(),
				GetAllFieldCartographicConversionSetLightPerDay = ConstructHistory(),
				GetAllFieldCartographicConversionSetPerDay = ConstructHistory(),
				PostFieldCartographicConversionSetPerDay = ConstructHistory(),
				PutFieldCartographicConversionSetByIdPerDay = ConstructHistory(),
				DeleteFieldCartographicConversionSetByIdPerDay = ConstructHistory(),
			};
		}
		public static Cluster ConstructCluster()
		{
			return new Cluster
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				FieldID = null, 
				IsSingleWell = false, 
				RigID = null, 
				IsFixedPlatform = false, 
				ReferenceLatitude = ConstructGaussianDrillingProperty(),
				ReferenceLongitude = ConstructGaussianDrillingProperty(),
				ReferenceDepth = ConstructGaussianDrillingProperty(),
				GroundMudLineDepth = ConstructGaussianDrillingProperty(),
				TopWaterDepth = ConstructGaussianDrillingProperty(),
				Slots = new Dictionary<string,Slot>
					{
						{ "", ConstructSlot() }
					},
			};
		}
		public static Slot ConstructSlot()
		{
			return new Slot
			{
				ID = new Guid(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				Latitude = ConstructGaussianDrillingProperty(),
				Longitude = ConstructGaussianDrillingProperty(),
			};
		}
		public static UsageStatisticsCluster ConstructUsageStatisticsCluster()
		{
			return new UsageStatisticsCluster
			{
				LastSaved = DateTimeOffset.UtcNow,
				BackUpInterval = "Default BackUpInterval",
				GetAllClusterIdPerDay = ConstructHistory(),
				GetAllClusterMetaInfoPerDay = ConstructHistory(),
				GetClusterByIdPerDay = ConstructHistory(),
				GetAllClusterPerDay = ConstructHistory(),
				PostClusterPerDay = ConstructHistory(),
				PutClusterByIdPerDay = ConstructHistory(),
				DeleteClusterByIdPerDay = ConstructHistory(),
			};
		}
		public static GaussianDrillingProperty ConstructGaussianDrillingProperty()
		{
			return new GaussianDrillingProperty
			{
				GaussianValue = ConstructGaussianDistribution(),
			};
		}
		public static GaussianDistribution ConstructGaussianDistribution()
		{
			return new GaussianDistribution
			{
				MinValue = 0.0, 
				MaxValue = 0.0, 
				Mean = null, 
				StandardDeviation = null, 
			};
		}
		public static UsageStatisticsWellBore ConstructUsageStatisticsWellBore()
		{
			return new UsageStatisticsWellBore
			{
				LastSaved = DateTimeOffset.UtcNow,
				BackUpInterval = "Default BackUpInterval",
				GetAllWellBoreIdPerDay = ConstructHistory(),
				GetAllWellBoreMetaInfoPerDay = ConstructHistory(),
				GetWellBoreByIdPerDay = ConstructHistory(),
				GetAllWellBorePerDay = ConstructHistory(),
				PostWellBorePerDay = ConstructHistory(),
				PutWellBoreByIdPerDay = ConstructHistory(),
				DeleteWellBoreByIdPerDay = ConstructHistory(),
			};
		}
		public static WellBore ConstructWellBore()
		{
			return new WellBore
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				WellID = null, 
				RigID = null, 
				IsSidetrack = false, 
				ParentWellBoreID = null, 
				TieInPointAlongHoleDepth = ConstructGaussianDrillingProperty(),
				SidetrackType = (SidetrackType)0,
			};
		}
		public static UsageStatisticsWell ConstructUsageStatisticsWell()
		{
			return new UsageStatisticsWell
			{
				LastSaved = DateTimeOffset.UtcNow,
				BackUpInterval = "Default BackUpInterval",
				GetAllWellIdPerDay = ConstructHistory(),
				GetAllWellMetaInfoPerDay = ConstructHistory(),
				GetWellByIdPerDay = ConstructHistory(),
				GetAllWellPerDay = ConstructHistory(),
				PostWellPerDay = ConstructHistory(),
				PutWellByIdPerDay = ConstructHistory(),
				DeleteWellByIdPerDay = ConstructHistory(),
			};
		}
		public static Well ConstructWell()
		{
			return new Well
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				SlotID = null, 
				ClusterID = null, 
				IsSingleWell = false, 
			};
		}
		public static BaseCompositionProperties ConstructBaseCompositionProperties()
		{
			return new BaseCompositionProperties
			{
				MassDensity = ConstructGaussianDrillingProperty(),
				MassFraction = ConstructGaussianDrillingProperty(),
				SpecificHeatCapacity = ConstructSpecificHeatCapacity(),
				ThermalConductivity = ConstructThermalConductivity(),
			};
		}
		public static BaseOil ConstructBaseOil()
		{
			return new BaseOil
			{
				MassDensity = ConstructGaussianDrillingProperty(),
				MassFraction = ConstructGaussianDrillingProperty(),
				SpecificHeatCapacity = ConstructSpecificHeatCapacity(),
				ThermalConductivity = ConstructThermalConductivity(),
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				PVTParameters = ConstructBaseOilPVTParameters(),
			};
		}
		public static BaseOilPVTParameters ConstructBaseOilPVTParameters()
		{
			return new BaseOilPVTParameters
			{
				A0 = ConstructGaussianDrillingProperty(),
				B0 = ConstructGaussianDrillingProperty(),
				C0 = ConstructGaussianDrillingProperty(),
				D0 = ConstructGaussianDrillingProperty(),
				E0 = ConstructGaussianDrillingProperty(),
				F0 = ConstructGaussianDrillingProperty(),
			};
		}
		public static Brine ConstructBrine()
		{
			return new Brine
			{
				MassDensity = ConstructGaussianDrillingProperty(),
				MassFraction = ConstructGaussianDrillingProperty(),
				SpecificHeatCapacity = ConstructSpecificHeatCapacity(),
				ThermalConductivity = ConstructThermalConductivity(),
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				PVTParameters = ConstructBrinePVTParameters(),
				SaltContent = new List<SaltContent>
					{
						ConstructSaltContent(),
					},
			};
		}
		public static BrinePVTParameters ConstructBrinePVTParameters()
		{
			return new BrinePVTParameters
			{
				S0 = ConstructGaussianDrillingProperty(),
				S1 = ConstructGaussianDrillingProperty(),
				S2 = ConstructGaussianDrillingProperty(),
				S3 = ConstructGaussianDrillingProperty(),
				Bw = ConstructGaussianDrillingProperty(),
				Cw = ConstructGaussianDrillingProperty(),
				Dw = ConstructGaussianDrillingProperty(),
				Ew = ConstructGaussianDrillingProperty(),
				Fw = ConstructGaussianDrillingProperty(),
			};
		}
		public static CompletedDrillingFluid ConstructCompletedDrillingFluid()
		{
			return new CompletedDrillingFluid
			{
				GellingProperties = ConstructGellingProperties(),
				SpecificHeatCapacity = ConstructSpecificHeatCapacity(),
				ThermalConductivity = ConstructThermalConductivity(),
				MassDensity = ConstructGaussianDrillingProperty(),
				PostProcessedFlowCurve = ConstructFlowCurve(),
				FluidPVTParameters = ConstructFluidPVTParameters(),
				RheologicalModelProperties = new List<GenericRheologicalModel>
					{
						ConstructGenericRheologicalModel(),
					},
			};
		}
		public static DrillingFluid ConstructDrillingFluid()
		{
			return new DrillingFluid
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				DrillingFluidDescription = ConstructDrillingFluidDescription(),
				DrillStringID = new Guid(),
				OutputParam = null, 
			};
		}
		public static DrillingFluidComposition ConstructDrillingFluidComposition()
		{
			return new DrillingFluidComposition
			{
				BrineProperies = ConstructBrine(),
				BaseOilProperies = ConstructBaseOil(),
				WaterOilRatio = ConstructGaussianDrillingProperty(),
				HighGravitySolid = ConstructBaseCompositionProperties(),
				LowGravitySolid = ConstructBaseCompositionProperties(),
				LostCirculationMaterials = new List<LostCirculationMaterial>
					{
						ConstructLostCirculationMaterial(),
					},
			};
		}
		public static DrillingFluidDescription ConstructDrillingFluidDescription()
		{
			return new DrillingFluidDescription
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				DrillingFluidComposition = ConstructDrillingFluidComposition(),
				FluidPVTParameters = null,
				FluidMassDensity = new GaussianDrillingProperty
				{
					GaussianValue = new GaussianDistribution
					{	
						Mean = 1284
					}
				},		
				ReferenceTemperature = new GaussianDrillingProperty
				{
					GaussianValue = new GaussianDistribution
					{	
						Mean = 283
					}
				},		
				GellingProperties = ConstructGellingProperties(),
				FlowCurve = ConstructFlowCurve(),
				SpecificHeatCapacity = null,
				ThermalConductivity = null,
			};
		}
		public static DrillingFluidOrder ConstructDrillingFluidOrder()
		{
			return new DrillingFluidOrder
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				DrillingFluidDescriptionID = new Guid(),
				DrillingFluidDescription = ConstructDrillingFluidDescription(),
				Temperature = 0.0, 
				PTModelExtrapolation = (PTModelExtrapolation)0,
				RheologicalModelsEnum = (RheologicalModelsEnum)0,
				Options = ConstructParticleSwarmSolverOptions(),
				CompletedDrillingFluid = ConstructCompletedDrillingFluid(),
				OutputParam = null, 
			};
		}
		public static FlowCurve ConstructFlowCurve()
		{
			return new FlowCurve
			{
				CouetteRheometer = ConstructCouetteRheometer(),
				TableValues = new List<FlowCurveTable>
					{
						ConstructFlowCurveTable(),
					},
			};
		}
		public static FlowCurveTable ConstructFlowCurveTable()
		{
			return new FlowCurveTable
			{
				Temperature = ConstructGaussianDrillingProperty(),
				Pressure = ConstructGaussianDrillingProperty(),
				RheometerMeasurements = new List<RheometerMeasurement>
					{
						ConstructRheometerMeasurement(),
					},
				ShearRateReference = (MeasurementTypesShearRate)0,
				ShearStressReference = (MeasurementTypesShearStress)0,
			};
		}
		public static FluidPVTParameters ConstructFluidPVTParameters()
		{
			return new FluidPVTParameters
			{
				A0 = ConstructGaussianDrillingProperty(),
				B0 = ConstructGaussianDrillingProperty(),
				C0 = ConstructGaussianDrillingProperty(),
				D0 = ConstructGaussianDrillingProperty(),
				E0 = ConstructGaussianDrillingProperty(),
				F0 = ConstructGaussianDrillingProperty(),
			};
		}
		public static GelData ConstructGelData()
		{
			return new GelData
			{
				GelDuration = ConstructGaussianDrillingProperty(),
				GelStrength = ConstructGaussianDrillingProperty(),
				Temperature = ConstructGaussianDrillingProperty(),
			};
		}
		public static GellingProperties ConstructGellingProperties()
		{
			return new GellingProperties
			{
				GelPropertiesTable = new List<GelData>
					{
						ConstructGelData(),
					},
			};
		}
		public static GenericRheologicalModel ConstructGenericRheologicalModel()
		{
			return new GenericRheologicalModel
			{
				RheologicalModelUsed = (RheologicalModelsEnum)0,
				Temperature = 0.0, 
				Pressure = 0.0, 
				PowerLawViscosityInfty = null, 
				PowerLawConsistencyIndex = null, 
				BinghamViscosityInfty = null, 
				BinghamYieldStress = null, 
				HerschelBulkleyViscosityInfty = null, 
				HerschelBulkleyYieldStress = null, 
				HerschelBulkleyConsistencyIndex = null, 
				QuemadaViscosityInfty = null, 
				QuemadaCharacteristicShearRate = null, 
				QuemadaConsistencyIndex = null, 
				QuemadaXi = null, 
				RobertsonStiffYieldStress = null, 
				RobertsonStiffCharacteristicShearRate = null, 
				RobertsonStiffConsistencyIndex = null, 
			};
		}
		public static LostCirculationMaterial ConstructLostCirculationMaterial()
		{
			return new LostCirculationMaterial
			{
				MassDensity = ConstructGaussianDrillingProperty(),
				MassFraction = ConstructGaussianDrillingProperty(),
				SpecificHeatCapacity = ConstructSpecificHeatCapacity(),
				ThermalConductivity = ConstructThermalConductivity(),
				GrainSize = (LostCiruclationMaterialGrainType)0,
			};
		}
		public static SaltContent ConstructSaltContent()
		{
			return new SaltContent
			{
				Salt = (SaltOptions)0,
				WeightPerCubicMeter = ConstructGaussianDrillingProperty(),
			};
		}
		public static SpecificHeatCapacity ConstructSpecificHeatCapacity()
		{
			return new SpecificHeatCapacity
			{
				Slope = ConstructGaussianDrillingProperty(),
				HeatCapacityAtZeroKelvin = ConstructGaussianDrillingProperty(),
				TemperatureRange = new List<double?>
					{
						null, 
					},
				CoefficientRange = new List<double?>
					{
						null, 
					},
			};
		}
		public static ThermalConductivity ConstructThermalConductivity()
		{
			return new ThermalConductivity
			{
				Slope = ConstructGaussianDrillingProperty(),
				ThermalConductivityAtZeroKelvin = ConstructGaussianDrillingProperty(),
				TemperatureRange = new List<double?>
					{
						null, 
					},
				CoefficientRange = new List<double?>
					{
						null, 
					},
			};
		}
		public static CouetteRheometer ConstructCouetteRheometer()
		{
			return new CouetteRheometer
			{
				Name = "Default Name",
				Description = "Default Description",
				Id = new Guid(),
				RheometerType = (RheometerTypeEnum)0,
				BobRadius = 0.0, 
				Gap = 0.0, 
				NewtonianEndEffectCorrection = 0.0, 
				BobLength = 0.0, 
				ConicalAngle = 0.0, 
				MeasurementPrecision = 0.0, 
				UseISOConvention = false, 
				FixedSpeedList = new List<double>
					{
						0.0, 
					},
			};
		}
		public static RheometerMeasurement ConstructRheometerMeasurement()
		{
			return new RheometerMeasurement
			{
				RotationalSpeed = 0.0, 
				Torque = 0.0, 
				IsoNewtonianShearRate = 0.0, 
				IsoNewtonianShearStress = 0.0, 
				BobNewtonianShearRate = 0.0, 
				BobNewtonianShearStress = 0.0, 
			};
		}
		public static ParticleSwarmSolverOptions ConstructParticleSwarmSolverOptions()
		{
			return new ParticleSwarmSolverOptions
			{
			};
		}
	}
}