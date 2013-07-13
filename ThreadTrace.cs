using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace GetIpNames
{
   /// <summary>
   /// Вывод трассировки с данными потока
   /// </summary>
   public class ThreadTrace
   {
      [DllImport("kernel32.dll")]
      private static extern uint GetCurrentThreadId();

      /// <summary>
      /// Трассировка с выводом данных потока
      /// </summary>
      /// <param name="msg">Собственно сообщение</param>
      public static void WriteLine(string msg)
      {
         Trace.WriteLine(string.Format("[{0,2:D}:{1,4:D}] {2,-10}: {3}",
            Thread.CurrentThread.ManagedThreadId,
            GetCurrentThreadId(),
            Thread.CurrentThread.Name ?? "",
            msg));
      }

      /// <summary>
      /// Трассировка с выводом данных потока
      /// </summary>
      /// <param name="e">Исключение</param>
      public static void WriteLine(Exception e)
      {
         WriteLine(ExceptionFullMessage.GetMessages(e));
      }
   }

   /// <summary>
   /// Сообщение из исключения, включаяя вложенные
   /// </summary>
   public static class ExceptionFullMessage
   {
      /// <summary>
      /// Собирает строку из сообщений переданного и вложенных исключений
      /// </summary>
      /// <param name="e">Исключение из которого надо получить сообщение об ошибке</param>
      /// <returns>Собранные в строку сообщения из исключений</returns>
      public static string GetMessages(Exception e)
      {
         string msgs = e.GetType() + ". " + e.Message;
         Exception ex = e;
         while(ex.InnerException != null)
         {
            msgs += ": " + ex.InnerException.Message;
            ex = ex.InnerException;
         }

         return msgs;
      }
   }
}
