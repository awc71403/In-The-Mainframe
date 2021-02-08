using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviourPunCallbacks, IPunObservable {
    [SerializeField]
    TMP_Text m_text;

    Image m_readyImage;
    [SerializeField]
    TextMeshProUGUI m_firewallText;
    PhotonView m_PV;
    Player m_player;
    bool m_firewallBool;
    bool m_readyBool;

    public const string FirewallProperty = "IsFirewall";

    void Awake() {
        m_firewallBool = true;
        m_readyBool = PhotonNetwork.IsMasterClient;
        m_readyImage = GetComponentInChildren<Image>();
        transform.SetParent(Launcher.m_singleton.GetPlayerListContent);
        transform.localScale = new Vector3(1, 1, 1);
        m_PV = GetComponent<PhotonView>();
        SetUp(m_PV.Owner);
    }

    void Start() {
        if (m_PV.IsMine) {
            transform.SetAsLastSibling();
        }

        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable
        {
            { FirewallProperty, false },
        };

        m_PV.Owner.SetCustomProperties(playerProps);
    }

    public void SetUp(Player player) {
        m_player = player;
        m_text.text = player.NickName;
    }

    void Update() {
        UpdateUI();
    }

    #region Getter
    public bool IsReady {
        get { return m_readyBool; }
    }

    public bool IsFirewall {
        get { return m_firewallBool; }
    }
    #endregion

    #region Override
    public override void OnPlayerLeftRoom(Player otherPlayer) {
        if (m_player == otherPlayer) {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom() {
        Destroy(gameObject);
    }
    #endregion

    public void ToggleFirewall() {
        m_firewallBool = !m_firewallBool;
        if (m_firewallBool) {
            Launcher.m_singleton.GetFirewallToggleButton.GetComponentInChildren<TextMeshProUGUI>().text = "Firewall: On";
        } else {
            Launcher.m_singleton.GetFirewallToggleButton.GetComponentInChildren<TextMeshProUGUI>().text = "Firewall: Off";
        }
    }

    public void ToggleReady() {
        m_readyBool = !m_readyBool;
        if (m_readyBool) {
            Launcher.m_singleton.GetReadyToggleButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unready";
        }
        else {
            Launcher.m_singleton.GetReadyToggleButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ready";
        }
    }

    #region Photon
    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(m_firewallBool);
            stream.SendNext(m_readyBool);
        }
        else {
            m_firewallBool = (bool)stream.ReceiveNext();
            m_readyBool = (bool)stream.ReceiveNext();
        }
    }
    #endregion

    void UpdateUI() {
        if (m_firewallBool) {
            m_firewallText.text = "F";
        }
        else {
            m_firewallText.text = "V";
        }
        if (m_readyBool) {
            m_readyImage.color = new Color(0, 1, 0);
        }
        else {
            m_readyImage.color = new Color(1, 0, 0);
        }
    }
}
