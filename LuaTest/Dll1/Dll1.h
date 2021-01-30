#include <winnt.h>

#include "lua.h"
#include "lualib.h"
#include "lauxlib.h"

#define Dll1 _declspec(dllexport)
EXTERN_C Dll1 int __stdcall Add(int a, int b);

EXTERN_C Dll1 const char* __stdcall lua_to_string(void* L,int index);
EXTERN_C Dll1 int __stdcall lua_to_number(void* L, int index);

EXTERN_C Dll1 void* __stdcall lua_new_state();

EXTERN_C Dll1 void __stdcall lua_push_string(void* L,const char* str);
EXTERN_C Dll1 void __stdcall lua_push_number(void* L, float number);


EXTERN_C Dll1 int __stdcall lua_get_top(void* intPtr);

EXTERN_C Dll1 int __stdcall lua_get_strlen(const char* str);

EXTERN_C Dll1 int __stdcall lua_p_call(void* intPtr);

EXTERN_C Dll1 int __stdcall lua_load_file(void* intPtr,const char* str);

EXTERN_C Dll1 int __stdcall lua_push_global(void* intPtr, const char* str);

EXTERN_C Dll1 void __stdcall lua_set_global(void* intPtr, const char* str);
EXTERN_C Dll1 void __stdcall lua_push_function(void* intPtr, void* intPtr1);

EXTERN_C Dll1 int __stdcall lua_raw_geti(void* intPtr, int refer);
EXTERN_C Dll1 int __stdcall lua_ref(void* intPtr);
EXTERN_C Dll1 void __stdcall lua_unref(void* intPtr, int refer);

EXTERN_C Dll1 int __stdcall luaC_error(void* intPtr,const char* msg);
EXTERN_C Dll1 int __stdcall luaopen_test(void* intPtr);


