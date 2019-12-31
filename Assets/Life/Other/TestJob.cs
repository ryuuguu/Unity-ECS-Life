using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

public class RotationSpeedSystem : JobComponentSystem
{
    // Use the [BurstCompile] attribute to compile a job with Burst.
    [BurstCompile]
    struct RotationSpeedJob : IJobForEach<Live>
    {
        public float DeltaTime;
        // The [ReadOnly] attribute tells the job scheduler that this job will not write to rotSpeed
        public void Execute(ref Live live) {
            // Rotate something about its up vector at the speed given by RotationSpeed.  
            live.value = 1;
        }
    }

// OnUpdate runs on the main thread.
// Any previously scheduled jobs reading/writing from Rotation or writing to RotationSpeed 
// will automatically be included in the inputDependencies.
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new RotationSpeedJob()
        {
            DeltaTime = Time.DeltaTime
        };
        return job.Schedule(this, inputDependencies);
    }
}