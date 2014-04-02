using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;

namespace GetIpNames
{
	class GetIpNames
	{
		// куда поместятся все имена
		static SortedDictionary<IPAddressComparable, string> _result = new SortedDictionary<IPAddressComparable, string>();

		static int _allips = 0;

		//public static ManualResetEvent _getHostEntryFinished = new ManualResetEvent(false);

		static string _outfilename;

		static IPAddress _adr_begin = IPAddress.Parse("192.168.1.1");
		static IPAddress _adr_end = IPAddress.Parse("192.168.1.255");

		static byte[] _bytes_begin;
		static byte[] _bytes_end;

		static bool IsParamsValid(string[] args)
		{
			if (args.Length >= 2)
			{
				if (!IPAddress.TryParse(args[0], out _adr_begin))
				{
					Console.WriteLine(args[0] + " не является IP адресом!");
					return false;
				}

				if (!IPAddress.TryParse(args[1], out _adr_end))
				{
					Console.WriteLine(args[1] + " не является IP адресом!");
					return false;
				}
			}

			_bytes_begin = _adr_begin.GetAddressBytes();
			_bytes_end = _adr_end.GetAddressBytes();

			if (_bytes_begin[0] != _bytes_end[0] ||
				 _bytes_begin[1] != _bytes_end[1] ||
				 _bytes_begin[2] != _bytes_end[2] ||
				 _bytes_begin[3] > _bytes_end[3])
			{
				Console.WriteLine("Некорректный диапазон IP адресов!");
				return false;
			}

			// всего будет
			_allips = _bytes_end[3] - _bytes_begin[3] + 1;

			return true;
		}

		static void CreateOutFileName()
		{
			DateTime now = DateTime.Now;

			_outfilename = string.Format("{0}\\ip_{1,4:0000}{2,2:00}{3,2:00}_{4,2:00}{5,2:00}.txt",
				Application.StartupPath, now.Year, now.Month, now.Day, now.Hour, now.Minute);

			if (File.Exists(_outfilename))
				File.Delete(_outfilename);
		}

		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU", false);
			Thread.CurrentThread.Name = "Main";

			if (!IsParamsValid(args))
				return;

			CreateOutFileName();

			var res = ScanIps();

			// ждём когда возвратится полный список
			res.Wait();

			_result = res.Result;

			WriteResultFile();

			ThreadTrace.WriteLine("Нажмите Enter для завершения...");
			Console.ReadLine();
		}

		private static async Task<SortedDictionary<IPAddressComparable, string>> ScanIps()
		{
			byte[] cur = new byte[4];
			Array.Copy(_bytes_begin, cur, 4);
			var result = new SortedDictionary<IPAddressComparable, string>();

			// инициирование заполнения
			for (; cur[3] <= _bytes_end[3]; ++cur[3])
			{
				IPAddressComparable adr = new IPAddressComparable(cur);
				string hostname = await GetHostName(adr);

				if (!result.ContainsKey(adr))
				{
					result.Add(adr, hostname);

					ThreadTrace.WriteLine(String.Format("{0,-16}  {1}", adr, hostname));
				}

				if (cur[3] == 255)
					break;
			}

			return result;
		}

		/// <summary>
		/// Получает, проверяет и возвращает имя хоста
		/// </summary>
		/// <param name="adr">IP адрес</param>
		/// <returns>Имя хоста</returns>
		private static async Task<string> GetHostName(IPAddressComparable adr)
		{
			string hostname = "";

			try
			{
				IPHostEntry curhost = await Dns.GetHostEntryAsync(adr);
				hostname = curhost.HostName;
				if (hostname == adr.ToString())
				{
					Ping pingsender = new Ping();
					PingReply repl = pingsender.Send(adr, 300);
					if (repl.Status != IPStatus.Success)
						hostname = "";
				}
			}
			catch (Exception ex)
			{
				ThreadTrace.WriteLine(String.Format("{0,-16}  --> {1}", adr, ex.GetMessages()));
			}

			return hostname;
		}

		static void WriteResultFile()
		{
			// вывод в файл
			using (StreamWriter outf = new StreamWriter(path:_outfilename, append:false, encoding:Encoding.UTF8))
			{
				foreach (var next in _result)
				{
					if (next.Value != "")
						outf.WriteLine(String.Format("{0,-16}  {1}", next.Key, next.Value));
				}
			}
		}

		private static async void ScanAllIps()
		{
			//List<byte[]> allipslist = new List<byte[]>();
			List<Task<IPHostEntry>> tasks = new List<Task<IPHostEntry>>();

			// инициирование заполнения
			byte[] cur = new byte[4];
			Array.Copy(_bytes_begin, cur, 4);
			for (; cur[3] <= _bytes_end[3]; ++cur[3])
			{
				IPAddressComparable adr = new IPAddressComparable(cur);
				tasks.Add(Dns.GetHostEntryAsync(adr));
			}

			Task<IPHostEntry[]> allscans = Task.WhenAll(tasks);
			IPHostEntry[] ips = await allscans;

			//IEnumerable<Task<IPHostEntry>> tasks = allipslist.Select(Dns.GetHostEntryAsync);
			//tasks = tasks.ToList();
		}

	}
}
