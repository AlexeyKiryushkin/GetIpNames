using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetIpNames
{
	class GetIpNames
	{
		// куда помест€тс€ все имена
		static SortedDictionary<IPAddressComparable, string> _result = new SortedDictionary<IPAddressComparable, string>();

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
					Console.WriteLine(args[0] + " не €вл€етс€ IP адресом!");
					return false;
				}

				if (!IPAddress.TryParse(args[1], out _adr_end))
				{
					Console.WriteLine(args[1] + " не €вл€етс€ IP адресом!");
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
				Console.WriteLine("Ќекорректный диапазон IP адресов!");
				return false;
			}

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
			if (!IsParamsValid(args))
				return;

			ThreadPool.SetMinThreads(200, 200);

			CreateOutFileName();

			_result = ScanIps();

			WriteResultFile();
		}

		private static SortedDictionary<IPAddressComparable, string> ScanIps()
		{
			byte[] cur = new byte[4];
			Array.Copy(_bytes_begin, cur, 4);

			var result = new SortedDictionary<IPAddressComparable, string>();

			var iplist = new List<IPAddressComparable>();

			// список IP дл€ сканировани€
			for (; cur[3] <= _bytes_end[3]; ++cur[3])
				iplist.Add(new IPAddressComparable(cur));

			var tasks = iplist.Select<IPAddressComparable, Task>(async ip =>
			{
				try
				{
					var host = await Dns.GetHostEntryAsync(ip);
					string hostname = host.HostName;

					ThreadTrace.WriteLine(String.Format("{0,-16} {1}", ip, hostname));

					lock (_result)
						result.Add(ip, hostname);
				}
				catch (Exception ex)
				{
					ThreadTrace.WriteLine(String.Format("{0,-16} {1}", ip, ex.Message));
				}
			}).ToArray();

			Task.WaitAll(tasks);

			return result;
		}

		static void WriteResultFile()
		{
			// вывод в файл
			using (StreamWriter outf = new StreamWriter(path:_outfilename, append:false, encoding:Encoding.UTF8))
			{
				foreach (var next in _result)
					outf.WriteLine(String.Format("{0,-16}  {1}", next.Key, next.Value));
			}
		}
	}
}
