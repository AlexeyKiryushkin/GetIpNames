using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

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
}
