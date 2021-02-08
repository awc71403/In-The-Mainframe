using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField]
    Transform m_playerBody;

    void LateUpdate()
    {
        transform.position = m_playerBody.position;
    }
}
