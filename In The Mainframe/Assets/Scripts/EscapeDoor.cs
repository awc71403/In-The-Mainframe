using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class EscapeDoor : MonoBehaviour, IPunObservable {

    #region Variables
    [SerializeField]
    private GameObject m_doorPiece;

    [SerializeField]
    private float m_uploadMax;
    [SerializeField]
    private float m_localOverrideProgress;
    [SerializeField]
    private float m_globalOverrideProgress;

    private PhotonView m_PV;

    private bool m_playerOn;

    private bool m_doorHacked;
    private bool m_mainframeHacked;

    private Collider m_overrideArea;

    public TextMeshProUGUI m_text;
    #endregion

    #region Initialization
    void Awake() {
        m_mainframeHacked = false;
        m_playerOn = false;
        m_overrideArea = GetComponentInChildren<Collider>();
        m_PV = GetComponent<PhotonView>();
        m_localOverrideProgress = 0;
        m_globalOverrideProgress = 0;
    }

    void Start() {
        UpdateUI();
    }
    #endregion

    #region Update
    void Update() {
        if (m_doorHacked) {
            return;
        } else if (m_localOverrideProgress >= m_uploadMax) {
            OpenDoor();
            return;
        } else if (m_PV.IsMine) {
            if (m_mainframeHacked && m_playerOn) {
                m_localOverrideProgress += Time.deltaTime;
                UpdateUI();
            }
        }
    }
    #endregion

    #region Setter
    public bool SetMainframeHacked {
        set { m_mainframeHacked = value; }
    }
    #endregion

    #region OnTrigger
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag(GameManager.VirusTag)) {
            VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
            if (hitVirus) {
                hitVirus.PriorityAction(VirusController.ActionStates.OVERRIDE);
                m_localOverrideProgress = m_globalOverrideProgress;
                hitVirus.GetPlayerManager.EnableAction(PlayerManager.OverrideAction, m_localOverrideProgress, m_uploadMax);
                PlayerController.StartActionEvent += PlayerEnterDoor;
                PlayerController.EndActionEvent += PlayerExitDoor;
            }
        }
    }

    void OnTriggerStay(Collider other) {
        if (other.gameObject.CompareTag(GameManager.VirusTag)) {
            VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
            if (hitVirus) {
                hitVirus.PriorityAction(VirusController.ActionStates.OVERRIDE);
                if (m_localOverrideProgress >= m_uploadMax) {
                    Debug.Log($"Finished in OnTriggerStay.");
                    hitVirus.GetPlayerManager.DisableAction();
                    PlayerController.StartActionEvent -= PlayerEnterDoor;
                    PlayerController.EndActionEvent -= PlayerExitDoor;
                    m_overrideArea.enabled = false;
                    return;
                }
                hitVirus.GetPlayerManager.EnableAction(PlayerManager.OverrideAction, m_localOverrideProgress, m_uploadMax);
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag(GameManager.VirusTag)) {
            VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
            if (hitVirus) {
                hitVirus.GetPlayerManager.DisableAction();
                PlayerController.StartActionEvent -= PlayerEnterDoor;
                PlayerController.EndActionEvent -= PlayerExitDoor;
            }
        }
    }
    #endregion

    void PlayerEnterDoor(PlayerController player) {
        if (((VirusController)player).GetCurrentActionState != VirusController.ActionStates.OVERRIDE) {
            return;
        }
        m_PV.RPC("RPC_PlayerEnterDoor", RpcTarget.MasterClient);
    }

    void PlayerExitDoor(PlayerController player) {
        if (((VirusController)player).GetCurrentActionState != VirusController.ActionStates.OVERRIDE) {
            return;
        }
        m_PV.RPC("RPC_PlayerExitDoor", RpcTarget.MasterClient);
    }

    #region Photon
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(m_localOverrideProgress);
            stream.SendNext(m_playerOn);
        }
        else {
            m_globalOverrideProgress = (float)stream.ReceiveNext();
            m_playerOn = (bool)stream.ReceiveNext();
        }
        UpdateUI();
    }

    [PunRPC]
    void RPC_PlayerEnterDoor() {
        m_playerOn = true;
        UpdateUI();
    }

    [PunRPC]
    void RPC_PlayerExitDoor() {
        m_playerOn = false;
        UpdateUI();
    }
    #endregion

    public void UpdateUI() {
        m_text.text = $"Hacked: {m_mainframeHacked}\nPlayer: {m_playerOn}\nProgress: {(int)m_localOverrideProgress}/{m_uploadMax}";
    }

    void OpenDoor() {
        m_doorHacked = true;
        m_doorPiece.SetActive(false);
    }
}