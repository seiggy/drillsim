using System.Text.Json;
using System.Text.Json.Nodes;

namespace DrillingOperations;

public sealed record PublicationArtifactSet(TruthBindingResponse Binding,MaterializedPlan Plan,DrillingExecutionArtifact Execution,SurveyArtifact Survey,LogObservationArtifact Logs,CompletionDesign Completion,ProductionSeriesArtifact Production,AnalysisScenarioDto Scenario,DrillingObservationArtifact? DrillingObservations=null);
public sealed record PublicationDrillingObservationReference(string ArtifactId,string ModelVersion,string CalibrationVersion,string ContentSha256,int SampleCount,int CuttingsSampleCount,int LossEventCount,int DysfunctionEventCount);

public static class DeterministicPublicationPlanner
{
 public const string CoordinatePublicationVersion="absolute-riemannian-collar-v2";
 static readonly (string Collection,string Service,string Route,string ReadRoute,string Kind)[] SourceKinds=
 [
  ("Field","FieldService","/field/api/Field","/field/api/Field","Field"),("Clusters","ClusterService","/cluster/api/Cluster","/cluster/api/Cluster","Cluster"),("Wells","WellService","/well/api/Well","/well/api/Well","Well"),("WellBores","WellBoreService","/wellbore/api/WellBore","/wellbore/api/WellBore","WellBore"),("WellBoreArchitectures","WellBoreArchitectureService","/wellborearchitecture/api/WellBoreArchitecture","/wellborearchitecture/api/WellBoreArchitecture","WellBoreArchitecture"),("Trajectories","TrajectoryService","/trajectory/api/Trajectory","/trajectory/api/Trajectory","Trajectory"),("GeologicalProperties","GeologicalPropertiesService","/geologicalproperties/api/internal/publication/GeologicalProperties","/geologicalproperties/api/GeologicalProperties","GeologicalProperties")
 ];
 public static PublicationDrillingObservationReference? DrillingObservationReference(PublicationArtifactSet artifacts)=>DrillingObservationReference(artifacts.DrillingObservations);
 public static PublicationDrillingObservationReference? DrillingObservationReference(DrillingObservationArtifact? artifact)=>artifact is null?null:new(artifact.ArtifactId,artifact.ModelVersion,artifact.CalibrationVersion,artifact.OutputHash,artifact.Summary.SampleCount,artifact.Summary.CuttingsSampleCount,artifact.Summary.LossEventCount,artifact.Summary.DysfunctionEventCount);
 public static PublicationStaging Create(AnalysisPackageDocument source,PublicationArtifactSet artifacts)
 {
  string observationModelVersion=RequireObservationModelVersion(artifacts.Scenario.ObservationModelVersion);
  if(source.FieldId!=artifacts.Scenario.SourceFieldId||source.Sha256!=artifacts.Binding.SourcePackageSha256)throw new PublicationUpstreamException(409,"SourcePackageMismatch","Authoritative source package identity changed.");
  if(source.SourceCounts.Fields!=1||source.Wells.Count!=29||source.WellBores.Count!=46)throw new PublicationUpstreamException(409,"SourceGraphCountMismatch","Canonical source package must retain 29 wells and 46 wellbores.");
  Guid scenario=artifacts.Scenario.ScenarioId,run=Guid.Parse(artifacts.Plan.RunId),clonedField=Guid.Parse(DeterministicIdentity.Create("published-field-v1",scenario.ToString("D"),source.FieldId.ToString("D"))),reveal=Guid.Parse(DeterministicIdentity.Create("reveal-v1",scenario.ToString("D"),run.ToString("D"),artifacts.Production.OutputHash));
  DateTimeOffset initial=artifacts.Scenario.InitialAsOfUtc??throw new PublicationUpstreamException(409,"ScenarioTimeMissing","Scenario InitialAsOfUtc is required for deterministic reveal time."),valid=initial.AddDays(1);
  var sourceNodes=new Dictionary<string,List<JsonNode>>(StringComparer.Ordinal){["Field"]=[JsonNode.Parse(source.Field.GetRawText())!],["Clusters"]=Nodes(source.Clusters),["Wells"]=Nodes(source.Wells),["WellBores"]=Nodes(source.WellBores),["WellBoreArchitectures"]=Nodes(source.WellBoreArchitectures),["Trajectories"]=Nodes(source.Trajectories),["GeologicalProperties"]=Nodes(source.GeologicalProperties)};
  var map=new Dictionary<Guid,Guid>();foreach(var node in sourceNodes.Values.SelectMany(x=>x)){Guid old=PublicationJson.EntityId(node);map[old]=old==source.FieldId?clonedField:Guid.Parse(DeterministicIdentity.Create("published-entity-v1",scenario.ToString("D"),old.ToString("D")));}
  var remapped=sourceNodes.ToDictionary(x=>x.Key,x=>x.Value.Select(n=>PublicationJson.Remap(n,map)).ToList(),StringComparer.Ordinal);
  Guid clusterId=Guid.Parse(DeterministicIdentity.Create("published-collar-cluster-v2",scenario.ToString("D"),run.ToString("D"))),wellId=Guid.Parse(artifacts.Plan.ScenarioWellId),boreId=Guid.Parse(artifacts.Plan.ScenarioWellBoreId),architectureId=Guid.Parse(DeterministicIdentity.Create("published-completion-architecture-v1",scenario.ToString("D"),run.ToString("D"))),geologyId=Guid.Parse(DeterministicIdentity.Create("published-observable-geology-v1",scenario.ToString("D"),run.ToString("D")));
  JsonNode[] scenarioTrajectories=ScenarioTrajectories(remapped["Field"].Single(),clonedField,clusterId,wellId,boreId,artifacts,valid).ToArray();
  JsonObject definitive=(JsonObject)scenarioTrajectories.Single(x=>x["IsDefinitive"]!.GetValue<bool>());
  JsonNode collar=definitive["SurveyStationList"]![0]!;
  remapped["Clusters"].Add(ScenarioCollarCluster(clusterId,clonedField,collar,artifacts,valid));
  remapped["Wells"].Add(ScenarioWell(wellId,clusterId,artifacts,valid));
  remapped["WellBores"].Add(ScenarioBore(boreId,wellId,artifacts,valid));
  remapped["WellBoreArchitectures"].Add(ScenarioArchitecture(remapped["WellBoreArchitectures"].First(),architectureId,boreId,artifacts,valid));
  remapped["Trajectories"].AddRange(scenarioTrajectories);
  Guid definitiveTrajectoryId=Guid.Parse(DeterministicIdentity.Create("published-definitive-survey-v1",artifacts.Scenario.ScenarioId.ToString("D"),artifacts.Plan.RunId));remapped["GeologicalProperties"].Add(ScenarioGeology(remapped["GeologicalProperties"].First(),geologyId,boreId,definitiveTrajectoryId,artifacts,valid));
  string visibilityMarker=$"[DrillSimScenario:{scenario:D};Reveal:{reveal:D};Visibility:ReceiptGated]";foreach(JsonObject entity in remapped.Values.SelectMany(x=>x).Cast<JsonObject>()){string? description=PublicationJson.StringProperty(entity,"Description");PublicationJson.Set(entity,"Description",string.IsNullOrWhiteSpace(description)?visibilityMarker:description+" "+visibilityMarker);}ValidateGraph(remapped,geologyId,definitiveTrajectoryId);if(remapped["Wells"].Count!=30||remapped["WellBores"].Count!=47)throw new PublicationUpstreamException(409,"PublishedGraphCountMismatch","Published graph count invariant failed.");
  var operations=new List<PublicationWriteOperation>();int sequence=0;foreach(var kind in SourceKinds){foreach(JsonNode node in remapped[kind.Collection].OrderBy(x=>PublicationJson.EntityId(x).ToString("D"),StringComparer.Ordinal)){Guid id=PublicationJson.EntityId(node);string payload=Canonical(node),hash=DeterministicIdentity.Sha256(payload),opId=DeterministicIdentity.Create("publication-operation-v1",scenario.ToString("D"),kind.Kind,id.ToString("D"),hash);operations.Add(new(++sequence,opId,kind.Service,kind.Route,$"{kind.ReadRoute}/{id:D}"+(kind.Kind=="Trajectory"?"?includeCalculatedStations=true":string.Empty),id.ToString("D"),kind.Kind,payload,hash));}}
  var evidence=operations.Select(x=>new PublicationEvidence(x.EntityId,x.RecordKind,x.PayloadHash)).OrderBy(x=>x.RecordKind,StringComparer.Ordinal).ThenBy(x=>x.EvidenceId,StringComparer.Ordinal).ToArray();
  string observationBatchId=artifacts.Logs.ObservationBatchId,planId=DeterministicIdentity.Create("publication-plan-v1",scenario.ToString("D"),run.ToString("D"),clonedField.ToString("D"),string.Join("\n",operations.Select(x=>x.PayloadHash)));string manifest=CanonicalJson.Serialize(new{revealId=reveal,runId=run,scenarioId=scenario,clonedFieldId=clonedField,validTimeUtc=valid,sourcePackageSha256=source.Sha256,observationBatchId,observationModelVersion,evidence,productionSeries=new{entityId=wellId,seriesId=artifacts.Production.ProductionSeriesId,modelVersion=artifacts.Production.ModelVersion,contentSha256=artifacts.Production.OutputHash,monthCount=artifacts.Production.Months.Count,checkpointYears=artifacts.Production.Checkpoints.Select(x=>x.Year)}}),manifestHash=DeterministicIdentity.Sha256(manifest);
  return new(planId,reveal.ToString("D"),run.ToString("D"),scenario.ToString("D"),clonedField.ToString("D"),observationBatchId,observationModelVersion,valid,operations,evidence,manifest,manifestHash,wellId.ToString("D"),artifacts.Production.ProductionSeriesId,artifacts.Production.ModelVersion,artifacts.Production.OutputHash);
 }
 public static string RequireObservationModelVersion(string? value)
 {
  if(string.IsNullOrWhiteSpace(value)||value.Length>100||value.Any(static c=>!(char.IsAsciiLetterOrDigit(c)||"-_.:".Contains(c))))throw new PublicationUpstreamException(409,"ScenarioObservationModelVersionInvalid","Authoritative scenario ObservationModelVersion must be a nonempty token of at most 100 safe characters.");return value;
 }
 static List<JsonNode> Nodes(IReadOnlyList<JsonElement> values)=>values.Select(x=>JsonNode.Parse(x.GetRawText())!).ToList();
 static string Canonical(JsonNode node){using var d=JsonDocument.Parse(node.ToJsonString());return CanonicalJson.Canonicalize(d.RootElement);}
 static void Identity(JsonObject node,Guid id,string name,string description,DateTimeOffset valid){PublicationJson.Set(node,"MetaInfo",new JsonObject{{"ID",id.ToString("D")}});PublicationJson.Set(node,"Name",name);PublicationJson.Set(node,"Description",description);PublicationJson.Set(node,"CreationDate",valid);PublicationJson.Set(node,"LastModificationDate",valid);}
 static JsonNode ScenarioCollarCluster(Guid id,Guid field,JsonNode collar,PublicationArtifactSet a,DateTimeOffset valid)
 {
  // Well/WellBore have no point properties. A dedicated single-well cluster is their supported map anchor.
  // Only horizontal coordinates are known here; observed TVD is not a WGS84 ground/mud-line elevation.
  var o=new JsonObject();
  Identity(o,id,"Scenario observable collar",$"Scenario {a.Scenario.ScenarioId:D}; {CoordinatePublicationVersion}; horizontal anchor at the first definitive observable survey station.",valid);
  PublicationJson.Set(o,"FieldID",field);PublicationJson.Set(o,"IsSingleWell",true);
  PublicationJson.Set(o,"IsFixedPlatform",false);
  PublicationJson.Set(o,"ReferencePoint",new JsonObject {["X"]=collar["X"]!.DeepClone(),["Y"]=collar["Y"]!.DeepClone()});
  return o;
 }
 static JsonNode ScenarioWell(Guid id,Guid cluster,PublicationArtifactSet a,DateTimeOffset valid)
 {
  var o=new JsonObject();Identity(o,id,"Scenario observable production well",$"Scenario {a.Scenario.ScenarioId:D}; {CoordinatePublicationVersion}; observable evidence only; production {a.Production.ProductionSeriesId}.",valid);PublicationJson.Set(o,"ClusterID",cluster);PublicationJson.Set(o,"SlotID",null);PublicationJson.Set(o,"IsSingleWell",true);
  var monthly=new JsonArray();foreach(var x in a.Production.Months){DateTimeOffset month=valid.AddMonths(x.Month-1);monthly.Add(new JsonObject{{"Year",month.Year},{"Month",month.Month},{"Oil",Quantity(x.CorrectedOilM3)},{"Gas",Quantity(x.CorrectedGasM3)},{"Water",Quantity(x.CorrectedWaterM3)},{"DaysOnProduction",x.Qc=="Downtime"?0:DateTime.DaysInMonth(month.Year,month.Month)},{"IsAllocated",true},{"Classification","Synthetic"}});}
  var artifacts=new JsonArray();artifacts.Add(new JsonObject{{"ID",a.Production.ProductionSeriesId},{"SHA256",a.Production.OutputHash},{"Attribution","Deterministic metered S7 observable series"}});
  var external=new JsonArray();external.Add(new JsonObject{{"Namespace","DrillSimScenario"},{"Value",a.Scenario.ScenarioId.ToString("D")}});
  var lifecycle=new JsonArray();lifecycle.Add(new JsonObject{{"EventType","Revealed"},{"EffectiveAt",valid},{"FirstSeenAt",valid}});
  var dataset=new JsonObject{{"Provenance",new JsonObject{{"DatasetName","DrillSim observable corrected production"},{"DatasetVersion",a.Production.ModelVersion},{"Classification","Synthetic"},{"SourceArtifacts",artifacts}}},{"ExternalIdentifiers",external},{"Temporal",new JsonObject{{"ValidTimeStart",valid},{"TransactionTimeStart",valid}}},{"LifecycleEvents",lifecycle},{"MonthlyProduction",monthly}};
  PublicationJson.Set(o,"Dataset",dataset);return o;
 }
 static JsonNode? Quantity(double? value)=>value is null?null:new JsonObject{{"Value",value},{"Unit","m3"}};
 static JsonNode ScenarioBore(Guid id,Guid well,PublicationArtifactSet a,DateTimeOffset valid){var o=new JsonObject();Identity(o,id,"Scenario wellbore",$"Scenario {a.Scenario.ScenarioId:D}; {CoordinatePublicationVersion}; drilled execution evidence {a.Execution.OutputHash}.",valid);PublicationJson.Set(o,"WellID",well);PublicationJson.Set(o,"IsSidetrack",false);PublicationJson.Set(o,"ParentWellBoreID",null);PublicationJson.Set(o,"SidetrackType","Undefined");return o;}
 static JsonNode ScenarioArchitecture(JsonNode template,Guid id,Guid bore,PublicationArtifactSet a,DateTimeOffset valid){var o=(JsonObject)template.DeepClone();Identity(o,id,"Approved observable completion",$"Approved observable-only completion {a.Completion.CompletionDesignId}; openings {a.Completion.Openings.Count}; content {a.Completion.OpeningsHash}.",valid);PublicationJson.Set(o,"WellBoreID",bore);return o;}
 static IEnumerable<JsonNode> ScenarioTrajectories(JsonNode clonedField,Guid field,Guid cluster,Guid well,Guid bore,PublicationArtifactSet a,DateTimeOffset valid)
 {
  var origin=RiemannianOrigin(clonedField);
  yield return Trajectory(origin,Guid.Parse(a.Plan.PlannedTrajectoryId),"Planned","Planned",false,a.Plan.Stations.Select(x=>(x.MeasuredDepthM,x.EastingM,x.NorthingM,x.TrueVerticalDepthM,0d,0d)),field,cluster,well,bore,a,valid);
  yield return Trajectory(origin,Guid.Parse(DeterministicIdentity.Create("published-as-drilled-observation-v1",a.Scenario.ScenarioId.ToString("D"),a.Plan.RunId)),"As-drilled observation","Actual",false,a.Survey.Stations.Select(x=>(x.MeasuredDepthM,x.ObservedEastingM,x.ObservedNorthingM,x.ObservedTrueVerticalDepthM,x.InclinationDegrees,x.AzimuthDegrees)),field,cluster,well,bore,a,valid);
  yield return Trajectory(origin,Guid.Parse(DeterministicIdentity.Create("published-definitive-survey-v1",a.Scenario.ScenarioId.ToString("D"),a.Plan.RunId)),"Definitive synthetic survey","Actual",true,a.Survey.Stations.Select(x=>(x.MeasuredDepthM,x.ObservedEastingM,x.ObservedNorthingM,x.ObservedTrueVerticalDepthM,x.InclinationDegrees,x.AzimuthDegrees)),field,cluster,well,bore,a,valid);
 }
 static (double East,double North) RiemannianOrigin(JsonNode clonedField)
 {
  using var document=JsonDocument.Parse(clonedField.ToJsonString());
  if(!PublicationJson.TryProperty(document.RootElement,"ReferencePoint",out var point)||point.ValueKind!=JsonValueKind.Object)
   throw InvalidOrigin();
  return (Axis("RiemannianEast","Y"),Axis("RiemannianNorth","X"));
  double Axis(string canonical,string alias)
  {
   if(point.EnumerateObject().Count(x=>x.Name.Equals(canonical,StringComparison.OrdinalIgnoreCase))>1||
      point.EnumerateObject().Count(x=>x.Name.Equals(alias,StringComparison.OrdinalIgnoreCase))>1)throw InvalidOrigin();
   bool hasCanonical=PublicationJson.TryProperty(point,canonical,out var value);
   bool hasAlias=PublicationJson.TryProperty(point,alias,out var aliasValue);
   if(!hasCanonical&&!hasAlias)throw InvalidOrigin();
   double coordinate=Read(hasCanonical?value:aliasValue);
   if(hasCanonical&&hasAlias&&Read(aliasValue)!=coordinate)throw InvalidOrigin();
   return coordinate;
  }
  static double Read(JsonElement value)
  {
   if(value.ValueKind!=JsonValueKind.Number||!value.TryGetDouble(out double result)||!double.IsFinite(result))throw InvalidOrigin();
   return result;
  }
  static PublicationUpstreamException InvalidOrigin()=>new(409,"PublishedTrajectoryOriginInvalid","A finite, unambiguous field Riemannian reference origin is required; geographic-only or missing origins cannot position new published trajectories.");
 }

