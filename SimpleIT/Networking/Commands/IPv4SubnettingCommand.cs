using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using SimpleIT.Networking.Models;
using System.Net;

namespace SimpleIT.Networking.Commands
{
    [Command("subnetting-ipv4", Description = "Calculates new subnets (IPv4)")]
    public class IPv4SubnettingCommand : ICommand
    {
        [CommandParameter(0, Description = "Given network address, to form subnets")]
        public required string StartIpAddress { get; init; }

        [CommandParameter(1, Description = "Amount of wanted subnets")]
        public required int AmountSubnets { get; init; }

        [CommandParameter(2, Description = "CIDR", IsRequired = false)]
        public int Cidr { get; set; } = 0;


        public ValueTask ExecuteAsync(IConsole console)
        {
            var isParsable = IPAddress.TryParse(StartIpAddress, out var ipAddress);
            if (!isParsable)
            {
                throw new CommandException("Given ip address was not valid");
            }

            var currentSubnetMask = CalculateSubnetmask(ipAddress, Cidr);
            console.Output.WriteLine(string.Concat("Current Subnetmask: ", currentSubnetMask));

            var networkAddress = CalculateNetworkAddress(ipAddress, Cidr);

            var neededNetworkBits = AmountSubnets > 2
                ? (int)Math.Ceiling(Math.Log2(AmountSubnets))
                : 1;

            Cidr += neededNetworkBits;
            var newSubnetMask = CalculateSubnetmask(networkAddress, Cidr);

            console.Output.WriteLine(string.Concat("New Subnetmask: ", newSubnetMask));

            // Subnetze generieren
            var subnets = GenerateSubnets(networkAddress, Cidr, AmountSubnets);

            // Ausgabe der Subnetze
            foreach (var subnet in subnets)
            {
                console.Output.WriteLine($"Subnet Name: {subnet.Name}");
                console.Output.WriteLine($"  Network Address: {subnet.NetworkAddress}");
                console.Output.WriteLine($"  Broadcast Address: {subnet.BroadcastAddress}");
                console.Output.WriteLine($"  Host Addresses: {string.Join(", ", subnet.HostAddresses)}");
                console.Output.WriteLine();
            }

            return default;
        }


        private IPAddress CalculateSubnetmask(IPAddress ipAddress, int cidr = 0)
        {
            var subnetMask = IPAddress.None;
            if (cidr != 0)
            {
                // Erzeuge die Subnetzmaske als 32-Bit-Wert
                uint mask = 0xFFFFFFFF << 32 - cidr;

                // Zerlege die Maske in Oktette
                byte[] bytes = BitConverter.GetBytes(mask);
                Array.Reverse(bytes); // Netzwerk-Byte-Reihenfolge (Big Endian)

                // Konvertiere in das Standardformat (x.x.x.x)
                subnetMask = IPAddress.Parse(string.Join(".", bytes));
            }
            else
            {
                var addressBytes = ipAddress.GetAddressBytes();
                var classIdentifier = addressBytes[0];
                if (classIdentifier <= 127)
                {
                    subnetMask = IPAddress.Parse("255.0.0.0");
                    Cidr = 8;
                }
                else if (classIdentifier > 127 && classIdentifier <= 191)
                {
                    subnetMask = IPAddress.Parse("255.255.0.0");
                    Cidr = 16;
                }
                else if (classIdentifier > 191 && classIdentifier <= 223)
                {
                    subnetMask = IPAddress.Parse("255.255.255.0");
                    Cidr = 24;
                }
                else
                {
                    throw new CommandException("IP-Address is out of supported network class");
                }
            }
            return subnetMask;
        }

        private List<SubnetModel> GenerateSubnets(IPAddress startIp, int cidr, int amountSubnets)
        {
            var subnets = new List<SubnetModel>();

            // Startadresse in ein UInt32 umwandeln
            uint startAddress = BitConverter.ToUInt32(startIp.GetAddressBytes().Reverse().ToArray(), 0);

            // Schrittweite zwischen Subnetzen berechnen
            int step = (int)Math.Pow(2, 32 - cidr);

            for (int i = 0; i < amountSubnets; i++)
            {
                uint networkAddress = startAddress + (uint)(i * step);

                // Netzwerk- und Broadcastadresse berechnen
                var networkIp = new IPAddress(BitConverter.GetBytes(networkAddress).Reverse().ToArray());
                var broadcastIp = new IPAddress(BitConverter.GetBytes(networkAddress + (uint)(step - 1)).Reverse().ToArray());

                // Hostadressen berechnen
                var hostAddresses = Enumerable.Range(1, step - 2)
                    .Select(offset => new IPAddress(BitConverter.GetBytes(networkAddress + (uint)offset).Reverse().ToArray()))
                    .ToList();

                // Subnetzmodell hinzufügen
                subnets.Add(new SubnetModel
                {
                    Name = $"Subnet-{i + 1}",
                    NetworkAddress = networkIp,
                    BroadcastAddress = broadcastIp,
                    HostAddresses = hostAddresses
                });
            }

            return subnets;
        }

        private IPAddress CalculateNetworkAddress(IPAddress ip, int cidr)
        {
            uint ipAddress = BitConverter.ToUInt32(ip.GetAddressBytes().Reverse().ToArray(), 0);
            uint mask = 0xFFFFFFFF << (32 - cidr);
            uint networkAddress = ipAddress & mask;
            return new IPAddress(BitConverter.GetBytes(networkAddress).Reverse().ToArray());
        }

    }
}
