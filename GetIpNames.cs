using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;

namespace GetIpNames
{
	class IPAddressComparable : IPAddress, IComparable<IPAddressComparable>
	{
		public IPAddressComparable(byte[] address)
			: base(address)
		{
		}

		public int CompareTo(IPAddressComparable other)
		{
			Int32 thisIp = NetworkToHostOrder(BitConverter.ToInt32(GetAddressBytes(), 0));
			Int32 otherIp = NetworkToHostOrder(BitConverter.ToInt32(other.GetAddressBytes(), 0));

			return thisIp.CompareTo(otherIp);
		}
	}

	class GetIpNames
	{
		// куда помест€тс€ все имена
		static SortedDictionary<IPAddressComparable, string> _result =
			 new SortedDictionary<IPAddressComparable, string>();

		static int _allips = 0;

		public static ManualResetEvent _getHostEntryFinished = new ManualResetEvent(false);

		static string _outfilename;

		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU", false);

			string ip_begin = "192.168.1.1";
			string ip_end = "192.168.1.255";
			IPAddress adr_begin = IPAddress.Parse(ip_begin);
			IPAddress adr_end = IPAddress.Parse(ip_end);

			DateTime now = DateTime.Now;
			_outfilename = string.Format("{0}\\ip_{1,4:0000}{2,2:00}{3,2:00}_{4,2:00}{5,2:00}.txt",
				Application.StartupPath, now.Year, now.Month, now.Day, now.Hour, now.Minute);

			if (File.Exists(_outfilename))
				File.Delete(_outfilename);

			if (args.Length >= 2)
			{
				if (IPAddress.TryParse(args[0], out adr_begin))
					ip_begin = args[0];
				else
				{
					Console.WriteLine(args[0] + " не €вл€етс€ IP адресом!");
					return;
				}

				if (IPAddress.TryParse(args[1], out adr_end))
					ip_end = args[1];
				else
				{
					Console.WriteLine(args[1] + " не €вл€етс€ IP адресом!");
					return;
				}
			}

			byte[] bytes_begin = adr_begin.GetAddressBytes();
			byte[] bytes_end = adr_end.GetAddressBytes();

			if (bytes_begin[0] != bytes_end[0] ||
				 bytes_begin[1] != bytes_end[1] ||
				 bytes_begin[2] != bytes_end[2] ||
				 bytes_begin[3] > bytes_end[3])
			{
				Console.WriteLine("Ќекорректный диапазон IP адресов!");
				return;
			}

			// всего будет
			_allips = bytes_end[3] - bytes_begin[3] + 1;

			_getHostEntryFinished.Reset();

			byte[] cur = new byte[4];
			Array.Copy(bytes_begin, cur, 4);

			// инициирование заполнени€
			for (; cur[3] <= bytes_end[3]; ++cur[3])
			{
				Thread.Sleep(10);
				IPAddressComparable adr = new IPAddressComparable(cur);
				Dns.BeginGetHostEntry(adr, new AsyncCallback(GetHostEntryCallback), adr);

				if (cur[3] == 255)
					break;
			}

			// ожидание окончани€
			_getHostEntryFinished.WaitOne();

			WriteResultFile();
		}

		public static void GetHostEntryCallback(IAsyncResult ar)
		{
			IPAddressComparable adr = (IPAddressComparable)ar.AsyncState;

			string hostname;
			try
			{
				hostname = Dns.EndGetHostEntry(ar).HostName;
			}
			catch (Exception ex)
			{
				hostname = ExceptionFullMessage.GetMessages(ex);
			}

			if (hostname == adr.ToString())
			{
				Ping pingsender = new Ping();
				PingReply repl = pingsender.Send(adr, 300);
				if (repl.Status != IPStatus.Success)
					hostname = "";
			}

			lock (_result)
			{
				if (!_result.ContainsKey(adr))
					_result.Add(adr, hostname);
			}

			Console.WriteLine(String.Format("{0,-16}  {1}", adr, hostname));

			if (_result.Count == _allips)
				_getHostEntryFinished.Set();
		}

		static void WriteResultFile()
		{

			// вывод в файл
			using (StreamWriter outf = new StreamWriter(_outfilename, false, Encoding.GetEncoding(1251)))
			{
				foreach (KeyValuePair<IPAddressComparable, string> next in _result)
				{
					if (next.Value != "")
						outf.WriteLine(String.Format("{0,-16}  {1}", next.Key, next.Value));
				}
			}
		}
	}
}

//// синхронный вариант
//using (StreamWriter outf = new StreamWriter(outfilename, false, Encoding.GetEncoding(1251)))
//{
//   byte[] cur = new byte[4];
//   Array.Copy(bytes_begin, cur, 4);

//   for (; cur[3] <= bytes_end[3]; ++cur[3])
//   {
//      IPAddress adr = new IPAddress(cur);
//      string hostname = "";
//      Ping pingsender = new Ping();
//      PingReply repl = pingsender.Send(adr, timeout);
//      if (repl.Status == IPStatus.Success)
//      {
//         try
//         {
//            hostname = Dns.GetHostEntry(adr).HostName;
//         }
//         catch (Exception)
//         {
//         }
//      }
//      string result = String.Format("{0,-16}  {1}", adr, hostname);
//      Console.WriteLine(result);
//      outf.WriteLine(result);
//   }
//}
