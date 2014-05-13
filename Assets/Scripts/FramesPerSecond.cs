using UnityEngine;
using System.Collections;

public class FramesPerSecond : MonoBehaviour {

    public float updateInterval = 0.5F;
    private float totalFPS = 0;
    private int totalFrames = 0;
    private float timeUntilUpdate;

    void Start () {
        guiText.fontSize = Mathf.RoundToInt(Camera.main.pixelHeight / 30f);
        guiText.alignment = TextAlignment.Right;
        guiText.anchor = TextAnchor.UpperRight;
        guiText.text = "0 FPS";
        timeUntilUpdate = updateInterval;
    }
    
    void Update () {
        timeUntilUpdate -= Time.deltaTime;
        totalFPS += Time.timeScale / Time.deltaTime;
        ++totalFrames;
        if(timeUntilUpdate <= 0.0) {
            float fps = totalFPS / totalFrames;
            guiText.text = System.String.Format("{0:F2} FPS", fps);
            timeUntilUpdate = updateInterval;
            totalFPS = 0.0F;
            totalFrames = 0;
        }
    }
}