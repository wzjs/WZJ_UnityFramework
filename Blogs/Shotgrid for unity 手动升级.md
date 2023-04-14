#  ShotGrid for Unity 技术分析
+ 此文主要是想记录一下升级ShotGrid for Unity(shotgun desktop从1.6->1.8)中所遇到的一些问题,原因,以及解决方式。
Unity对Shotgrid的集成维护已经是在两年前,导致新版Shotgun已经无法支持落后的Unity插件版本
,但由于设计到Unity for python部分未开源的代码,导致一些我目前无法解决的问题。
***

unity的集成主要是下面三种文件目录
* com.unity.integrations.shotgrid lets Unity communicate with ShotGrid
* tk-unity provides the engine description for ShotGrid
* tk-config-unity is a sample pipeline config that includes Unity support
---
1. `[ShotgunClient] Exception stack trace:
Traceback (most recent call last):
  File "Z:\FC1608CG_clone\install\app_store\tk-unity\v1.1\plugins\basic\bootstrap.py", line 71, in plugin_startup
    UnityEditor.Integrations.Shotgrid.Bootstrap.OnEngineInitialized()
AttributeError: `
 


+ Shotgrid Unity的Shotgrid插件代码问题,tk-unity插件中的bootstrap.py(版本不同名字可能不同),首先运行
会报这个错误,原因是com.unity.integrations.shotgrid中的命名空间中是ShotGrid关键字而不是Shotgrid,
我不确定是不是Unity开发人员的开发环境大小写不敏感导致的还是什么问题,这都是一个低级错误(具体代码:`UnityEditor.Integrations.Shotgrid.Bootstrap.OnEngineInitialized()`)。


2. `Traceback (most recent call last):
  File "Z:\FC1608CG_clone\install\app_store\tk-unity\v1.1\plugins\basic\bootstrap.py", line 37, in plugin_startup
    if not qt.QtGui.QApplication.instance():
AttributeError: 'NoneType' object has no attribute 'QApplication'`

+ 首先需要知道Shotgrid在运行时使用的python解释器和unity运行插件时用的python解释器默认是不同的,
unity会使用python package中的python.exe,所以两者的虚拟机环境是不同的,unity需要将shotgrid的环境
复制到unity中,例如sys.path的复制。  
现在来说说第一个问题,下面这行代码简单粗暴的将Python替换成了Python3,但由于升级后的shotgrid增加了
Python3目录,导致path变成了Python33,因此无法解析到Pyside包的路径,因此QtGui为NoneType。简单操作直接注释掉这行即可。
`pysideFilePath=pysideFilePath.replace("Python","Python3").replace("python2.7","python3.7")`

3. 解决了上面的问题又会出现另外一个问题,找不到该模块,如果对python了解不够的话是很难去想象这个问题的。
当时我先去找Pyside包,再去找shiboken包我发现他们都存在。Python的包对不同版本的python解释器不一定兼容
需要去查询包的版本情况。shotgrid的python版本为3.9,unity当时com.unity.integrations.shotgrid的python依赖只到了3.6
因此就需要手动去升级到python3.9(python for unity version:[5.0.0-pre.5]),但是unity又会出现很多的编译报错,因为python3.9 for unity中很多API
已经不兼容之前的3.6,我们需要手动去查找相关的API变动情况并且修正。
`shiboken2,ModuleNotFoundError: No module named 'shiboken2.shiboken2`

+ 解决了这么多的问题兼容了最新版的shotgrid的大部分功能,但是Unity中的Publish Recorder与该版本的Python会产生冲突,在域重载后`PythonEngine.Initialize();`
调用后会导致Unity闪退并且大概率不会产生crash log,Python.Runtime库包含对Python的封装API,以及很多不安全代码,无法进行
直接调试,所以这一块解决起来比较费时间,可以等后续Unity官方的升级。另外一种思路是禁止python初始化多次(即使是真初始化),
但是Unity域重载过程会清除静态字段。可以考虑在Project Setting->Editor->Enter Play Mode Settings  
开启后会禁用域重载(domain reload),实践了一下确实是可行的。






