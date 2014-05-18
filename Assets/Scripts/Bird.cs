using UnityEngine;
using System.Collections;

public class Bird : MonoBehaviour {

    public Vector2 jumpForce = new Vector2(0, 250);
    public AudioClip jumpSound;
    private Vector2 birdPosition;
    private Transform cachedTransform;

    /*
     * Caching the transform component is more efficient.  Store the 
     * transform component when the script wakes up
     */
    void Awake() {
        cachedTransform = transform;
    }
	
	void Update () {
        // Add force to the bird if space key or a touch has occurred
        if(Input.GetKeyUp("space") || (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began)) {
            rigidbody2D.velocity = Vector2.zero;
            rigidbody2D.AddForce(jumpForce);
            AudioSource.PlayClipAtPoint(jumpSound, new Vector3(0, 0, 0), 0.2f);
        }
        
        // Determine where the bird is in relation to screen points and game over if out of bounds
        birdPosition = Camera.main.WorldToScreenPoint(cachedTransform.position);
        if (birdPosition.y > Screen.height || birdPosition.y < 0) {
            GameOver();
        }
	}

    /*
     * Unlock the achievement for getting game over and then load the
     * game over screen
     */
    void GameOver() {
        #if UNITY_ANDROID
            SocialManager.UnlockAchievement("CgkI-uuXy7ESEAIQAg");
        #elif UNITY_IPHONE
            SocialManager.UnlockAchievement("CgkI_uuXy7ESEAIQAg");
        #endif
        Application.LoadLevel("game_over");
    }

    /*
     * If the bird collides with a bottom or top wall object then 
     * initiate game over
     */
    void OnTriggerEnter2D(Collider2D other) {
        if(other != null && other.CompareTag("wall")) {
            GameOver();
        }
    }

}
