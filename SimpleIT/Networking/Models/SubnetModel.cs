using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIT.Networking.Models
{
    public class SubnetModel
    {
        public string Name { get; init; }

        public IPAddress NetworkAddress { get; init; }

        public IPAddress BroadcastAddress { get; init; }

        public IEnumerable<IPAddress> HostAddresses { get; init; }
    }
}
