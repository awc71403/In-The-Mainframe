using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    PlayerController m_playerController;


    void Awake()
    {
        m_playerController = GetComponentInParent<PlayerController>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject == m_playerController.gameObject) {
            return;
        }
        m_playerController.SetGroundedState(true);
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject == m_playerController.gameObject) {
            return;
        }
        m_playerController.SetGroundedState(false);
    }

    void OnTriggerStay(Collider other) {
        if (other.gameObject == m_playerController.gameObject) {
            return;
        }
        m_playerController.SetGroundedState(true);
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject == m_playerController.gameObject) {
            return;
        }
        m_playerController.SetGroundedState(true);
    }

    void OnCollisionExit(Collision collision) {
        if (collision.gameObject == m_playerController.gameObject) {
            return;
        }
        m_playerController.SetGroundedState(false);
    }

    void OnCollisionStay(Collision collision) {
        if (collision.gameObject == m_playerController.gameObject) {
            return;
        }
        m_playerController.SetGroundedState(true);
    }
}
