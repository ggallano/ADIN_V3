using System;

namespace Helper.Feedback
{
    public class FeedbackModel
    {
        public event Action LogActivityChanged;
        public string Message { get; set; }
        public FeedbackType FeedBackType{ get; set; }
    }
}
