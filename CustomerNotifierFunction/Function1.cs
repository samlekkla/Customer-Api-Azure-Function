using CustomerNotifierFunction.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomerNotifierFunction.Functions
{
    public class CustomerNotifier
    {
        private readonly ILogger _logger;
        private readonly string _sendGridApiKey = Environment.GetEnvironmentVariable("SendGridApiKey");

        public CustomerNotifier(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CustomerNotifier>();
        }

        [Function("NotifySalesPerson")]
        public async Task Run(
            [CosmosDBTrigger(
                databaseName: "CustomerDb",
                containerName: "Customers",
                Connection = "CosmosDbConnection",
                LeaseContainerName = "leases",
                CreateLeaseContainerIfNotExists = true)] IReadOnlyList<Customer> customers)
        {
            if (customers == null || customers.Count == 0)
            {
                _logger.LogInformation("No changes in CosmosDB.");
                return;
            }

            foreach (var customer in customers)
            {
                _logger.LogInformation($"New/Customer updated: {customer.Name}");

                if (customer.ResponsibleSalesPerson == null || string.IsNullOrWhiteSpace(customer.ResponsibleSalesPerson.Email))
                {
                    _logger.LogWarning("Saleman's email is missing. Skip this message.");
                    continue;
                }

                var client = new SendGridClient(_sendGridApiKey);

                var from = new EmailAddress("mglobal.th@gmail.com", "Customersystem");
                var to = new EmailAddress(customer.ResponsibleSalesPerson.Email, customer.ResponsibleSalesPerson.Name);
                var subject = $"New customer connectected to you: {customer.Name}";
                var plainText = $"Hi, {customer.ResponsibleSalesPerson.Name},\n\nA customer has registered/updated:\n\n" +
                                $"Name: {customer.Name}\nTitle: {customer.Title}\nTelephone: {customer.Phone}\nE-post: {customer.Email}\nAddress: {customer.Address}";

                var htmlContent = $@"
                    <p><strong>Hi {customer.ResponsibleSalesPerson.Name},</strong></p>
                    <p>A customer has registered or updated:</p>
                    <ul>
                        <li><strong>Name:</strong> {customer.Name}</li>
                        <li><strong>Title:</strong> {customer.Title}</li>
                        <li><strong>Telephone:</strong> {customer.Phone}</li>
                        <li><strong>Email:</strong> {customer.Email}</li>
                        <li><strong>Address:</strong> {customer.Address}</li>
                    </ul>
                    <p>Best regards,<br>Customer System</p>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlContent);
                var response = await client.SendEmailAsync(msg);

                _logger.LogInformation($"A email has send to : {to.Email} | Status: {response.StatusCode}");
            }
        }
    }
}