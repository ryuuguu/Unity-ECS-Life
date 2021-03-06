﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Analytics;

public class ECSGridSCDeaths : MonoBehaviour {
    public Vector2Int size = new Vector2Int(10,10);
    public bool stressTest = false;
    public float worldSize = 10f;
    public Transform holder;
    public Transform holderSC;
    public GameObject prefabCell;
    public GameObject prefabMesh;
    public Vector2 _offset;
    public Vector2 _scale ;
    
     // for next tutorial
    public Material[] materials;
    public static Material[] materialsStatic;
    public int superCellScale = 2;
    
    Entity[,] _cells;
    private static MeshRenderer[,] _meshRenderersSC;
    private static MeshRenderer[,] _meshRenderers;
    private static List<ShowSuperCellData> SuperCellCommandBuffer = new List<ShowSuperCellData>();
    private static List<ShowSuperCellData> CellCommandBuffer = new List<ShowSuperCellData>();

    public float zDeadSetter;
    public static float zLive = -1;

    public void Start() {
        // clearing buffers in case "Entering Playmode with Reload Domain disabled."
        // is set. This experimental but is set by something in the preview packages
        //SuperCellCommandBuffer.Clear();
        //CellCommandBuffer.Clear();
        InitDisplay();
        InitSuperCellDisplay();
        InitECS();
    }

    private void Update() {
        //RunSCCommandBuffer();
        //RunCellCommandBuffer();
    }

    

    public void InitDisplay() {
        _scale = ( Vector2.one / size);
        _offset = ((-1 * Vector2.one) + _scale)/2;
        _meshRenderers = new MeshRenderer[size.x+2,size.y+2];
        var cellLocalScale  = new Vector3(_scale.x,_scale.y,_scale.x);
        for (int i = 0; i < size.x+2; i++) {
            for (int j = 0; j < size.y+2; j++) {
                var c = Instantiate(prefabMesh, holder);
                var pos = new Vector3((i-1) * _scale.x + _offset.x, (j-1) * _scale.y + _offset.y, zLive);
                c.transform.localScale = cellLocalScale; 
                c.transform.localPosition = pos;
                c.name += new Vector2Int(i, j);
                _meshRenderers[i,j] = c.GetComponent<MeshRenderer>();
            }
        }
    }
    
    
    public void InitSuperCellDisplay() {
        _scale = ( Vector2.one / size);
        _offset = ((-1 * Vector2.one) + _scale)/2;
        _meshRenderersSC = new MeshRenderer[size.x+2,size.y+2];
        materialsStatic = materials;
        var cellLocalScale  = new Vector3(_scale.x,_scale.y,_scale.x) * superCellScale;
        for (int i = 0; i < size.x+2; i++) {
            for (int j = 0; j < size.y+2; j++) {
                var coord = Cell2Supercell(i, j);
                if (coord[0] != i || coord[1] != j) continue;
                var c = Instantiate(prefabMesh, holderSC);
                var pos = new Vector3((1f/superCellScale +i-1) * _scale.x + _offset.x,
                    (1f/superCellScale +j-1) * _scale.y + _offset.y, zLive);
                c.transform.localScale = cellLocalScale; 
                c.transform.localPosition = pos;
                c.name += new Vector2Int(i, j);
                _meshRenderersSC[i,j] = c.GetComponent<MeshRenderer>();
                
            }
        }
    }
    
