namespace RadioLinkPlannervfV.Terrain.Models;

public sealed class OnlineTerrainOptions
{
    public string BaseUrl { get; set; }
        = "https://api.open-meteo.com";


    public bool Enabled { get; set; }
        = true;
}