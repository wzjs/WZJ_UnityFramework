# Unity 笔记

## Asset
1. 用BuildAssetBundles的方式去打ab包时,每个资源都会去找相关依赖的资源是否有ab名，如果有的话会直接把依赖资源打成ab包，及时你并没有去打这个依赖资源。
如果这个依赖资源并没有相关ab信息的话就会直接把这个依赖打进目标ab包中。

## 行为树&状态机
1. 行为树:由控制节点,行为节点,前提条件组成,控制节点主要是用来做决策行为,行为节点是决策的结果,控制节点的决策有选择,序列,并行三种方式,如果前提条件满足则转移。
状态机:由状态,条件,转移,动作组成。
两者区别: 行为树能够处理更加复杂的情况,状态机在复杂情况下难以维护。比如n个状态 状态机就最多就有n*n中状态转移,我很难知道我为什么在这个状态即状态的上下文会变得难以追溯。
1.行为树的灵活性比较强,状态机无法同时处理两个状态而行为树可以。 但在少数状态时,状态机还易于维护,就显得比较方便,而行为树就稍显繁琐,因为每次都需要增加控制节点
2.行为树可以简单的对当前状态做出各种反应,但是状态机需要花费大量状态和状态转移

## UI框架 
1. 设计了一套完备的UI框架 采用MVC框架 纯lua实现,包含各种生命周期函数 栈实现 命令队列处理ui消息 存储 销毁 框架内几乎所有操作不产生Gc 除了输入产生的数据包,并且支持同时开启多个UI ,能够对控制器进行存储
并且所有逻辑都写在已经定义好的生命周期内,比如一开始需要进行网络请求,结束后请求资源 ,然后设置面板数据等等,并且定义好gameObject后自动生成lua文件,开发者一般只需要在特定的方法内写逻辑即可,效率非常高 PSDImport 然后通过进场入场函数来配置动画资源

2. 进场动画会导致很多位置偏移,很多地方会出现问题 项目坑

## 批处理
1.  批处理一般用于静态物体，只需要进行一次合并然后把数据提交给GPU就结束了，但是动态物体每帧都需要渲染，所以每帧都需要提交drawcall给GPU就导致了每帧CPU都需要大量的运算合并
一次drawcall,GPU会使用相同的渲染状态来绘制，所以批处理的物体必须是相同的渲染状态

2. Static Batch   Dynamic Batch  GPU Instance SRP Batcher : 
共享相同的材质
Dynamic Batch:
1，900个顶点以下的模型。
2，如果我们使用了顶点坐标，法线，UV，那么就只能最多300个顶点。
3，如果我们使用了UV0，UV1，和切线，又更少了，只能最多150个顶点。
4，如果两个模型缩放大小不同，不能被合批的，即模型之间的缩放必须一致。
5，合并网格的材质球的实例必须相同。即材质球属性不能被区分对待，材质球对象实例必须是同一个。
6，如果他们有Lightmap数据，必须相同的才有机会合批。
7，使用多个pass的Shader是绝对不会被合批。因为Multi-pass Shader通常会导致一个物体要连续绘制多次，并切换渲染状态。这会打破其跟其他物体进行Dynamic batching的机会。
8，延迟渲染是无法被合批。
Static Batch:设置了静态合批选项后会在打包时将所有静态合批对象的Mesh合成一个Mesh保存起来,但必须使用相同材质
GPU Instance:必须使用相同Mesh和Material,原理只不过是使用不用的Per-Instance data来处理不同材质,需要使用特殊方法来标记,并不是进行合批操作,不过确实能减少drawcall。
GPU Instance原理:CPU 只需要把Mesh 和 Material 和 一个包含实例数据的数组交给GPU ,GPU就会进行相应处理。这个过程CPU 只需要和GPU进行一次,只需要一个drawcall。
UNITY_INSTANCING_BUFFER_START(Props)
           UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
UNITY_INSTANCING_BUFFER_END(Props)

UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
SRP Batcher:  不减少drawcall,但是会缩短drawcall之间的提交效率, 以相同shader为前提,在CPU层提交绑定CBUFF和渲染数据和材质,后续如果材质不发生改变就不会提交drawcall

## 红点系统
1. 游戏中的红点系统
实现内容
1.所有红点动态生成，数据改变能改及时刷新当前显示中的红点
思路一.
通过注册所有红点事件 参数1 生成红点的父物体 参数2 是否生成红点的func  不显示时移除注册
刷新红点的方式1. 数据改变通过广播的方式来刷新 但是得处理每一条红点数据  方式2.通过update每帧刷新当前注册且显示的红点

