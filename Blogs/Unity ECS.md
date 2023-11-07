# ECS

## Entity

---
## Component
* 用于传递entity的组件数据给系统提供逻辑处理
* Unmanager Component vs manager Component  
  1. 前者支持Burst Compile
  2. 后者需要走GC机制
  3. 前者支持IJob,后置不支持
* 
---

### System
* 概念: 提供了能够将组件数据从当前状态转换到下一个状态的逻辑
* ISystem vs SystemBase  
  1. Burst Compile : Yes No
  2. Unmanager memory allocated : Yes No
  3. GC allocated : No Yes
  4. Can store managed data directly in system type : No Yes
  5. Idiomatic foreach(习惯用法) : Yes Yes
  6. Entities.ForEach : No Yes
  7. https://docs.unity3d.com/Packages/com.unity.entities@1.1/manual/systems-comparison.html

* 关于数据的存储和管理: 数据应该存储在组件中,而不是S系统中
    1. 数据存储在系统中会与系统耦合,不利于面向数据编程
    2. 无法保证访问数据时的线程或生命周期安全性

* SystemGroup:
    1. 概念: 字面意思,存放系统的组,Update时就是SystemGroup下发到System进行Update,另外注意SystemGroup不仅可以容纳System,也可以嵌套SystemGroup
    2. 系统的创建顺序: 系统的创建不遵循SystemGroup,而是通过属性[CreateBefore]和[CreateAfter]。系统的创造方式有1.通过World帮我们创建 2.手动创建World.GetOrCreateSystem.
    3. 系统的销毁顺序: 遵循后创建的System先销毁
    4. 系统的更新顺序: 可以通过属性[UpdateBefore]和[UpdateAfter]来修改更新顺序,另外需要注意的是更新只会更新子节点,像是SystemGroup的话它的节点是不受影响的。还有属性[OrderLast]和[OrderFirst]可以移动更新至最前和最后
    5. 默认存在的SystemGroup: 以下三个是ECS的三个根Group,其余所有的SystemGroup和System都是他们的子节点,我们可以自己选择UpdateInGroup到哪个Group中
        * InitializationSystemGroup
        * SimulationSystemGroup
        * PresentationSystemGroup
    6. 手动创建System: 设置[DisableAutoCreation]来禁用自动创建,然后通过World.CreateSystem创建。需要注意手动创建的系统需要手动增加到Group,通过AddSystemToUpdateList方法
    7. Multiple World: 详见ICustomBootstrap

