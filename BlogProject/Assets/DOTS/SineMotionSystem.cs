using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class SineMotionSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(SineTag))]
    struct SineMotionJob : IJobForEach<Translation, MotionRange>
    {
        public float time;
        public void Execute(ref Translation pos, [ReadOnly] ref MotionRange range)
        {
            var sin = math.sin(time);
            sin *= sin;
            pos.Value.y = sin * (range.Value.y - range.Value.x) + range.Value.x;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new SineMotionJob();
        job.time = Time.time;
        return job.Schedule(this, inputDeps);
    }
}
