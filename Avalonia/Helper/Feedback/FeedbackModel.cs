using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Feedback
{
    public class FeedbackModel
    {
        public event Action LogActivityChanged;
        public string Message { get; set; }
        public FeedbackType FeedBackType { get; set; }
    }
}