# Material

1. 只要调用过render.material,那材质球就会替换成material instance 在编辑器中的话就会丢失
2. Render SetPropertyBlock  在项目中多次遇到bug都是因为这个方法会重新设置shader的属性 想要增加改动必须先获取到PropertyBlock
在该对象上进行增加删除


# NGUI 

1. NGUI Panel >>>>>> 简单看了一下NGUI的渲染,最主要的类是一个UIDrawCall,继承MonoBehaviour,其实是一个游戏物体,只不过
被隐藏了,当初觉得所有继承widget的组件其实很奇怪因为他们没有材质没有Render那怎么能渲染出东西呢,其实渲染的工作
都在DrawCall上,Panel会对所有他底下的Widget进行depth升序,然后进行循环遍历如果连续的两个widget的shader material texture
相同,那么会合并mesh,这个工作交给UIDrawCall完成.

# 性能
1. 闪退的几种原因:
1.底层报错,比如第三方C++库
2.资源问题导致unity底层异常的闪退,这个问题就非常广泛,之前遇到过资源名见有空格会导致偶发闪退
3.unity自身bug导致
Unity相关测试方法：UnityEngine.Diagnostics.Utils.ForceCrash
调试真机闪退的方式 如果development勾选的话 包中会自带调试信息 ，如果没有勾选的话需要导出symbols调试文件,当遇到奔溃时 通过arm-linux-androideabi-addr2line -f -C -e   /Applications/Unity.app/Content/PlaybackEngines/AndroidPlayer/Variations/mono/Release/Symbols/armeabi-v7a/libunity.sym.so 0043a05c 来查看堆栈信息
4. A* 算法:启发式函数 f(n) = g(n) + h(n) ,g(n) 代表n节点到起点的代价, h(n)代表一个从n节点到终点的最佳路径的代价的估算函数。
1.初始化: 以f(n)为基准初始化一个最小堆或者是优先队列,openset,一开始只有一个节点即起点, 和一个hash字典存储每个节点的g(n),起点为0. 和一个节点之间的一个关系comeFrom,key为目标节点,value为目标节点的父节点
2.核心算法: 循环遍历节点,直到找到终点,或者没有节点。设当前节点为f(n)最小的节点设为A,对周围节点设为B进行如下操作：首先先从openset中移除A保证下一轮循环不会出现。判断从A出发到B的g(n)是否比原来更小,如果是的话就更新B的f(n)和g(n)
然后设置comeFrom[B] = A即A是B指向的路径,最后如果B还没有在openset中,就将其加入。这就是一个循环的流程。
h(n)有两个属性,1.admissable 可接纳性 2.consistent 一致性。 
admissable需要保证估算函数不能高估到达终点的代价,这会导致无法找到最优路径
consistent 假设   N h(N)---S       那么 h(N) <=  c(N,N') + h(N')        设S为终点, h(S)为0 , h(NA) <= C(N,S) + 0 即满足admissable
                c(N,N') |          /
                            |        /
                            |      /
                            |    /
                            N'  h(N')  
refer to https://en.wikipedia.org/wiki/A*_search_algorithm

5. Prime算法:寻找最小生成树即 能连同所有节点并且权值和最小的树, 采用贪心算法 首先随便取一个节点放入容器中,然后寻找与姐容器内节点相邻
的权值最小的边加入到 边容器中(要剔除边的另外一个已经加入到容器中的节点)一直循环该操作,直到所有节点都遍历完毕, 边容器中的就是最小生成树的边

6. 四叉树 八叉树 快速碰撞检测 GPUInstance  
GPUInstanceAnim https://github.com/chenjd/Render-Crowd-Of-Animated-Characters  对动画信息进行采样生成到贴图当中,在GPU中做uv动画

7. 入资源 使用脚本自动设置 对象池 unity自身函数
对象池 资源压缩(贴图 音频 网格 材质) drawcall 
降低分辨率 拆分透明通道 crunch comporession mipmap  lod
内存换CPU  多线程 ECS 

