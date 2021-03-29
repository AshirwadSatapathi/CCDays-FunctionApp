using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionApp.Product.Models
{
    public class Feedback
    {
        public int CustomerId { get; set; }
        public string ProductName { get; set; }
        public string CustomerFeedback { get; set; }
        public string FeedbackSentiment { get; set; }
    }
}
