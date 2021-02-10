using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VirusController : PlayerController, IPunObservable {

    #region Variables
    [SerializeField]
    VirusListItem m_virusListItem;
    [SerializeField]
    VirusListItem m_virusListItemPrefab;
    VirusInjectArea m_virusInjectArea;
    int m_virusHP;


    [SerializeField]
    private float m_sprintSpeed, m_walkSpeed, m_turnSpeed, m_downedSpeed;

    [SerializeField]
    float m_downedMaxTimer;
    float m_downedCurrentTimer;
    int m_timesDowned;

    [SerializeField]
    private float m_injectMax;
    [SerializeField]
    private float m_localInjectProgress;

    private float m_globalInjectProgress;

    private int m_playersInjecting;

    private Spectator m_spectator;
    private SkillCheck m_skillCheck;
    private bool m_skillChecking;

    private bool m_rebooting;
    private bool m_sprinting;
    private bool m_escaped;

    private bool m_hurt;
    [SerializeField]
    private float m_maxHurtSpeed;
    private float m_hurtSpeed;

    //Will need more for audio later
    [SerializeField]
    private AudioSource m_skillCheckAudio;
    [SerializeField]
    private AudioSource m_terrorAudio;
    [SerializeField]
    private AudioSource m_terrorAudio2;

    private ParticleSystem m_trail;

    [SerializeField]
    private MeshRenderer m_mesh;

    [SerializeField]
    private Material[] m_bodyHighlight;

    [SerializeField]
    private Material[] m_glow;

    [SerializeField]
    private GameObject m_downedParticle;

    private ActionStates m_currentAction;

    [SerializeField]
    private float m_terrorTimer;
    [SerializeField]
    private float m_currentTerrorTimer;

    private const int maxHP = 2;
    private const float rebootingPercent = .95f;
    private const int hurtTimeDecay = 3;
    #endregion

    #region Enum
    public enum ActionStates { NONE, HACK, OVERRIDE, INJECT, REBOOT, INJECTBY };
    #endregion

    #region Initialization
    public override void Awake() {
        base.Awake();
        GameManager.m_singleton.AddVirus();
        m_virusHP = maxHP;
        m_hurt = false;
        m_timesDowned = 0;
        m_downedCurrentTimer = m_downedMaxTimer;
        m_currentAction = ActionStates.NONE;
        m_trail = GetComponentInChildren<ParticleSystem>();
        m_spectator = GetComponentInChildren<Spectator>();

        m_virusListItem = Instantiate(m_virusListItemPrefab);
        m_virusListItem.gameObject.transform.SetParent(GameManager.m_singleton.GetVirusList);
        m_virusListItem.gameObject.transform.localScale = new Vector3(1, 1, 1);

        m_virusInjectArea = gameObject.GetComponentInChildren<VirusInjectArea>();

        transform.SetParent(PlayerList.m_singleton.GetVirusesTransform);

        if (m_PV.IsMine) {
            GameManager.m_singleton.GetSkillCheckHolder.SetActive(true);

            m_skillCheck = GameManager.m_singleton.GetSkillCheckHolder.GetComponentInChildren<SkillCheck>();
            m_skillCheck.VirusController = this;

            GameManager.m_singleton.GetSkillCheckHolder.SetActive(false);
        }

        m_playersInjecting = 0;
        //if (m_PV.IsMine) {
            //Change to regular Instantiate
            //m_virusListItem = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "VirusListItem"), Vector3.zero, Quaternion.identity).GetComponent<VirusListItem>();
            //m_virusListItem.gameObject.transform.SetParent(GameManager.m_singleton.GetVirusList);
        //}
    }

    public override void Start() {
        base.Start();
        m_virusListItem.SetUp(m_PV.Owner);
        UpdateController();
        m_virusListItem.VirusController = this;
        m_virusListItem.MaxDownedTimer = m_downedMaxTimer;
        m_virusListItem.UpdateHealthUI(m_virusHP, m_timesDowned);
    }

    private void OnEnable() {
        Console.ConsoleFinishedEvent += EndSkillCheck;
        VirusInjectArea.InjectFinishedEvent += EndSkillCheck;
    }
    #endregion

    #region Update
    public override void Update() {
        base.Update();

        if (m_hurt) {
            //Make 3 into constant
            m_hurtSpeed -= ((m_maxHurtSpeed - m_sprintSpeed) / hurtTimeDecay) * Time.deltaTime;
        }

        if (m_sprinting) {
            m_trail.enableEmission = true;
        }
        else {
            m_trail.enableEmission = false;
        }

        TerrorCheck();

        Injecting();

        SkillCheckChance();
    }
    #endregion

    #region Override
    public override void CameraLook() {
        m_verticalLookRotation += Input.GetAxisRaw("Mouse Y") * m_mouseSensitivity;
        m_verticalLookRotation = Mathf.Clamp(m_verticalLookRotation, -85f, 20f);

        Vector3 cameraAngle = m_cameraHolder.transform.localEulerAngles;
        cameraAngle.x = (Vector3.left * m_verticalLookRotation).x;
        cameraAngle.y += (Vector3.up * Input.GetAxisRaw("Mouse X") * m_mouseSensitivity).y;
        m_cameraHolder.transform.localEulerAngles = cameraAngle;
    }

    public override void Move() {
        Vector3 moveF = m_cameraHolder.transform.forward;
        Vector3 moveR = m_cameraHolder.transform.right;

        moveF.y = 0;
        moveR.y = 0;

        Vector3 moveDir = (moveF.normalized * Input.GetAxisRaw("Vertical") + moveR.normalized * Input.GetAxisRaw("Horizontal")).normalized;

        if (m_inAction) {
            m_moveAmount = Vector3.zero;
            moveDir = Vector3.zero;
        }

        if (Input.GetKey(KeyCode.LeftShift) && m_virusHP != 0) {
            m_sprinting = true;
        }
        else {
            m_sprinting = false;
        }

        m_moveAmount = Vector3.SmoothDamp(m_moveAmount, moveDir * (m_hurt ? m_hurtSpeed : ((m_virusHP == 0) ? m_downedSpeed : (Input.GetKey(KeyCode.LeftShift) ? m_sprintSpeed : m_walkSpeed))), ref m_smoothMoveVelocity, m_smoothTime);

        PlayerLook(moveDir);
    }

    public override void Action() {
        if (m_actionEnabled && Input.GetMouseButtonDown(0) && m_playersInjecting == 0) {
            //Debug.LogError("Action has started.");
            base.StartPlayerAction(this);
            m_playerManager.ActiveAction();
            m_inAction = true;
        }
        else if (m_inAction && Input.GetMouseButtonUp(0)) {
            //Debug.LogError("Action has stopped.");
            //Add the LeftSkillCheck?
            if (m_skillChecking) {
                m_skillCheck.gameObject.SetActive(true);
                m_skillCheck.Action = m_currentAction;
                m_skillCheck.LeftOnSkillCheck();
            }
            base.EndPlayerAction(this);
            m_playerManager.InactiveAction();
            m_inAction = false;
        }
    }
    #endregion

    #region Getter/Setter
    public int GetCurrentHP {
        get { return m_virusHP; }
    }

    public float GetInjectMax {
        get { return m_injectMax; }
    }

    public float SetTerrorTimer {
        set { m_terrorTimer = value; }
    }

    public ActionStates GetCurrentActionState {
        get { return m_currentAction; }
    }

    public float LocalInjectProgress {
        get { return m_localInjectProgress; }
        set { m_localInjectProgress = value; }
    }

    public float GlobalInjectProgress {
        get { return m_globalInjectProgress; }
        set { m_globalInjectProgress = value; }
    }

    public int PlayersInjecting {
        get { return m_playersInjecting; }
        set { m_playersInjecting = value; }
    }

    public bool SkillChecking {
        get { return m_skillChecking; }
        set { m_skillChecking = value; }
    }

    public bool Escaped {
        get { return m_escaped; }
    }

    public bool Hurt {
        get { return m_hurt; }
    }
    #endregion

    public void Injecting() {
        if (m_virusHP < maxHP) {
            if (m_playersInjecting > 0) {
                m_localInjectProgress += Time.deltaTime * m_playersInjecting;
                m_globalInjectProgress = m_localInjectProgress;
                if (m_PV.IsMine) {
                    m_playerManager.ActivateAction();
                    if (m_playersInjecting == 1) {
                        m_playerManager.UpdateAction(PlayerManager.InjectedByAction + $" {m_playersInjecting} " + PlayerManager.InjectedByActionSingle, m_localInjectProgress, m_injectMax);
                    }
                    else if (m_playersInjecting == 2) {
                        m_playerManager.UpdateAction(PlayerManager.InjectedByAction + $" {m_playersInjecting} " + PlayerManager.InjectedByActionMultiple, m_localInjectProgress, m_injectMax);
                    }
                    m_playerManager.InactiveAction();
                }
            }
            else if (m_rebooting) {
                if (m_localInjectProgress < m_injectMax * rebootingPercent) {
                    m_localInjectProgress += Time.deltaTime / 2;
                    m_globalInjectProgress = m_localInjectProgress;
                }
            }
            else {
                if (!m_inAction && m_PV.IsMine) {
                    m_playerManager.DeactivateAction();
                }
            }
            //Debug.LogError($"For player {m_PV.Owner.NickName}, the global progress is {m_globalInjectProgress}.");
            //Put in a seperate function
            if (m_PV.IsMine) {
                if (m_virusHP == 0) {
                    PriorityAction(ActionStates.REBOOT);
                    if (m_localInjectProgress < m_injectMax * rebootingPercent) {
                        m_playerManager.EnableAction(PlayerManager.RebootingAction, m_localInjectProgress, m_injectMax);
                    }
                    else {
                        m_playerManager.EnableAction(PlayerManager.RebootingCapped, m_localInjectProgress, m_injectMax);
                    }
                }
            }
        }
    }

    public void PlayerLook(Vector3 direction) {
        if (direction != Vector3.zero) {
            m_rb.gameObject.transform.rotation = Quaternion.Slerp(m_rb.gameObject.transform.rotation, Quaternion.LookRotation(direction), m_turnSpeed * Time.deltaTime);
        }
    }

    private void TerrorCheck() {
        if (!m_PV.IsMine) {
            return;
        }
        if (m_currentTerrorTimer <= 0) {
            StartCoroutine(TerrorSound());
            m_currentTerrorTimer = m_terrorTimer;
        }
        m_currentTerrorTimer -= Time.deltaTime;
    }

    public override void DisableAction() {
        if (m_inAction) {
            base.EndPlayerAction(this);
            m_playerManager.InactiveAction();
            m_inAction = false;
        }
        m_currentAction = ActionStates.NONE;
        base.DisableAction();
    }

    #region RPC Callers
    public void TakeDamage() {
        m_PV.RPC("RPC_TakeDamage", RpcTarget.All);
    }

    public void Injected() {
        m_PV.RPC("RPC_Injected", RpcTarget.All);
    }

    public void EnteredMainFrame() {
        m_PV.RPC("RPC_EnteredMainFrame", RpcTarget.All);
    }

    public void PlayerStartsInject(PlayerController player) {
        if (((VirusController)player).GetCurrentActionState != ActionStates.INJECT && (m_currentAction != ActionStates.NONE || m_currentAction != ActionStates.REBOOT)) {
            return;
        }
        m_PV.RPC("RPC_PlayerStartsInject", RpcTarget.All);
    }

    public void PlayerStopsInject(PlayerController player) {
        if (((VirusController)player).GetCurrentActionState != ActionStates.INJECT) {
            return;
        }
        m_PV.RPC("RPC_PlayerStopsInject", RpcTarget.All);
    }
    #endregion

    void StartRebooting(PlayerController player) {
        if (((VirusController)player).GetCurrentActionState != ActionStates.REBOOT) {
            return;
        }
        m_rebooting = true;
    }

    void StopRebooting(PlayerController player) {
        if (((VirusController)player).GetCurrentActionState != ActionStates.REBOOT) {
            return;
        }
        m_rebooting = false;
    }

    #region Skill Check
    void SkillCheckChance() {
        if (m_currentAction == ActionStates.INJECT || m_currentAction == ActionStates.HACK) {
            int random = Random.Range(1, 500);
            if (random == 1 && !m_skillChecking && m_inAction) {
                StartSkillCheck();
            }
        }
    }

    void StartSkillCheck() {
        StartCoroutine(PlaySkillCheck());
    }

    public void InjectSkillCheck(int outcome) {
        switch (outcome) {
            case 0:
                m_PV.RPC("RPC_BadInjectSkillCheck", RpcTarget.All);
                break;
            case 1:
                break;
            case 2:
                m_PV.RPC("RPC_GreatInjectSkillCheck", RpcTarget.All);
                break;
        }
    }

    void EndSkillCheck() {
        m_skillChecking = false;
    }
    #endregion

    void UpdateController() {
        if (m_virusHP < maxHP) {
            m_virusInjectArea.gameObject.GetComponent<BoxCollider>().enabled = true;
        }
    }

    public void PriorityAction(ActionStates action) {
        if (m_currentAction < action) {
            m_currentAction = action;
        }
    }

    public void TerrorSound(float delay, float volume, bool mute) {
        m_terrorTimer = delay;
        m_terrorAudio.volume = volume;
        m_terrorAudio.mute = mute;
        m_terrorAudio2.volume = volume;
        m_terrorAudio2.mute = mute;
    }

    private void ModelUpdate(int virusHP) {
        Material[] mats = m_mesh.materials;
        switch (virusHP) {
            case 0:
                if (!(bool)PhotonNetwork.LocalPlayer.CustomProperties[PlayerListItem.FirewallProperty]) {
                    mats[0] = m_bodyHighlight[1];
                }
                else {
                    mats[0] = m_bodyHighlight[0];
                }
                mats[1] = m_glow[2];
                m_mesh.materials = mats;
                break;
            case 1:
                mats[0] = m_bodyHighlight[0];
                mats[1] = m_glow[1];
                m_mesh.materials = mats;
                break;
            case 2:
                mats[0] = m_bodyHighlight[0];
                mats[1] = m_glow[0];
                m_mesh.materials = mats;
                break;
        }
    }

    private void SeeViruses(bool boolean) {
        VirusController[] viruses = PlayerList.m_singleton.GetVirusesTransform.GetComponentsInChildren<VirusController>();
        Debug.Log($"Virus {viruses.Length}");
        foreach (VirusController virus in viruses) {
            Debug.Log(virus);
            if (this != virus || virus.GetCurrentHP != 0) {
                if (boolean) {
                    virus.DownedVirusVision();
                }
                else {
                    virus.RegularVirusVision();
                }
            }
        }
    }

    public void Death() {
        gameObject.SetActive(false);
        Debug.Log("Death is called");
        if (m_PV.IsMine) {
            Debug.Log("Death PV is called");
            m_playerManager.DisableAction();
            StartActionEvent -= StartRebooting;
            EndActionEvent -= StopRebooting;
            m_spectator.Enable();
        }
        
    }

    public void DownedVirusVision() {
        Material[] mats = m_mesh.materials;
        mats[0] = m_bodyHighlight[2];
        m_mesh.materials = mats;
    }

    public void RegularVirusVision() {
        Material[] mats = m_mesh.materials;
        mats[0] = m_bodyHighlight[0];
        m_mesh.materials = mats;
    }

    private void OnDisable() {
        Console.ConsoleFinishedEvent -= EndSkillCheck;
        VirusInjectArea.InjectFinishedEvent -= EndSkillCheck;

        m_spectator = GetComponentInChildren<Spectator>();
        if (m_spectator) {
            m_spectator.Change();
        }
    }

    #region Coroutine
    IEnumerator PlaySkillCheck() {
        m_skillChecking = true;
        m_skillCheckAudio.Play();
        GameManager.m_singleton.GetSkillCheckHolder.SetActive(true);
        m_skillCheck.gameObject.SetActive(false);

        yield return new WaitForSeconds(1);

        if (m_skillChecking) {
            m_skillCheck.gameObject.SetActive(true);
            m_skillCheck.Action = m_currentAction;
            m_skillCheck.Initial();
        }
    }

    IEnumerator TerrorSound() {
        m_terrorAudio.Play();
        yield return new WaitForSeconds(m_terrorTimer / 2);
        m_terrorAudio2.Play();
    }

    IEnumerator TakeDamageSpeed() {
        m_hurt = true;
        m_hurtSpeed = m_maxHurtSpeed;

        // Need firewall reference
        foreach (FirewallController firewall in GameManager.m_singleton.Firewalls) {
            Physics.IgnoreCollision(m_body, firewall.Body, true);
        }
        yield return new WaitForSeconds(1);

        foreach (FirewallController firewall in GameManager.m_singleton.Firewalls) {
            Physics.IgnoreCollision(m_body, firewall.Body, false);
        }

        yield return new WaitWhile(() => m_hurtSpeed > m_sprintSpeed);

        m_hurt = false;
    }
    #endregion

    #region Photon
    [PunRPC]
    void RPC_TakeDamage() {
        //if (!m_PV.IsMine) {
        //    return;
        //}
        //Debug.Log($"RPC_TakeDamage called. {m_PV.Owner.NickName} took damage.");
        if (m_virusHP > 0 && !m_hurt) {
            m_virusHP--;
            ModelUpdate(m_virusHP);
            if (m_virusHP == 0) {
                GameManager.m_singleton.CheckEnd();
                m_timesDowned++;
                m_downedParticle.SetActive(true);
                if (m_PV.IsMine) {
                    Debug.Log("PV is mine");
                    SeeViruses(true);
                    m_playerManager.EnableAction(PlayerManager.RebootingAction, m_localInjectProgress, m_injectMax);
                    StartActionEvent += StartRebooting;
                    EndActionEvent += StopRebooting;
                }
            }
            m_virusListItem.UpdateHealthUI(m_virusHP, m_timesDowned);
            if (m_virusHP != 0) {
                StartCoroutine(TakeDamageSpeed());
            }
            m_localInjectProgress = 0;
            m_globalInjectProgress = 0;
        }
        m_virusInjectArea.gameObject.GetComponent<BoxCollider>().enabled = true;

    }

    [PunRPC]
    void RPC_Injected() {
        Debug.Log($"HP is {m_virusHP}");
        if (m_virusHP < maxHP) {
            m_virusHP++;
            m_downedParticle.SetActive(false);
            ModelUpdate(m_virusHP);
            if (m_PV.IsMine) {
                SeeViruses(false);
                if (m_virusHP == 1) {
                    m_playerManager.DisableAction();
                    StartActionEvent -= StartRebooting;
                    EndActionEvent -= StopRebooting;
                }
            }
            if (m_virusHP >= maxHP) {
                Debug.Log("Disabled healing collider");
                m_virusInjectArea.gameObject.GetComponent<BoxCollider>().enabled = false;
            }
            m_virusListItem.UpdateHealthUI(m_virusHP, m_timesDowned);
            m_localInjectProgress = 0;
            m_globalInjectProgress = 0;
        }
    }

    [PunRPC]
    void RPC_EnteredMainFrame() {
        Debug.Log("RPC for entering mainframe called");
        m_escaped = true;
        gameObject.SetActive(false);
        //If all virus are dead or have left, reset game.
        GameManager.m_singleton.AddEscapedVirus();
    }

    [PunRPC]
    void RPC_PlayerStartsInject() {
        if (m_PV.IsMine) {

        }
        m_playersInjecting++;
    }

    [PunRPC]
    void RPC_PlayerStopsInject() {
        m_playersInjecting--;
    }

    [PunRPC]
    void RPC_GreatInjectSkillCheck() {
        m_localInjectProgress += m_injectMax / 100;
    }

    [PunRPC]
    void RPC_BadInjectSkillCheck() {
        m_localInjectProgress -= m_injectMax / 10;
        if (m_localInjectProgress < 0) {
            m_localInjectProgress = 0;
        }
    }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            //Do I need to sync HP?
            stream.SendNext(m_virusHP);
            stream.SendNext(m_globalInjectProgress);
            stream.SendNext(m_playersInjecting);
            stream.SendNext(m_rebooting);
            stream.SendNext(m_sprinting);
        }
        else {
            m_virusHP = (int)stream.ReceiveNext();
            m_globalInjectProgress = (float)stream.ReceiveNext();
            m_playersInjecting = (int)stream.ReceiveNext();
            m_rebooting = (bool)stream.ReceiveNext();
            m_sprinting = (bool)stream.ReceiveNext();
        }
    }
    #endregion
}
