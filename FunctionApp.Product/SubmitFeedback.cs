using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.AI.TextAnalytics;
using FunctionApp.Product.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionApp.Product
{
    public static class SubmitFeedback
    {
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential(Environment.GetEnvironmentVariable("key"));
        private static readonly Uri endpoint = new Uri(Environment.GetEnvironmentVariable("endpoint"));
        private static TextAnalyticsClient client = new TextAnalyticsClient(endpoint, credentials);
        [FunctionName("SubmitFeedback")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            Feedback data;
            int insertRecord;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            try
            {
                data = JsonConvert.DeserializeObject<Feedback>(requestBody);
                data.FeedbackSentiment = GetFeedbackSentiment(data.CustomerFeedback);

                insertRecord = InsertData(data);

                return new OkObjectResult(data);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        private static int InsertData(Feedback data)
        {
            int recordsInserted;
            try
            {
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("connectionString")))
                {
                    string queryString = @"INSERT INTO [Feedback](CustomerId,ProductName,CustomerFeedback,FeedbackSentiment)
                                       VALUES(@CustomerId,@ProductName,@CustomerFeedback,@FeedbackSentiment)";

                    using (SqlCommand cmd = new SqlCommand(queryString))
                    {
                        cmd.Parameters.AddWithValue("@CustomerId", data.CustomerId);
                        cmd.Parameters.AddWithValue("@ProductName", data.ProductName);
                        cmd.Parameters.AddWithValue("@CustomerFeedback", data.CustomerFeedback);
                        cmd.Parameters.AddWithValue("@FeedbackSentiment", data.FeedbackSentiment);
                        cmd.Connection = connection;
                        connection.Open();
                        recordsInserted = cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                recordsInserted = 0;
            }
            return recordsInserted;
        }

        private static string GetFeedbackSentiment(string customerFeedback)
        {
            DocumentSentiment documentSentiment = client.AnalyzeSentiment(customerFeedback);
            return documentSentiment.Sentiment.ToString();
        }
    }
}

