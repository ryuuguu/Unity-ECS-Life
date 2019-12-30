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
    // Trying to get job to run on worker threads
    //[ReadOnly] public ComponentDataFromEntity<Live> liveLookupX;
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    { 
        // did not use ECSGrid.stay & ECSGrid.born because they caused a Burst error
        // born[numLiveNeighbors] if born is a pointer to ECSGrid.born this cause burst error
        int[] stay = new int[9];
        int[] born = new int[9];
        stay[2] = stay[3] = 1; // does NOT include self in count
        born[3] = 1;

        var liveLookup = GetComponentDataFromEntity<Live>(); 
        //var liveLookup = liveLookupX ;
        Entities
            // Burst error about class that I do not understand    
            //.WithoutBurst()
            // for debugging only
            //.ForEach((ref NextState nextState, ref DebugIndex debugIndex, in Live live, in Neighbors neighbors) => {
            .ForEach((ref NextState nextState, in Live live, in Neighbors neighbors) => {
                int numLiveNeighbors = 0;
                
                numLiveNeighbors += liveLookup[neighbors.nw].value;
                numLiveNeighbors += liveLookup[neighbors.n].value;
                numLiveNeighbors += liveLookup[neighbors.ne].value;
                numLiveNeighbors += liveLookup[neighbors.w].value;
                numLiveNeighbors += liveLookup[neighbors.e].value;
                numLiveNeighbors += liveLookup[neighbors.sw].value;
                numLiveNeighbors += liveLookup[neighbors.s].value;
                numLiveNeighbors += liveLookup[neighbors.se].value;
            
                // for debugging only
                /*
                debugIndex.index = numLiveNeighbors;
                debugIndex.countNext += 1;
                */
                
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
            // for debugging only
            //.ForEach((ref Live live, ref DebugIndex debugIndex, ref Translation translation, in  NextState nextState, in Neighbors neighbor) => {
            .ForEach((ref Live live,  ref Translation translation, in  NextState nextState, in Neighbors neighbor) => {
                live.value = nextState.value;
                translation.Value = new float3(translation.Value.x, translation.Value.y,
                    math.select( zDead,zLive, live.value == 1));
                // for debugging only
                //debugIndex.countLive += 1;
            }).Run();
        return default;
    }
}