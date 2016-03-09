using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Net.Http.Headers;

namespace SecureChat
{
    //Primary Class that Handles the user list
    class UserAndConversation                       
    {
        class ConversationDetails
        {
            [JsonProperty("url")]
            public string conversation_url { get; set; }

            [JsonProperty("participant_1")]
            public string participant_1 { get; set; }

            [JsonProperty("participant_2")]
            public string participant_2 { get; set; }

            [JsonProperty("messages")]
            public List<String> conversation_Messages { get; set; }

        }

        //Class variables
        List<UserDetails> userList;

        //You
        UserDetails currentUser;

        //Person you're speaking with
        UserDetails otherUser;


        ConversationDetails currentConversation;


        //Class Methods
        public async void initUserAndConversation(ListView aCurrentUserListView) {
            //GET request to server to update chatListView with recentely POSTed message
            using (HttpClient client = new HttpClient()) //using block makes the object disposable (one time use)
            {
                using (HttpResponseMessage response = await client.GetAsync("http://159.203.252.197/users/"))
                {
                    if (App.DEBUG_MODE)
                    {
                        Debug.WriteLine("GET Status Code:  " + response.StatusCode);
                        Debug.WriteLine("GET Reason: " + response.ReasonPhrase);
                    }
                    using (HttpContent content = response.Content)
                    {
                        string content_string = await content.ReadAsStringAsync();
                        System.Net.Http.Headers.HttpContentHeaders content_headers = content.Headers;
                        if (App.DEBUG_MODE)
                        {
                            Debug.WriteLine("GET content:  " + content_string);
                            Debug.WriteLine("GET content headers:  " + content_headers);
                        }

                        //TEMPORARY METHOD TO POST MESSAGES FROM SERVER TO CHAT VIEW
                        this.userList = JsonConvert.DeserializeObject<List<UserDetails>>(content_string);
                        Debug.WriteLine("CONVERSATION PRINT:  "+userList[0].conversations_leading[0]);
                        for (int x = 0; x < userList.Count; x++)
                        {
                            ListViewItem aUser = new ListViewItem();
                            aUser.Content = userList[x].user_name;                 //Outputs desired text into the actual chat window
                            aUser.Tag = userList[x];                               //References the UserAndConversation object
                            aCurrentUserListView.Items.Add(aUser);
                        }
                        currentUser = userList[0];
                    }
                }
            }
        }

        public string getCurrentUser_string()
        {
            return currentUser.user_name;
        }

        //RETURNS PUBLIC KEY URL
        public string getPublicKey_string()
        {
            return currentUser.public_key;
        }

        //RETURNS THE ACTUAL PUBLIC KEY
        public string getPublicKeyACTUAL_string()
        {
            return currentUser.public_key_ACTUAL;
        }

        public string getURL_string()
        {
            return currentUser.user_url;
        }

        public void setCurrentUser(UserDetails userObject)
        {
            currentUser = userObject;
        }

        public string getConversationURL_string()
        {
            return currentConversation.conversation_url;
        }


        public string getOtherUserURL_string()
        {
            return otherUser.user_url;
        }

        public string getOtherUser_string()
        {
            return otherUser.user_name;
        }

