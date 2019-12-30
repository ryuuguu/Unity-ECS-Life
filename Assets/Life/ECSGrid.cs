using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class ECSGrid : MonoBehaviour {
    public bool makeDebugComponents = false;
    public Vector2Int size = new Vector2Int(10,10);
    public float worldSize = 10f;
    public static float zLive = -1;
    public static float zDead = 1;
    public Transform holder;
    public GameObject prefabCell;
    public Vector2 _offset;
    public Vector2 _scale ;
    Entity[,] _cells;
    public static  int[] stay = new int[9];
    public static int[] born = new int[9];

    void Awake() {
        stay[2] = stay[3] = 1; // does NOT include self in count
        born[3] = 1;
    }
    
    void Start() {
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabCell, settings);
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        
        _scale = ( Vector2.one / size);
        _offset = ((-1 * Vector2.one) + _scale)/2;
        _cells = new Entity[size.x+2,size.y+2];
        
        for (int i = 0; i < size.x+2; i++) {
            for (int j = 0; j < size.y+2; j++) {
                var instance = entityManager.Instantiate(entity);
                var position = new float3((i-1) * _scale.x + _offset.x, (j-1) * _scale.y + _offset.y, zDead)*worldSize;
                entityManager.SetComponentData(instance, new Translation {Value = position});
                entityManager.AddComponentData(instance, new Scale {Value = _scale.x*worldSize});
                entityManager.AddComponentData(instance, new Live { value = 0});
                _cells[i, j] = instance;
                if (makeDebugComponents) {
                    entityManager.AddComponentData(instance, new DebugIJ() { pos = new int2(i,j)});
                    entityManager.AddComponentData(instance, new DebugIndex() { index = -1 });

                }
            }
        }
        entityManager.DestroyEntity(entity);
        for (int i = 1; i < size.x+1; i++) {
            for (int j = 1; j < size.y+1; j++) {
                var instance = _cells[i, j];
                entityManager.AddComponentData(instance, new NextState() {value = 0});
                

                entityManager.AddComponentData(instance, new Neighbors() {
                    nw = _cells[i - 1, j - 1], n = _cells[i - 1, j], ne =  _cells[i - 1, j+1],
                    w = _cells[i , j-1], e = _cells[i, j + 1],
                    sw = _cells[i + 1, j - 1], s = _cells[i + 1, j], se =  _cells[i + 1, j + 1]
                });
                
                if ((i + j) % 2 == 0) {
                    var position = new float3((i - 1) * _scale.x + _offset.x, (j - 1) * _scale.y + _offset.y, zLive)*worldSize;
                    entityManager.SetComponentData(instance, new Translation {Value = position});
                    entityManager.SetComponentData(instance, new Live { value = 1});
                    entityManager.SetComponentData(instance, new NextState() { value = 1});
                } 
                
            }
        }
        RPentonomio((size+2*Vector2Int.one)/2);
        
    }

    void RPentonomio(Vector2Int center) {
        /*
        _cells[center.x, center.y].live = true;
        _cells[center.x, center.y+1].live = true;
        _cells[center.x+1, center.y+1].live = true;
        _cells[center.x, center.y-1].live = true;
        _cells[center.x-1, center.y].live = true;
        */
        
    }
    /*
    void Update() {
        for (int i = 1; i < size.x + 1; i++) {
            for (int j = 1; j < size.y + 1; j++) {
                int count = 0;
                for (int k = -1; k < 2; k++) {
                    for (int l = -1; l < 2; l++) {
                        if (_cells[i + k, j + l].live) count++;
                    }
                }
                _cells[i, j].nextState = _cells[i, j].live ? stay[count] : born[count];
            }
        }

        for (int i = 1; i < size.x + 1; i++) {
            for (int j = 1; j < size.y + 1; j++) {
                _cells[i, j].live = _cells[i, j].nextState;
            }
        }
    }
    */
}
