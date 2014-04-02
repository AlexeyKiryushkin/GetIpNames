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
		/// <summary>
		/// ����������� � ������� ������ ������
		/// </summary>
		/// <param name="msg">���������� ���������</param>
		public static void WriteLine(string msg)
		{
			Console.WriteLine(string.Format("{0:HH:mm:ss} [{1,2:D}]: {2}",
				DateTime.Now,
				Thread.CurrentThread.ManagedThreadId,
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
