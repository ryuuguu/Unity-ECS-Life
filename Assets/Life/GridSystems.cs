using System.Text.RegularExpressions;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// calc next generatio values and ut in next state
/// what is causing burst error ? 
/// </summary>
[AlwaysSynchronizeSystem]
public class nextGeneration : JobComponentSystem
{
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        int[] stay = ECSGrid.stay;
        int[] born = ECSGrid.born;
            
       var liveLookup =  GetComponentDataFromEntity<Live>() ;
        
        Entities
            .WithoutBurst()
            .ForEach((ref NextState nextState, in Live live, in Neighbors neighbors) =>
            {
                
                int numLiveNeighbors = 0;

                // check current life status of all neighbors
                for (int i = 0; i < 9; i++) {
                    numLiveNeighbors += liveLookup[neighbors.nw].value;
                    numLiveNeighbors += liveLookup[neighbors.n].value;
                    numLiveNeighbors += liveLookup[neighbors.ne].value;
                    numLiveNeighbors += liveLookup[neighbors.w].value;
                    numLiveNeighbors += liveLookup[neighbors.e].value;
                    numLiveNeighbors += liveLookup[neighbors.sw].value;
                    numLiveNeighbors += liveLookup[neighbors.s].value;
                    numLiveNeighbors += liveLookup[neighbors.se].value;
                }

                nextState.value = math.select(stay[numLiveNeighbors], born[numLiveNeighbors], live.value== 1);

            }).Run();

        return default;
    }
}