8. 资源优化: Crunch Compression : 优点 减少资源大小 减少硬盘使用空间 提高下载速度  缺点是在编辑器中压缩时间过长。 
在unity 2017版本中增加了可以对ETC ETC2 DXT继续压缩  支持了android 和ios中的使用。 对ETC压缩后的资源进行 Crunch压缩, 解压后就是ETC压缩资源
DXT1:将像素分成4*4的块,每个块包含 两个极端颜色 （每个颜色占16位, 其余颜色通过插值计算）和32位索引,每个像素2位,不包含alpha。
DXT2,DXT3: 在DXT1的基础上增加了64位来表示alpha信息,每个像素4位表示alpha, DXT2 预先乘上了alpha  DXT3则没有预先乘上。预先乘上可以减少透明混合时的额外计算 适用于有明显透明变化
DXT4,DXT5:在DXT1的基础上增加了64位来表示alpha信息,与DXT2 3不同的是 DXT4 5将alpha也分成了两个8位的极值,和3位的索引 通过插值来计算每位alpha, DXT4是预先计算alpha,DXT5没有预先计算 适用于缓慢的透明变化
Ericsson Texture Compression (ETC): 和DXT一样 4*4为一个块 一共64位,  又分成了两个子块 2*4 由flip位来控制是竖直划分还是水平划分。
每个子块有一个3位的修饰表索引代表了8组偏移和一个基本颜色块 (2*R4G4B4 或者 R5G5B5+R3G3B3) 每个像素由有一个选择器数据来选择哪组颜色
两个子块 base color 2*12 + 1diff位(控制子块的颜色类型) + 1flip(控制方向) +  2*3 修饰索引 +  16*2 每个像素两位索引器
Adaptive scalable texture compression (ASTC): 支持不同块大小
PVRTC (PowerVR Texture Compression):由两张缩小4倍的图和一张调制图像进行线性插值
AssetBundle.Unload(flase)是释放AssetBundle文件的内存镜像，不包含Load创建的Asset内存对象。
AssetBundle.Unload(true)是释放那个AssetBundle文件内存镜像和并销毁所有用Load创建的Asset内存对象。

# 源码阅读

1. UGUI 框架源码阅读记录:
事件系统:
PointerEventData 对应鼠标来说 通过延迟创建的方式实例，每个按钮对应一个pointdata,并且每帧都会更新
IPointerExitHandler IPointerEnterHandler 关于这两个接口 进入和退出是指 层级,就是说两者通用的父类到自身之间的所有实现该接口的UI都会调用
一次点击事件的流程：EventSystem类每帧都会驱动输入模块执行,也就是说PointEventData每帧都会刷新数据,比如没有点击时也会刷新位置信息
点击时则会通知EventSystem去进行射线检测,EventSystem再通知Raycaster进行真正的射线检测,第一个检测点就会被记录到PointEvetnData中待使用
PointEventData都填充完毕之后就会处理事件,比如按压 拖拽 移动等,PointEventData是鼠标数据, MouseState 则保存了每个鼠标按键的状态。比如这帧是否进行了点击

