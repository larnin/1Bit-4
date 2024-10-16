using UnityEngine;
using System.Collections;

public class DestroyAfterTimer : MonoBehaviour
{
    [SerializeField] float m_timer = 1;
    
    void Start()
    {
        Destroy(gameObject, m_timer);
    }
}
