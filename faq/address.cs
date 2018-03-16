
/// <summary>
/// 曾经在项目中碰到的一个问题，变量没有刷新的原因。。
/// </summary>

public class Man
{
    public Foot foot;
}
/// <summary>
/// 这是一个类
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
        Man t = new Man();  //有一个残疾人
        Foot t1 = new Foot(); //这是一只脚
        t1.length = 3;   //长3米
        t.foot = t1;   //然后把这只脚接给这个残疾人

        Foot t2 = t.foot;  //然后有一个变量去保存这只脚的信息  假设t2的指针指向 0x1111111 这个地址当然就是 t.foot的地址
        Debug.Log(t2.length);  //输出这只脚长3米

        Foot t3 = new Foot();  //这时又有一只脚
        t3.length = 2; //长2米
        t.foot = t3;  //把之前那只脚拆掉装上这只脚    
                      //t.foot 的地址本来是指向 0x1111111 的对吧 ，然后t3是新的脚 他的地址是 0x2222222 那t.foot 就指向0x2222222 
                      //这跟 t2的指向没有半毛钱关系 所以t2还是指向 0x1111111  因为t2他的指针根本就没有被改变
        Debug.Log(t2.length); //那么问 之前那个变量存的是哪只脚
        Foot f = new Foot();
    }
}