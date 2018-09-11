using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour {

    [HideInInspector] public ObjectPooler m_ObjectPooler;

    public void GetReturned() {
        m_ObjectPooler.ReturnObject(gameObject);
    }
}
