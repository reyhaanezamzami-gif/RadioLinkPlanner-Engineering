
using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface ILinkAnalyzer
{
    LinkAnalysisResult Analyze(
        Link link,
        IReadOnlyList<TerrainPoint> terrainProfile);
}
