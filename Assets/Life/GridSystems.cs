using System.Text.RegularExpressions;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

/// <summary>
/// calc next generation values and put in nextState
/// what is causing burst error ? 
/// </summary>

[AlwaysSynchronizeSystem]
public class GenerateNextStateSystem : JobComponentSystem
{
    
    //[ReadOnly] public ComponentDataFromEntity<Live> liveLookupX;
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    { 
        int[] stay = ECSGrid.stay;
        int[] born = ECSGrid.born;

        var liveLookup = GetComponentDataFromEntity<Live>(); 
        //var liveLookup = liveLookupX ;
        Entities
            .WithoutBurst()
            .ForEach((ref NextState nextState, ref DebugIndex debugIndex, in Live live, in Neighbors neighbors) => {
                int numLiveNeighbors = 0;
                
                numLiveNeighbors += liveLookup[neighbors.nw].value;
                numLiveNeighbors += liveLookup[neighbors.n].value;
                numLiveNeighbors += liveLookup[neighbors.ne].value;
                numLiveNeighbors += liveLookup[neighbors.w].value;
                numLiveNeighbors += liveLookup[neighbors.e].value;
                numLiveNeighbors += liveLookup[neighbors.sw].value;
                numLiveNeighbors += liveLookup[neighbors.s].value;
                numLiveNeighbors += liveLookup[neighbors.se].value;
            
                debugIndex.index = numLiveNeighbors;
                debugIndex.countNext += 1;
                //Note math.Select(falseValue, trueValue, boolSelector)
                nextState.value = math.select( born[numLiveNeighbors],stay[numLiveNeighbors], live.value== 1);
            }).Run();
        return default;
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
        Entities
            //.WithoutBurst()
            .ForEach((ref Live live, ref DebugIndex debugIndex, ref Translation translation, in  NextState nextState, in Neighbors neighbor) => {
                live.value = nextState.value;
                translation.Value = new float3(translation.Value.x, translation.Value.y,
                    math.select( zDead,zLive, live.value == 1));
                debugIndex.countLive += 1;
            }).Run();
        return default;
    }
}