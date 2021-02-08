using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Console : MonoBehaviour, IPunObservable
{
    #region Variables
    #region Private
    [SerializeField]
    private float m_uploadMax;
    [SerializeField]
    private float m_localUploadProgress;

    private float m_globalUploadProgress;

    [SerializeField]
    private float m_protectMax;
    private float m_protectProgress;

    [SerializeField]
    private AudioSource m_failAudio, m_hackedAudio;

    private PhotonView m_PV;
    [SerializeField]
    private int m_playersOnConsole;
    private bool m_activated;
    private bool m_protected;
    private bool m_protecting;
    #endregion

    [SerializeField]
    private MeshRenderer m_bottom, m_top;

    [SerializeField]
    private Material[] m_regularMaterial, m_damagedMaterial, m_finishedMaterial, m_protectedMaterial;

    [SerializeField]
    private Collider m_hackArea;

    public TextMeshProUGUI m_text;

    [SerializeField]
    private GameObject m_workingParticle, m_hackedParticle, m_shieldedParticle, m_hackingParticle;

    public const float Inefficiency = .15f;
    public const float ProtectedLoss = .25f;
    #endregion

    #region Events and Delegates
    public delegate void ConsoleFinished();
    public static event ConsoleFinished ConsoleFinishedEvent;
    #endregion

    #region Initialization
    void Awake() {
        m_PV = GetComponent<PhotonView>();
        m_protected = false;
        m_protecting = false;
        m_protectProgress = 0;
        m_playersOnConsole = 0;
        m_localUploadProgress = 0;
        m_globalUploadProgress = 0;
    }

    void Start() {
        UpdateUI();
    }
    #endregion

    #region Update
    void Update() {
        if (m_activated) {
            return;
        }

        if (m_protecting && !m_protected) {
            if (m_protectProgress >= m_protectMax) {
                if (PhotonNetwork.IsMasterClient) {
                    m_PV.RPC("RPC_PlayerProtectedConsole", RpcTarget.All);
                }
                ProtectedConsole();
                m_protected = true;
            }
            else {
                m_protectProgress += Time.deltaTime;
                UpdateUI();
            }
        }
        else {
            m_protectProgress = 0;
        }

        if (m_protected) {
            m_hackingParticle.SetActive(false);
            m_localUploadProgress -= Time.deltaTime * ProtectedLoss;
            m_globalUploadProgress = m_localUploadProgress;
            if (m_localUploadProgress <= 0) {
                m_localUploadProgress = 0;
                m_globalUploadProgress = m_localUploadProgress;
                m_protected = false;
                RegularConsole();
            }
            UpdateUI();
        }

        if (m_localUploadProgress >= m_uploadMax || m_globalUploadProgress >= m_uploadMax) {
            if (PhotonNetwork.IsMasterClient) {
                m_PV.RPC("RPC_ConsoleFinished", RpcTarget.All);
            }
            return;
        }
        else {
            if (m_playersOnConsole > 0) {
                m_hackingParticle.SetActive(true);
                m_localUploadProgress += Time.deltaTime * (1 - Inefficiency * (m_playersOnConsole - 1)) * m_playersOnConsole;
                m_globalUploadProgress = m_localUploadProgress;
                m_protected = false;
                RegularConsole();
                UpdateUI();
            }
        }
    }
    #endregion

    #region OnTrigger
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag(GameManager.VirusTag)) {
            VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
            if (hitVirus) {
                hitVirus.PriorityAction(VirusController.ActionStates.HACK);
                m_localUploadProgress = m_globalUploadProgress;
                hitVirus.GetPlayerManager.EnableAction(PlayerManager.HackAction, m_localUploadProgress, m_uploadMax);
                PlayerController.StartActionEvent += PlayerEnterConsole;
                SkillCheck.SkillCheckOutcomeEvent += ConsoleSkillCheck;
                PlayerController.EndActionEvent += PlayerExitConsole;
            }
        }
        else if (m_localUploadProgress > 0 && other.gameObject.CompareTag(GameManager.FirewallTag)) {
            FirewallController hitFirewall = other.gameObject.GetComponent<FirewallController>();
            if (!m_protected) {
                hitFirewall.GetPlayerManager.EnableAction(PlayerManager.ProtectAction, m_protectProgress, m_protectMax);
                PlayerController.StartActionEvent += PlayerProtectConsole;
                PlayerController.EndActionEvent += PlayerEndProtectConsole;
            }
        }
    }

    void OnTriggerStay(Collider other) {
        if (other.gameObject.CompareTag(GameManager.VirusTag)) {
            VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
            if (hitVirus) {
                hitVirus.PriorityAction(VirusController.ActionStates.HACK);
                if (m_localUploadProgress >= m_uploadMax) {
                    Debug.Log($"Finished in OnTriggerStay.");
                    //If skill check hasn't begun, this fails
                    if (GameManager.m_singleton.GetSkillCheckHolder.activeSelf) {
                        //Doesn't work cause the events are only added in the SkillCheck not the Holder
                        ConsoleFinishedEvent();
                    }
                    hitVirus.GetPlayerManager.DisableAction();
                    PlayerController.StartActionEvent -= PlayerEnterConsole;
                    SkillCheck.SkillCheckOutcomeEvent -= ConsoleSkillCheck;
                    PlayerController.EndActionEvent -= PlayerExitConsole;
                    m_hackArea.enabled = false;
                    return;
                }
                hitVirus.GetPlayerManager.EnableAction(PlayerManager.HackAction, m_localUploadProgress, m_uploadMax);
            }
        }
        else if (m_localUploadProgress > 0 && other.gameObject.CompareTag(GameManager.FirewallTag)) {
            FirewallController hitFirewall = other.gameObject.GetComponent<FirewallController>();
            if (hitFirewall) {
                if (!m_protected) {
                    hitFirewall.GetPlayerManager.EnableAction(PlayerManager.ProtectAction, m_protectProgress, m_protectMax);
                    PlayerController.StartActionEvent += PlayerProtectConsole;
                    PlayerController.EndActionEvent += PlayerEndProtectConsole;
                }
                else {
                    hitFirewall.GetPlayerManager.DisableAction();
                    PlayerController.StartActionEvent -= PlayerProtectConsole;
                    PlayerController.EndActionEvent -= PlayerEndProtectConsole;
                }
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag(GameManager.VirusTag)) {
            VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
            if (hitVirus) {
                hitVirus.GetPlayerManager.DisableAction();
                PlayerController.StartActionEvent -= PlayerEnterConsole;
                SkillCheck.SkillCheckOutcomeEvent -= ConsoleSkillCheck;
                PlayerController.EndActionEvent -= PlayerExitConsole;
            }
        }
        else if (m_localUploadProgress > 0 && !m_protected && other.gameObject.CompareTag(GameManager.FirewallTag)) {
            FirewallController hitFirewall = other.gameObject.GetComponent<FirewallController>();
            if (hitFirewall) {
                hitFirewall.GetPlayerManager.DisableAction();
                PlayerController.StartActionEvent -= PlayerProtectConsole;
                PlayerController.EndActionEvent -= PlayerEndProtectConsole;
            }
        }
    }
    #endregion

    void PlayerEnterConsole(PlayerController player) {
        if (((VirusController)player).GetCurrentActionState != VirusController.ActionStates.HACK) {
            return;
        }
        m_PV.RPC("RPC_PlayerEnterConsole", RpcTarget.MasterClient);
    }

    void PlayerExitConsole(PlayerController player) {
        if (((VirusController)player).GetCurrentActionState != VirusController.ActionStates.HACK) {
            return;
        }
        m_PV.RPC("RPC_PlayerExitConsole", RpcTarget.MasterClient);
    }

    void PlayerProtectConsole(PlayerController player) {
        m_PV.RPC("RPC_PlayerProtectConsole", RpcTarget.MasterClient);
    }

    void PlayerEndProtectConsole(PlayerController player) {
        m_PV.RPC("RPC_PlayerEndProtectConsole", RpcTarget.MasterClient);
    }

    void ConsoleSkillCheck(int outcome, VirusController.ActionStates action) {
        Debug.Log($"Console Skill Check. Outcome: {outcome}. Action: {action}");
        if (action != VirusController.ActionStates.HACK) {
            return;
        }
        switch (outcome) {
            case 0:
                m_PV.RPC("RPC_FailConsoleSkillCheck", RpcTarget.All);
                break;
            case 1:
                break;
            case 2:
                m_PV.RPC("RPC_GreatConsoleSkillCheck", RpcTarget.All);
                break;
        }
    }

    #region Photon
    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(m_globalUploadProgress);
            stream.SendNext(m_playersOnConsole);
            stream.SendNext(m_protecting);
        }
        else {
            m_globalUploadProgress = (float)stream.ReceiveNext();
            m_playersOnConsole = (int)stream.ReceiveNext();
            m_protecting = (bool)stream.ReceiveNext();
        }
        UpdateUI();
    }

    [PunRPC]
    void RPC_ConsoleFinished() {
        m_localUploadProgress = 80;
        m_globalUploadProgress = 80;
        UpdateUI();

        m_activated = true;
        m_workingParticle.SetActive(false);
        m_hackingParticle.SetActive(false);
        m_hackedParticle.SetActive(true);
        GameManager.m_singleton.ConsoleFinished();
        m_hackedAudio.Play();

        StartCoroutine(HackedAnimation());
    }

    [PunRPC]
    void RPC_PlayerEnterConsole() {
        m_playersOnConsole++;
        UpdateUI();
    }

    [PunRPC]
    void RPC_PlayerExitConsole() {
        m_playersOnConsole--;
        UpdateUI();
    }

    [PunRPC]
    void RPC_PlayerProtectConsole() {
        m_protecting = true;
    }

    [PunRPC]
    void RPC_PlayerEndProtectConsole() {
        m_protecting = false;
    }

    [PunRPC]
    void RPC_PlayerProtectedConsole() {
        m_protectProgress = m_protectMax;
    }

    [PunRPC]
    void RPC_GreatConsoleSkillCheck() {
        m_localUploadProgress += m_uploadMax / 100;
    }

    [PunRPC]
    void RPC_FailConsoleSkillCheck() {
        m_localUploadProgress -= m_uploadMax / 10;
        if (m_localUploadProgress < 0) {
            m_localUploadProgress = 0;
        }
        m_failAudio.Play();
        StartCoroutine(FailedAnimation());
    }
    #endregion

    void UpdateUI() {
        m_text.text = $"Players: {m_playersOnConsole}\nProgress: {(int)m_localUploadProgress}/{m_uploadMax}";
    }

    void RegularConsole() {
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties[PlayerListItem.FirewallProperty]) {
            m_regularMaterial[0].shader = GameManager.m_singleton.GetFirewallShader;
            m_regularMaterial[1].shader = GameManager.m_singleton.GetFirewallShader;
        }
        else {
            m_regularMaterial[0].shader = GameManager.m_singleton.GetVirusShader;
            m_regularMaterial[1].shader = GameManager.m_singleton.GetVirusShader;
        }

        Material[] bottomMats = m_bottom.materials;
        bottomMats[0] = m_regularMaterial[0];
        m_bottom.materials = bottomMats;

        Material[] topMats = m_top.materials;
        topMats[0] = m_regularMaterial[1];
        m_top.materials = topMats;

        m_shieldedParticle.gameObject.SetActive(false);
        m_workingParticle.gameObject.SetActive(true);
    }

    void ProtectedConsole() {
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties[PlayerListItem.FirewallProperty]) {
            m_protectedMaterial[0].shader = GameManager.m_singleton.GetFirewallShader;
            m_protectedMaterial[1].shader = GameManager.m_singleton.GetFirewallShader;
        }
        else {
            m_protectedMaterial[0].shader = GameManager.m_singleton.GetVirusShader;
            m_protectedMaterial[1].shader = GameManager.m_singleton.GetVirusShader;
        }

        Material[] bottomMats = m_bottom.materials;
        bottomMats[0] = m_protectedMaterial[0];
        m_bottom.materials = bottomMats;

        Material[] topMats = m_top.materials;
        topMats[0] = m_protectedMaterial[1];
        m_top.materials = topMats;

        m_shieldedParticle.gameObject.SetActive(true);
        m_workingParticle.gameObject.SetActive(false);
    }

    IEnumerator HackedAnimation() {
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties[PlayerListItem.FirewallProperty]) {
            m_finishedMaterial[0].shader = GameManager.m_singleton.GetFirewallShader;
            m_finishedMaterial[1].shader = GameManager.m_singleton.GetFirewallShader;
        }
        else {
            m_finishedMaterial[0].shader = GameManager.m_singleton.GetVirusShader;
            m_finishedMaterial[1].shader = GameManager.m_singleton.GetVirusShader;
        }

        Material[] bottomMats = m_bottom.materials;
        bottomMats[0] = m_finishedMaterial[0];
        m_bottom.materials = bottomMats;

        Material[] topMats = m_top.materials;
        topMats[0] = m_finishedMaterial[1];
        m_top.materials = topMats;

        yield return new WaitForSeconds(3);

        m_finishedMaterial[0].shader = GameManager.m_singleton.GetVirusShader;
        m_finishedMaterial[1].shader = GameManager.m_singleton.GetVirusShader;

        bottomMats = m_bottom.materials;
        bottomMats[0] = m_finishedMaterial[0];
        m_bottom.materials = bottomMats;

        topMats = m_top.materials;
        topMats[0] = m_finishedMaterial[1];
        m_top.materials = topMats;
    }

    IEnumerator FailedAnimation() {
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties[PlayerListItem.FirewallProperty]) {
            m_damagedMaterial[0].shader = GameManager.m_singleton.GetFirewallShader;
            m_damagedMaterial[1].shader = GameManager.m_singleton.GetFirewallShader;
        }
        else {
            m_damagedMaterial[0].shader = GameManager.m_singleton.GetVirusShader;
            m_damagedMaterial[1].shader = GameManager.m_singleton.GetVirusShader;
        }

        Material[] bottomMats = m_bottom.materials;
        bottomMats[0] = m_damagedMaterial[0];
        m_bottom.materials = bottomMats;

        Material[] topMats = m_top.materials;
        topMats[0] = m_damagedMaterial[1];
        m_top.materials = topMats;

        yield return new WaitForSeconds(3);

        bottomMats[0] = m_regularMaterial[0];
        m_bottom.materials = bottomMats;

        topMats[0] = m_regularMaterial[1];
        m_top.materials = topMats;
    }
}
