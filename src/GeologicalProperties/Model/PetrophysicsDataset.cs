using System;
using System.Collections.Generic;

namespace GeologicalProperties.Model;

public enum PetrophysicsDataClassification
{
    Observed,
    HumanInterpreted,
    Derived,
    ModelEstimated,
    Synthetic
}

public enum PetrophysicsDepthReference
{
    MeasuredDepth,
    TrueVerticalDepth,
    TrueVerticalDepthSubsea
}

public class PetrophysicsDataset
{
    public PetrophysicsDatasetProvenance? Provenance { get; set; }
    public List<PetrophysicsExternalIdentifier>? ExternalIdentifiers { get; set; }
    public PetrophysicsBitemporalTimestamps? Temporal { get; set; }
    public List<PetrophysicsLogRun>? LogRuns { get; set; }
    public List<PetrophysicsFormationTop>? FormationTops { get; set; }
    public List<PetrophysicsFormationInterval>? FormationIntervals { get; set; }
}

public class PetrophysicsDatasetProvenance
{
    public string? DatasetName { get; set; }
    public string? DatasetVersion { get; set; }
    public PetrophysicsDataClassification? Classification { get; set; }
    public List<PetrophysicsSourceArtifact>? SourceArtifacts { get; set; }
}

public class PetrophysicsSourceArtifact
{
    public Guid ID { get; set; }
    public string? Url { get; set; }
    public string? License { get; set; }
    public string? Attribution { get; set; }
    public string? SHA256 { get; set; }
}

public class PetrophysicsExternalIdentifier
{
    public string? Namespace { get; set; }
    public string? Value { get; set; }
}

public class PetrophysicsBitemporalTimestamps
{
    public DateTimeOffset? ValidTimeStart { get; set; }
    public DateTimeOffset? ValidTimeEnd { get; set; }
    public DateTimeOffset? TransactionTimeStart { get; set; }
    public DateTimeOffset? TransactionTimeEnd { get; set; }
}

public class PetrophysicsLogRun
{
    public Guid ID { get; set; }
    public string? Name { get; set; }
    public string? Tool { get; set; }
    public Guid? SourceArtifactID { get; set; }
    public PetrophysicsBitemporalTimestamps? Temporal { get; set; }
    public PetrophysicsDepthAxis? DepthAxis { get; set; }
    public List<double>? DepthValues { get; set; }
    public List<PetrophysicsLogCurve>? Curves { get; set; }
}

public class PetrophysicsDepthAxis
{
    public PetrophysicsDepthReference Reference { get; set; }
    public string? OriginalUnit { get; set; }
    public string? CanonicalUnit { get; set; }
    public string? Datum { get; set; }
    public bool PositiveDown { get; set; } = true;
}

public class PetrophysicsLogCurve
{
    public Guid ID { get; set; }
    public string? OriginalMnemonic { get; set; }
    public string? CanonicalMnemonic { get; set; }
    public string? OriginalUnit { get; set; }
    public string? CanonicalUnit { get; set; }
    public List<double?>? Values { get; set; }
    public List<bool>? NullFlags { get; set; }
    public List<string?>? QualityFlags { get; set; }
    public double? OriginalNullValue { get; set; }
    public PetrophysicsDataClassification? Classification { get; set; }
}

public class PetrophysicsDepthCoordinate
{
    public PetrophysicsDepthReference Reference { get; set; }
    public double? OriginalValue { get; set; }
    public string? OriginalUnit { get; set; }
    public double Value { get; set; }
    public string? Unit { get; set; }
    public string? Datum { get; set; }
    public bool? PositiveDown { get; set; }
}

public class PetrophysicsFormationTop
{
    public Guid ID { get; set; }
    public string? FormationName { get; set; }
    public List<PetrophysicsDepthCoordinate>? Depths { get; set; }
    public double? Confidence { get; set; }
    public string? Method { get; set; }
    public Guid? SourceArtifactID { get; set; }
    public PetrophysicsDataClassification? Classification { get; set; }
}

public class PetrophysicsFormationInterval
{
    public Guid ID { get; set; }
    public string? FormationName { get; set; }
    public PetrophysicsDepthCoordinate? TopDepth { get; set; }
    public PetrophysicsDepthCoordinate? BaseDepth { get; set; }
    public double? Confidence { get; set; }
    public string? Method { get; set; }
    public Guid? SourceArtifactID { get; set; }
    public PetrophysicsDataClassification? Classification { get; set; }
}
