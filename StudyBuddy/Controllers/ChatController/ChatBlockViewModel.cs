using StudyBuddy.Shared.Abstractions;

namespace StudyBuddy.Controllers.ChatController
{
    public class ChatBlockViewModel
    {
        public String? BlockingUser { get; set; }
        public String? BlockedUser { get; set; }
    }
}
