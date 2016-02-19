using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace SecureChat
{
    public class MessageItem
    {
        string message;
        string userid;
        string timeStamp;
        Boolean isEncrypted;

        public void storeData(string msg, string theUser, string sentTime, Boolean isSecured, IBuffer publicKey)
        {
            this.userid = theUser;
            this.timeStamp = sentTime;
            this.isEncrypted = isSecured;
            if(this.isEncrypted == false)
            {
                this.message = msg;
            } else
            {
                this.message = Crypto.Encrypt(App.strAsymmetricAlgName, publicKey, msg);
            }
        }

        public string messageText()
        {
            string output = "(" + this.timeStamp + ")" + " " + this.userid + ": " + this.message;
            return output;
        }

        public string messageTextDecrypt()
        {
            string output;
            if (this.isEncrypted == true)
            {
                String decryptedMessage = Crypto.Decrypt(App.strAsymmetricAlgName, Crypto.returnPrivateKey(App.secrets), this.message);
                output = "(" + this.timeStamp + ")" + " " + this.userid + ": " + "**DECRYPTED MESSAGE**:  " + decryptedMessage;
            }
            else {
                output = "(" + this.timeStamp + ")" + " " + this.userid + ": " + this.message;
            }

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
