#  ShotGrid for Unity ��������
+ ������Ҫ�����¼һ������ShotGrid for Unity(shotgun desktop��1.6->1.8)����������һЩ����,ԭ��,�Լ������ʽ��
Unity��Shotgrid�ļ���ά���Ѿ���������ǰ,�����°�Shotgun�Ѿ��޷�֧������Unity����汾
,��������Ƶ�Unity for python����δ��Դ�Ĵ���,����һЩ��Ŀǰ�޷���������⡣
***

unity�ļ�����Ҫ�����������ļ�Ŀ¼
* com.unity.integrations.shotgrid lets Unity communicate with ShotGrid
* tk-unity provides the engine description for ShotGrid
* tk-config-unity is a sample pipeline config that includes Unity support
---
1. `[ShotgunClient] Exception stack trace:
Traceback (most recent call last):
  File "Z:\FC1608CG_clone\install\app_store\tk-unity\v1.1\plugins\basic\bootstrap.py", line 71, in plugin_startup
    UnityEditor.Integrations.Shotgrid.Bootstrap.OnEngineInitialized()
AttributeError: `
 


+ Shotgrid Unity��Shotgrid�����������,tk-unity����е�bootstrap.py(�汾��ͬ���ֿ��ܲ�ͬ),��������
�ᱨ�������,ԭ����com.unity.integrations.shotgrid�е������ռ�����ShotGrid�ؼ��ֶ�����Shotgrid,
�Ҳ�ȷ���ǲ���Unity������Ա�Ŀ���������Сд�����е��µĻ���ʲô����,�ⶼ��һ���ͼ�����(�������:`UnityEditor.Integrations.Shotgrid.Bootstrap.OnEngineInitialized()`)��


2. `Traceback (most recent call last):
  File "Z:\FC1608CG_clone\install\app_store\tk-unity\v1.1\plugins\basic\bootstrap.py", line 37, in plugin_startup
    if not qt.QtGui.QApplication.instance():
AttributeError: 'NoneType' object has no attribute 'QApplication'`

+ ������Ҫ֪��Shotgrid������ʱʹ�õ�python��������unity���в��ʱ�õ�python������Ĭ���ǲ�ͬ��,
unity��ʹ��python package�е�python.exe,�������ߵ�����������ǲ�ͬ��,unity��Ҫ��shotgrid�Ļ���
���Ƶ�unity��,����sys.path�ĸ��ơ�  
������˵˵��һ������,�������д���򵥴ֱ��Ľ�Python�滻����Python3,�������������shotgrid������
Python3Ŀ¼,����path�����Python33,����޷�������Pyside����·��,���QtGuiΪNoneType���򵥲���ֱ��ע�͵����м��ɡ�
`pysideFilePath=pysideFilePath.replace("Python","Python3").replace("python2.7","python3.7")`

3. ���������������ֻ��������һ������,�Ҳ�����ģ��,�����python�˽ⲻ���Ļ��Ǻ���ȥ�����������ġ�
��ʱ����ȥ��Pyside��,��ȥ��shiboken���ҷ������Ƕ����ڡ�Python�İ��Բ�ͬ�汾��python��������һ������
��Ҫȥ��ѯ���İ汾�����shotgrid��python�汾Ϊ3.9,unity��ʱcom.unity.integrations.shotgrid��python����ֻ����3.6
��˾���Ҫ�ֶ�ȥ������python3.9(python for unity version:[5.0.0-pre.5]),����unity�ֻ���ֺܶ�ı��뱨��,��Ϊpython3.9 for unity�кܶ�API
�Ѿ�������֮ǰ��3.6,������Ҫ�ֶ�ȥ������ص�API�䶯�������������
`shiboken2,ModuleNotFoundError: No module named 'shiboken2.shiboken2`

+ �������ô���������������°��shotgrid�Ĵ󲿷ֹ���,����Unity�е�Publish Recorder��ð汾��Python�������ͻ,�������غ�`PythonEngine.Initialize();`
���ú�ᵼ��Unity���˲��Ҵ���ʲ������crash log,Python.Runtime�������Python�ķ�װAPI,�Լ��ܶ಻��ȫ����,�޷�����
ֱ�ӵ���,������һ���������ȽϷ�ʱ��,���ԵȺ���Unity�ٷ�������������һ��˼·�ǽ�ֹpython��ʼ�����(��ʹ�����ʼ��),
����Unity�����ع��̻������̬�ֶΡ����Կ�����Project Setting->Editor->Enter Play Mode Settings  
����������������(domain reload),ʵ����һ��ȷʵ�ǿ��еġ�






