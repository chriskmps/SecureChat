using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureChat
{
    class UserAndConversation
    {
        public string currentUser_selected { get; set; }
        public string currentUser_conversation { get; set; }

        public override string ToString()
        {
            return string.Format("Current User: {0}, Current Conversation: {1}", currentUser_selected, currentUser_conversation);
        }
    }
}
