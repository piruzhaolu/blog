using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


[ExecuteInEditMode]
public class MathematicsTest : MonoBehaviour
{
    public GameObject Target;
    public GameObject ChildTarget;

    public bool LookRotation = false;
    public bool w2lTranform = false;


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
            var v = math.mul((quaternion)transform.rotation, new float3(0, 0, 1));
            transform.position += (Vector3) v;
        }

        if (LookRotation)
        {
            float3 f3 = Target.transform.position - transform.position;
           // f3.y = 0;
            var b = quaternion.LookRotationSafe(float3.zero, math.up());
            transform.rotation  = quaternion.LookRotation(f3, math.up());
        }
        if (w2lTranform)
        {
            var wPos = Target.transform.position;
            var f4x4 = (float4x4) transform.localToWorldMatrix;
            var pos = math.transform(math.inverse(f4x4), wPos);
            ChildTarget.transform.localPosition = pos;
        }

    }
}
