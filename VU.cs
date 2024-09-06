using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Serilog;
using Serilog.Events;


namespace FastStreamFix
{
    public class VU
    {


        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);


        public static void log(params object[] args)
        {
            debug(args);
        }


        public static bool isShiftDown()
        {
            const int VK_SHIFT = 0x10; // Virtual key code for Shift key
            short state = GetKeyState(VK_SHIFT);
            bool clicked = (state & 0x8000) != 0;
            return clicked;
        }
        
        
        
        public static bool isCtrlDown()
        {
            const int VK_SHIFT = 0x11; // Virtual key code for Shift key
            short state = GetKeyState(VK_SHIFT);
            bool clicked = (state & 0x8000) != 0;
            return clicked;
        }


        public static StringBuilder hey(StringBuilder msg, params object[] o)
        {
            var seped = false;
            string sep = ", ";
            if (msg == null)
                msg = new StringBuilder(1024);
            for (int i = 0; i < o.Length; i++)
            {
                object o1 = o[i];
                if (o1 != null)
                {
                    if (o1 is Exception ex)
                    {
                        msg.Append("err: " + ex.Message);
                        msg.Append(ex.StackTrace);
                        continue;
                    }

                    Type itemType = o1.GetType();
                    if (itemType.IsGenericType && itemType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        if (seped) msg.Append(sep);
                        else seped = true;
                        // msg.Append("《"+itemType+"》");
                        msg.Append("[");
                        // Handle nested list
                        var nestedList = o1 as IList;
                        if (nestedList != null)
                        {
                            // Console.WriteLine("Nested List:");
                            var seped1 = false;
                            foreach (var o2 in nestedList)
                            {
                                if (seped1) msg.Append(sep);
                                else seped1 = true;
                                hey(msg, o2);
                            }
                        }

                        msg.Append("]");
                        continue;
                    }

                    if (itemType.IsGenericType && itemType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        if (seped) msg.Append(sep);
                        else seped = true;
                        // msg.Append("《"+itemType+"》");
                        msg.Append("{");
                        // Handle nested list
                        var nestedList = o1 as IDictionary;
                        if (nestedList != null)
                        {
                            // Console.WriteLine("Nested List:");
                            var seped1 = false;
                            foreach (System.Collections.DictionaryEntry pair in nestedList)
                            {
                                // debug(pair.Key, pair.Value.GetType(), pair.Value);
                                if (seped1) msg.Append(sep);
                                else seped1 = true;
                                hey(msg, pair.Key);
                                msg.Append(":");
                                hey(msg, pair.Value);
                                msg.Append(" ");
                            }
                        }

                        msg.Append("}");
                        continue;
                    }
                    // if (o1.GetType().ToString().IndexOf(".Generic.List")>0)
                    // {
                    //     List x;
                    //     List lst = (List<object>)o1;
                    //     foreach (object item in lst)
                    //     {
                    //     }
                    // }
                }

                if (seped) msg.Append(sep);
                else seped = true;
                msg.Append(o1);
            }

            sep = " ";
            return msg;
        }

        public static string logPath;

        public static string debug(params object[] o)
        {
            string message = hey(null, o).ToString();
            try
            {
                Console.Out.WriteLine(message);
                Trace.WriteLine(message);
                if (true)
                {
                    try
                    {
                        File.AppendAllText(logPath, message);
                        File.AppendAllText(logPath, "\n");
                    }
                    catch (Exception e)
                    {
                        // Console.WriteLine(e);
                    }
                }
            }
            catch (Exception e)
            {
                // Console.WriteLine(e);
            }
            return message;
        }

        public int Reduce(List<int> arr, int p, int st, int ed)
        {
            int len = ed - st;
            if (len > 1)
            {
                len = len >> 1;
                //log('reduce', st, len, ed);
                return p > (arr[st + len - 1])
                    ? Reduce(arr, p, st + len, ed)
                    : Reduce(arr, p, st, st + len);
            }
            else
            {
                return st;
            }
        }

 
    }

}
