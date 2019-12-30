using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOCell : MonoBehaviour {
    public MeshRenderer meshRenderer;
    protected bool _live = false;
    public bool nextState = false;
    public bool live {
        get => _live;
        set { meshRenderer.enabled = value;
            _live = value;
        }
    }
}