UI框架:
pixelsPerUnit ReferencePixelsPer 默认都是100 那么最终Unit：Pixels = 1，如果 pixelperunit = 10 相当于一个单元只有十个像素,那么显示上就大了十倍 通过SetNativeSize可以测试
RectTransform
Rect  左下角为坐标原点  center为中心点
布局系统: ILayoutElement 想要生效必须有一个或者以上的ILayoutController。
LayoutRebuilder:dirty时会将gameobject包装到该类中,然后通过该类进行rebuild,该类有对象池获取,build分为计算布局属性和设置属性部分
计算时先计算子节点再结算父节点以保证结果的正确性
一次自动布局的流程:setdirty()后进入队列等待布局, 在Canvas被渲染前调用,循环rebuild每个ICanvasElement,先计算gameobject的布局属性 (MinHeight PreferHeight FixeibleHeight
该属性由ILayoutElement接口实现,比如Text Min 总是为 0 Prefer 总是等于内容的size,  因此使用ContentSizeFitter 控制器就可以根据内容的大小来自适应控制size)
然后调用 控制器的 SetLayoutHorizontal SetLayoutVertical 来设置 自身或者是child的 属性
ChildControlsSize:控制子节点的大小在MinHeight 和PreferHeight 之间 比如如果父节点的Height足够,那么会用PreferHeight,如果还有多余空间则根据子节点Fixeible的权重来分配剩余空间
ChildForceExpand:将所有子节点均匀占满整个区域  如果ChildControlsSize开启则会拉伸
Cull 和 Mask的区别:Cull 只是用Rect对一个矩阵区域进行裁剪,Mask使用模板测试剔除alpha为0的部分
Cull和Clip 剔除和裁剪都是clipper算出rect后调用CanvasRender接口
Mask 模块测试的实现: 嵌套mask和单mask区别不太一样,嵌套mask要复杂需要. 单mask 只需要 always ref 随便 
嵌套Mask   
Ref WriteMask ReadMask
1        255    255
3        3         1
7        7         3
嵌套Mask  需要先比较模板值通过后 改变每一层的模块值  读取时需要读上一层的模板值, 因为在嵌套Mask里一定会显示子Mask(如果父Mask有模板值)(只有有像素的地方才会有模板值)
Mask DrawCall 合批:
性质1：Mask会在首尾（首=Mask节点，尾=Mask节点下的孩子遍历完后）多出两个drawcall，多个Mask间如果符合合批条件这两个drawcall可以对应合批（mask1 的首 和 mask2 的首合；mask1 的尾 和 mask2 的尾合。首尾不能合）
性质2：计算depth的时候，当遍历到一个Mask的首，把它当做一个不可合批的UI节点看待，但注意可以作为其孩子UI节点的bottomUI。
性质3：Mask内的UI节点和非Mask外的UI节点不能合批，但多个Mask内的UI节点间如果符合合批条件，可以合批。
原理是 不同层级的UI使用了不用版本的模板材质,只要模板信息相同就会使用相同的材质 自然就能合批,Mask外的UI节点之所以不能和里面的合批是因为使用的材质本身就不同
RectMask2D
RectMask2D不需要依赖一个Image组件，其裁剪区域就是它的RectTransform的rect大小.
性质1：RectMask2D节点下的所有孩子都不能与外界UI节点合批且多个RectMask2D之间不能合批。
性质2：计算depth的时候，所有的RectMask2D都按一般UI节点看待，只是它没有CanvasRenderer组件，不能看做任何UI控件的bottomUI。
原理是 Shader中RectClip裁剪区域不同,挂在RectMask2D上的Image能合批是因为没有裁剪区域
ShowMaskGraphic ColorMask 0

# 万向节死锁
1. 万向节死锁:两条轴共面导致,理论: unity中旋转的顺序是(y-x-z),y是随着惯性坐标轴旋转,x和z 都是随着模型坐标轴旋转, 所以当某一个时刻(比如x旋转90°) ,两个轴平行共面,导致失去了一个方向的旋转


# Editor
1. UnityEditor记录：EditorWindow 可以自定义一个窗口, Editor 主要是用来对Inspector面板上的某个组件进行编辑,通常配合CustomEditor特性一起使用
GUI 和 GUILayout : GUI 是固定布局,GUILayout是自动布局  EditorGUI和EditorGUILayout只能使用在编辑器种,无法在游戏中使用

# playable

1. Unity中Playable和Timeline的关系:
Timeline是通过playable来实现的,例如TimelinePlayable  Animator底层也是通过playable实现 查看animtor的源码可知
Playable的核心接口是IPlayable,  实现该接口表明这是一个可以工作的可玩逻辑。 例如一些Unity内置的playable,如AnimationClipPlayable(Animator就是用这个来实现的),AudioClipPlayable(内置Audio实现), 目前公开源码提供了ScriptPlayable<T>的实现来使用自定义IPlayable
,例如timeline中的ScriptPlayable<ParticleControlPlayable>,ScriptPlayable<ActivationMixerPlayable>等,因此我们可以用自定义的ScriptPlayable来实现自己的IPlayable。
ScriptPlayable<T> 接受一个泛型参数 继承自PlayableBehaviour, PlayableBehaviour 实现了各种生命周期函数直接使用。 
另外一个核心的接口叫IPlayableAsset,用来设置IPlayable 所需的一些设置,（例如AudioPlayable需要一个AudioClip等信息） 和创建一个Playable对象到Graph

# 生命周期
1. (FixUpdate FixDeltaTime) & (Update DeltaTime)  
Update:每帧都会调用且只一次该事件，DeltaTime就是从上帧到改帧所经历的时间，由于渲染帧率是浮动的，因此DeltaTime也是不固定的
FixUpdate:FixDeltaTime的时间是固定的，可以通过设置改变，默认是0.02s，根据时间不同，FixUpdate调用的次数也是不固定的，可能每帧调用多次，也可能一帧不调用

2. 注意Awake帧一些初始化设置可能还没结束,尽量不要把一些逻辑相关的代码放到awake中,例如 获取postion 不一定是准确的


# DOTS
## Entity Physics
1. EP其实就是基于ECS库的最佳实践(DOTS)实现了一套确定性的物理系统包括了刚体动力学和空间查询系统

