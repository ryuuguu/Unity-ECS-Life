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
