using UnityEngine;
using System.Collections;
using Chartboost;

public class ChartboostConfig : MonoBehaviour {

    public static bool wasShown = false;

    void Awake() {
        #if UNITY_ANDROID
            CBBinding.init("1111", "2222");
        #elif UNITY_IPHONE
            CBBinding.init("3333", "4444");
        #endif
    }

    void Start() {
        CBBinding.cacheInterstitial("Default");
    }

}
