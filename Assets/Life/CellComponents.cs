﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


// made states int for simpler code 
// maybe making them bool for better packing would be faster 


[GenerateAuthoringComponent]
public struct Live : IComponentData {
    public int value;
}

public struct NextState : IComponentData {
    public int value;
}

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