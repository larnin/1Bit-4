using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SimpleSinMoveAnimation : MonoBehaviour
{
    [SerializeField] float m_amplitude = 5;
    [SerializeField] float m_speed = 1;
    [SerializeField] Vector3 m_direction = Vector3.up;

    Vector3 m_initialPosition;
    float m_time = 0;

    private void Start()
    {
        m_initialPosition = transform.localPosition;
    }

    private void Update()
    {
        m_time += Time.deltaTime;

        float offset = Mathf.Sin(m_time * 2 * Mathf.PI * m_speed) * m_amplitude;
        Vector3 pos = m_initialPosition + offset * m_direction;
        transform.localPosition = pos;
    }
}
