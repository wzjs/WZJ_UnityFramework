实现上的注意事项  
1. 固定渲染帧,例如固定30ms更新一次

2. UIToolkit 和 GraphView  
其实两者应该分开来讲，但是正好在学GraphView就一起讲一下吧。GraphView其实是UIToolkit的实现,他实现一个相对较完整的树结构?他包含Node,Graph,Edge,Port.并且提供了非常多的扩展来帮助我们实现我们想要的效果(更确切的说是它基于UIToolkit，所以它可以使用UIToolkit中的功能来扩展,另外GrapView中也包含了很多容器（container）来让我们在不同的区域放置不同的组件,例如titleContainer可以放置一些顶部的组件。还有例如我们可以在Node中增加uxml以及uss) 再来说说UIToolkit,他是一套全新的UI系统,有点像网页开发，html对应uxml,css对应uss,javescript对应c#，并且他配备了一套UI的可视化编辑器较UIBuild,我们可以通过UIbuild来做表现，也可以通过代码来做表现。内部所有元素都继承VisualElement,画布中可以增加VisualElement,VisualElement也可以继续增加,一直嵌套,并且还可以继承某个VisualElement继续扩展,像GraphView就是继承了VisualElement基类来实现的,另外例如Node,Edge则是继承了GraphElement(GraphView中所有元素的基类,GraphView又继承了VisualElement)。  
GraphView可以用来做任务编辑器，AI编辑器等，本质上都是去扩展不同业务的Node,做出通俗易懂的界面给策划去编辑,编辑完之后将数据保存下来用来下次能够恢复之前的界面，另外还需要保存数据在Runtime时执行逻辑,这两者数据的保存可根据具体情况可以进行合并。再说说撤销功能,unity的撤销功能是正对对象的,编辑器开发中经常会用到ScriptObject,我们可以去监听ScriptObject对象,当撤销时就会回到ScriptObject的上一个状态然后我们在重新加载编辑器的界面来实现撤销功能 [https://discussions.unity.com/t/undo-redo-in-graph-view/247636]

            It’s an old question, but decided to answer it because it would’ve been useful to know myself.

            You can just use the Unity Undo system, but you have to do a bit more manual work to make it work properly. With Unity’s Undo.RecordObject you can record the state of an object and whenever you press undo, Unity will automatically go back to the previous stored state in the Undo stack.

            When working with custom editor windows and graphviews, you will most likely store all your data in a scriptable object and the state of this scriptable object can be recorded. However when just pressing undo the data in your scriptable object will change, but your graphview will not yet be reloaded.
            So you can listen to Undo.undoRedoPerformed to reload your graphview every time an Undo or Redo is performed.

            You now just have to decide which action you want to make undoable, and before every action you want to undo, call Undo.RecordObject. You can listen to events like graphViewChanged and deleteSelection to be able to make the built-in behaviour undoable


            