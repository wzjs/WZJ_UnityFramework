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

### JobSystem
 * 用于并行处理System中的逻辑
 * IJobEntity vs IJobChunk
---
* Authoring: 通常类名带XXXAuthoring表示为一个Monobehaviour,在Unity层记录组件的数据,作为参数传递给Baker进行使用(组件和Entity)
* Baker: 我的理解是Baker将来自Authoring中的数据进行进行烘培(Entity和组件的一些操作,例如在entity上增加组件,修改等),并且Authoring数据改变时会重新烘培,以保证ECS上的数据是实时性,正确性的.
* Archetypes:  一组组件的集合,它是唯一的,例如 CompA + CompB 组成一个Archetype, CompA + CompC 又是另外一个Archetypes, CompB + CompC 又是另外一个Archetypes.
* Archetype chunks： 其实就是一个唯一Archetype中所包含的数据:每个组件拥有一个数组 + entity数组。 例如CompA + CompB的组成部分就是compA[],compB[],entityid[]

* World: 进入PlayMode后,会创建World实例,并初始化所有System和EntityManager,就相当于是一个入口实例
  
