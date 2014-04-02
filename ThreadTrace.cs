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
		/// <summary>
		/// Трассировка с выводом данных потока
		/// </summary>
		/// <param name="msg">Собственно сообщение</param>
		public static void WriteLine(string msg)
		{
			Console.WriteLine(string.Format("{0:HH:mm:ss} [{1,2:D}]: {2}",
				DateTime.Now,
				Thread.CurrentThread.ManagedThreadId,
				msg));
		}

		/// <summary>
		/// Трассировка с выводом данных потока
		/// </summary>
		/// <param name="e">Исключение</param>
		public static void WriteLine(Exception e)
		{
			WriteLine(e.GetMessages());
		}

		public static void LogThreads()
		{
			int workerThreads;
			int portThreads;

			ThreadPool.GetMaxThreads(out workerThreads, out portThreads);
			WriteLine(string.Format("Максимально рабочих потоков в пуле: {0}. Максимально потоков портов ввода/вывода: {1}", 
				workerThreads, portThreads));

			ThreadPool.GetAvailableThreads(out workerThreads, out portThreads);
			WriteLine(string.Format("Доступно рабочих потоков в пуле: {0}. Доступно потоков портов ввода/вывода: {1}{2}",
				workerThreads, portThreads, Environment.NewLine));
		}
	}

	/// <summary>
	/// Расширение для получения подного сообщения об ошибке
	/// </summary>
	public static class FullExceptionMessage
	{
		/// <summary>
		/// Собирает строку из сообщений переданного и вложенных исключений
		/// </summary>
		/// <param name="e">Исключение из которого надо получить сообщение об ошибке</param>
		/// <returns>Собранные в строку сообщения из исключений</returns>
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