* Working with System
    1. Access data: 其实用的都是SystemState数据结构,只不过SystemBase和SyetemAPI进行了封装。但是只有SystemAPI对数据进行了缓存,后续代码生成时可能会优化成如下:

            /// SystemAPI call
            SystemAPI.QueryBuilder().WithAll<HealthData>().Build();

            /// ECS compiles it like so:
            EntityQuery query;
            public void OnCreate(ref SystemState state){
                query = new EntityQueryBuilder(state.WorldUpdateAllocator).WithAll<HealthData>().Build(ref state);
            }

            public void OnUpdate(ref SystemState state){
                query;
            }

        * SystemState
        * SystemBase
        * SystemAPI
    2. SystemAPI
    3. Schedule data change
      * Way to Schedule data change: ECB是将修改先记录下来,然后在主线程调用Playback去执行,可以做到延迟性和多次生效修改以及执行多个data change更加高效,一般用在Job中。EntityManager是在主线程中使用,立即生效.常见的结构性修改有比如CreateEntity,DestroyEntity,SetComponent等。data change会导致产生一个Sync point,使用ECB可以使多个data change合并在一起只产生一个Sync point
        1. Entity command buffer (ECB): 其实就是调用每条指令时(例如CreateEntity)将指令加入缓冲区,只有在playback调用后才会生效指令。需要注意CreateEntity和Instantiate创建的entity是临时的(注意: 此时方法返回的Entity只是一个Placeholder,并不是真正的Entity),但是是可以给临时entitiy增加组件和temp entitiy引用,但需要是同一个ecb下创建的entity,如果引用来自另外一个ecb,则会抛出异常。
        2. EntitiyManager (只能在main thread使用,其实ECB也是一样,他只是在Job中缓存)
        3. ECB Playback
           1. ECB的命令分散在多个并行job中不能保证在每个命令在ECB中的顺序,但你可以控制Playback时的顺序,通过在方法中加入sortkey,详见EntityCommandBuffer.ParallelWriter.
           2. 如果需要多次调用Playback,需要在创建时传递PlaybackPolicy.MultiPlayback
           3. 可以使用EntityCommandBufferSystem来处理data change的创建播放和销毁。ECS自身默认都多个ECBSystem,当然我们也可以创建自己的ECBSystem。在每次ECBSystem更新时
              1. 完成所有已注册的Job,确保所有Job都已完成命令的记录。
              2. Playback所有通过该ECBSystem创建的ECB
              3. 销毁所有ECB
   1. Iterate over data : 使用job来遍历可以高效的利用可用核心以及数据缓存
      1. Entity.Foreach : 编译期间会进一步生成代码,内部会获取lambda中传入的组件参数,生成相应的entity query,然后在job中对entity进行遍历,但编译效率要比SystemAPI.Query和IJobEntity满了四倍。
           * lambda的参数顺序必须是nomodify->ref->in,另外所有组件都必须要包含修饰符,不然传递的组件是通过复制的方式传递,Unity在该参数的生命周期结束之后会默默丢弃.如果没有修改组件,尽量使用in修饰符,这样效率会更高。 
           * Foreach的lambda可以自定义委托来实现更多的参数
           * lambda的参数中除了我们自己定义的组件类型外,ECS还提供了几个类型参数
             1. Entity entity, 只要类型是Entity,名字可以随意取
             2. int entityInQueryIndex,命名必须是这个
             3. int nativeThreadIndex,命名必须是这个
             4. EntityCommands commands,只要类型是EntityCommands,名字可以随意取 (改用EntityCommandBuffer? EntityCommand似乎被移除了),必须配合WithDeferredPlaybackSystem<T>和WithImmediatePlayback一起使用
           * Capture variables: 在lambda中只能捕获原生容器类型(native container)和blitable types(内存布局上与unmanage一致)。但是只有native container才能写入数据(即修改后能在主线程获取到,或者使用run(本身就在主线程所以没关系)),另外如果要返回一个int值(随便举个),就创建native array,长度设置为1。想要dispose掉捕获的类型,调用WithDisposeOnCompletion(variable)。默认形况下run()在lambda运行完之后立即释放,Schedule和ScheduleParallel会在job之后处理
           * WithAll(),WithAny(),WithNone()可以用来过滤筛选entity
           * WithStoreEntityQueryInField(ref query)因为Foreach在编译时提前生成好了query,我们只需要通过该方法就可以获取到query了
           * Foreach无法访问optional components
           * WithChangeFilter: 只有当该组件发生变化时才会更新。这个变化是指Archetype chunk层面只要有代码访问到可写的组件,那就设定为发生变化
      2. IJobEntity: 其实用的还是IJobChunk,只是封装了一层使步骤简化,最后代码生成器会生成IJobChunk的代码
      3. IJobChunk —————————————————————————
      ？？？？？？？？？？？
        * 手动遍历(manually iterate over data)
      4. Run():在主线程使用 vs Schedule() vs ScheduleParalled()
        * support features : https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/iterating-data-entities-foreach.html
   2. Query data with a entity query
      1. Create query: 应该尽量用read-only来声明,更加高效,还有例如 WithAny() WithNone()等 

              EntityQuery query = new EntityQueryBuilder(Allocator.Temp)
              .WithAllRW<ObjectRotation>()
              .WithAll<ObjectRotationSpeed>()
              .Build(this);
      
      2. Combine queries:
          ```
          EntityQuery query = new EntityQueryBuilder(Allocator.Temp)
          .WithAllRW<ObjectRotation>()
          // Start a new query description
          .AddAdditionalQuery()
          .WithAllRW<ObjectRotationSpeed>()
          .Build(this);
          ```
      3. Execute the query: 通过schedule job或者转换成数组
      4. Filter: 有以下几种
          1. Shared component filter:
            ```
            query.SetSharedComponentFilter(new SharedGrouping { Group = 1 });
            ```
          2. Change filter
          3. Enableable components
      5. Version number 用于检测是否有新的潜在Change发生
   3. Loopup the data
      1. 在System中: Entities.Foreach或者Job.WithCode中直接使用GetComponent<T>(Entity)或者SystemAPI.GetComponent<T>(Entity). 如果要访问Dynamic buffer中的数据,需要在外部声明好类似BufferLoopup<TestData>,然后在Lambda中捕获
      2. 在Job中: ComponentLoopup和BufferLookup
   4. Time： FixedStepSimulationSystemGroup
