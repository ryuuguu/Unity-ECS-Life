using System.Text.RegularExpressions;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// calc next generation values and put in nextState
/// what is causing burst error ? 
/// </summary>
[AlwaysSynchronizeSystem]
public class GenerateNextStateSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    { 
        int[] stay = ECSGrid.stay;
        int[] born = ECSGrid.born;
        //stay[2] = stay[3] = 1;
        //born[3] = 1;
        var liveLookup =  GetComponentDataFromEntity<Live>() ;
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
                nextState.value = math.select(stay[numLiveNeighbors], born[numLiveNeighbors], live.value== 1);
            }).Run();
        return default;
    }
}

/// <summary>
/// update Live from NextState and set location 
/// </summary>

public class UpdateLiveSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    { 
        float zDead = ECSGrid.zDead;
        float zLive = ECSGrid.zLive;
        Entities
            //.WithoutBurst()
            .ForEach((ref Live live, ref Translation translation, in  NextState nextState, in Neighbors neighbor) => {
                live.value = nextState.value;
                translation.Value = new float3(translation.Value.x, translation.Value.x,
                    math.select( zDead,zLive, live.value == 1));

            }).Run();
        return default;
    }
}