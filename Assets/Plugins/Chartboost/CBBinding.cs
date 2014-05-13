using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;


#if UNITY_ANDROID || UNITY_IPHONE
namespace Chartboost {
	public class CBBinding {
	
#if UNITY_ANDROID
		private static AndroidJavaObject _plugin;
		
		static CBBinding() {
			if (Application.platform != RuntimePlatform.Android)
				return;
	
			// find the plugin instance
			using (var pluginClass = new AndroidJavaClass("com.chartboost.sdk.unity.CBPlugin"))
				_plugin = pluginClass.CallStatic<AndroidJavaObject>("instance");
		}
#elif UNITY_IPHONE
		[DllImport("__Internal")]
		private static extern void _chartBoostInit(string appId, string appSignature);
		[DllImport("__Internal")]
		private static extern void _chartBoostCacheInterstitial(string location);
		[DllImport("__Internal")]
		private static extern bool _chartBoostHasCachedInterstitial(string location);
		[DllImport("__Internal")]
		private static extern void _chartBoostShowInterstitial(string location);
		[DllImport("__Internal")]
		private static extern void _chartBoostCacheMoreApps();
		[DllImport("__Internal")]
		private static extern bool _chartBoostHasCachedMoreApps();
		[DllImport("__Internal")]
		private static extern void _chartBoostShowMoreApps();
		[DllImport("__Internal")]
		private static extern void _chartBoostForceOrientation(string orient);
#endif

		private static bool initialized = false;
		
		private static bool checkInitialized() {
#if UNITY_ANDROID
			if (Application.platform != RuntimePlatform.Android)
				return false;
#elif UNITY_IPHONE
			if (Application.platform != RuntimePlatform.IPhonePlayer)
				return false;
#endif
			if (initialized) {
				return true;
			} else {
				Debug.Log("Please call CBBinding.init() before using any other features of this library.");
				return false;
			}
		}
		
	
#if UNITY_ANDROID
		/// Initializes the Chartboost plugin.
		/// This must be called before using any other Chartboost features.
		public static void init(string appId, string appSignature) {
			if (Application.platform == RuntimePlatform.Android)
				_plugin.Call("init", appId, appSignature);
				
			initialized = true;
		}
		
		/// Informs the Chartboost SDK about the lifecycle of your app
		public static void pause(bool paused) {
			if (!checkInitialized())
				return;
			
			_plugin.Call("pause", paused);
		}
		
		/// Shuts down the Chartboost plugin
		public static void destroy() {
			if (!checkInitialized())
				return;
			
			_plugin.Call("destroy");
			initialized = false;
		}
#elif UNITY_IPHONE
		/// Initializes the Chartboost plugin and records the beginning of a user session
		/// This must be called before using any other Chartboost features
		public static void init(string appId, string appSignature) {
			if (Application.platform == RuntimePlatform.IPhonePlayer)
				_chartBoostInit(appId, appSignature);
			
			initialized = true;
		}
#endif


#if UNITY_ANDROID
		/// Used to notify Chartboost that the Android back button has been pressed
		/// Returns true to indicate that Chartboost has handled the event and it should not be further processed
		public static bool onBackPressed() {
			if (!checkInitialized())
				return false;
			
			return _plugin.Call<bool>("onBackPressed");
		}
#endif

		
		/// Returns true if an impression (interstitial or more apps page) is currently visible
		/// Due to Unity optimizations, touch events will pass through Chartboost impressions.
		/// You will have to use this method to check if a Chartboost impression is open in any code that responds to touch events
		public static bool isImpressionVisible() {
			if (!checkInitialized())
				return false;
			
			return CBManager.isImpressionVisible();
		}
	
		
		/// Caches an interstitial. Location is optional. Pass in null if you do not want to specify the location.
		public static void cacheInterstitial(string location) {
			if (!checkInitialized())
				return;
			
#if UNITY_ANDROID
			if (location == null)
				location = string.Empty;
			_plugin.Call("cacheInterstitial", location);
#elif UNITY_IPHONE
			_chartBoostCacheInterstitial(location);
#endif
		}
		
		
		/// Checks for a cached an interstitial. Location is optional. Pass in null if you do not want to specify the location.
		public static bool hasCachedInterstitial(string location) {
			if (!checkInitialized())
				return false;
			
			if (location == null)
				location = string.Empty;
	
#if UNITY_ANDROID
			if (location == null)
				location = string.Empty;
			return _plugin.Call<bool>("hasCachedInterstitial", location);
#elif UNITY_IPHONE
			return _chartBoostHasCachedInterstitial(location);
#endif
		}
		
		
		/// Loads an interstitial. Location is optional. Pass in null if you do not want to specify the location.
		public static void showInterstitial(string location) {
			if (!checkInitialized())
				return;
			
#if UNITY_ANDROID
			if (location == null)
				location = string.Empty;
			_plugin.Call("showInterstitial", location);
#elif UNITY_IPHONE
			_chartBoostShowInterstitial(location);
#endif
		}
	
		
		/// Caches the more apps screen
		public static void cacheMoreApps() {
			if (!checkInitialized())
				return;
			
#if UNITY_ANDROID
			_plugin.Call("cacheMoreApps");
#elif UNITY_IPHONE
			_chartBoostCacheMoreApps();
#endif
		}
		
		
		/// Checks to see if the more apps screen is cached
		public static bool hasCachedMoreApps() {
			if (!checkInitialized())
				return false;
			
#if UNITY_ANDROID
			return _plugin.Call<bool>("hasCachedMoreApps");
#elif UNITY_IPHONE
			return _chartBoostHasCachedMoreApps();
#endif
		}
		
		
		/// Shows the more apps screen
		public static void showMoreApps() {
			if (!checkInitialized())
				return;
			
#if UNITY_ANDROID
			_plugin.Call("showMoreApps");
#elif UNITY_IPHONE
			_chartBoostShowMoreApps();
#endif
		}
		
		
		/// Forces the orientation of impressions to the given orientation.
#if UNITY_IPHONE
		/// If your project is properly setup to autorotate, animated native views
		/// will work as expected and you should not need to set this.
#elif UNITY_ANDROID
		/// To use this method, you must set the targetSdkVersion in your
		/// AndroidManifest.xml to 13 or less.
#endif
		public static void forceOrientation(ScreenOrientation orient) {
			if (!checkInitialized())
				return;
				
#if UNITY_ANDROID
			_plugin.Call("forceOrientation", orient.ToString());
#elif UNITY_IPHONE
			_chartBoostForceOrientation(orient.ToString());
#endif
		}
		
	}
}
// UNITY_ANDROID || UNITY_IPHONE
#endif
