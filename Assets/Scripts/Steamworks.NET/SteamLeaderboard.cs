using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SteamLeaderboard : MonoBehaviour {

    private SteamLeaderboard_t m_SteamLeaderboard;
    private SteamLeaderboardEntries_t m_SteamLeaderboardEntries;

    private CallResult<LeaderboardFindResult_t> OnLeaderboardFindResultCallResult;
    private CallResult<LeaderboardScoresDownloaded_t> OnLeaderboardScoresDownloadCallResult;
    private CallResult<LeaderboardScoreUploaded_t> OnLeaderboardScoreUploadedCallResult;
    private CallResult<LeaderboardUGCSet_t> OnLeaderboardUGCSetCallResult;


    private void OnEnable() {
        OnLeaderboardFindResultCallResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFindResult);
        OnLeaderboardScoresDownloadCallResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);
        OnLeaderboardScoreUploadedCallResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);
        OnLeaderboardUGCSetCallResult = CallResult<LeaderboardUGCSet_t>.Create(OnLeaderboardUGCSet);
    }


    private void Update() {
        // Test to create score leaderboard.
        if (Input.GetKeyDown(KeyCode.Alpha8)) {
            SteamAPICall_t handle = SteamUserStats.FindOrCreateLeaderboard("High Score", ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric);
            OnLeaderboardFindResultCallResult.Set(handle);
            print("SteamUserStats.FindOrCreateLeaderboard(" + "\"High Score\"" + ", " + ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending + ", " + ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric + ") : " + handle);
        }

        // Spams SteamAPI Warnings that the SteamLeaderboard does not exist.
        // This is how to check if the leaderboard exists.
        if (m_SteamLeaderboard != new SteamLeaderboard_t(0)) {
            Debug.Log("GetLeaderboardName(m_SteamLeaderboard) : " + SteamUserStats.GetLeaderboardName(m_SteamLeaderboard));
            Debug.Log("GetLeaderboardEntryCount(m_SteamLeaderboard) : " + SteamUserStats.GetLeaderboardEntryCount(m_SteamLeaderboard));
            Debug.Log("GetLeaderboardSortMethod(m_SteamLeaderboard) : " + SteamUserStats.GetLeaderboardSortMethod(m_SteamLeaderboard));
            Debug.Log("GetLeaderboardDisplayType(m_SteamLeaderboard) : " + SteamUserStats.GetLeaderboardDisplayType(m_SteamLeaderboard));
        } else {
            Debug.Log("The leader board, it doesn't exist :(");
            Debug.Log("GetLeaderboardName(m_SteamLeaderboard) : ");
            Debug.Log("GetLeaderboardEntryCount(m_SteamLeaderboard) : ");
            Debug.Log("GetLeaderboardSortMethod(m_SteamLeaderboard) : ");
            Debug.Log("GetLeaderboardDisplayType(m_SteamLeaderboard) : ");
        }
    }


    private void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool bIOFailure) {
        Debug.Log("[" + LeaderboardFindResult_t.k_iCallback + " - LeaderboardFindResult] - " + pCallback.m_hSteamLeaderboard + " -- " + pCallback.m_bLeaderboardFound);

        if (pCallback.m_bLeaderboardFound != 0 && !bIOFailure) {
            Debug.Log("I think I'm creating the leaderboard now");
            m_SteamLeaderboard = pCallback.m_hSteamLeaderboard;
        }
    }


    private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure) {

    }


    private void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure) {

    }


    private void OnLeaderboardUGCSet(LeaderboardUGCSet_t pCallback, bool bIOFailure) {

    }
}
