using UnityEngine;
using System;
using System.Collections;
using Chartboost;


public class CBEventListener : MonoBehaviour {
	
#if UNITY_ANDROID || UNITY_IPHONE

	void OnEnable() {
		// Listen to all impression-related events
		CBManager.didFailToLoadInterstitialEvent += didFailToLoadInterstitialEvent;
		CBManager.didDismissInterstitialEvent += didDismissInterstitialEvent;
		CBManager.didCloseInterstitialEvent += didCloseInterstitialEvent;
		CBManager.didClickInterstitialEvent += didClickInterstitialEvent;
		CBManager.didCacheInterstitialEvent += didCacheInterstitialEvent;
		CBManager.didShowInterstitialEvent += didShowInterstitialEvent;
		CBManager.didFailToLoadMoreAppsEvent += didFailToLoadMoreAppsEvent;
		CBManager.didDismissMoreAppsEvent += didDismissMoreAppsEvent;
		CBManager.didCloseMoreAppsEvent += didCloseMoreAppsEvent;
		CBManager.didClickMoreAppsEvent += didClickMoreAppsEvent;
		CBManager.didCacheMoreAppsEvent += didCacheMoreAppsEvent;
		CBManager.didShowMoreAppsEvent += didShowMoreAppsEvent;
	}


	void OnDisable() {
		// Remove event handlers
		CBManager.didFailToLoadInterstitialEvent -= didFailToLoadInterstitialEvent;
		CBManager.didDismissInterstitialEvent -= didDismissInterstitialEvent;
		CBManager.didCloseInterstitialEvent -= didCloseInterstitialEvent;
		CBManager.didClickInterstitialEvent -= didClickInterstitialEvent;
		CBManager.didCacheInterstitialEvent -= didCacheInterstitialEvent;
		CBManager.didShowInterstitialEvent -= didShowInterstitialEvent;
		CBManager.didFailToLoadMoreAppsEvent -= didFailToLoadMoreAppsEvent;
		CBManager.didDismissMoreAppsEvent -= didDismissMoreAppsEvent;
		CBManager.didCloseMoreAppsEvent -= didCloseMoreAppsEvent;
		CBManager.didClickMoreAppsEvent -= didClickMoreAppsEvent;
		CBManager.didCacheMoreAppsEvent -= didCacheMoreAppsEvent;
		CBManager.didShowMoreAppsEvent -= didShowMoreAppsEvent;
	}



	void didFailToLoadInterstitialEvent(string location, CBManager.CBImpressionError error) {
		Debug.Log(string.Format("didFailToLoadInterstitialEvent: {0} at location {1}", error, location));
	}
	
	void didDismissInterstitialEvent( string location ) {
		Debug.Log( "didDismissInterstitialEvent: " + location );
	}
	
	void didCloseInterstitialEvent( string location ) {
		Debug.Log( "didCloseInterstitialEvent: " + location );
	}
	
	void didClickInterstitialEvent( string location ) {
		Debug.Log( "didClickInterstitialEvent: " + location );
	}
	
	void didCacheInterstitialEvent( string location ) {
		Debug.Log( "didCacheInterstitialEvent: " + location );
	}
	
	void didShowInterstitialEvent( string location ) {
		Debug.Log( "didShowInterstitialEvent: " + location );
	}
	
	void didFailToLoadMoreAppsEvent(CBManager.CBImpressionError error) {
		Debug.Log(string.Format("didFailToLoadMoreAppsEvent: {0}", error));
	}
	
	void didDismissMoreAppsEvent() {
		Debug.Log( "didDismissMoreAppsEvent" );
	}
	
	void didCloseMoreAppsEvent() {
		Debug.Log( "didCloseMoreAppsEvent" );
	}
	
	void didClickMoreAppsEvent() {
		Debug.Log( "didClickMoreAppsEvent" );
	}
	
	void didCacheMoreAppsEvent() {
		Debug.Log( "didCacheMoreAppsEvent" );
	}
	
	void didShowMoreAppsEvent() {
		Debug.Log( "didShowMoreAppsEvent" );
	}
	
// UNITY_ANDROID || UNITY_IPHONE
#endif
}