1.  ECS工作流
    1. Bake
        1.  Baking: 将Unity的Obj转换成Entity数据,写入entity场景。
            * Authoring and runtime data: Authoring data是在Unity中可以任意编辑存储的数据,例如Monobehaviour。runtime data是ECS运行时处理的数据,这种数据是做过性能优化和存储优化,专门设计为计算机高效处理
            * Baking process: authoring scenes就表示我们目前项目中的拥有subscene组件下的场景资源。 authoring component表示subscene下的obj中baker的组件。转换authoring gameobject到ECS数据叫做baking,baking只会发生在editor,不会在游戏中,就像资源导入一样。当打开subscene时会触发实时烘焙。当关闭subscene时unity会在后台进行异步烘焙而且是全量烘焙
            * 增量烘焙和全量烘焙:全量烘焙是在加载整个实体场景时是在后台子资产导入器中进行的,这个导入器是一个编辑器进程,所以是异步方式进行的。所以可能会导致运行后等了几秒后才会显示烘焙好的实体场景。烘焙后的产生的烘焙文件会存在于硬盘中。增量烘焙是当实体场景中某些改动后会进行烘焙,而且数据是先存储在内存中。增量烘焙主要用在改动比较频繁的时刻.
            * Baking phases: 不同Baker的烘焙顺序时不知道的,所以Baker必须是没有依赖性的,只能控制自己的entitiy
            * Baking System:   在运行完所有Baking System后,unity存储entity数据在实体场景并且序列化到硬盘中,或者在实时烘焙形况下直接反映到ECS中
              1. PreBakingSystemGroup (this executes before the bakers)
              2. TransformBakingSystemGroup
              3. BakingSystemGroup (the default baking system group)
              4. PostBakingSystemGroup
           1. Baker: 将Unity中的component例如Monobehaviour作为泛型参数传给Baker,然后再Baker中进行Baking。Baker只会实例化一次,但是会随着数据的改变而调用多次在一个不确定顺序里。数据的改变例如本来是一个Cube,后来修改后变成一个Sphere,~~或者材质上发生了改变等等即可能包含的一些依赖都会导致rebaking~~对于单个System来说,只有Authoring中值的改变才会触发rebaker,如果是个引用类型中的数据发生变化是不会触发的,因为引用本身没有变,因此需要通过dependson来增加依赖
           2. Baking System: 于Baker不同的是 BS能够批量处理Entity数据,而Baker是从托管的创作数据中读取信息并逐个处理组件.但是在烘焙系统中创建的实体不会出现在实体场景中。使用[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]来表示是一个Baking System
           3. Baker World: 在Conversion烘焙完之后会与Shadow World中的副本进行比较,获取差异的数据然后复制到主要世界,并更新Shadow World以匹配当前的Conversion World。举个例子,例如宝箱的transformAuthor修改了其位置,那么Conversion World触发烘焙,结束之后与Shadow World中先前的烘焙数据进行比较,将差异的数据复制到主要世界中。
             * Shadow World:包含之前烘焙的输出,Unity用这份数据来比较与上次烘焙的改变
             * Conversion World: 即Baking和Baking System发生的世界,
          1. Filter baking output
             1. 增加该BakingOnlyEntity组件的Entity不会被存储在ECS中。
             2. 或者使用BakingType和TemporaryBakingType标记到组件上来过滤,两者区别我的想法是BakingType会保留到所有Baker结束之前,~~TemporaryBakingType只会在单个Baker之后就销毁~~官方的说法是TemporaryBakingType只会存在在相同的Bake pass中
          2.  Prefab in Baking: Prefab Entity是拥有两个组件的Entity,1是Prefab:IComponent 2是LinkedEntityGroup:IComponent,存储着所有子节点信息。必须使用资源文件的prefab才是prefab entitiy,如果引用场景中的prefab,那么对待为一个普通obj
              1. Create a entity prefab: 

                      var entityPrefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic);
              2. Instantiate prefabs: 使用EntityManager或者ECB都行
              3. 注意Prefab在查询中必须增加
                  ```
                  WithOptions(EntityQueryOptions.IncludePrefab)
                  ```
              4. Destroy prefab: EntityManager or ECS
      1. Scenes overview
         * Authoring scenes: 是用来baker的场景,会执行场景里的Baker即将Unity的Monobehaviour组件数据转换成ECS数据
         * Entity scenes: 即Baker产生的数据都会在该场景
         * Subscenes: 在Unity中是一个组件,用来引用场景,例如authoring scene。当打开该组件时1.会在场景层级窗口展示所有该场景下的obj 2.一个初始化的baking pass会运行在所有authoring组件上 3. 所有在authoring components上的改动都会触发一个增量baking pass。 注意当关闭该组件时,Play需要几帧的时间才能使subscene可用.
         *  Scene streaming: 
            *  概念: 所有subscene场景的载入都是异步的,这被称为streaming
            *  好处: 
               1. 应用仍然可以相应,当在载入场景时。 
               2. 场景可以做到无缝切换加载,即使超过内存容量也不会中断(闪退?)
               3. 在运行时如果实体场景文件丢失或者过失,unity会根据需要转换场景.实体场景的烘焙和加载是异步的,所以编辑器还能保证响应
         * Scene Load: 通过Subscene组件或者是API``SceneSystem。LoadSceneAsync``,参数要求是一个EntitySceneReference,或者是GUID,或者是一个scene meta Entity?
         * Scene section: Unity组织一个场景中的所有entities到section中,默认在section0中.每个entitiy都有一个shared component记录该entity在哪个section以及在哪个scene中,scene中GUID表示。通过SceneSectionComponent(是一个mono)可以修改所在的section index,或者通过custom baking system来设置。
         一个场景中的多个section可以分开进行载入和卸载,但index0必须先载入或者是最后再卸载
         * Scene and section meta entity: 烘焙一个authoring scene会产生entity scene文件,每个场景文件的头包含了
             1. list of sections
             2. list of assetbundle
             3. optional custom metadata
          sections和assetbundle决定了一个场景的加载,custom metadata可以自定义放入一些数据用于后续使用。  
          加载场景时会为每个scene和section创建meta entity,meta entity可以用来控制streaming。场景载入后可以使用获取scene entity ResolvedSectionEntity的组件获取section meta entity。另外可以在beking system中为meta entity增加数据组件,需使用SerializeUtility.GetSceneSectionEntity 获取section entity
         * Scene instance: 创建多个相同的场景实例,
            > SceneSystem.LoadSceneAsync with the flag SceneLoadFlags.NewInstance
            
            但是实际上scene和sction meta entity不会发生变化。所以如果想要做出不同的变化可以再ProcessAfterLoadGroup中,(注意:load section是在另外一个世界执行的叫做streaming world,load完毕之后才会把数据移动到主世界。),这个group执行发生在load完毕,转移之前,所以可以在这个地方增加system来做出区别,例如增加位置信息.用PostLoadCommandBuffer来存储信息,这是一个组件,里面包含了一个普通ecb,我们可以在这里增加我们的组件信息。当section在加载时会检查这个ecb去执行,然后我们可以在ProcessAfterLoadGroup标记的System中去处理该组件。

        
                