        public Boolean conversationIsNotNull()
        {
            if (currentConversation != null)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public void getAvailableConversations(ListView friendsList) {
            if (currentUser != null)
            {
                for (int x = 0; x < currentUser.conversations_leading.Count; x++)
                {
                    friendsList.Items.Add(currentUser.conversations_leading[x]);
                }
                for (int x = 0; x < currentUser.conversations_following.Count; x++)
                {
                    friendsList.Items.Add(currentUser.conversations_following[x]);
                }
            }
        }

        //Loads conversation for the currentUser
        public async void loadLastConversation(ListView aChatListView)
        {
            //GET request to server (fetch new messages from server at the same time)
            using (HttpClient client = new HttpClient()) //using block makes the object disposable (one time use)
            {
                using (HttpResponseMessage response = await client.GetAsync("http://159.203.252.197/messages"))      //BUG:  HANDLE OFFLINE MODE
                {
                    if (App.DEBUG_MODE)
                    {
                        Debug.WriteLine("GET Status Code:  " + response.StatusCode);
                        Debug.WriteLine("GET Reason: " + response.ReasonPhrase);
                    }
                    using (HttpContent content = response.Content)
                    {
                        string content_string = await content.ReadAsStringAsync();
                        System.Net.Http.Headers.HttpContentHeaders content_headers = content.Headers;
                        if (App.DEBUG_MODE)
                        {
                            Debug.WriteLine("GET content:  " + content_string);
                            Debug.WriteLine("GET content headers:  " + content_headers);
                        }

                        //Load messages into chat list window
                        List<MessageItem> incomingMessages = JsonConvert.DeserializeObject<List<MessageItem>>(content_string);
                        for (int x = 0; x < incomingMessages.Count; x++)
                        {
                            //FILTER MESSAGES BASED ON PARTICIPANTS
                            string checkUser = incomingMessages[x].returnObject_userid(incomingMessages[x]);


                            if (checkUser == currentUser.user_name || checkUser == otherUser.user_name)
                            {
                                ListViewItem incomingItems = new ListViewItem();

                                //Outputs desired text into the actual chat window
                                incomingItems.Content = incomingMessages[x].messageText();

                                //References the MessageItem object
                                incomingItems.Tag = incomingMessages[x];
                                aChatListView.Items.Add(incomingItems);
                            }
                        }
                    }
                }
            }
        }

       /* public async void pullOtherUserPublicKeyFromServer()
        {
            HttpClient subClient = new HttpClient();
            HttpResponseMessage subResponse = await subClient.GetAsync(otherUser.public_key);
            HttpContent subContent = subResponse.Content;
            string content_string2 = await subContent.ReadAsStringAsync();
            Debug.WriteLine("PBCS:  "+content_string2);
            dynamic incomingJSON = JsonConvert.DeserializeObject(content_string2);
            otherUser.public_key_ACTUAL= incomingJSON.key;
            Debug.WriteLine("PUB KEY ACTUAL:  "+otherUser.public_key_ACTUAL);
        }*/

        public async void parseSelectedConversation(string urlOfConversation)
        {
            using (HttpClient client = new HttpClient()) //using block makes the object disposable (one time use)
            {
                using (HttpResponseMessage response = await client.GetAsync(urlOfConversation))
                {
                    using (HttpContent content = response.Content)
                    {
                        string content_string = await content.ReadAsStringAsync();
                        System.Net.Http.Headers.HttpContentHeaders content_headers = content.Headers;
                        currentConversation = JsonConvert.DeserializeObject<ConversationDetails>(content_string);

                        //Load the otherUser variable (person you're speaking with) by matching the currentUser's url tag with participants
                        //If they match, set the otherUser variable to the other participant in the conversation
                        if (currentConversation.participant_1 == currentUser.user_url)
                        {
                            HttpClient subClient = new HttpClient();
                            HttpResponseMessage subResponse = await subClient.GetAsync(this.currentConversation.participant_2);
                            HttpContent subContent = subResponse.Content;
                            string content_string2 = await subContent.ReadAsStringAsync();
                            //System.Net.Http.Headers.HttpContentHeaders content_headers2 = subContent.Headers;
                            otherUser = JsonConvert.DeserializeObject<UserDetails>(content_string2);
                        } else if (currentConversation.participant_2 == currentUser.user_url)
                        {
                            HttpClient subClient = new HttpClient();
                            HttpResponseMessage subResponse = await subClient.GetAsync(this.currentConversation.participant_1);
                            HttpContent subContent = subResponse.Content;
                            string content_string2 = await subContent.ReadAsStringAsync();
                            //System.Net.Http.Headers.HttpContentHeaders content_headers2 = subContent.Headers;
                            otherUser = JsonConvert.DeserializeObject<UserDetails>(content_string2);
                        }

                        {
                            HttpClient subClient = new HttpClient();
                            HttpResponseMessage subResponse = await subClient.GetAsync(otherUser.public_key);
                            HttpContent subContent = subResponse.Content;
                            string content_string2 = await subContent.ReadAsStringAsync();
                            Debug.WriteLine("PBCS:  " + content_string2);
                            dynamic incomingJSON = JsonConvert.DeserializeObject(content_string2);
                            otherUser.public_key_ACTUAL = incomingJSON.key;
                            Debug.WriteLine("PUB KEY ACTUAL:  " + otherUser.public_key_ACTUAL);
                        }

                        if (App.DEBUG_MODE == true)
                        {
                            Debug.WriteLine("CONVO URL:  "+this.currentConversation.conversation_url);
                            Debug.WriteLine("PART 1:  "+this.currentConversation.participant_1);
                            Debug.WriteLine("PART 2:  "+this.currentConversation.participant_2);
                            for (int x = 0; x < this.currentConversation.conversation_Messages.Count; x++)
                            {
                                Debug.WriteLine("MESSAGES:  "+this.currentConversation.conversation_Messages[x]);
                            }
                        }
                    }
                }
            }
            App.secrets.loadOtherUserPublicKey(otherUser.public_key_ACTUAL);
        }
    }
}
