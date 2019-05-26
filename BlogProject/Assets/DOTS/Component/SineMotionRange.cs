using Unity.Entities;
using Unity.Mathematics;

[System.Serializable]
public struct MotionRange : IComponentData
{
    public float2 Value;
}

public struct SineTag : IComponentData
{
}