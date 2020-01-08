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
        StartCoroutine(FPS());
    }

    private IEnumerator FPS() {
        for (;;) {
            int lastFrameCount = Time.frameCount;
            float lastTime = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(frequency);
            float timeSpan = Time.realtimeSinceStartup - lastTime;
            int frameCount = Time.frameCount - lastFrameCount;

            // Display it
            FramesPerSec = Mathf.RoundToInt(frameCount / timeSpan);
            _text.text = FramesPerSec.ToString() + " fps";
        }
    }
}