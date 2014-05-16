using UnityEngine;
using System.Collections;
using Chartboost;

public class ChartboostConfig : MonoBehaviour {

    public static bool wasShown = false;

    void Awake() {
        #if UNITY_ANDROID
            CBBinding.init("0000", "1111");
        #elif UNITY_IPHONE
            CBBinding.init("2222", "3333");
        #endif
    }

    void Start() {
        CBBinding.cacheInterstitial("Default");
    }

}
