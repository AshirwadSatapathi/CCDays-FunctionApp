using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using FunctionApp.Product.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionApp.Product
{
    public static class GetFeedback
    {
        [FunctionName("GetFeedback")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            List<Feedback> data = new List<Feedback>();


            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("connectionString")))
            {
                string oString = "select ProductName,CustomerFeedback,FeedbackSentiment FROM Feedback";
                SqlCommand oCmd = new SqlCommand(oString, connection);
                connection.Open();
                using (SqlDataReader oReader = oCmd.ExecuteReader())
                {
                    while (oReader.Read())
                    {
                        Feedback feedback = new Feedback();
                        feedback.ProductName = oReader["ProductName"].ToString();
                        feedback.CustomerFeedback = oReader["CustomerFeedback"].ToString();
                        feedback.FeedbackSentiment = oReader["FeedbackSentiment"].ToString();
                        data.Add(feedback);
                    }

                    connection.Close();
                }
            }

            return new OkObjectResult(data);
        }
    }
}

