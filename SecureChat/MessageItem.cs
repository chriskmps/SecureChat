using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureChat
{
    public class MessageItem
    {
        string message;
        string userid;
        string timeStamp;
        Boolean isEncrypted;

        public void storeData(string msg, string theUser, string sentTime, Boolean secured)
        {
            this.message = msg;
            this.userid = theUser;
            this.timeStamp = sentTime;
            this.isEncrypted = secured;
        }

        public string messageText()
        {
            string output = "(" + this.timeStamp + ")" + " " + this.userid + ": " + this.message;
            return output;
        }

        public string returnObject_message(MessageItem anItem)
        {
            return anItem.message;
        }
        public string returnObject_userid(MessageItem anItem)
        {
            return anItem.userid;
        }
        public string returnObject_timeStamp(MessageItem anItem)
        {
            return anItem.timeStamp;
        }
        public Boolean returnObject_isEncrypted(MessageItem anItem)
        {
            return anItem.isEncrypted;
        }
    }
}
