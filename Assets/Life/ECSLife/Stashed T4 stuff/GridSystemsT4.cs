﻿using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using UnityEngine;




/*
/// <summary>
/// update Live from NextState and add ChangedTag
/// </summary>


[AlwaysSynchronizeSystem]
[UpdateBefore(typeof(UpdateNextSateSystem))]
//[UpdateBefore(typeof(UpdateMarkChangeSystem))]
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


[UpdateBefore(typeof(UpdateSuperCellIndexSystem))]
[AlwaysSynchronizeSystem]
[BurstCompile]
public class UpdateNextSateSystem : JobComponentSystem {
    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        
        JobHandle jobHandle = Entities
            .ForEach((Entity entity, int entityInQueryIndex, ref Live live, in  NextState nextState)=> {
                live.value = nextState.value;
            }).Schedule( inputDeps);
        return jobHandle;
    }
}

*/
/*
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

[UpdateInGroup(typeof(PresentationSystemGroup))]
[AlwaysSynchronizeSystem]
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


[UpdateInGroup(typeof(PresentationSystemGroup))]
[AlwaysSynchronizeSystem]
//[UpdateAfter(typeof(UpdateDisplayChangedSystem))]
[BurstCompile]
public class UpdateClearChangedSystem : JobComponentSystem {
    // I would like to do this in EndPresentationEntityCommandBufferSystem 
    // but it does not exist
    protected BeginSimulationEntityCommandBufferSystem m_BeginSimulationEcbSystem;
    
 
    protected override void OnCreate() {
        base.OnCreate();
        // Find the ECB system once and store it for later usage
        m_BeginSimulationEcbSystem = World
            .GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }
 
    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        var ecb = m_BeginSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
        var jobHandle =
            Entities
                .WithAll<ChangedTag>()
                .ForEach((Entity entity, int entityInQueryIndex) => {
                    ecb.RemoveComponent<ChangedTag>(entityInQueryIndex, entity);
                }).Schedule(inputDeps);
        m_BeginSimulationEcbSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}
*/
/*
[AlwaysSynchronizeSystem]
[BurstCompile]
public class UpdateSuperCellIndexSystem : JobComponentSystem {
    EntityQuery m_Group;

    protected override void OnCreate() {
        // Cached access to a set of ComponentData based on a specific query
        m_Group = GetEntityQuery(
            ComponentType.ReadOnly<Live>(),
            ComponentType.ReadOnly<SubcellIndex>(),
            ComponentType.ReadOnly<PosXY>(),
            ComponentType.ChunkComponent<SuperCellLives>()
            
        );
    }
    
    struct SuperCellIndexJob : IJobChunk {
        
        [ReadOnly]public ArchetypeChunkComponentType<Live> LiveType;
        [ReadOnly]public ArchetypeChunkComponentType<SubcellIndex> SubcellIndexType;
        [ReadOnly]public ArchetypeChunkComponentType<PosXY> PosXYType;
        public ArchetypeChunkComponentType<SuperCellLives> SuperCellLivesType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var lives = chunk.GetNativeArray(LiveType);
            var SubcellIndices = chunk.GetNativeArray(SubcellIndexType);
            var posXYs = chunk.GetNativeArray(PosXYType);
            
            var scLives = new int4();
            for (var i = 0; i < chunk.Count; i++) {
                scLives[SubcellIndices[i].index] = lives[i].value;
            }
            int index = 0;
            for (int i = 0; i < 4; i++) {
                index +=   scLives[i]<< i;
            }
            
            var pos = new int2();
            pos[0] = (posXYs[0].pos.x / 2) * 2; //(0,1) -> 0, (2,3) -> 2, etc.
            pos[1] = (posXYs[0].pos.y  / 2) * 2;
            
            
            var chunkData = chunk.GetChunkComponentData(SuperCellLivesType);
            bool changed = index != chunkData.index;
            chunk.SetChunkComponentData(SuperCellLivesType,
                new SuperCellLives() {
                    index = index,
                    changed = changed,
                    pos = pos
                });
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies) {
        var LiveType = GetArchetypeChunkComponentType<Live>(true);
        var SubcellIndexType = GetArchetypeChunkComponentType<SubcellIndex>(false);
        var SuperCellLivesType = GetArchetypeChunkComponentType<SuperCellLives>();
        var PosXYType = GetArchetypeChunkComponentType<PosXY>();

        var job = new SuperCellIndexJob() {
            SubcellIndexType = SubcellIndexType,
            LiveType = LiveType,
            SuperCellLivesType = SuperCellLivesType,
            PosXYType = PosXYType
        };
        return job.Schedule(m_Group, inputDependencies);
    }
}


[AlwaysSynchronizeSystem]
[UpdateAfter(typeof(UpdateSuperCellIndexSystem))]
public class UpdateSuperCellChangedSystem : JobComponentSystem {
    EntityQuery m_Group;

    protected override void OnCreate() {
        // Cached access to a set of ComponentData based on a specific query
        m_Group = GetEntityQuery(
            ComponentType.ChunkComponentReadOnly<SuperCellLives>()
           
        );
    }
    
    struct SuperCellDisplayJob : IJobChunk {
        
        public ArchetypeChunkComponentType<SuperCellLives> SuperCellLivesType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var chunkData = chunk.GetChunkComponentData(SuperCellLivesType);
            if (chunkData.changed) {
               ECSGridSuperCell.ShowSuperCell(chunkData.pos, chunkData.index);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies) {
        
        var SuperCellLivesType = GetArchetypeChunkComponentType<SuperCellLives>();

        var job = new SuperCellDisplayJob() {
            SuperCellLivesType = SuperCellLivesType
        };
        job.Run(m_Group);
        return default;
    }
    
}
*/
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
    
    struct DebugSuperCellLivesJob : IJobChunk {
        
        public ArchetypeChunkComponentType<DebugSuperCellLives> DebugSuperCellLivesType;
        public ArchetypeChunkComponentType<SuperCellLives> SuperCellLivesType;
        [ReadOnly] public ArchetypeChunkSharedComponentType<SuperCellXY> superCellSharedType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {

            var debugSuperCellLives = chunk.GetNativeArray(DebugSuperCellLivesType);
            
            int sharedComponentIndex = chunk.GetSharedComponentIndex(superCellSharedType);
            int numSc = chunk.NumSharedComponents();
            //var x = chunk.GetSharedComponentData()
            var chunkData = chunk.GetChunkComponentData(SuperCellLivesType);
            
            //int uniqueIndex = chunkData.indices.IndexOf(sharedComponentIndex);
            //chunk.GetSharedComponentData<SuperCellXY>()
            for (var i = 0; i < chunk.Count; i++) {
                int4 livesDecoded = new int4();
                int encoded = chunkData.index;
                for (int j = 0; j < 4; j++) {
                    livesDecoded[j] = encoded % 2;
                    encoded >>= 1;
                }
                debugSuperCellLives[i] = new DebugSuperCellLives() {
                    index = chunkData.index,
                    livesDecoded  = livesDecoded,
                    changed = chunkData.changed,
                    pos = chunkData.pos
                };
            }
        }


    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies) {
        
        var DebugSuperCellLivesType = GetArchetypeChunkComponentType<DebugSuperCellLives>();
        var SuperCellLivesType = GetArchetypeChunkComponentType<SuperCellLives>();
        var SuperCellXYType = GetArchetypeChunkSharedComponentType<SuperCellXY>();
        
        var job = new DebugSuperCellLivesJob() {
            DebugSuperCellLivesType = DebugSuperCellLivesType,
            SuperCellLivesType = SuperCellLivesType,
            superCellSharedType = SuperCellXYType
        };
        return job.Schedule(m_Group, inputDependencies);
    }
}
*/

