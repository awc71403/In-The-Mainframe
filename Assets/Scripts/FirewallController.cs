using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirewallController : PlayerController
{
    [SerializeField]
    Collider m_attackBox;
    private bool m_attacking;

    [SerializeField]
    private float m_jumpForce;

    [SerializeField]
    private float m_jumpCD;
    private float m_currentJumpCD;

    [SerializeField]
    private Light m_glow;

    private AudioSource m_attackAudio;

    [SerializeField]
    private float m_baseSpeed, m_lungeSpeed, m_recoilSpeed;
    [SerializeField]
    private float m_hitTimer, m_missTimer;
    [SerializeField]
    private float m_speed, m_recoilTimer;

    private Animator m_animator;
    private Slider m_jumpSlider;

    [SerializeField]
    private GameObject m_particle;

    public override void Awake() {
        base.Awake();
        m_attackAudio = GetComponent<AudioSource>();
        m_attacking = false;
        m_speed = m_baseSpeed;
        //For Testing
        m_recoilTimer = 1;

        m_currentJumpCD = 0;

        if (m_PV.IsMine) {
            m_glow.gameObject.SetActive(false);
            m_particle.SetActive(false);
            m_jumpSlider = GameManager.m_singleton.GetJumpSlider;
            m_jumpSlider.gameObject.SetActive(true);
        }

        transform.SetParent(PlayerList.m_singleton.GetFirewallsTransform);

        StartCoroutine(InitialRecoil());

        m_animator = GetComponentInChildren<Animator>();
    }

    public override void Update() {
        base.Update();

        Jump();

        Attack();
    }

    public override void CameraLook() {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * m_mouseSensitivity);

        m_verticalLookRotation += Input.GetAxisRaw("Mouse Y") * m_mouseSensitivity;
        m_verticalLookRotation = Mathf.Clamp(m_verticalLookRotation, -90f, 90f);

        m_cameraHolder.transform.localEulerAngles = Vector3.left * m_verticalLookRotation;
    }

    public override void Move() {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        if (m_inAction) {
            m_moveAmount = Vector3.zero;
            moveDir = Vector3.zero;
        }

        m_moveAmount = Vector3.SmoothDamp(m_moveAmount, moveDir * m_speed, ref m_smoothMoveVelocity, m_smoothTime);
    }

    public override void Action() {
        if (m_actionEnabled && Input.GetKeyDown(KeyCode.Space)) {
            //Debug.LogError("Action has started.");
            base.StartPlayerAction(this);
            m_playerManager.ActiveAction();
            m_inAction = true;
        }
        else if (m_inAction && Input.GetKeyUp(KeyCode.Space)) {
            base.EndPlayerAction(this);
            m_playerManager.InactiveAction();
            m_inAction = false;
        }
    }

    public void Jump() {
        if (m_jumpSlider) {
            m_jumpSlider.value = (float)(10 - m_currentJumpCD) / (float)m_jumpCD;
        }

        if (m_currentJumpCD > 0) {
            m_currentJumpCD -= Time.deltaTime;
        }

        if (m_inAction) {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Space) && m_grounded && m_currentJumpCD <= 0) {
            Debug.Log("Jump");
            m_rb.AddForce(m_rb.transform.up * m_jumpForce);
            m_currentJumpCD = m_jumpCD;
        }
    }

    private void Attack() {
        if (m_PV.IsMine && Input.GetMouseButton(0) && !m_attacking) {
            m_attacking = true;
            m_PV.RPC("RPC_Attack", RpcTarget.All);
        }
    }

    IEnumerator AttackAction() {
        m_attackAudio.Play();
        m_attackBox.enabled = true;
        m_animator.SetBool("isAttacking", true);
        m_speed = 8;
        while (m_speed < m_lungeSpeed) {
            m_speed += Time.deltaTime * 10;
            yield return null;
        }
        m_animator.SetBool("isAttacking", false);
    }

    IEnumerator Recoil() {
        m_speed = m_recoilSpeed;
        if (m_attackBox.enabled) {
            m_attackBox.enabled = false;
            m_recoilTimer = m_missTimer;
            m_animator.SetBool("hit", false);
        }
        else {
            m_recoilTimer = m_hitTimer;
            m_animator.SetBool("hit", true);
        }
        yield return new WaitForSeconds(m_recoilTimer);
        m_speed = m_baseSpeed;
        m_attacking = false;

    }

    IEnumerator InitialRecoil() {
        m_attacking = true;
        yield return new WaitForSeconds(m_recoilTimer);
        m_attacking = false;
    }

    public void RecoilCall() {
        StartCoroutine(Recoil());
    }

    public void Resetter() {
        m_attacking = false;
        m_speed = m_baseSpeed;
    }

    #region RPC
    [PunRPC]
    void RPC_Attack() {
        StartCoroutine(AttackAction());
    }
    #endregion
}
