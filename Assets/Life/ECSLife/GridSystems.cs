﻿using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;


public class GenerateNextStateSystem : JobComponentSystem
{
    
    [BurstCompile]
    struct SetLive : IJobForEach<NextState, Live, Neighbors>
    {
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
        ComponentDataFromEntity<Live> statuses = GetComponentDataFromEntity<Live>();
        
        SetLive neighborCounterJob = new SetLive() {
            liveLookup = statuses,
        };
        JobHandle jobHandle = neighborCounterJob.Schedule(this, inputDeps);
    
        return jobHandle;
    }
}

/// <summary>
/// update Live from NextState and set location 
/// </summary>

[AlwaysSynchronizeSystem]
public class UpdateLiveSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    { 
        float zDead = ECSGrid.zDead;
        float zLive = ECSGrid.zLive;
        var job = Entities
             .WithChangeFilter<NextState>()
             .ForEach((ref Live live,  ref Translation translation, in  NextState nextState) => {
                live.value = nextState.value;
                translation.Value = new float3(translation.Value.x, translation.Value.y,
                    math.select( zDead,zLive, live.value == 1));
             }).Schedule(inputDeps);
        return job;
    }
}