using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class ECSGrid : MonoBehaviour {
    public Vector2Int size = new Vector2Int(10,10);
    public bool stressTest = false;
    public float worldSize = 10f;
    public Transform holder;
    public GameObject prefabCell;
    public GameObject prefabMesh;
    public Vector2 _offset;
    public Vector2 _scale ;
    /*
     // for next tutorial
    public Material[] materials;
    public static Material[] materialsStatic;
    */
    
    Entity[,] _cells;
    private static MeshRenderer[,] _meshRenderers;
    public static  int[] stay = new int[9];
    public static int[] born = new int[9];

    

    public float zDeadSetter;
    public static float zLive = -1;
    
    void Start() {
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabCell, settings);
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        _scale = ( Vector2.one / size);
        _offset = ((-1 * Vector2.one) + _scale)/2;
        _cells = new Entity[size.x+2,size.y+2];
        _meshRenderers = new MeshRenderer[size.x+2,size.y+2];
        var cellLocalScale  = new Vector3(_scale.x,_scale.y,_scale.x);
        
        for (int i = 0; i < size.x+2; i++) {
            for (int j = 0; j < size.y+2; j++) {
                var instance = entityManager.CreateEntity();
                entityManager.AddComponentData(instance, new Live { value = 0});
                entityManager.AddComponentData(instance, new PosXY { pos = new int2(i,j)});
                _cells[i, j] = instance;
                var c = Instantiate(prefabMesh, holder);
                var pos = new Vector3((i-1) * _scale.x + _offset.x, (j-1) * _scale.y + _offset.y, zLive);
                c.transform.localScale = cellLocalScale; 
                c.transform.localPosition = pos;
                c.name += new Vector2Int(i, j);
                _meshRenderers[i,j] = c.GetComponent<MeshRenderer>();
            }
        }
        
        for (int i = 1; i < size.x+1; i++) {
            for (int j = 1; j < size.y+1; j++) {
                var instance = _cells[i, j];
                
                entityManager.AddComponentData(instance, new SubcellIndex() {
                    index = ((i+1)%2) + (((j+1)%2)*2)
                });
                entityManager.AddComponentData(instance, new NextState() {value = 0});
                entityManager.AddComponentData(instance, new Neighbors() {
                    nw = _cells[i - 1, j - 1], n = _cells[i - 1, j], ne =  _cells[i - 1, j+1],
                    w = _cells[i , j-1], e = _cells[i, j + 1],
                    sw = _cells[i + 1, j - 1], s = _cells[i + 1, j], se =  _cells[i + 1, j + 1]
                });
                /*
                // This code is for next Tutorial
                var pos = Cell2Supercell(i,j);
                entityManager.AddSharedComponentData(instance, new SuperCellXY() {pos = pos});
                entityManager.AddChunkComponentData<SuperCellLives>(instance);
                var entityChunk = entityManager.GetChunk(instance);
                entityManager.SetChunkComponentData<SuperCellLives>(entityChunk, 
                    new SuperCellLives(){index = 0});
                */
            }
        }
        /*
        // This code is for next Tutorial 
         
        //for clarity creation of supercells is in a separate loop
        for (int i = 1; i < size.x + 1; i++) {
            for (int j = 1; j < size.y + 1; j++) {
                var pos = Cell2Supercell(i,j);
                if(i!=pos[0] || j!=pos[1]) continue;
                if (i != j) continue;
                var instance = entityManager.Instantiate(entity);
                var position = new float3((i-1) * _scale.x + _offset.x, (j-1) * _scale.y + _offset.y, zLive)*worldSize;
                entityManager.SetComponentData(instance, new Translation {Value = position});
                entityManager.AddComponentData(instance, new Scale {Value = _scale.x*worldSize*SupercellScale()});
                entityManager.AddSharedComponentData(instance, new SuperCellXY() {pos = pos});
                entityManager.AddChunkComponentData<SuperCellLives>(instance);
                var entityChunk = entityManager.GetChunk(instance);
                entityManager.SetChunkComponentData<SuperCellLives>(entityChunk, 
                    new SuperCellLives(){index = 0});
                entityManager.AddComponentData(instance, new DebugSuperCellLives { lives = new int4()});
            }
        }
        */
        entityManager.DestroyEntity(entity);
        if (stressTest) {
            //FlasherTest((size + 2 * Vector2Int.one) / 2, entityManager);
            StressTest(entityManager);
        } else {
            RPentonomio((size + 2 * Vector2Int.one) / 2, entityManager);
        }
    }
    /*
    // This code is for next Tutorial 
    public int2 Cell2Supercell(int i, int j) {
        var pos = new int2();
        pos[0] = (i  / 2) * 2; //(0,1) -> 0, (2,3) -> 2, etc.
        pos[1] = (j  / 2) * 2;
        return pos;
    }
    
    public int SupercellScale() {
        return 2;
    }
    */
    
    private void SetLive(int i, int j, EntityManager entityManager) { 
        var instance = _cells[i, j];
        entityManager.SetComponentData(instance, new Live {value = 1});
        entityManager.SetComponentData(instance, new NextState() {value = 1});
        ShowCell(new int2(i, j), true);
    }

    public static void ShowCell(int2 pos, bool val) {
        _meshRenderers[pos.x, pos.y].enabled = val;
    }
    
    void RPentonomio(Vector2Int center, EntityManager entityManager) {
        SetLive(center.x, center.y, entityManager);
        SetLive(center.x, center.y+1, entityManager);
        SetLive(center.x+1, center.y+1, entityManager);
        SetLive(center.x, center.y-1, entityManager);
        SetLive(center.x-1, center.y, entityManager);
    }

    void FlasherTest(Vector2Int center, EntityManager entityManager) {
        SetLive(center.x, center.y, entityManager);
        SetLive(center.x, center.y+1, entityManager);
        SetLive(center.x, center.y-1, entityManager);
        
    }
    
    void StressTest(EntityManager em) {
        for (int i = 1; i < size.x + 1; i++) {
            for (int j = 1; j < size.y + 1; j++) {

                if ((i + j) % 2 == 0) {
                    SetLive(i, j, em);
                }
            }
        }
    }
}
