using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Orleans;
using Orleans.Providers;

namespace feed_grain
{
    [StorageProvider(ProviderName = "RedisSubscriptionGrain")]
    public class ClientSubscriptionGrain : Grain<ClientSubscriptionState>, IClientSubscriptionGrain
    {
        public async Task Subscribe(string firstName, string lastName, string emailAddress)
        {
            Console.WriteLine($"Subscribing client {firstName}{lastName}{emailAddress} on {GetLocalIPAddress()}");

            State.IPAddress = GetLocalIPAddress();
            State.MachineName = Environment.MachineName;
            State.FirstName = firstName;
            State.LastName = lastName;
            State.EmailAddress = emailAddress;
            State.Key = $"{firstName}{lastName}{emailAddress}";
            State.Id = this.GetPrimaryKeyString();

            await WriteStateAsync();
        }

        public async Task<string> GetStatus()
        {
            Console.WriteLine("Get Status for Feed");
            return JsonConvert.SerializeObject(State);
        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "Unknown";
        }
    }
}