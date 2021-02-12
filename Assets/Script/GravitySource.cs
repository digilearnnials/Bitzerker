using System.Collections.Generic;
using UnityEngine;

public class GravitySource : MonoBehaviour {

    static List<GravitySource> sources = new List<GravitySource>();
    
    public virtual Vector3 GetGravity (Vector3 position) {
        return Physics.gravity;
    }
    
    void OnEnable () {
        CustomGravity.Register(this);
    }

    void OnDisable () {
        CustomGravity.Unregister(this);
    }
}
