using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    RoomManager m_roomManager;
    PhotonView m_PV;
    PlayerController m_playerController;
    Slider m_actionSlider;

    Image m_actionBackground, m_actionFill;

    public const string HackAction = "[L-Click] Hack";
    public const string InjectAction = "[L-Click] Inject";
    public const string RebootingAction = "[L-Click] Rebooting";
    public const string InjectedByAction = "Inject from";
    public const string InjectedByActionSingle = "other virus";
    public const string InjectedByActionMultiple = "other viruses";
    public const string RebootingCapped = "Find another virus";
    public const string OverrideAction = "[L-Click] Override";

    public const string ProtectAction = "[Space] Protect";
    void Awake()
    {
        m_roomManager = RoomManager.m_singleton;
        m_PV = GetComponent<PhotonView>();
    }

    void Start() {
        if (m_PV.IsMine) {
            CreateController();
        }
        //(bool)m_PV.Owner.CustomProperties[PlayerListItem.FirewallProperty]
        if ((bool)m_PV.Owner.CustomProperties[PlayerListItem.FirewallProperty]) {
            gameObject.transform.SetParent(PlayerList.m_singleton.GetFirewallsTransform);
        }
        else {
            gameObject.transform.SetParent(PlayerList.m_singleton.GetVirusesTransform);
        }

        m_actionSlider = GameManager.m_singleton.GetActionSlider;
        m_actionBackground = m_actionSlider.GetComponentsInChildren<Image>()[0];
        m_actionFill = m_actionSlider.GetComponentsInChildren<Image>()[1];
    }

    #region Getter
    public PhotonView GetPhotonView {
        get { return m_PV; }
    }

    public PlayerController GetPlayerController {
        get { return m_playerController; }
    }
    #endregion

    void CreateController() {
        //(bool)m_PV.Owner.CustomProperties[PlayerListItem.FirewallProperty]
        if ((bool)m_PV.Owner.CustomProperties[PlayerListItem.FirewallProperty]) {
            Transform spawnPoint = SpawnManager.m_singleton.GetFirewallSpawnPoint();
            m_playerController = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "FirewallController"), spawnPoint.position, spawnPoint.rotation, 0, new object[] { m_PV.ViewID }).GetComponent<PlayerController>();
            GameManager.m_singleton.Firewalls.Add(m_playerController.GetComponent<FirewallController>());
            GameManager.m_singleton.FirewallShader();
        }
        else {
            Transform spawnPoint = SpawnManager.m_singleton.GetVirusSpawnPoint();
            m_playerController = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "VirusController"), spawnPoint.position, Quaternion.identity, 0, new object[] { m_PV.ViewID }).GetComponent<PlayerController>();
            m_playerController.GetComponentInChildren<Rigidbody>().gameObject.transform.rotation = spawnPoint.rotation;
            m_playerController.GetCameraHolder.transform.rotation = spawnPoint.rotation;
            GameManager.m_singleton.VirusShader();
        }
    }

    #region UI
    public void EnableAction(string action, float progress, float max) {
        ActivateAction();
        m_actionSlider.value = progress / max;
        m_actionSlider.GetComponentInChildren<TextMeshProUGUI>().text = action;

        m_playerController.EnableAction();
    }

    public void DisableAction() {
        Debug.Log("PlayerManager DisableAction called");
        DeactivateAction();

        m_playerController.DisableAction();
    }

    public void UpdateAction(string action, float progress, float max) {
        m_actionSlider.gameObject.SetActive(true);
        m_actionSlider.value = progress / max;
        m_actionSlider.GetComponentInChildren<TextMeshProUGUI>().text = action;
    }

    public void ActivateAction() {
        m_actionSlider.gameObject.SetActive(true);
    }

    public void DeactivateAction() {
        m_actionSlider.gameObject.SetActive(false);
    }

    public void ActiveAction() {
        m_actionBackground.color = new Color(1, 1, 1);
        m_actionFill.color = new Color(1, 1, 1);
    }

    public void InactiveAction() {
        m_actionBackground.color = new Color(.5f, .5f, .5f);
        m_actionFill.color = new Color(.5f, .5f, .5f);
    }
    #endregion
}
