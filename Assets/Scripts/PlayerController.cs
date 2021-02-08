using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController : MonoBehaviour {
    #region Variables
    [SerializeField]
    protected Collider m_body;
    [SerializeField]
    protected GameObject m_cameraHolder;
    [SerializeField]
    protected float m_mouseSensitivity, m_smoothTime;

    protected float m_verticalLookRotation;
    protected bool m_grounded;
    protected Vector3 m_smoothMoveVelocity;
    protected Vector3 m_moveAmount;

    protected Rigidbody m_rb;

    protected PhotonView m_PV;

    protected PlayerManager m_playerManager;

    protected bool m_actionEnabled;
    protected bool m_inAction;
    #endregion

    #region Events and Delegates
    public delegate void StartAction(PlayerController player);
    public static event StartAction StartActionEvent;

    public delegate void EndAction(PlayerController player);
    public static event EndAction EndActionEvent;
    #endregion

    #region Initialization
    public virtual void Awake() {
        m_rb = GetComponentInChildren<Rigidbody>();
        m_PV = GetComponent<PhotonView>();

        m_playerManager = PhotonView.Find((int)m_PV.InstantiationData[0]).GetComponent<PlayerManager>();

        m_actionEnabled = false;
        m_inAction = false;
    }

    public virtual void Start() {
        if (!m_PV.IsMine) {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(m_rb);
        }
    }
    #endregion

    #region Update
    public virtual void Update() {
        if (!m_PV.IsMine) {
            return;
        }
        CameraLook();

        Action();

        Move();
    }

    void FixedUpdate() {
        if (!m_PV.IsMine) {
            return;
        }

        m_rb.MovePosition(m_rb.position + transform.TransformDirection(m_moveAmount) * Time.fixedDeltaTime);
    }
    #endregion

    #region Getter/Setter
    public PlayerManager GetPlayerManager {
        get { return m_playerManager; }
    }

    public Collider Body {
        get { return m_body; }
    }

    public GameObject GetCameraHolder {
        get { return m_cameraHolder; }
    }

    public bool SetActionEnabled {
        set { m_actionEnabled = value; }
    }
    #endregion

    public virtual void StartPlayerAction(PlayerController player) {
        Debug.Log($"{player} player in StartPlayerAction in PlayerController");
        StartActionEvent(player);
    }

    public virtual void EndPlayerAction(PlayerController player) {
        EndActionEvent(player);
    }

    public virtual void EnableAction() {
        m_actionEnabled = true;
    }

    public virtual void DisableAction() {
        m_actionEnabled = false;
    }

    public abstract void CameraLook();

    public abstract void Move();

    public abstract void Action();

    public void SetGroundedState(bool grounded) {
        m_grounded = grounded;
    }
}
