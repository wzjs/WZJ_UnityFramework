
/// <summary>
/// ��������Ŀ��������һ�����⣬����û��ˢ�µ�ԭ�򡣡�
/// </summary>

public class Man
{
    public Foot foot;
}
/// <summary>
/// ����һ����
/// </summary>
public class Foot : Man
{
    public int length;
}

public abstract class test
{
    int leng;
}
public class Spr : MonoBehaviour
{

    private void Awake()
    {
        Man t = new Man();  //��һ���м���
        Foot t1 = new Foot(); //����һֻ��
        t1.length = 3;   //��3��
        t.foot = t1;   //Ȼ�����ֻ�ŽӸ�����м���

        Foot t2 = t.foot;  //Ȼ����һ������ȥ������ֻ�ŵ���Ϣ  ����t2��ָ��ָ�� 0x1111111 �����ַ��Ȼ���� t.foot�ĵ�ַ
        Debug.Log(t2.length);  //�����ֻ�ų�3��

        Foot t3 = new Foot();  //��ʱ����һֻ��
        t3.length = 2; //��2��
        t.foot = t3;  //��֮ǰ��ֻ�Ų��װ����ֻ��    
                      //t.foot �ĵ�ַ������ָ�� 0x1111111 �Ķ԰� ��Ȼ��t3���µĽ� ���ĵ�ַ�� 0x2222222 ��t.foot ��ָ��0x2222222 
                      //��� t2��ָ��û�а�ëǮ��ϵ ����t2����ָ�� 0x1111111  ��Ϊt2����ָ�������û�б��ı�
        Debug.Log(t2.length); //��ô�� ֮ǰ�Ǹ������������ֻ��
        Foot f = new Foot();
    }
}