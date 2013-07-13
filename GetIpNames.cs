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
	class GetIpNames
	{
		// ���� ���������� ��� �����
		static SortedDictionary<IPAddressComparable, string> _result = new SortedDictionary<IPAddressComparable, string>();

		static int _allips = 0;

		public static ManualResetEvent _getHostEntryFinished = new ManualResetEvent(false);

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
					Console.WriteLine(args[0] + " �� �������� IP �������!");
					return false;
				}

				if (!IPAddress.TryParse(args[1], out _adr_end))
				{
					Console.WriteLine(args[1] + " �� �������� IP �������!");
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
				Console.WriteLine("������������ �������� IP �������!");
				return false;
			}

			// ����� �����
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

			if (!IsParamsValid(args))
				return;

			CreateOutFileName();

			ScanIps();

			WriteResultFile();
		}

		private static async void ScanIps()
		{
			_getHostEntryFinished.Reset();

			byte[] cur = new byte[4];
			Array.Copy(_bytes_begin, cur, 4);

			// ������������� ����������
			for (; cur[3] <= _bytes_end[3]; ++cur[3])
			{
				IPAddressComparable adr = new IPAddressComparable(cur);

				IPHostEntry curhost = await Dns.GetHostEntryAsync(adr);

				AddNewHostName(adr, curhost.HostName);

				if (cur[3] == 255)
					break;
			}

			// �������� ���������
			_getHostEntryFinished.WaitOne();
		}

		static void AddNewHostName(IPAddressComparable adr, string hostname)
		{
			if (hostname == adr.ToString())
			{
				Ping pingsender = new Ping();
				PingReply repl = pingsender.Send(adr, 300);
				if (repl.Status != IPStatus.Success)
					hostname = "";
			}

			if (!_result.ContainsKey(adr))
					_result.Add(adr, hostname);

			Console.WriteLine(String.Format("{0,-16}  {1}", adr, hostname));
		}

		//public static void GetHostEntryCallback(IAsyncResult ar)
		//{
		//	IPAddressComparable adr = (IPAddressComparable)ar.AsyncState;

		//	string hostname;
		//	try
		//	{
		//		hostname = Dns.EndGetHostEntry(ar).HostName;
		//	}
		//	catch (Exception ex)
		//	{
		//		hostname = ExceptionFullMessage.GetMessages(ex);
		//	}

		//	if (_result.Count == _allips)
		//		_getHostEntryFinished.Set();
		//}

		static void WriteResultFile()
		{

			// ����� � ����
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

//// ���������� �������
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