### JobSystem
 * ~~用于并行处理System中的逻辑~~ JobSystem是Unity的核心模块,可以独立于ECS使用,这里会有一个误区,一直以为JobSystem一定得配合System来使用,其实不是的。
 * Dependency: Job的依赖是指组件读写上的依赖,例如A系统先读取X组件,后来B系统写入X组件,那么B系统就依赖于A系统,反之亦然.在ECS中,System下的Dependency就是帮助我们分析并完成合并了这个依赖关系的顺序，当然我们也可以传入一个JobHandle自己来处理依赖
 * IJobEntity vs IJobChunk: 前者只需要在方法参数中指定好组件类型,ECS会帮我们去quary到所有合适的entity进行调用并且帮我们开始job,IJobChunk需要自己去获取组件相应的entities query,然后我们自己手动去进行设置Dependency
---

* Transform in Entities:Transform和Unity的Transform类似,都是一个层次性的结构,每个Entity下都有Child和Parent结构,顶层的Entity没有Parent称之为root,需要注意的是在设置Entity的父子关系时需要遵循从下到上的原则即需要使用Parent来设置,而不是设置Child.在使用LocalTransform时需要需要所有的API都不会改变组件的数据,而是会生成一个拷贝,需要手动去赋值 例如``myTransform = myTransform.RotateZ(someAngle);``
  * 相关的组件有
    * LocalToWorld: 用于本地坐标到世界坐标的转换,由LocalToWorldSystem使用,用于几何图形的渲染(RenderSystem)
    * LocalTransform: 即控制Positon,Rotate,Scale,默认相对父节点
    * PostTransformMatrix: 用于不统一的缩放,因为LocalTransform的缩放只能是统一的,具体用法暂时不清
    * Parent: 该Entity的父Entity
    * Child: 该Entity的子Entity
  * 相关的系统有:
    * ParentSystem: 维护了Child Buffer,类似Child的管理类
    * LocalToWorldSystem: 用于更新LocalToWorld组件
  * Entity Transforms 对比 Unity Transform见[https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/transforms-comparison.html]

