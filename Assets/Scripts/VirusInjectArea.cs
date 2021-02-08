using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusInjectArea : MonoBehaviour
{
    #region Variables
    VirusController m_virusController;
    private PhotonView m_PV;
    #endregion

    #region Initialization
    void Awake() {
        m_virusController = GetComponentInParent<VirusController>();
        m_PV = GetComponentInParent<PhotonView>();
    }
    #endregion

    #region Events and Delegates
    public delegate void InjectFinished();
    public static event InjectFinished InjectFinishedEvent;
    #endregion

    #region OnTrigger
    void OnTriggerEnter(Collider other) {
        if (!m_PV.IsMine && other.gameObject.CompareTag(GameManager.VirusTag)) {
            Debug.Log($"Another player has entered the healing area of {m_PV.Owner.NickName}. OnTriggerEnter.");
            VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
            if (hitVirus) {
                hitVirus.PriorityAction(VirusController.ActionStates.INJECT);
                float localInjectProgress = m_virusController.GlobalInjectProgress;
                m_virusController.LocalInjectProgress = localInjectProgress;
                hitVirus.GetPlayerManager.EnableAction(PlayerManager.InjectAction, localInjectProgress, m_virusController.GetInjectMax);
                PlayerController.StartActionEvent += PlayerEnterInjectArea;
                SkillCheck.SkillCheckOutcomeEvent += InjectSkillCheck;
                PlayerController.EndActionEvent += PlayerExitInjectArea;
            }
        }
    }

    void OnTriggerStay(Collider other) {
        if (!m_PV.IsMine && other.gameObject.CompareTag(GameManager.VirusTag)) {
            Debug.Log($"Another player is possibly healing {m_PV.Owner.NickName}. OnTriggerStay.");
            VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
            if (hitVirus) {
                hitVirus.PriorityAction(VirusController.ActionStates.INJECT);
                float localInjectProgress = m_virusController.LocalInjectProgress;
                float injectMax = m_virusController.GetInjectMax;
                if (localInjectProgress >= injectMax) {
                    m_virusController.LocalInjectProgress = 0;
                    m_virusController.GlobalInjectProgress = 0;
                    m_virusController.Injected();
                    if (m_virusController.GetCurrentHP == 2) {
                        if (GameManager.m_singleton.GetSkillCheckHolder.activeSelf) {
                            InjectFinishedEvent();
                        }
                        hitVirus.GetPlayerManager.DisableAction();
                        PlayerController.StartActionEvent -= PlayerEnterInjectArea;
                        SkillCheck.SkillCheckOutcomeEvent -= InjectSkillCheck;
                        PlayerController.EndActionEvent -= PlayerExitInjectArea;
                    }
                } else {
                    hitVirus.GetPlayerManager.EnableAction(PlayerManager.InjectAction, localInjectProgress, injectMax);
                }
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (!m_PV.IsMine && other.gameObject.CompareTag(GameManager.VirusTag)) {
            Debug.Log($"Another player has left the healing area of {m_PV.Owner.NickName}. OnTriggerExit.");
            VirusController hitVirus = other.gameObject.GetComponentInParent<VirusController>();
            if (hitVirus) {
                hitVirus.GetPlayerManager.DisableAction();
                PlayerController.StartActionEvent -= PlayerEnterInjectArea;
                SkillCheck.SkillCheckOutcomeEvent -= InjectSkillCheck;
                PlayerController.EndActionEvent -= PlayerExitInjectArea;
            }
        }
    }
    #endregion

    void PlayerEnterInjectArea(PlayerController player) {
        m_virusController.PlayerStartsInject(player);
    }

    void PlayerExitInjectArea(PlayerController player) {
        m_virusController.PlayerStopsInject(player);
    }

    void InjectSkillCheck(int outcome, VirusController.ActionStates action) {
        if (action != VirusController.ActionStates.INJECT) {
            return;
        }
        m_virusController.InjectSkillCheck(outcome);
    }
}
