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
			Console.WriteLine(string.Format("[{0,2:D}:{1,4:D}] {2,-10}: {3}",
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
			WriteLine(e.GetMessages());
		}

		public static void LogThreads()
		{
			int workerThreads;
			int portThreads;

			ThreadPool.GetMaxThreads(out workerThreads, out portThreads);
			WriteLine(string.Format("����������� ������� ������� � ����: {0}. ����������� ������� ������ �����/������: {1}", 
				workerThreads, portThreads));

			ThreadPool.GetAvailableThreads(out workerThreads, out portThreads);
			WriteLine(string.Format("�������� ������� ������� � ����: {0}. �������� ������� ������ �����/������: {1}{2}",
				workerThreads, portThreads, Environment.NewLine));
		}
	}

	/// <summary>
	/// ���������� ��� ��������� ������� ��������� �� ������
	/// </summary>
	public static class FullExceptionMessage
	{
		/// <summary>
		/// �������� ������ �� ��������� ����������� � ��������� ����������
		/// </summary>
		/// <param name="e">���������� �� �������� ���� �������� ��������� �� ������</param>
		/// <returns>��������� � ������ ��������� �� ����������</returns>
		public static string GetMessages(this Exception e)
		{
			string msgs = e.GetType() + ". " + e.Message;
			Exception ex = e;
			while (ex.InnerException != null)
			{
				msgs += ": " + ex.InnerException.Message;
				ex = ex.InnerException;
			}

			return msgs;
		}
	}
}
