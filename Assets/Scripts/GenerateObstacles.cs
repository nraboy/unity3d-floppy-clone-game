using UnityEngine;
using System.Collections;

public class GenerateObstacles : MonoBehaviour {

	void Start () {
        StartCoroutine("CreateObstacle");
	}

    IEnumerator CreateObstacle() {
        float waitTime = 2.5f;
        while(true) {
            ObjectPool.instance.GetObjectForType("Obstacle", true);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
