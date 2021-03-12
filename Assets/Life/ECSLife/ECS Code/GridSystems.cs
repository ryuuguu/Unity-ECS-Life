using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using UnityEngine;


/// <summary>
/// GenerateNextStateSystem 
/// generate state of Cell in next genration and store in Next State
/// </summary>
[AlwaysSynchronizeSystem]
[UpdateBefore(typeof(UpdateNextSateSystem))]
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

/// <summary>
/// UpdateNextSateSystem
///   copy NExtState to live
/// </summary>
[UpdateBefore(typeof(UpdateSuperCellIndexSystem))]
[AlwaysSynchronizeSystem]
[BurstCompile]
public class UpdateNextSateSystem : JobComponentSystem {
    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        
        Entities
            .WithoutBurst()
            .ForEach((Entity entity, int entityInQueryIndex, ref Live live, in  NextState nextState, in PosXY posXY)=> {
                if (live.value != nextState.value) {
                    live.value = nextState.value;
                    ECSGridSCDeaths.ShowCell(posXY.pos, live.value==1);
                }
            }).Run();
        return default;
    }
}

/// <summary>
/// UpdateSuperCellIndexSystem
///     Calculate new image index for SuperCellLives
///     Set pos of SuperCellLives
///     set changed of SuperCellLives
/// </summary>
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
        [ReadOnly]public ComponentTypeHandle<Live> LiveType;
        [ReadOnly]public ComponentTypeHandle<SubcellIndex> SubcellIndexType;
        [ReadOnly]public ComponentTypeHandle<PosXY> PosXYType;
        public ComponentTypeHandle<SuperCellLives> SuperCellLivesType;

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
                    // for faster less robust code uncomment the 3 lines at the end of
                    // ECSGridSuperCell.InitECS() around SetChunkComponentData<SuperCellLives>
                    // uncomment the next line and comment the one after 
                    //pos = chunkdata.pos
                    pos = pos
                });
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies) {
        var LiveType = GetComponentTypeHandle<Live>(true);
        var SubcellIndexType = GetComponentTypeHandle<SubcellIndex>(false);
        var SuperCellLivesType = GetComponentTypeHandle<SuperCellLives>();
        var PosXYType = GetComponentTypeHandle<PosXY>();

        var job = new SuperCellIndexJob() {
            SubcellIndexType = SubcellIndexType,
            LiveType = LiveType,
            SuperCellLivesType = SuperCellLivesType,
            PosXYType = PosXYType
        };
        return job.Schedule(m_Group, inputDependencies);
    }
}


/// <summary>
/// UpdateSuperCellChangedSystem
///   Check all SuperCells
///   Call Monobehaviour to update changed SuperCells 
/// </summary>
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
        
        public ComponentTypeHandle<SuperCellLives> SuperCellLivesType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var chunkData = chunk.GetChunkComponentData(SuperCellLivesType);
            if (chunkData.changed) {
               //ECSGridSuperCell.ShowSuperCell(chunkData.pos, chunkData.index);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies) {
        
        var SuperCellLivesType = GetComponentTypeHandle<SuperCellLives>();

        var job = new SuperCellDisplayJob() {
            SuperCellLivesType = SuperCellLivesType
        };
        job.Run(m_Group);
        return default;
    }
    
}
/*
/// <summary>
/// Copies ChunkComponent to instance component so it can checked in debugger
/// </summary>
[UpdateAfter(typeof(UpdateSuperCellIndexSystem))]
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
            var chunkData = chunk.GetChunkComponentData(SuperCellLivesType);
            
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

