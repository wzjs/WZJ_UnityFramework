using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CShaperTest
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int LuaCSFunction(IntPtr luaState);
    class Program
    {
        static void Main(string[] args)
        {
            IntPtr L = lua_new_state();

            //Console.WriteLine(luaopen_test(L));


            //RegirstFunc(L, CShaperTest, "CTest");
            //RegirstFunc(L, print, "WriteLine");
            //RegirstFunc(L, PushNumber, "Test");
            lua_push_number(L, 2);
            int refer = lua_ref(L);
            lua_raw_geti(L, refer);
            Console.WriteLine(lua_to_number(L, 1));
            
            //int error = lua_load_file(L, "test.lua");
            //if (error != 0)
            //{
            //    IntPtr intPtr = lua_to_string(L, 1);
            //    int strLen = lua_get_strlen(intPtr);
            //    string str = lua_ptrtostring(intPtr, strLen);
            //    Console.WriteLine(str);
            //    return;
            //}
        }

        private static void RegirstFunc(IntPtr L, Func<IntPtr, int> func, string luaName)
        {
            LuaCSFunction luaCSFunction = new LuaCSFunction(func);
            IntPtr funcPtr1 = Marshal.GetFunctionPointerForDelegate(luaCSFunction);
            lua_push_function(L, funcPtr1);
            lua_set_global(L, luaName);
        }

        static int print(IntPtr intPtr)
        {
            string msg = LuaToCString(intPtr, 1);
            string stackInfo = new StackTrace(true).ToString();
            Console.WriteLine(msg  + "\n" + stackInfo);
            return 0;
        }

        static int PushNumber(IntPtr intPtr)
        {
            //lua_push_string(intPtr, "Hello");
            //lua_push_string(intPtr, "World");
            lua_push_number(intPtr, 0.999f);
            return 2;
        }

        static void Test(int a)
        {
            try
            {
                int b = 1 / a;
            }
            catch (DivideByZeroException e)
            {
                Console.WriteLine(e.StackTrace);
                //throw;
            }
        }

        static int CShaperTest(IntPtr L)
        {
            int number = lua_to_number(L, 1);
            try
            {
                int b = 1 / number;
            }
            catch (DivideByZeroException e)
            {
                Console.WriteLine(e.StackTrace);
                //throw;
            }
            return 0;
        }

        public static string lua_ptrtostring(IntPtr str, int len)
        {
            string ss = Marshal.PtrToStringAnsi(str, len);

            if (ss == null)
            {
                byte[] buffer = new byte[len];
                Marshal.Copy(str, buffer, 0, len);
                return Encoding.UTF8.GetString(buffer);
            }

            return ss;
        }



        static string LuaToCString(IntPtr L, int index)
        {
            IntPtr intPtr = lua_to_string(L, index);
            int strLen = lua_get_strlen(intPtr);
            string str = lua_ptrtostring(intPtr, strLen);
            return str;
        }


        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "Add")]
        extern static int Add(int a, int b);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_to_string")]
        extern static IntPtr lua_to_string(IntPtr ptr ,  int index);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_to_number")]
        extern static int lua_to_number(IntPtr ptr, int index);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_push_string")]
        extern static void lua_push_string(IntPtr ptr, string str);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_push_number")]
        extern static void lua_push_number(IntPtr ptr, float number);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_new_state")]
        extern static IntPtr lua_new_state();
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_get_top")]
        extern static int lua_get_top(IntPtr intPtr);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_get_strlen")]
        extern static int lua_get_strlen(IntPtr intPtr);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_p_call")]
        extern static int lua_p_call(IntPtr intPtr);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_load_file")]
        extern static int lua_load_file(IntPtr intPtr,string fileName);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_push_global")]
        extern static int lua_push_global(IntPtr intPtr, string str);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_set_global")]
        extern static int lua_set_global(IntPtr intPtr, string str);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_push_function")]
        extern static int lua_push_function(IntPtr intPtr, IntPtr func);

        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_raw_geti")]
        extern static int lua_raw_geti(IntPtr intPtr,int refer);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_ref")]
        extern static int lua_ref(IntPtr intPtr);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "lua_unref")]
        extern static void lua_unref(IntPtr intPtr,int refer);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "luaC_error")]
        extern static int luaC_error(IntPtr intPtr,string msg);
        [DllImport(@"C:\Users\DELL\source\repos\LuaTest\Debug\Dll1.dll", EntryPoint = "luaopen_test")]
        extern static int luaopen_test(IntPtr intPtr);
    }
}
