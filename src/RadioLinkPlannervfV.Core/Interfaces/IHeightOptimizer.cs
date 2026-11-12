using RadioLinkPlannervfV.Core.Models;

namespace RadioLinkPlannervfV.Core.Interfaces;

public interface IHeightOptimizer
{
    HeightOptimizationResult Optimize(
        Link link,
        IReadOnlyList<TerrainPoint> terrainProfile,
        double requiredClearanceMeters);
}