* Authoring: 通常类名带XXXAuthoring表示为一个Monobehaviour,在Unity层记录组件的数据,作为参数传递给Baker进行使用(组件和Entity)
* Baker: 我的理解是Baker将来自Authoring中的数据进行进行烘培(Entity和组件的一些操作,例如在entity上增加组件,修改等),并且Authoring数据改变时会重新烘培,以保证ECS上的数据是实时性,正确性的.
* Archetypes:  一组组件的集合,它是唯一的,例如 CompA + CompB 组成一个Archetype, CompA + CompC 又是另外一个Archetypes, CompB + CompC 又是另外一个Archetypes. 注意在一组Archetypes中的entitiy组件一定是一致的,不可能出现EntityA拥有CompA+CompB,EntityB拥有CompA+CompB,又包含了CompC。与此有关的是在查询(query)CompA+CompB时,他会包含所有拥有CompA+CompB的组件,包括CompA+CompB+CompC
  
* Archetype chunks： 其实就是一个唯一Archetype中所包含的数据:每个组件拥有一个数组 + entity数组。 例如CompA + CompB的组成部分就是compA[],compB[],entityid[]

* World: 进入PlayMode后,会创建World实例,并初始化所有System和EntityManager,就相当于是一个入口实例
  
* Strutural changes change: 暂时放
* Safety In Entity: ECS为了增强性能,很多地方使用了原生指针以及不安全代码,即使框架层面在Editor下会抛出安全信息(runtime build不能保证),使用不当可能会造成安全问题
