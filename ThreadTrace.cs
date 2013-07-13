using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace GetIpNames
{
   /// <summary>
   /// ����� ����������� � ������� ������
   /// </summary>
   public class ThreadTrace
   {
      [DllImport("kernel32.dll")]
      private static extern uint GetCurrentThreadId();

      /// <summary>
      /// ����������� � ������� ������ ������
      /// </summary>
      /// <param name="msg">���������� ���������</param>
      public static void WriteLine(string msg)
      {
         Trace.WriteLine(string.Format("[{0,2:D}:{1,4:D}] {2,-10}: {3}",
            Thread.CurrentThread.ManagedThreadId,
            GetCurrentThreadId(),
            Thread.CurrentThread.Name ?? "",
            msg));
      }

      /// <summary>
      /// ����������� � ������� ������ ������
      /// </summary>
      /// <param name="e">����������</param>
      public static void WriteLine(Exception e)
      {
         WriteLine(ExceptionFullMessage.GetMessages(e));
      }
   }

   /// <summary>
   /// ��������� �� ����������, �������� ���������
   /// </summary>
   public static class ExceptionFullMessage
   {
      /// <summary>
      /// �������� ������ �� ��������� ����������� � ��������� ����������
      /// </summary>
      /// <param name="e">���������� �� �������� ���� �������� ��������� �� ������</param>
      /// <returns>��������� � ������ ��������� �� ����������</returns>
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
