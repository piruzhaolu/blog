using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class MathematicsTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            RigidTransform rigidTransform = new RigidTransform(transform.rotation, transform.position);
            rigidTransform = math.mul(rigidTransform, RigidTransform.Translate(new float3(0,0,1)));
            transform.rotation = rigidTransform.rot;
            transform.position = rigidTransform.pos;
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            RigidTransform rigidTransform = new RigidTransform(transform.rotation, transform.position);
            var v = math.mul((quaternion)transform.rotation, new float3(0, 0, 1));
            transform.position += (Vector3) v;
        }
    }
}
