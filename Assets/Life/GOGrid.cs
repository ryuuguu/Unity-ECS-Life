using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// GameObject Grid
/// this is not a good OO design
/// it is designed to show what the ECS version is doing in way that is readable
/// for people who do ECS yet
/// </summary>
public class GOGrid : MonoBehaviour {
    public Vector2Int size = new Vector2Int(10,10);
    public float zLive = -1;
    public Transform holder;
    public GOCell prefabCell;
    public Vector2 _offset;
    public Vector2 _scale ;
    GOCell[,] _cells;
    public bool[] stay = new bool[10];
    public bool[] born = new bool[10];
    void Start() {
        _scale = Vector2.one / size;
        _offset = ((-1 * Vector2.one) + _scale)/2;
        _cells = new GOCell[size.x+2,size.y+2];
        var cellLocalScale  = new Vector3(_scale.x,_scale.y,_scale.x);
        for (int i = 0; i < size.x+2; i++) {
            for (int j = 0; j < size.y+2; j++) {
                var c = Instantiate(prefabCell, holder);
                var pos = new Vector3((i-1) * _scale.x + _offset.x, (j-1) * _scale.y + _offset.y, zLive);
                c.transform.localScale = cellLocalScale; 
                c.transform.localPosition = pos;
                c.name += new Vector2Int(i, j);
                c.live = false;
                _cells[i, j] = c;
            }
        }
        RPentonomio((size+2*Vector2Int.one)/2);
        stay[3] = stay[4] = true; // includes self in count
        born[3] = true;
    }

    void RPentonomio(Vector2Int center) {
        
        _cells[center.x, center.y].live = true;
        _cells[center.x, center.y+1].live = true;
        _cells[center.x+1, center.y+1].live = true;
        _cells[center.x, center.y-1].live = true;
        _cells[center.x-1, center.y].live = true;
        
    }
    
    void Update() {
        
        //this is done by GenerateNextStateSystem in ECS version
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

        //this is done by UpdateLiveSystem  in ECS version
        for (int i = 1; i < size.x + 1; i++) {
            for (int j = 1; j < size.y + 1; j++) {
                _cells[i, j].live = _cells[i, j].nextState;
            }
        }
    }
}
