using System.Threading.Tasks;
using Orleans;

namespace feed_grain
{
    public interface IClientSubscriptionGrain : IGrainWithStringKey
    {
        Task Subscribe(string firstName, string lastName, string emailAddress);
        Task<string> GetStatus();
    }
}