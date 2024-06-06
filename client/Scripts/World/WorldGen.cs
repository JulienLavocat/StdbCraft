using Godot;

namespace StDBCraft.Scripts.World;

[GlobalClass]
public partial class WorldGen : Resource
{
    [Export] private FastNoiseLite _continentalness;
    [Export] private Curve _height;

    public int GetBlock(int x, int y, int z)
    {
        var continentalness = _continentalness.GetNoise2D(x, z);
        var terrainHeight = Mathf.Round(_height.Sample((continentalness + 1) / 2) * 200);

        return y < terrainHeight ? 2 : 1; // Stone ? Air
    }
}