    void InitECS() {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        _cells = new Entity[size.x+2,size.y+2];
        
        for (int i = 0; i < size.x+2; i++) {
            for (int j = 0; j < size.y+2; j++) {
                var instance = entityManager.CreateEntity();
                entityManager.AddComponentData(instance, new Live { value = 0});
                entityManager.AddComponentData(instance, new PosXY { pos = new int2(i,j)});
                _cells[i, j] = instance;
            }
        }
        
        
        for (int i = 1; i < size.x+1; i++) {
            for (int j = 1; j < size.y+1; j++) {
                var instance = _cells[i, j];
                
                // This code is for next Tutorial
                entityManager.AddComponentData(instance, new SubcellIndex() {
                    index = ((i)%2) + (((j+1)%2)*2)
                });
                
                entityManager.AddComponentData(instance, new NextState() {value = 0});
                entityManager.AddComponentData(instance, new Neighbors() {
                    nw = _cells[i - 1, j - 1], n = _cells[i - 1, j], ne =  _cells[i - 1, j+1],
                    w = _cells[i , j-1], e = _cells[i, j + 1],
                    sw = _cells[i + 1, j - 1], s = _cells[i + 1, j], se =  _cells[i + 1, j + 1]
                });
                
                var pos = Cell2Supercell(i,j);
                entityManager.AddSharedComponentData(instance, new SuperCellXY() {pos = pos});
                entityManager.AddChunkComponentData<SuperCellLives>(instance);
                
               
                var coord = Cell2Supercell(i, j);
                if (coord[0] == i || coord[1] == j) {
                    var instanceSuperCell = entityManager.CreateEntity();
                    entityManager.AddComponentData(instanceSuperCell, new PosXY { pos = new int2(i,j)});
                    entityManager.AddComponentData(instanceSuperCell, new SuperCellDisplay() { changed = false, deaths = 0});

                }
                
            }
        }
        
        InitLive(entityManager);
    }
    
    // This code is for next Tutorial 
    public int2 Cell2Supercell(int i, int j) {
        var pos = new int2();
        pos[0] = (i  / 2) * 2; //(0,1) -> 0, (2,3) -> 2, etc.
        pos[1] = (j  / 2) * 2;
        return pos;
    }
    

    public void InitLive(EntityManager entityManager) {
        if (stressTest) {
            //FlasherTest((size + 2 * Vector2Int.one) / 2, entityManager);
            BarTest( entityManager);
            //StressTest(entityManager);
        } 
        RPentonomio((size + 2 * Vector2Int.one) / 2, entityManager);
        
    }
    
    private void SetLive(int i, int j, EntityManager entityManager) {
        if (_cells != null) {
            var instance = _cells[i, j];
            entityManager.SetComponentData(instance, new Live {value = 1});
            entityManager.SetComponentData(instance, new NextState() {value = 0});
        }
        else {
            ShowCell(new int2(i, j), true);
        }
    }

    public static void ShowCell(int2 pos, bool val) {
        //Debug.Log(" ShowCell: "+ pos + " : "+ val);
        
        _meshRenderers[pos.x, pos.y].enabled = val;
        
    }
    
    public static void ShowSuperCell(int2 pos,int val) {
        Debug.Log(" ShowSuperCell: "+ pos + " : "+ val);
        _meshRenderersSC[pos.x, pos.y].enabled = val > 10;
        
        
    }
    
    private static void RunSCCommandBuffer() {
        foreach (var command in SuperCellCommandBuffer) {
            Debug.Log(" ShowSuperCell: "+ command.pos + " : "+ command.val);
            _meshRenderersSC[command.pos.x,command. pos.y].enabled = command.val != 0;
            if (command.val != 0) {
                _meshRenderersSC[command.pos.x, command.pos.y].material = materialsStatic[command.val];
            }
        }
        SuperCellCommandBuffer.Clear();
    }
    

    
    void RPentonomio(Vector2Int center, EntityManager entityManager) {
        SetLive(center.x, center.y, entityManager);
        SetLive(center.x, center.y+1, entityManager);
        SetLive(center.x+1, center.y-1, entityManager);
        SetLive(center.x, center.y-1, entityManager);
        SetLive(center.x-1, center.y, entityManager);
    }

    void FlasherTest(Vector2Int center, EntityManager entityManager) {
        SetLive(center.x, center.y, entityManager);
        SetLive(center.x, center.y+1, entityManager);
        SetLive(center.x, center.y-1, entityManager);
        
    }
    
    void BarTest(EntityManager em) {
        int i = 1;
        for (int j = 1; j < size.y + 1; j++) {
            SetLive(i, j, em);
        }
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

    struct ShowSuperCellData {
        public int2 pos;
        public int val;
    }
    
}
