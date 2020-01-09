using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/* **************************************************************************
 * FPS COUNTER
 * **************************************************************************
 * Written by: Annop "Nargus" Prapasapong
 * Created: 7 June 2012
 * update 2020 by Grant Morgan
 * *************************************************************************/


[RequireComponent(typeof(Text))]
public class FPSCounter : MonoBehaviour {
    public float frequency = 0.5f;
    public int FramesPerSec { get; protected set; }

    private Text _text;
    private void Start() {
        _text = GetComponent<Text>();
        //StartCoroutine(FPS());
    }

    void Update() {
        float timeSpan = Time.deltaTime;

        // Display it
        FramesPerSec = Mathf.RoundToInt(1/ timeSpan);
        _text.text = FramesPerSec.ToString() + " fps\nFrame:" + Time.frameCount;
        
    }
}