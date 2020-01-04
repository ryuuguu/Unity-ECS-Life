using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;


// made states int for simpler code 
// maybe making them bool for better packing would be faster 


[GenerateAuthoringComponent]
public struct Live : IComponentData {
    public int value;
}

public struct NextState : IComponentData {
    public int value;
}


//this might be better as an array but arrays are not blitable
// components fields must be blitable
// maybe dynamics buffers would be faster but probably not
public struct Neighbors : IComponentData {
    public Entity nw;
    public Entity n;
    public Entity ne;
    public Entity w;
    public Entity e;
    public Entity sw;
    public Entity s;
    public Entity se;
}


public struct ChangedTag : IComponentData { }

/// <summary>
/// used to make it easier to check SuperCellLive values are being set correctly
/// </summary>
public struct DebugPosXY : IComponentData {
    public int2 pos;
}


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

public struct debugFilterCount : IComponentData {
    public int Value;
}