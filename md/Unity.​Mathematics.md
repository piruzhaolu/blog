# Mathematics 常用API简介
[代码参考](../BlogProject/Assets/Unity.​Mathematics/MathematicsTest.cs)

### RigidTransform 
存储位置与旋转信息(没有缩放)。可通过new和一堆静态方法构建一个RigidTransform, RotateX(float angle),RotateY(float angle),RotateZ(float angle) 三个静态方法的参数是弧度值。

### math.mul
math.mul方法有199个重载，不过逻辑都一样就是各种相乘
* RigidTransform **math.mul(RigidTransform a,RigidTransform b)** 表示a通过b变换后的值[参考：KeyCode.B](../BlogProject/Assets/Unity.​Mathematics/MathematicsTest.cs)
* float3 **math.mul(quaternion a,float3 b)** 相对于a旋转下进行b向量的偏移值 [参考：KeyCode.B](../BlogProject/Assets/Unity.​Mathematics/MathematicsTest.cs)



### math.all 和 math.any
* math.all(int3) math.all(bool3) xyz都不等于0 或 都为true 返回true, 否则返回false。 int4 float2之类同理
* math.any xyz有一个不等于0或有一个等true 返回true, 否则返回false

### quaternion
quaternion **LookRotation(float3 forward, float3 up)** 朝向forward向量指向的方向
