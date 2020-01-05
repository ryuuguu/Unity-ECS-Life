using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

//This stuff, including the commented out JobSystems, works but is not needed till next Tutorial


/// <summary>
/// SubcellIndex
///   index of cell in SuperCellLives.lives
/// </summary>
public struct SubcellIndex : IComponentData {
    public int index;
}

/// <summary>
/// SharedData Component
///   chunks cells into correct chunk
/// </summary>
public struct SuperCellXY : ISharedComponentData {
    public int2 pos; // these coordinates are the xMin, yMin corner
}

/// <summary>
/// SuperCellLives
///  Chunk Component
///  uses lives of cells to calculate image index 
/// </summary>
public struct SuperCellLives : IComponentData {
    //public int4 lives;  //was only used for creating index and debugging so removed
    // live values for 
    // p = SuperCellXY.pos
    // (p+(0,0), p+(0,1), p+(1,0), p+(1,1)
    public int index; //index of image to be displayed
} 

/// <summary>
/// DebugSuperCellLives
/// used for debugging SuperCellLives since the debugger
/// is broken for ChunkComponents 
/// </summary>
public struct DebugSuperCellLives : IComponentData {
    public int4 lives;
    public int4 livesDecoded;
    public int index;
} 

/*


/// <summary>
/// Copies ChunkComponent to instance component so it can checked in debugger
/// </summary>
[AlwaysSynchronizeSystem]
[BurstCompile]
public class UpdateDebugSuperCellLivesSystem : JobComponentSystem {
    EntityQuery m_Group;

    protected override void OnCreate() {
        // Cached access to a set of ComponentData based on a specific query
        m_Group = GetEntityQuery(ComponentType.ReadWrite<DebugSuperCellLives>(),
            ComponentType.ChunkComponent<SuperCellLives>()
        );
    }
    
    struct CellEnergyJob : IJobChunk {
        
        public ArchetypeChunkComponentType<DebugSuperCellLives> DebugSuperCellLivesType;
        public ArchetypeChunkComponentType<SuperCellLives> SuperCellLivesType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {

            var debugSuperCellLives = chunk.GetNativeArray(DebugSuperCellLivesType);

            var chunkData = chunk.GetChunkComponentData<SuperCellLives>(SuperCellLivesType);
            for (var i = 0; i < chunk.Count; i++) {
                int4 livesDecoded = new int4();
                int encoded = chunkData.index;
                for (int j = 0; j < 4; j++) {
                    livesDecoded[j] = encoded % 2;
                    encoded >>= 1;
                }
                debugSuperCellLives[i] = new DebugSuperCellLives() {
                    //lives = chunkData.lives,
                    index = chunkData.index,
                    livesDecoded  = livesDecoded
                };
            }
        }


    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies) {
        
        var DebugSuperCellLivesType = GetArchetypeChunkComponentType<DebugSuperCellLives>();
        var SuperCellLivesType = GetArchetypeChunkComponentType<SuperCellLives>();

        var job = new CellEnergyJob() {
            DebugSuperCellLivesType = DebugSuperCellLivesType,
            SuperCellLivesType = SuperCellLivesType
        };
        return job.Schedule(m_Group, inputDependencies);
    }
}


*/