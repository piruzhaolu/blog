# DOTS之一 Job System 介绍

Job System 官方全名叫 Unity C# Job System，个人理解就是Unity开发的基于C#的多线程管理系统。对于多线程Unity一直都支持的，可以直接在unity中用C#创建线程，不过内部除了IO、渲染少数几个模块，主逻辑模块没有支持多线程，开发者也不能使用多线程处理GameObject。

没有很好支持多线程很大原因在于传统的多线程在处理游戏逻辑上有些问题，游戏逻辑代码的特点是单代码块的执行成本低，但要在16毫秒或33毫秒内执行大量的代码块。不管是为每个代码块创建一个线程还是用线程池，基中线程处理本身消耗的成本都会抵销其带来的好处。没有很好支持多线程还有个问题是多线程编程本身的复杂度问题，两个线程写冲突和依赖关系很容易引起难于调试的异常。

虽然多线程处理游戏会有些问题，但当前千元的手机都有八核CPU配置，没办法利用这么多核心是极大的资源浪费。而Unity在这个时间点配合ECS推出Job System也就理所当然。接下来看看Job System怎么使用以及如何处理传统多线程的一些问题。

Job System与线程相应的概念的叫Job,但Job并不是对线程的封装，个人觉得把Job当成一个函数会更容易理解。一个Job像一个函数处理单一逻辑。Job System在初始化时会创建CPU核心数减1的线程（实际比这复杂，内部会根据大小核等硬件资源做一些选择），然后把所有Job根据一定策略分配到这些线程并行执行。因为Job System只会在初始化时创建与CPU核心匹配的线程，所以不存在频繁创建线程的成本，也不会因为多个线程抢占一个CPU资源和引起上下文切换时的损耗，只要Job System能合理分配Job就能充分利用CPU资源而较少的副作用。

一个Job的实际代码像这样：
```C#
struct Multiply : IJob
{
    public int a;
    public int b;
    public NativeArray<int> result;
    public void Execute()
    {
        result[0] = a * b;
    }
}
```
整个Job是一个实现IJob接口的struct，a、b 是输入的两个参数， result是执行的结果，Execute是执行逻辑。整体还是比较简单的，除了NativeArray\<int\>类型的result比较容易引起困惑，NativeArray\<int\>是DOTS引入的一部分，官方叫NativeContainer，除了NativeArray还有NativeHashMap等其它类型，NativeContainer的目的是让不同的Job能访问共享的内存。虽然Job并不代表一个线程，但不同的Job很可能在不同的线程处理，不同的Job之间是不能互相访问的，它们也不被允许访问全局变量，所以NativeContainer是它们进行数据交互的方式。我们先看完Job调用代码再说说NativeContainer问题

Job的调用代码如下：
```C#
NativeArray<int> result = new NativeArray<int>(1, Allocator.TempJob);
var handle = new Multiply() { a = 76, b = 93, result = result}.Schedule();
handle.Complete();
Debug.Log(result[0]);
result.Dispose();
```
Job struct的创建和普通的struct创建一样。Schedule是IJob的扩展方法，作用是把Job安排到线程中。Schedule方法并不会执行Job, 它返回一个JobHandle，调用JobHandle.Complete()才可以确保Job执行完成。然后就可以访问NativeArray中的数据了。但这段代码有个问题，Schedule方法后Job并不会马上执行，Job System的策略是尽量延迟Job的执行，所以Job是在Complete调用时才执行的，而Complete方法会使主线程处于等待Job执行完毕的状态，就是说没有任何代码在并行。要使代码能并行需要先调用JobHandle.ScheduleBatchedJobs()再调用Complete()，这时候JobHandle.ScheduleBatchedJobs()和Complete()之间的代码和Job是并行执行的。原理是JobHandle.ScheduleBatchedJobs()会让当前等待的Job进入执行状态。如果有多个Job可以按下面方式写，两个Job也可以并行。
```C#
var handle = new Multiply() { a = 76, b = 93, result = result}.Schedule();
var handle2 = new Multiply() { a = 15, b = 30, result = result2 }.Schedule();
handle.Complete();
handle2.Complete();
```

这里的最佳实践是尽可能让更多Job进入并行，比如把一帧中所有要执行的Job先安排完毕，然后调用JobHandle.ScheduleBatchedJobs()使它们在多个线程中并行。这其中有不少工作要做，好在ECS已经帮我们处理好了，在ECS中开发者不用调用和JobHandle.ScheduleBatchedJobs()和Complete(), 内部会进行处理。Job除了并行还有依赖执行的问题，Schedule()还有个重载，可以将要依赖的Job的JobHandle传入
```C#
var handle2 = new Multiply() { a = 15, b = 30, result = result2 }.Schedule(handle);
```

回头说下NativeContainer。代码中使用NativeArray来存储执行的结果，NativeArray的声明和Array相比多了一个Allocator参数，Allocator参数是个枚举，值分别是Allocator.Temp、Allocator.TempJob、Allocator.Persistent。三者的区别是生命周期不同和性能差别，Temp用于一帧内即回收数据，性能最好；TempJob生命周期是4帧，通常用于Job中，性能次于Temp；Persistent可以在程序运行过程中一直在，但性能最差。NativeArray与Array另一个不同是需要手动回收内存，即调用Dispose()。

在正常的Job System代码中，NativeContainer是不同Job之间传递数据主要方式，对多线程的写冲突也主要在NativeContainer上处理。实际上JobSystem并不能完全避免写入冲突问题，开发者还是需要考虑这个问题，JobSystem处理方向是以抛出异常方式对各种问题进行限制。

主要限制: 如果一个NativeContainer在某一个Job是有写入权限的，那么它不能被另一个Job访问。解决方式: 可以在所有Job struct中标记这个NativeContainer为[ReadOnly]; 用Schedule(handle)标记不同Job的依赖关系，让它们不要并行；对NativeContainer添加[NativeDisableContainerSafetyRestriction]标记，它会关闭安全检查，如果确信不会出问题的话；重新设计数据的组织方式，有时候用新的组织方式并多一个Job来处理两Job的交集问题是更好的方法
