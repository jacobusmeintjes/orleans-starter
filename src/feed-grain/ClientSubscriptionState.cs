using System;

namespace feed_grain
{
    [Serializable]
    public class ClientSubscriptionState
    {
        public string Id { get; set; }
        public string Key { get; set; }

        public string MachineName { get; set; }
        public string IPAddress { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
    }
}