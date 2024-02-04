using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class AdDisplay : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsShowListener, IUnityAdsLoadListener
{
    // Make sure to set your Game ID in the Unity Ads settings
    private string myGameIdAndroid = "5497360";
    public string adUnitIdAndroid = "Rewarded_Android";
    public static bool adStarted = false;
    private bool testMode = true; // TODO: Set to false for production

    private float waitTime = 10f;

    private Queue<Action> rewardQueue = new Queue<Action>();

    private void Start()
    {
        adStarted = false;
        Advertisement.Initialize(myGameIdAndroid, testMode, this);
    }

    /// <summary>
    /// Call this method when clicking the show Ad button
    /// </summary>
    public void TryShowAd(Action rewardMethod)
    {
        rewardQueue.Enqueue(rewardMethod);
        StartCoroutine(LoadAd());
    }

    private IEnumerator LoadAd()
    {
        float maxWaitTime = Time.time + waitTime;
        yield return new WaitUntil(() => Advertisement.isInitialized || maxWaitTime < Time.time);

        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning($"The Ad couldn't be loaded as it wasn't initialized after {waitTime} seconds");
        }
        else
        {
            ShowRewardedAd();
        }
    }

    private void ShowRewardedAd()
    {
        if (Advertisement.isInitialized && !adStarted)
        {
            ShowOptions options = new ShowOptions();
            Advertisement.Load(adUnitIdAndroid, this);
            Advertisement.Show(adUnitIdAndroid, this);
            adStarted = true;
        }
        else
        {
            Debug.Log("Rewarded ad not ready. Check if it's initialized and ready to show.");
        }
    }

    // If the ad successfully loads, add a listener to the button and enable it:
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log("Ad Loaded: " + adUnitId);
    }

    // Implement the Show Listener's OnUnityAdsShowComplete callback method to determine if the user gets a reward:
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(adUnitIdAndroid) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");
            
            if (rewardQueue.Count > 0)
            {
                // Execute the first method in the queue
                Action rewardMethod = rewardQueue.Dequeue();
                rewardMethod.Invoke();
            }
        }

        // reset teh bool
        adStarted = false;

        // If the queue is not empty, start over
        if (rewardQueue.Count > 0)
        {
            StartCoroutine(LoadAd());
        }
    }

    // Implement Load and Show Listener error callbacks:
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Use the error details to determine whether to try to load another ad.

        // reset teh bool
        adStarted = false;
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Use the error details to determine whether to try to load another ad.

        // reset teh bool
        adStarted = false;
    }

    void OnDestroy()
    {
        
    }

    #region Interface Implementations
    public void OnInitializationComplete()
    {
        Debug.Log("Init Success");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Init Failed: [{error}]: {message}");
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log($"OnUnityAdsShowStart: {placementId}");
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log($"OnUnityAdsShowClick: {placementId}");
    }
    #endregion
}
