// Dll1.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"

#include "Dll1.h"


Dll1 int __stdcall Add(int a, int b)
{
	return a + b;
}

Dll1 const char* __stdcall lua_to_string(void* intPtr, int index)
{
	lua_State* agw = (lua_State*)intPtr;
	const char* str = lua_tostring(agw, index);
	return str;
}

Dll1 int __stdcall lua_to_number(void* intPtr, int index)
{
	lua_State* agw = (lua_State*)intPtr;
	int number = lua_tonumber(agw, index);
	return number;
}


Dll1 void __stdcall lua_push_string(void* intPtr, const char* str)
{
	lua_State* agw = (lua_State*)intPtr;
	lua_pushstring(agw, str);
}

Dll1 void __stdcall lua_push_number(void* intPtr, float number)
{
	lua_State* agw = (lua_State*)intPtr;
	lua_pushnumber(agw, number);
}

Dll1 void* __stdcall lua_new_state()
{
	lua_State* l = luaL_newstate();
	luaL_openlibs(l);
	return (void*)l;
}

Dll1 int __stdcall lua_get_top(void* intPtr) {
	lua_State* agw = (lua_State*)intPtr;
	return lua_gettop(agw);
}

Dll1 int __stdcall lua_get_strlen(const char* str) 
{
	return strlen(str);
}

Dll1 int __stdcall lua_p_call(lua_State* L)
{
	return lua_pcall(L, 0, 1, 0);
}

Dll1 int __stdcall lua_load_file(lua_State* L,const char* fileName)
{
	return luaL_dofile(L, fileName);
	//return luaL_loadfile(L, fileName);
}


Dll1 int __stdcall lua_push_global(lua_State* L, const char* str)
{
	return lua_getglobal(L, str);
}

Dll1 void __stdcall lua_set_global(lua_State* L, const char* str)
{
	return lua_setglobal(L, str);
}

Dll1 void __stdcall lua_push_function(lua_State* L, lua_CFunction f)
{
	return lua_pushcfunction(L, f);
}

Dll1 int __stdcall lua_raw_geti(lua_State* L, int refer)
{
	return lua_rawgeti(L, LUA_REGISTRYINDEX, refer);
}


Dll1 int __stdcall lua_ref(lua_State* L)
{
	return luaL_ref(L, LUA_REGISTRYINDEX);
}

Dll1 void __stdcall lua_unref(lua_State* L,int ref)
{
	return luaL_unref(L, LUA_REGISTRYINDEX, ref);
}

Dll1 int __stdcall luaC_error(lua_State* L,const char* msg)
{
	return luaL_error(L, msg);
}

Dll1 int __stdcall luaopen_test(lua_State* L)
{
	lua_pushnumber(L, 10);
	lua_pushnumber(L, 20);
	return 1;
}
