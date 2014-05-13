using UnityEngine;
using System;
using System.Collections;


namespace Chartboost {
	public class CBManager : MonoBehaviour {

#if UNITY_ANDROID || UNITY_IPHONE
		
#if UNITY_IPHONE
		[System.Runtime.InteropServices.DllImport("__Internal")]
		private static extern void _chartBoostPauseUnity();
#endif

		/// Status enum for impression loading errors
		public enum CBImpressionError : int {
			/// An error internal to the Chartboost SDK
			Internal = 0, // 4, 7 also, 8 on ios
			/// No internet connection was found
			InternetUnavailable = 1,
			/// Too many simultaneous requests of the same type
			TooManyConnections = 2,
			/// The impression sent was not compatible with the device orientation
			WrongOrientation = 3,
			/// An error occurred during network communication with the Chartboost server
			NetworkFailure = 5,
			/// No ad was available for the user from the Chartboost server
			NoAdFound = 6,
			/// There is already an impression visible
			ImpressionAlreadyVisible = 8,
			/// Android Only: There is no currently active activity with Chartboost properly integrated
			NoHostActivity = 9,
		};
		
	
		/// Fired when an interstitial fails to load
		/// First parameter is the location.
		public static event Action<string,CBImpressionError> didFailToLoadInterstitialEvent;
	
		/// Fired when an interstitial is finished via any method
		/// This will always be paired with either a close or click event
		/// First parameter is the location.
		public static event Action<string> didDismissInterstitialEvent;
		
		/// Fired when an interstitial is closed (i.e. by tapping the X or hitting the Android back button)
		/// First parameter is the location.
		public static event Action<string> didCloseInterstitialEvent;
		
		/// Fired when an interstitial is clicked
		/// First parameter is the location.
		public static event Action<string> didClickInterstitialEvent;
	
		/// Fired when an interstitial is cached
		/// First parameter is the location.
		public static event Action<string> didCacheInterstitialEvent;
	
		/// Fired when an interstitial is shown
		/// First parameter is the location.
		public static event Action<string> didShowInterstitialEvent;
		
		/// Fired when the more apps screen fails to load
		public static event Action<CBImpressionError> didFailToLoadMoreAppsEvent;
	
		/// Fired when the more apps screen is finished via any method
		/// This will always be paired with either a close or click event
		public static event Action didDismissMoreAppsEvent;
		
		/// Fired when the more apps screen is closed (i.e. by tapping the X or hitting the Android back button)
		public static event Action didCloseMoreAppsEvent;
		
		/// Fired when a listing on the more apps screen is clicked
		public static event Action didClickMoreAppsEvent;
	
		/// Fired when the more apps screen is cached
		public static event Action didCacheMoreAppsEvent;
	
		/// Fired when the more app screen is shown
		public static event Action didShowMoreAppsEvent;
		
		
		void Awake() {
			gameObject.name = "ChartBoostManager";
			DontDestroyOnLoad(gameObject);
		}
	
	
		public void didFailToLoadInterstitial(string dataString) {
			Hashtable data = (Hashtable)CBJSON.Deserialize(dataString);
			CBImpressionError error = impressionErrorFromInt(data["errorCode"]);

			if (didFailToLoadInterstitialEvent != null)
				didFailToLoadInterstitialEvent(data["location"] as string, error);
		}
	
	
		public void didDismissInterstitial(string location) {
			doUnityPause(false);
			
			if (didDismissInterstitialEvent != null)
				didDismissInterstitialEvent(location);
		}
		
		
		public void didClickInterstitial(string location) {
			if (didClickInterstitialEvent != null)
				didClickInterstitialEvent(location);
		}

		
		public void didCloseInterstitial(string location) {
			if (didCloseInterstitialEvent != null)
				didCloseInterstitialEvent(location);
		}
	
	
		public void didCacheInterstitial(string location) {
			if (didCacheInterstitialEvent != null)
				didCacheInterstitialEvent(location);
		}
	
	
		public void didShowInterstitial(string location) {
			doUnityPause(true);
#if UNITY_IPHONE
			_chartBoostPauseUnity();
#endif
			
			if (didShowInterstitialEvent != null)
				didShowInterstitialEvent(location);
		}
	
	
		public void didFailToLoadMoreApps(string dataString) {
			Hashtable data = (Hashtable)CBJSON.Deserialize(dataString);
			CBImpressionError error = impressionErrorFromInt(data["errorCode"]);
			
			if (didFailToLoadMoreAppsEvent != null)
				didFailToLoadMoreAppsEvent(error);
		}
		
	
		public void didDismissMoreApps(string empty) {
			doUnityPause(false);
			
			if (didDismissMoreAppsEvent != null)
				didDismissMoreAppsEvent();
		}
		
		
		public void didClickMoreApps(string empty) {
			if (didClickMoreAppsEvent != null)
				didClickMoreAppsEvent();
		}

		
		public void didCloseMoreApps(string empty) {
			if (didCloseMoreAppsEvent != null)
				didCloseMoreAppsEvent();
		}
	
	
		public void didCacheMoreApps(string empty) {
			if (didCacheMoreAppsEvent != null)
				didCacheMoreAppsEvent();
		}
	
	
		public void didShowMoreApps(string empty) {
			doUnityPause(true);
#if UNITY_IPHONE
			_chartBoostPauseUnity();
#endif
			
			if (didShowMoreAppsEvent != null)
				didShowMoreAppsEvent();
		}
		
		// Utility methods
		
		/// var used internally for managing game pause state
		private static bool isPaused = false;
		
		/// Manages pausing
		private static void doUnityPause(bool pause) {
#if UNITY_ANDROID
			bool useCustomPause = true;
#endif
			if (pause) {
#if UNITY_ANDROID
				if (isPaused) {
					useCustomPause = false;
				}
#endif
				isPaused = true;
#if UNITY_ANDROID
				if (useCustomPause)
					doCustomPause(pause);
#endif
			} else {
#if UNITY_ANDROID
				if (!isPaused) {
					useCustomPause = false;
				}
#endif
				isPaused = false;
#if UNITY_ANDROID
				if (useCustomPause)
					doCustomPause(pause);
#endif
			}
		}
		
		public static bool isImpressionVisible() {
			return isPaused;
		}
		
#if UNITY_ANDROID
		/// Var used for custom pause method
		private static float lastTimeScale = 0;
		
		/// Update this method if you would like to change how your game is paused
		///   when impressions are shown.
		private static void doCustomPause(bool pause) {
			if (pause) {
				lastTimeScale = Time.timeScale;
				Time.timeScale = 0;
			} else {
				Time.timeScale = lastTimeScale;
			}
		}
#endif

		
		private static CBImpressionError impressionErrorFromInt(object errorObj) {
			bool ios = Application.platform == RuntimePlatform.IPhonePlayer;
			int error;
			try {
				error = Convert.ToInt32(errorObj);
			} catch {
				error = -1;
			}
			
			if (error < 0 || error > 9 || (ios && error > 8) // out of bounds
					|| error == 4 || error == 7 || (ios && error == 8)) // unused enums: interstitialsDisabledOnFirstSession, sessionNotStarted, ageGateFailure
				return CBImpressionError.Internal;
			else
				return (CBImpressionError)error;
		}

// UNITY_ANDROID || UNITY_IPHONE
#endif

	}
}

