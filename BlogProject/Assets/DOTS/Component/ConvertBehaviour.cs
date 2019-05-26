using Unity.Entities;
using UnityEngine;

public class ConvertBehaviour : MonoBehaviour, IConvertGameObjectToEntity
{
    public bool ConvertRotateSpeed = false;//是否对RotateSpeed转换的开关
    public RotateSpeed RotateSpeed;

    public bool ConvertSineMotionRange = false; //是否对SineMotionRange转换的开关
    public MotionRange SineMotionRange;

    public bool SineTag = false;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (ConvertRotateSpeed) dstManager.AddComponentData(entity, RotateSpeed);
        if (ConvertSineMotionRange) dstManager.AddComponentData(entity, SineMotionRange);
        if (SineTag) dstManager.AddComponentData(entity, new SineTag());
    }
}