 static JsonNode Trajectory(
  (double East,double North) origin,Guid id,string name,string type,bool definitive,
  IEnumerable<(double Md,double E,double N,double Tvd,double Inc,double Azi)> stations,
  Guid field,Guid cluster,Guid well,Guid bore,PublicationArtifactSet a,DateTimeOffset valid)
 {
  var values=stations.ToArray();
  if(values.Length is <2 or >10000)throw InvalidCoordinates();
  var published=new JsonArray();
  double previousMd=-1;
  foreach(var station in values)
  {
   double north=origin.North+station.N,east=origin.East+station.E;
   if(!double.IsFinite(station.Md)||station.Md<0||station.Md<=previousMd||
      !double.IsFinite(north)||!double.IsFinite(east)||!double.IsFinite(station.Tvd)||station.Tvd<0||
      !double.IsFinite(station.Inc)||station.Inc is <0 or >180||
      !double.IsFinite(station.Azi)||station.Azi is <0 or >360)throw InvalidCoordinates();
   previousMd=station.Md;
   // SurveyStation inherits absolute X/RiemannianNorth and Y/RiemannianEast aliases.
   // MD and positive-down TVD retain the approved/observed datum; no geographic conversion is guessed.
   published.Add(new JsonObject
   {
    ["Abscissa"]=station.Md,["MD"]=station.Md,["X"]=north,["Y"]=east,["TVD"]=station.Tvd,
    ["Inclination"]=station.Inc*Math.PI/180,["Azimuth"]=station.Azi*Math.PI/180
   });
  }
  // A new path must not inherit a source tie-in, survey-run sections, uncertainty, or location metadata.
  var o=new JsonObject();
  Identity(o,id,name,$"Scenario {a.Scenario.ScenarioId:D}; {CoordinatePublicationVersion}; {name}; positive-down; absolute Riemannian north/east; observable or approved path only.",valid);
  PublicationJson.Set(o,"FieldID",field);PublicationJson.Set(o,"ClusterID",cluster);
  PublicationJson.Set(o,"WellID",well);PublicationJson.Set(o,"WellBoreID",bore);
  PublicationJson.Set(o,"TrajectoryType",type);PublicationJson.Set(o,"IsDefinitive",definitive);
  PublicationJson.Set(o,"CalculationState","Completed");PublicationJson.Set(o,"CalculationProgress",1);
  PublicationJson.Set(o,"CalculationType","MinimumCurvatureMethod");
  PublicationJson.Set(o,"TieInPoint",published[0]!.DeepClone());
  PublicationJson.Set(o,"SurveyStationList",published);
  PublicationJson.Set(o,"MDStep",Math.Max(1,values.Zip(values.Skip(1),(x,y)=>y.Md-x.Md).Min()));
  return o;
  static PublicationUpstreamException InvalidCoordinates()=>new(409,"PublishedTrajectoryCoordinatesInvalid","New published trajectories require finite approved/observed coordinates and strictly increasing nonnegative measured depths.");
 }
 static JsonNode ScenarioGeology(JsonNode template,Guid id,Guid bore,Guid trajectory,PublicationArtifactSet a,DateTimeOffset valid)
 {
  var o=(JsonObject)template.DeepClone();Identity(o,id,"Synthetic observable petrophysics",$"Observable-only logs {a.Logs.ObservationBatchId}; no hidden truth.",valid);PublicationJson.Set(o,"WellBoreID",bore);PublicationJson.Set(o,"TrajectoryID",trajectory);Guid source=Guid.Parse(a.Logs.ObservationBatchId),logId=Guid.Parse(DeterministicIdentity.Create("published-log-run-v1",a.Logs.ObservationBatchId));
  var depths=new JsonArray(a.Logs.Samples.Select(x=>(JsonNode)JsonValue.Create(x.MeasuredDepthM)!).ToArray());var curves=new JsonArray();AddCurve("GR","API",a.Logs.Samples.Select(x=>x.GrApi));AddCurve("RHOB","kg/m3",a.Logs.Samples.Select(x=>x.RhobKgM3));AddCurve("NPHI","fraction",a.Logs.Samples.Select(x=>x.NphiFraction));AddCurve("RT","ohm.m",a.Logs.Samples.Select(x=>x.DeepResistivityOhmM));AddCurve("CALI","m",a.Logs.Samples.Select(x=>x.CaliperM));
  var tops=new JsonArray();var intervals=new JsonArray();var qualified=a.Logs.Samples.Where(x=>x.GrApi is<75&&x.DeepResistivityOhmM is>2&&x.RhobKgM3 is not null&&x.NphiFraction is not null).ToArray();if(qualified.Length>0){double top=qualified.Min(x=>x.MeasuredDepthM),bottom=qualified.Max(x=>x.MeasuredDepthM);var topDepths=new JsonArray();topDepths.Add(Depth(top));tops.Add(new JsonObject{{"ID",DeterministicIdentity.Create("published-top-v1",a.Plan.RunId)},{"FormationName",a.Completion.ReservoirName},{"Depths",topDepths},{"Confidence",.75},{"Method","Observable petrophysics cutoff"},{"SourceArtifactID",source.ToString("D")},{"Classification","Derived"}});intervals.Add(new JsonObject{{"ID",DeterministicIdentity.Create("published-pay-v1",a.Plan.RunId)},{"FormationName",a.Completion.ReservoirName+" observable pay"},{"TopDepth",Depth(top)},{"BaseDepth",Depth(bottom)},{"Confidence",.7},{"Method","GR-RHOB-NPHI-RT observable interpretation"},{"SourceArtifactID",source.ToString("D")},{"Classification","Derived"}});}
  foreach(var opening in a.Completion.Openings.OrderBy(x=>x.TopMdM).ThenBy(x=>x.OpeningId,StringComparer.Ordinal))intervals.Add(new JsonObject{{"ID",opening.OpeningId},{"FormationName",opening.ReservoirName+" "+opening.Type},{"TopDepth",Depth(opening.TopMdM)},{"BaseDepth",Depth(opening.BaseMdM)},{"Confidence",.8},{"Method","Approved observable-log completion opening"},{"SourceArtifactID",source.ToString("D")},{"Classification","HumanInterpreted"}});
   var pressure=a.Logs.PressureTests.Where(x=>x.PressurePa is not null&&double.IsFinite(x.PressurePa.Value)).OrderBy(x=>x.MeasuredDepthM).ToArray();if(pressure.Length>=3){int contactIndex=Enumerable.Range(1,pressure.Length-2).MaxBy(i=>Math.Abs((pressure[i+1].PressurePa!.Value-pressure[i].PressurePa!.Value)/(pressure[i+1].MeasuredDepthM-pressure[i].MeasuredDepthM)-(pressure[i].PressurePa!.Value-pressure[i-1].PressurePa!.Value)/(pressure[i].MeasuredDepthM-pressure[i-1].MeasuredDepthM)));double contact=(pressure[contactIndex-1].MeasuredDepthM+pressure[contactIndex].MeasuredDepthM)/2;var contactDepths=new JsonArray();contactDepths.Add(Depth(contact));tops.Add(new JsonObject{{"ID",DeterministicIdentity.Create("published-observable-contact-v1",a.Plan.RunId)},{"FormationName",a.Completion.ReservoirName+" inferred fluid contact"},{"Depths",contactDepths},{"Confidence",.55},{"Method","Observable MDT pressure-gradient change"},{"SourceArtifactID",source.ToString("D")},{"Classification","Derived"}});}
   var sourceArtifacts=new JsonArray();sourceArtifacts.Add(new JsonObject{{"ID",source.ToString("D")},{"SHA256",a.Logs.OutputHash},{"Attribution","S5 observable log batch"}});var external=new JsonArray();external.Add(new JsonObject{{"Namespace","DrillSimScenario"},{"Value",a.Scenario.ScenarioId.ToString("D")}});var logRuns=new JsonArray();logRuns.Add(new JsonObject{{"ID",logId.ToString("D")},{"Name","Definitive synthetic petrophysics"},{"Tool",a.Logs.ObservationModelVersion},{"SourceArtifactID",source.ToString("D")},{"Temporal",new JsonObject{{"ValidTimeStart",valid},{"TransactionTimeStart",valid}}},{"DepthAxis",new JsonObject{{"Reference","MeasuredDepth"},{"OriginalUnit","m"},{"CanonicalUnit","m"},{"Datum","well collar"},{"PositiveDown",true}}},{"DepthValues",depths},{"Curves",curves}});
  var petrophysics=new JsonObject{{"Provenance",new JsonObject{{"DatasetName","DrillSim synthetic observable logs"},{"DatasetVersion",a.Logs.ObservationModelVersion},{"Classification","Synthetic"},{"SourceArtifacts",sourceArtifacts}}},{"ExternalIdentifiers",external},{"Temporal",new JsonObject{{"ValidTimeStart",valid},{"TransactionTimeStart",valid}}},{"LogRuns",logRuns},{"FormationTops",tops},{"FormationIntervals",intervals}};PublicationJson.Set(o,"Petrophysics",petrophysics);return o;
  void AddCurve(string mnemonic,string unit,IEnumerable<double?> values){var array=values.ToArray();curves.Add(new JsonObject{{"ID",DeterministicIdentity.Create("published-curve-v1",a.Plan.RunId,mnemonic)},{"OriginalMnemonic",mnemonic},{"CanonicalMnemonic",mnemonic},{"OriginalUnit",unit},{"CanonicalUnit",unit},{"Values",new JsonArray(array.Select(x=>(JsonNode?)JsonValue.Create(x)).ToArray())},{"NullFlags",new JsonArray(array.Select(x=>(JsonNode)JsonValue.Create(x is null)!).ToArray())},{"QualityFlags",new JsonArray(a.Logs.Samples.Select(x=>(JsonNode?)JsonValue.Create(string.Join(";",x.QcFlags))).ToArray())},{"Classification","Synthetic"}});}static JsonObject Depth(double value)=>new(){{"Reference","MeasuredDepth"},{"Value",value},{"Unit","m"},{"Datum","well collar"},{"PositiveDown",true}};
 }

