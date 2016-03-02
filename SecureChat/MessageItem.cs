using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Newtonsoft.Json;

namespace SecureChat
{
    public class MessageItem
    {
        [JsonProperty("conversation")]
       public string conversation { get; set; }

        [JsonProperty("sentTime")]
        public string timeStamp { get; set; }

        [JsonProperty("encrypted")]
        public Boolean isEncrypted { get; set; }

        [JsonProperty("sentTo")]
        public string userSent { get; set; }

        [JsonProperty("text")]
        public string message { get; set; }

        [JsonIgnoreAttribute]
        public string userid { get; set; }


       /* public void storeData(string msg, string theUser, string sentTime, Boolean isSecured, IBuffer publicKey)
        {
            this.userid = theUser;
            this.timeStamp = sentTime;
            this.isEncrypted = isSecured;
            //if(this.isEncrypted == false)
            //{
            //    this.message = msg;
            //} else
            //{
            //    this.message = Crypto.Encrypt(App.strAsymmetricAlgName, publicKey, msg);
            //}
        }*/

        public override string ToString()
        {
            return string.Format("conversation: {0}, timeStamp: {1}, encrypted: {2}, sentTo {3}, text: {4}", conversation, timeStamp, isEncrypted, "http://159.203.252.197/users/1/",message);
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

        public string returnObject_conversation(MessageItem anItem)
        {
            return anItem.conversation;
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
        public string returnObject_isEncryptedString(MessageItem anItem)
        {
            return anItem.isEncrypted.ToString();
        }
        public Boolean returnObject_isEncrypted(MessageItem anItem)
        {
            return anItem.isEncrypted;
        }
    }
}
