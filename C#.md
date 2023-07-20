# C# 随便写写

## GC
1. C# GC是基于“代”的概念，C#的GC会在代满的时候触发GC，并将最后幸存下来的对象放置到上一代，久而久之第2代（祖先）的对象会更加稳定。
遍历所有堆栈中的局部变量，方法参数，静态变量是否引用了对象，是的话就把同步位索引设置成0

## 协程
1. 有栈协程和无栈协商的区别是能否挂起一个嵌套函数。
2. C#协程：
  public  static IEnumerable CorInternal()
{
	yield return 1;
}
协程其实是一个迭代器,调用CorInternal其实会生成一个迭代器的实例对象,C#会根据方法实现生成不同代码的迭代器主要是MoveNext(),
每执行一次yield return其实就是执行了MoveNext(),这个迭代器内部会用一个int保存当前的执行状态,根据这个int来直接跳转到指定的代码
Unity的协程是如何实现的？   我们无法修改C#为我们自动生成的MoveNext,但是我们可以封装一层在获取到Current的时候,通过这个Current(准确来说应该是上一个Current)
下一个MoveNext能不能执行。 准确来讲应该是调用MoveNext去判断上一个Current是否允许执行MoveNext

## .net mono .net core .net Framework
1. .net 和mono ： mon是基于.net开源库开发的一个跨平台的实现方案,主要由社群和xamarin维护  .net无法跨平台,.net core 可以
.Net vs .Net Framework   目前.Net就是指.Net Core .Net Core 支持跨平台,  .Net Framework 只能在Windows上开发

## lua

1. C# 调用 Lua 实际上是 C#调用lua的C API 去和Lua交互
Lua 调用 C# 
2. 总计一下为什么C调用Lua这么简单，Lua调用C相对就比较麻烦。
Lua是一个嵌入式语言，他可以很容易的嵌入到其他语言中（宿主语言）它是由C语言开发，因此它可以很容易得被C调用,它包含了许多C函数。
Lua调用C：想象一下C是一个大圈，Lua是一个在大圈中的小圈，Lua想要调用他的宿主那必然得经过许可登记。Lua中没有关于C的信息，简单说,C中有Lua虚拟机，因此可以虚拟机这个桥梁来控制Lua。
Lua调用C/C#的两个重点是1.C/C#得把Lua虚拟栈中的参数转换成方法需要的类型 2.如果有返回值，要把返回值压回栈中。
在Lua中调用的C#对象的不管是字段，属性还是方法在虚拟机中都是有登记的，都对应一个方法，不过没有登记那么就访问不到
所有Lua类型参数传到C#中都会经过转换,具体规则可以自己定制
Lua调用C#方法都是通过函数指针为依据
tolua同时能打印lua层和C#层堆栈其实就是在C#层调用了一个结合Lua和C#堆栈信息的字符串的方法，lua中通过debug.traceback打印堆栈信息，C#通过StackTrace打印堆栈信息
tolua会对调用GetFunction的方法进行弱引用缓存,因此项目中改该方法的代码会导致失效

## protobuf 
1. protocolbuf 实现 编译器 proto 
pbc 需要准备 pb文件（由proto转换而来）首先需要把pb字节都注册到pbc中 前后端公用的协议ID 分别对应各个协议名  数据结构  四字节 数据大小  四字节 协议id  xxx字节 主要内容
客户端获取到协议ID 然后知道对应的协议再用pbc来解析数据
C#的protobuf解析分析：在将proto编译成cs文件时，编译器会直接把固定的序列化反序列硬编码到方法中，通过tag来区分不同的字段

## 语法

1. C#Lambda的一些理解 ： 使用lambda一般会生成一个嵌套类 用来存储方法和外部变量 然后将方法指针返回给委托
引用了外部变量的lambda和没有引用外部变量的lambda会生成不同的嵌套类(没有外部变量直接静态构造函数中实例化详细见IL)，外部变量会被嵌套类记录，查看IL发现
在一个方法中局部变量在IL中都是通过嵌套类的变量来使用的,因此当改变局部变量时在IL中就是改变了嵌套类中的变量
在同一个方法中 所有lambda(有外部变量和无外部变量)的嵌套类对象都是共享的。
   int bc = 1;
   Action act = () => { Console.WriteLine("222" + bc); bc = 2; };
   Action<int, int> act1 = (a, b) => {Console.WriteLine("222" + bc); };
结果为: 2221     2222 
当Lambda语句相同时嵌套类只会有一个对应的函数,Action委托中都只是存了相同的指针
一个很经典的例子：  
	Action[] b = new Action[5];
	for (int i = 0; i < 5; i++)
	{
		b[i] = () => { Console.WriteLine("111" + i); };
	}
结果都为 1114 因为他们共享了相同的对象和指针 所以i都是4,一种高级的用法就是在语句中改变变量的值.
有一个很经典的例子：
	Action[] b = new Action[5];
	for (int i = 0; i < 5; i++)
	{
		int index = i;
		b[i] = () => { Console.WriteLine("111" + index);  };
	}
