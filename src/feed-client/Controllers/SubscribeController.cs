using System.Threading.Tasks;
using feed_grain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;

namespace feed_client.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubscribeController : Controller
    {
        private ILogger<SubscribeController> _logger;
        private readonly IClusterClient _clusterClient;

        public SubscribeController(ILogger<SubscribeController> logger, IClusterClient clusterClient)
        {
            _logger = logger;
            _clusterClient = clusterClient;
        }


        [HttpGet]
        public async Task<IActionResult> GetStatus(string firstName, string lastName, string emailAddress)
        {
            var subscriptionGrain = _clusterClient.GetGrain<IClientSubscriptionGrain>($"{firstName}{lastName}{emailAddress}");
            var result = await subscriptionGrain.GetStatus();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(string firstName, string lastName, string emailAddress)
        {
            var subscriptionGrain = _clusterClient.GetGrain<IClientSubscriptionGrain>($"{firstName}{lastName}{emailAddress}");
            await subscriptionGrain.Subscribe(firstName, lastName, emailAddress);

            return Ok();
        }

    }
}