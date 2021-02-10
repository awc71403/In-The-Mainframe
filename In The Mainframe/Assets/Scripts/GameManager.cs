using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks {
    #region Variables
    public static GameManager m_singleton;

    [SerializeField]
    private Console[] m_consoles;

    [SerializeField]
    private EscapeDoor[] m_escapeDoors;

    [SerializeField]
    private int m_consolesDone;
    [SerializeField]
    private int m_consolesNeeded;

    [SerializeField]
    private Transform m_virusList;

    [SerializeField]
    private Slider m_actionSlider;
    [SerializeField]
    private Slider m_jumpSlider;
    [SerializeField]
    private Image m_sliderBackground;
    [SerializeField]
    private Image m_sliderFill;

    [SerializeField]
    private TextMeshProUGUI m_spectateName;

    [SerializeField]
    private TextMeshProUGUI m_consolesRemaining;

    [SerializeField]
    private GameObject m_skillCheckHolder;

    [SerializeField]
    private Material m_consoleTop, m_consoleBottom;
    [SerializeField]
    private Shader m_firewallShader, m_virusShader;

    private int m_virusAmount;
    private int m_virusEscaped;
    private int m_virusDead;


    private List<FirewallController> m_firewalls;

    public const string VirusTag = "Virus";
    public const string FirewallTag = "Firewall";
    #endregion

    #region Initialization
    void Awake() {
        if (m_singleton) {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        m_singleton = this;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        m_consolesDone = 0;
        m_consolesRemaining.text = (m_consolesNeeded).ToString();
        m_firewalls = new List<FirewallController>();
    }
    #endregion

    #region Getter
    public Transform GetVirusList {
        get { return m_virusList; }
    }

    public Shader GetFirewallShader {
        get { return m_firewallShader; }
    }

    public Shader GetVirusShader {
        get { return m_virusShader; }
    }

    public List<FirewallController> Firewalls {
        get { return m_firewalls; }
    }

    public Slider GetActionSlider {
        get { return m_actionSlider; }
    }

    public Slider GetJumpSlider {
        get { return m_jumpSlider; }
    }

    public Image GetSliderBackground {
        get { return m_sliderBackground; }
    }

    public Image GetSliderFill {
        get { return m_sliderFill; }
    }

    public GameObject GetSkillCheckHolder {
        get { return m_skillCheckHolder; }
    }
    #endregion

    #region Consoles
    public void ConsoleFinished() {
        m_consolesDone++;
        m_consolesRemaining.text = (m_consolesNeeded - m_consolesDone).ToString();
        if (m_consolesDone >= m_consolesNeeded) {
            HackedEscapeDoors();
        }
        //De-activate the rest of the consoles
    }

    public void HackedEscapeDoors() {
        foreach (EscapeDoor door in m_escapeDoors) {
            door.SetMainframeHacked = true;
            door.UpdateUI();
        }
    }

    public void FirewallShader() {
        m_consoleTop.shader = m_firewallShader;
        m_consoleBottom.shader = m_firewallShader;
    }

    public void VirusShader() {
        m_consoleTop.shader = m_virusShader;
        m_consoleBottom.shader = m_virusShader;
    }
    #endregion

    #region Virus
    public void AddVirus() {
        m_virusAmount++;
    }

    public void AddEscapedVirus() {
        m_virusEscaped++;
        if (m_virusEscaped + m_virusDead >= m_virusAmount) {
            Leave();
        }
    }

    public void AddDeadVirus() {
        m_virusDead++;
        if (m_virusEscaped + m_virusDead >= m_virusAmount) {
            Leave();
        }
    }

    public void CheckEnd() {
        Debug.Log(PlayerList.m_singleton.GetVirusesTransform.GetComponentsInChildren<VirusController>().Length);
        foreach (VirusController virus in PlayerList.m_singleton.GetVirusesTransform.GetComponentsInChildren<VirusController>()) {
            if (virus.Escaped || virus.GetCurrentHP == 0) {
                continue;
            }
            else {
                return;
            }
        }
        Leave();
    }

    private void Leave() {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom() {
        Destroy(RoomManager.m_singleton.gameObject);
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }
    #endregion

    #region Spectating
    public void Spectating(string name) {
        m_spectateName.gameObject.SetActive(true);
        m_spectateName.text = name;
    }
    #endregion
}
