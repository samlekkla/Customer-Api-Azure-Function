using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Customer_API.Entities
{
    public class Customer
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public SalesPerson ResponsibleSalesPerson { get; set; }
    }
}
