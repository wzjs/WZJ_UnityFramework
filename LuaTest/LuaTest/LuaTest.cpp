// LuaTest.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include "pch.h"
#include <iostream>
#include <lua.hpp>
//extern int d = 3, f = 5;    // d 和 f 的声明 
char a;
extern int globalVar;
void test();
int main()
{
	/*lua_State *l = luaL_newstate();
	lua_pushstring(l, "Hello,World");
	int count = lua_gettop(l);
	std::cout << count;
	const char* str = lua_tostring(l, -1);
	std::cout << str;*/
	//int d = 3, f = 5;
	int d = 2;
	int *addr = &d;
	int **addr1 = &addr;
	//std::cout << globalVar;
	int  var[3] = { 10, 100, 200 };

	for (int i = 0; i < 3; i++)
	{
		*var = i;    // 这是正确的语法
	}
	std::cout << var[0];
	test();
}

void test() {
	std::cout << "hello,world";
}
// 运行程序: Ctrl + F5 或调试 >“开始执行(不调试)”菜单
// 调试程序: F5 或调试 >“开始调试”菜单

// 入门提示: 
//   1. 使用解决方案资源管理器窗口添加/管理文件
//   2. 使用团队资源管理器窗口连接到源代码管理
//   3. 使用输出窗口查看生成输出和其他消息
//   4. 使用错误列表窗口查看错误
//   5. 转到“项目”>“添加新项”以创建新的代码文件，或转到“项目”>“添加现有项”以将现有代码文件添加到项目
//   6. 将来，若要再次打开此项目，请转到“文件”>“打开”>“项目”并选择 .sln 文件
