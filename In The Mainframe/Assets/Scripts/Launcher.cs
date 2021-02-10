using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;
using System.IO;
using System;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

public class Launcher : MonoBehaviourPunCallbacks
{
    #region Variables
    public static Launcher m_singleton;

    [SerializeField]
    TMP_InputField nameInputField;
    [SerializeField]
    Button continueButton;
    [SerializeField]
    TMP_InputField roomNameInputField;
    [SerializeField]
    TMP_Text errorText;
    [SerializeField]
    TMP_Text roomNameText;
    [SerializeField]
    Transform roomListContent;
    [SerializeField]
    Transform playerListContent;
    [SerializeField]
    GameObject roomListItemPrefab;
    [SerializeField]
    Button firewallToggleButton;
    [SerializeField]
    GameObject startGameButton;
    [SerializeField]
    GameObject readyToggleButton;

    private const int smallLobbyAmount = 5;

    private List<RoomInfo> fullRoomList;
    private List<PlayerListItem> possibleFirewalls;
    private List<PlayerListItem> chosenFirewalls;

    private PlayerListItem[] allPlayers;

    private const string PlayerPrefsNameKey = "PlayerName";
    #endregion

    #region Override
    public override void OnConnectedToMaster() {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby() {
        MenuManager.m_singleton.OpenMenu("title");
    }

    public override void OnJoinedRoom() {
        MenuManager.m_singleton.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        PlayerListItem playerItem = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerListItem"), Vector3.zero, Quaternion.identity).GetComponent<PlayerListItem>();
        firewallToggleButton.GetComponent<Button>().onClick.AddListener(() => playerItem.ToggleFirewall());
        readyToggleButton.GetComponent<Button>().onClick.AddListener(() => playerItem.ToggleReady());

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
        readyToggleButton.SetActive(!PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient) {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
        readyToggleButton.SetActive(!PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message) {
        errorText.text = $"Room Creation Failed: {message}";
        MenuManager.m_singleton.OpenMenu("error");
    }

    public override void OnLeftRoom() {
        MenuManager.m_singleton.OpenMenu("title");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        foreach (Transform trans in roomListContent) {
            Destroy(trans.gameObject);

        }

        for (int i = 0; i <= roomList.Count - 1; i++) {
            if (roomList[i].RemovedFromList) {
                for (int a = 0; a < fullRoomList.Count; a++) {
                    if (fullRoomList[a].Name.Equals(roomList[i].Name)) fullRoomList.RemoveAt(a);
                }
            }

            if (!fullRoomList.Contains(roomList[i])) fullRoomList.Add(roomList[i]);

            for (int b = 0; b < fullRoomList.Count; b++) {
                if (fullRoomList[b].Name.Equals(roomList[i].Name)) fullRoomList[b] = roomList[i];
            }
        }

        if (!(fullRoomList.Count == 0))
        {
            for (int i = 0; i < fullRoomList.Count; i++) {
                if (fullRoomList[i].RemovedFromList == false)
                {
                    Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(fullRoomList[i]);
                }

            }
        }

    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) {
        if ((bool)changedProps[PlayerListItem.FirewallProperty]) {
            ChangeToGameScene();
        }
    }
    #endregion

    #region Initialization
    private void Awake() {
        m_singleton = this;
    }

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SetUpInputField();
        fullRoomList = new List<RoomInfo>();
        chosenFirewalls = new List<PlayerListItem>();
    }

    void Update()
    {
        
    }
    #endregion

    #region Getter
    public Button GetFirewallToggleButton {
        get { return firewallToggleButton; }
    }

    public Button GetReadyToggleButton {
        get { return readyToggleButton.GetComponent<Button>(); }
    }

    public Transform GetPlayerListContent {
        get { return playerListContent; }
    }
    #endregion

    #region Name
    private void SetUpInputField() {
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey)) {
            return;
        }
        string defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);

        nameInputField.text = defaultName;

        SetPlayerName(defaultName);
    }

    public void SetPlayerName(string name) {
        if (name.Length > 15) {
            nameInputField.text = name.Substring(0, 15);
        }

        continueButton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName() {
        string playerName = nameInputField.text;

        PhotonNetwork.NickName = playerName;

        PlayerPrefs.SetString(PlayerPrefsNameKey, playerName);

        MenuManager.m_singleton.OpenMenu("loading");

        PhotonNetwork.ConnectUsingSettings();
    }
    #endregion

    #region Room
    public void SetRoomName(string name) {
        if (name.Length > 20) {
            roomNameInputField.text = name.Substring(0, 20);
        }
    }

    public void CreateRoom() {
        if (string.IsNullOrEmpty(roomNameInputField.text)) {
            return;
        }
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.m_singleton.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo info) {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.m_singleton.OpenMenu("loading");
    }

    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
        MenuManager.m_singleton.OpenMenu("loading");
    }

    public void StartGame() {
        possibleFirewalls = new List<PlayerListItem>();
        allPlayers = playerListContent.GetComponentsInChildren<PlayerListItem>();
        foreach (PlayerListItem player in allPlayers) {
            if (!player.IsReady) {
                return;
            }
            if (player.IsFirewall) {
                possibleFirewalls.Add(player);
            }
        }

        PickFirewalls();
    }

    void PickFirewalls() {
        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable
        {
            { PlayerListItem.FirewallProperty, true },
        };

        if (allPlayers.Length <= smallLobbyAmount) {
            if (possibleFirewalls.Count == 0) {
                chosenFirewalls.Add(Picker(allPlayers));
            }
            else {
                chosenFirewalls.Add(Picker(possibleFirewalls));
            }
            chosenFirewalls[0].photonView.Owner.SetCustomProperties(playerProps);
        }
        else {
            if (possibleFirewalls.Count == 0) {
                chosenFirewalls.Add(Picker(allPlayers));
                chosenFirewalls.Add(Picker(allPlayers));
            }
            else if (possibleFirewalls.Count == 1) {
                chosenFirewalls.Add(Picker(possibleFirewalls));
                chosenFirewalls.Add(Picker(allPlayers));
            }
            else {
                chosenFirewalls.Add(Picker(possibleFirewalls));
                chosenFirewalls.Add(Picker(possibleFirewalls));
            }
            chosenFirewalls[0].photonView.Owner.SetCustomProperties(playerProps);
            chosenFirewalls[1].photonView.Owner.SetCustomProperties(playerProps);
        }
    }

    void ChangeToGameScene() {
        SceneManager.LoadScene(1);
    }
    #endregion

    public void QuitGame() {
        Application.Quit();
    }

    #region Helper
    private PlayerListItem Picker(PlayerListItem[] array) {
        int randomPicker = Random.Range(0, array.Length);
        while (chosenFirewalls.Contains(array[randomPicker])) {
            randomPicker = Random.Range(0, array.Length);
        }
        return array[randomPicker];
    }

    private PlayerListItem Picker(List<PlayerListItem> list) {
        int randomPicker = Random.Range(0, list.Count);
        while (chosenFirewalls.Contains(list[randomPicker])) {
            randomPicker = Random.Range(0, list.Count);
        }
        return list[randomPicker];
    }
    #endregion
}
