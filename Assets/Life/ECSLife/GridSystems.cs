using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using UnityEngine;


/// <summary>
/// update Live from NextState and add ChangedTag
/// </summary>

[AlwaysSynchronizeSystem]
[BurstCompile]
public class UpdateMarkChangeSystem : JobComponentSystem {
    protected EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;
    
    protected override void OnCreate() {
        base.OnCreate();
        // Find the ECB system once and store it for later usage
        m_EndSimulationEcbSystem = World
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
        JobHandle jobHandle = Entities
            .ForEach((Entity entity, int entityInQueryIndex, ref Live live, in  NextState nextState)=> {
                if (live.value != nextState.value) {
                    ecb.AddComponent<ChangedTag>(entityInQueryIndex, entity);
                }
                live.value = nextState.value;
            }).Schedule( inputDeps);
        m_EndSimulationEcbSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}

/// <summary>
/// set location on cells marked as changed and remove ChangedTag
/// </summary>

// .WithAll<ChangedTag>() limits changes to only meshes whose lives status changed

[UpdateAfter(typeof(UpdateMarkChangeSystem))]
[AlwaysSynchronizeSystem]
[BurstCompile]
public class UpdateDisplayChangedSystem : JobComponentSystem {
    
    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        
        Entities
            .WithoutBurst()
            .WithAll<ChangedTag>()
            .ForEach((Entity entity, int entityInQueryIndex, in Live live, in PosXY posXY) => {
              ECSGrid.ShowCell(posXY.pos, live.value ==1);  
            }).Run();
   
        return inputDeps;
    }
}

[UpdateAfter(typeof(UpdateDisplayChangedSystem))]
[BurstCompile]
public class UpdateClearChangedSystem : JobComponentSystem {
    protected EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate() {
        base.OnCreate();
        // Find the ECB system once and store it for later usage
        m_EndSimulationEcbSystem = World
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
        var jobHandle =
            Entities
                .WithAll<ChangedTag>()
                .ForEach((Entity entity, int entityInQueryIndex) => {
                    ecb.RemoveComponent<ChangedTag>(entityInQueryIndex, entity);
                }).Schedule(inputDeps);
        m_EndSimulationEcbSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}

[UpdateAfter(typeof(UpdateClearChangedSystem))]
public class GenerateNextStateSystem : JobComponentSystem {
    // For Burst or Schedule (worker thread) jobs to access data outside the a job an explicit struct with a
    // read only variable is needed
    [BurstCompile]
    struct SetLive : IJobForEach<NextState, Live, Neighbors> {
        // liveLookup is a pointer to a native array of live components indexed by entity
        // since allows access outside set of entities being handled a single job o thread that is running 
        // concurrently with other threads accessing the same native array it must be marked read only 
        [ReadOnly]public ComponentDataFromEntity<Live> liveLookup; 
        public void Execute(ref NextState nextState, [ReadOnly] ref Live live,[ReadOnly] ref  Neighbors neighbors){
            
            int numLiveNeighbors = 0;
                
            numLiveNeighbors += liveLookup[neighbors.nw].value;
            numLiveNeighbors += liveLookup[neighbors.n].value;
            numLiveNeighbors += liveLookup[neighbors.ne].value;
            numLiveNeighbors += liveLookup[neighbors.w].value;
            numLiveNeighbors += liveLookup[neighbors.e].value;
            numLiveNeighbors += liveLookup[neighbors.sw].value;
            numLiveNeighbors += liveLookup[neighbors.s].value;
            numLiveNeighbors += liveLookup[neighbors.se].value;
            
            //Note math.Select(falseValue, trueValue, boolSelector)
            // did not want to pass in arrays so change to
            // 3 selects
            int bornValue = math.select(0, 1, numLiveNeighbors == 3);
            int stayValue = math.select(0, 1, numLiveNeighbors == 2);
            stayValue = math.select(stayValue, 1, numLiveNeighbors == 3);
            
            nextState.value = math.select( bornValue,stayValue, live.value== 1);
        }
    }
        
    protected override JobHandle OnUpdate(JobHandle inputDeps) { 
        // make a native array of live components indexed by entity
        ComponentDataFromEntity<Live> statuses = GetComponentDataFromEntity<Live>();
        
        SetLive neighborCounterJob = new SetLive() {
            liveLookup = statuses,
        };
        JobHandle jobHandle = neighborCounterJob.Schedule(this, inputDeps);
    
        return jobHandle;
    }
}


