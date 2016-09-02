using UnityEngine;
using System.Collections;
using UnityStandardAssets.Network;
using UnityEngine.Networking;

public class NetworkLobbyHook : LobbyHook {

	//TODO: Change the Color to Type 
	//TODO: Rename file to something like LobbyToGameInformationTransfer?
	public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager aManager, GameObject aLobbyPlayer, GameObject aGamePlayer) {
		LobbyPlayer lobbyPlayer = aLobbyPlayer.GetComponent<LobbyPlayer> ();
		LocalPlayerSetup localPlayer = aGamePlayer.GetComponent<LocalPlayerSetup> ();

		localPlayer.mName = lobbyPlayer.playerName;
		localPlayer.mPlayerColour = lobbyPlayer.playerColor;
	}
}
