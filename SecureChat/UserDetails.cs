using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Diagnostics;

namespace SecureChat
{
    class UserDetails
    {
        [JsonProperty("url")]
        public string user_url { get; set; }

        [JsonProperty("username")]
        public string user_name { get; set; }

        //THIS IS A LINK TO THE USER'S PUBLIC KEY
        [JsonProperty("publicKey")]
        public string public_key { get; set; }

        [JsonProperty("conversationLeader")]
        public List<string> conversations_leading { get; set; }

        [JsonProperty("conversationFollower")]
        public List<string> conversations_following { get; set; }

        //THIS IS THE ACTUAL PUBLIC KEY IN STRING FORM
        [JsonIgnore]
        public string public_key_ACTUAL { get; set; }

        //mmmhmm
    }
}
