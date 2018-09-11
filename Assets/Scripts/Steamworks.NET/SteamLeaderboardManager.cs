using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SteamLeaderboardManager : MonoBehaviour {

    [SerializeField] string leaderboardName;

    private SteamLeaderboard_t m_SteamLeaderboard;
    private SteamLeaderboardEntries_t m_SteamLeaderboardEntries;

    private CallResult<LeaderboardFindResult_t> OnLeaderboardFindResultCallResult;
    private CallResult<LeaderboardScoresDownloaded_t> OnLeaderboardScoresDownloadedCallResult;
    private CallResult<LeaderboardScoreUploaded_t> OnLeaderboardScoreUploadedCallResult;

    [HideInInspector] public bool isLeaderboardInitialized;
    [HideInInspector] public bool isScoreEntriesDownloaded;
    [HideInInspector] public bool isScoreUploaded;


    private void OnEnable() {
        OnLeaderboardFindResultCallResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFindResult);
        OnLeaderboardScoresDownloadedCallResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);
        OnLeaderboardScoreUploadedCallResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);
    }


    private void Start() {
        // Find the leaderboard.
        SteamAPICall_t handle = SteamUserStats.FindLeaderboard(leaderboardName);
        OnLeaderboardFindResultCallResult.Set(handle, OnLeaderboardFindResult);
    }


    public void UploadScore(int score) {
        if (!isLeaderboardInitialized) {
            Debug.LogWarning("Could not upload score to the Steam leaderboard because it was not initialized.");
        } else {
            isScoreUploaded = false;
            Debug.Log("Uploading score (" + score + ") to leaderboard.");
            SteamAPICall_t handle = SteamUserStats.UploadLeaderboardScore(m_SteamLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, score, null, 0);
            OnLeaderboardScoreUploadedCallResult.Set(handle, OnLeaderboardScoreUploaded);
        }
    }


    public void DownloadLeaderboardEntries(int rangeStart, int rangeEnd) {
        SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries(m_SteamLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, rangeStart, rangeEnd);
        OnLeaderboardScoresDownloadedCallResult.Set(handle, OnLeaderboardScoresDownloaded);
        isScoreEntriesDownloaded = false;
    }


    public LeaderboardEntry_t GetDownloadedLeaderboardEntry(int index) {
        LeaderboardEntry_t LeaderboardEntry;
        bool ret = SteamUserStats.GetDownloadedLeaderboardEntry(m_SteamLeaderboardEntries, index, out LeaderboardEntry, null, 0);
        return LeaderboardEntry;
    }


    /* CALLRESULT DELEGATES */

    private void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool bIOFailure) {
        if (pCallback.m_bLeaderboardFound != 0 && !bIOFailure) {
            // Save a reference to the leaderboard.
            m_SteamLeaderboard = pCallback.m_hSteamLeaderboard;
            Debug.Log("Found leaderboard: " + leaderboardName);
            isLeaderboardInitialized = true;
        }
    }


    private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure) {
        if (bIOFailure) {
            Debug.LogWarning("Could not download leaderboard for some reason.");
        } 
        
        else {
            m_SteamLeaderboardEntries = pCallback.m_hSteamLeaderboardEntries;
            isScoreEntriesDownloaded = true;
        }
    }


    private void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure) {
        if (pCallback.m_bSuccess == 0 || bIOFailure) {
            Debug.LogWarning("Was unable to upload score to Steam leaderboard.");
        } else {
            Debug.Log("Uploaded score successfully. Score: " + pCallback.m_nScore + ". New player rank: " + pCallback.m_nGlobalRankNew + ". Score changed: " + pCallback.m_bScoreChanged);
            isScoreUploaded = true;
        }
    }
}
