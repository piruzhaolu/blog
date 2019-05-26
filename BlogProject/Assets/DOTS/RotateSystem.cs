using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class RotateSystem : JobComponentSystem
{
    [BurstCompile]
    struct RotateJob : IJobForEach<Rotation, RotateSpeed>
    {
        public float dt;
        public void Execute(ref Rotation rotation, ref RotateSpeed speed)
        {
            rotation.Value = math.mul(rotation.Value, quaternion.RotateY(speed.Value* dt));
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var rotateJob = new RotateJob();
        rotateJob.dt = Time.deltaTime;
        return rotateJob.Schedule(this,inputDeps);
    }
}


