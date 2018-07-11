using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SteamExample : MonoBehaviour {

    // Callbacks are used to retrieve data asynchronously from Steam. You need to declare them at class scope like this:
    // This callback type checks when the steam overlay is active.
    private Callback<GameOverlayActivated_t> m_GameOverlayActivated;

    // A CallResult is a lot like a callback except it is triggered by a specific function rather than as a global event.
    // We implement them in more or less the same way.
    private CallResult<NumberOfCurrentPlayers_t> m_NumberOfCurrentPlayers;

    private void OnEnable() {
        if (SteamManager.Initialized) {
            // Here we create the callback and assign it to the variable we declared above. The argument is a delegate for a function that we will write below.
            m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);

            // CallResults are implemented similarly.
            m_NumberOfCurrentPlayers = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);
        }
    }


    void Start() {
        // Always check Initialized or else bad things
        if (SteamManager.Initialized) {
            string name = SteamFriends.GetPersonaName();
            Debug.Log("steam name: " + name);
        }
    }


    private void Update() {
        if (SteamManager.Initialized) {
            if (Input.GetKeyDown(KeyCode.H)) {
                // To trigger the CallResult, we need call the function on steam, and then store the results in a handle...
                SteamAPICall_t handle = SteamUserStats.GetNumberOfCurrentPlayers();
                // ...Then associate that handle with our CallResult (which will trigger the delegate function we wrote below).
                m_NumberOfCurrentPlayers.Set(handle);
                //Debug.Log("Called GetNumberOfCurrentPlayers()");
            }
        }
    }


    // To be used as a delegate in Callback.Create() we need to have one argument of the appropriate type.
    // I don't know what the 'pXXX' convention is all about.
    // This could be useful to pause the game when the overlay is active ;-)
    void OnGameOverlayActivated(GameOverlayActivated_t pCallback) {
        if (pCallback.m_bActive != 0) {
            Debug.Log("The steam overlay is active.");
        }
        else {
            Debug.Log("The steam overlay has been closed.");
        }
    }


    // The delegate format for CallResults are similar to Callbacks but with the addition of a second bool argument.
    void OnNumberOfCurrentPlayers(NumberOfCurrentPlayers_t pCallback, bool bIOFailure) {
        if (pCallback.m_bSuccess != 1 || bIOFailure) {
            Debug.Log("There was an error retrieving hte number of current players.");
        }
        else {
            Debug.Log("There are " + pCallback.m_cPlayers + " players currently playing this game.");
        }
    }
}
