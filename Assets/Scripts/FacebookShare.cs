using UnityEngine;
using System.Collections;

public class FacebookShare : MonoBehaviour {
	
	void Update () {
        if(isTouched()) {
            FacebookManager.Share("Check out my Floppy Clone greatness!", "I have a high score of " + (PlayerPrefs.GetInt("high_score", 0)).ToString() + " points.  Can you beat me?", "Open source Flappy Bird type game created with Unity3D", "https://raw.githubusercontent.com/nraboy/Floppy-Clone/master/Assets/Icons/android.png", "https://github.com/nraboy/Floppy-Clone");
        }
	}

    public bool isTouched() {
        bool result = false;
        if(Input.touchCount == 1) {
            if(Input.touches[0].phase == TouchPhase.Ended) {
                Vector3 wp = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                Vector2 touchPos = new Vector2(wp.x, wp.y);
                if (collider2D == Physics2D.OverlapPoint(touchPos)) {
                    result = true;
                }
            }
        }
        if(Input.GetMouseButtonUp(0)) {
            Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos = new Vector2(wp.x, wp.y);
            if (collider2D == Physics2D.OverlapPoint(mousePos)) {
                result = true;
            }
        }
        return result;
    }

}
