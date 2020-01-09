using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;


/// <summary>
/// SharedData Component
///   chunks cells into correct chunk
///    pos is only used to decide what cell goes into which Chunk
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
    public int index; //index of image to be displayed
    public bool changed;
    public int2 pos;
} 

/// <summary>
/// DebugSuperCellLives
/// used for debugging SuperCellLives since the debugger
/// is broken for ChunkComponents 
/// </summary>
public struct DebugSuperCellLives : IComponentData {
    public int4 livesDecoded;
    public int index;
    public bool changed;
    public int2 pos;
} 

/// <summary>
/// SubcellIndex
///   relative pos of a cell in its SuperCell
/// </summary>
public struct SubcellIndex : IComponentData {
    public int index;
}