2. 几个特点
    * 无状态(Stateless): 现代物理引擎为了提高性能和保持稳定性引入了很多缓存,同时也使得系统变得复杂甚至到难以修改代码的程度
    * 模块化(Modular): EP的核心代码与ECS和Job解藕,使得这些代码可以重用不止限于ECS,同时也可以摆脱底层框架接入自己的实现
    * 高性能(Highly Performant): 各种特性例如无状态的原生查询使得EP的性能相当于目前主流物理引擎
    * 互操作性(Interoperable): 可接入Havok Physics Integration (HPI)。 代码的模块化使得可以写入用户代码来接入HPI,例如碰撞修改,触发回调
3. 物理模拟的执行顺序
    1. PS(Physics System)从场景中所有?Body Entitiy中获取组件信息。这一步是模拟必须做的步骤，因为PS并不缓存信息,即无状态的。
    2. 开始BoardPhase??(宽域阶段,针对全局,对应下一步的窄域阶段,针对每个entity)，PS获取所有active body的信息检查哪些aabb发生了重叠即碰撞。这一阶段快速获取所有潜在的body,抛弃其他的body。
    3. 开始NarrowPhase,根据上步获取到的碰撞信息，用他们的各自的碰撞体进一步计算精确的碰撞点。
    4. 根据上两步计算得到的信息,返回一个结果,该结果中包含了一些参数，例如碰撞点,双方质量等
    5. PS根据结果solves碰撞体和joints。该步骤会对受影响的body产生新的速度
    6. 上一步结束了碰撞的一系列操作,这一步将所有动态body向前整合到时间中,To do this, the physics system moves the dynamic bodies according to their linear and angular velocities, while taking the current time step into account。
    7. 最后PS应用新的变换到对应的body上

4. 物理模拟前的准备工作以及注意事项
    * 需要为物体赋予碰撞体和刚体两个组件,如果只需要碰撞的话可以不需要增加刚体. 有两种方式  

        1. 使用unity自带的物理系统组件 collider和rigidbody 
        2. Entity Physics实现了自己的 collider和rigidbody 分别叫 Physics Shape 和 Physics Body。 
    * Subscene：在该组件下的场景中会自动将内部的obj转换为entitiy

5. 几个重要的Data Component
    * 第四步的body和(collider or shape)会将自身(术语:authoring)携带的数据baker到component中供不同的系统使用。
    1. PhysicsCollider:  
        * 概念: PS中最重要的一个组件，增加该组件才能参与碰撞和空间查询,该结构中的BlobAssetReference是最重要的一个成员,内部包含了Collider的引用,参与碰撞检测,PS支持各种Collider。
        * Scale: 缩放Transform的scale会影响到collider的大小,即一起缩放类似原生物理系统,另外不同的collider中有不同参数也可以缩放，例如SpereCollider中的radius等,关于不统一缩放,只有简单的类型可以,例如SpereCollider,像Mesh,Vertice是不可以的。
        * Collision filter: 可以设置自身的层和能够检测的层来过滤碰撞
        * Modifying PhysicsCollider: 修改数据有两种方式
            1. EntityManager.GetComponentData :这种方式只是取出数据,改完值需要重新赋值,并且不支持Job API EntityManager.SetComponentData
            2. 支持Job(推荐)


                        using UnityEngine;
                        using Unity.Entities;
                        using Unity.Burst;
                        using Unity.Physics;

                        public partial class ChangeColliderSystem : ISystem
                        {
                            [BurstCompile]
                            public partial struct ChangeColliderJob : IJobEntity
                            {
                                [WithAll(typeof(ChangeColliderFilterJob))]
                                public void Execute(ref PhysicsCollider collider)
                                {
                                    collider.Value.Value.SetCollisionFilter(CollisionFilter.Zero);
                                }
                            }

                            [BurstCompile]
                            public void OnUpdate(ref SystemState state)
                            {
                                state.Dependency = new ChangeColliderJob().Schedule(state.Dependency);
                            }
                        }

        * Dynamic bodies: 该组件事实上不会做任何事情,需增加其他的组件来支持一些功能,例如PhysicsVelocity。顺带一提,像我们在Editor中使用的Physics Body已经为我们定义并且实现好了各种数据和对应的组件

        * Mass: 组件名为Physics Mass, 通常在使用时Physics Body会为我们增加该组件, 如果没有增加该组件那该collider拥有无限质量,赋予一个速度的话会给物体一个作用体