 static void ValidateGraph(IReadOnlyDictionary<string,List<JsonNode>> graph,Guid scenarioGeologyId,Guid definitiveTrajectoryId)
 {
  var fields=graph["Field"].Select(PublicationJson.EntityId).ToHashSet();var clusters=graph["Clusters"].ToDictionary(PublicationJson.EntityId);var wells=graph["Wells"].ToDictionary(PublicationJson.EntityId);var bores=graph["WellBores"].ToDictionary(PublicationJson.EntityId);var trajectories=graph["Trajectories"].ToDictionary(PublicationJson.EntityId);
  bool invalid=graph["Clusters"].Any(x=>!fields.Contains(PublicationJson.RequiredGuid(x,"FieldID")))||graph["Wells"].Any(x=>!clusters.ContainsKey(PublicationJson.RequiredGuid(x,"ClusterID")))||graph["WellBores"].Any(x=>!wells.ContainsKey(PublicationJson.RequiredGuid(x,"WellID")))||graph["WellBoreArchitectures"].Any(x=>!bores.ContainsKey(PublicationJson.RequiredGuid(x,"WellBoreID")));
  foreach(JsonNode trajectory in graph["Trajectories"]){Guid field=PublicationJson.RequiredGuid(trajectory,"FieldID"),cluster=PublicationJson.RequiredGuid(trajectory,"ClusterID"),well=PublicationJson.RequiredGuid(trajectory,"WellID"),bore=PublicationJson.RequiredGuid(trajectory,"WellBoreID");invalid|=!fields.Contains(field)||!clusters.ContainsKey(cluster)||!wells.ContainsKey(well)||!bores.ContainsKey(bore)||PublicationJson.RequiredGuid(wells[well],"ClusterID")!=cluster||PublicationJson.RequiredGuid(bores[bore],"WellID")!=well;}
  JsonNode geology=graph["GeologicalProperties"].Single(x=>PublicationJson.EntityId(x)==scenarioGeologyId);Guid geologyBore=PublicationJson.RequiredGuid(geology,"WellBoreID"),geologyTrajectory=PublicationJson.RequiredGuid(geology,"TrajectoryID");invalid|=geologyTrajectory!=definitiveTrajectoryId||!trajectories.TryGetValue(geologyTrajectory,out JsonNode? owner)||PublicationJson.RequiredGuid(owner!,"WellBoreID")!=geologyBore;
  if(invalid)throw new PublicationUpstreamException(409,"PublishedGraphDanglingReference","Staged publication graph has invalid ownership or a dangling foreign key.");
 }

}