结果为 1110,1111,1112,1113,1114
如果是这样写的话 编译器会为每个lambda表达式都构造一个对象并存入每一个index的值
public delegate void MessageGet();  内部解析：MessageGet其实是一个内嵌类 继承Delegate 
MessageGet messageGet;  声明一个MessageGet类型的变量 
messageGet = () => { Console.WriteLine("111111"); }; lambda用自己内嵌类对象和对象的方法指针
作为参数构造了一个MessageGet对象,调用时其实就是通过方法的指针
messageGet()  == messageGet.Invoke()
public event MessageGet name;
当定义事件时,会生成一个私有的委托变量和一个公开的事件类型变量,并且生成两个方法add和remove
事件是对委托的一层封装 
事件是委托类型的成员 委托是类型
事件会为其所在的类中增加remove和add两个方法 对应 -= +=  
事件成员在其class外部不能使用invoke 即调用方法（只能使用+= 或者-= 不能进行其他操作 比如= 赋值操作） 因为委托是私有变量,所以外部无法调用
事件能够增加安全性 暴露的接口少

简单的看了Dictionary的实现：Dictionary有两个核心的变量 一个叫 buckets 用来存储真正的key所对应的下标，
一个叫entries,是一个结构体数组 存储了哈希值 key value next(下一个index) ,当往字典中插入时，key会进行
哈希运算 int num = this.comparer.GetHashCode(key) & 0x7fff_ffff; 
int index = num % this.buckets.Length; 
再用index去buckets用寻找真正的下标代入到entries中如果key正好和所需的key相同就返回，否则继续从next中遍历

2. 使用异步来加载资源时应该极小心当时加载完后的一个上下文状态， 因为不是同步操作,所以状态很有可能会发生改变,应该在使用前做好各种判断.
3. 闭包 ：upvalue实际是局部变量，而局部变量是保存在函数堆栈框架上的，所以只要upvalue还没有离开自己的作用域，它就一直生存在函数堆栈上。
这种情况下，闭包将通过指向堆栈上的upvalue的引用来访问它们，一旦upvalue即将离开自己的作用域，在从堆栈上消除之前，闭包就会为它分配空间并保存当前的值，
以后便可通过指向新分配空间的引用来访问该upvalue。当执行到f1(1979)的n　=　n　+　10时，闭包已经创建了，但是变量n并没有离开作用域，
所以闭包仍然引用堆栈上的n，当return　f2完成时，n即将结束生命，此时闭包便将变量n(已经是1989了)复制到自己管理的空间中以便将来访问。 

4. 我知道值类型为啥不需要类型对象了 因为值类型是密封的，没有多态的概念 直接去查元数据就好了， 引用类型会有不确定的继承关系
5. C++ 指针和引用: 概念上来区别 指针是一个类型,引用只是一个别名。在实现上 引用相当于一个受限的指针,它无法主动进行 *(dereferencing )获取内容和referencing ,并且
必须得声明时就初始化,之后无法改变 int number1 = 1, int number2 = 2 , int& pNumber1 = number1(必须得初始化,否则编译器报错), pNumber1 = &number2 (报错,无法改变)
pNumber1 = number2 (可以,把number2的值赋值给了number1 ,因为pNumber1只不过是number1的一个别名)
.refer to https://www3.ntu.edu.sg/home/ehchua/programming/cpp/cp4_PointerReference.html
C++ C# Const 不同的是 C++ 指针地址所指向的内容无法改变,即 *pNumber1 = 2 是无效的, 但是 pNumber1 = &pNumber2 确实有效的, 引用的话内容本身就是无法改变
如果加了const 则内容和指针地址指向的内容都无法改变。
C++ const 分两种情况,1. const int* pNumber 2. int* const pNumber , 
第一种是无法改变pNumber的指针指向的内容,但是可以改变 pNumber的指向（即 *pNumber = 2 错误 pNumber = &number2 正确）
第二种是无法改变pNumber的指向,但是可以改变pNumber的指针指向的内容(即 *pNumber = 2 正确 pNumber =&number2 错误)
C++ 引用则本身特性就无法改变引用,但是可以改变引用的内容，增加了const,则引用内容和引用都无法改变。
 C#的引用只针对变量所存储的地址是无法改变的,但是却可以改变地址指向的对象的内容
 6. class son : father
virtul eat   void run
father= new son()  eat()  run()   首先判断father变量的类型 如果方法是虚函数 则执行实际类型的函数,如果没有就往上代回溯。如果方法不是虚函数,则执行变量类型的函数,如果没有就向上代回溯
7. 协变(covariance)和逆变(contravariance): 是指类型构造器构造出的多个复杂类型之间的父子关系。 
例如数组 car[] 和 animal[] ，泛型 委托和接口Action<Parent> 和 Action<Son> 。
协变是保持了子类型和基类型的关系 子类型<=基类型 
逆变是逆转了子类型序关系
单分派：通过一个宗量来决定所调用的方法
多分派：通过多个宗量来决定所调用的方法
动态分派：在运行期间来决定方法。 
动态单分派 如面向对象语言中的重写方法 会根据实际类型来选择方法(只通过实际类型来确定 所以是单分派)
动态多分派 比较少见,方法的选择通过运行期间各参数实际类型来确定
静态单/多分派：在编译期间确定一个方法(对应单个参数和多个参数)

8. a xor b xor b ＝＝ a   => a + b + b = a  异或中 b+b = 0 => a + 0 = a  数据恢复