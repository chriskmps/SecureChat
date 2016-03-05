using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Security.Cryptography;
using System.Diagnostics;
using Newtonsoft.Json;
using Windows.Storage;
using System.Net.Http;
using System.Net.Http.Headers;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409


namespace SecureChat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary   

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            //Populate User List
            App.secrets.initCrypto();
            loadLastConversation();
            foreach (string x in App.Users)
            {
                currentUserList.Items.Add(x);
            }
        }

        private async void loadLastConversation()
        {
            //GET request to server (fetch new messages from server at the same time)
            using (HttpClient client = new HttpClient()) //using block makes the object disposable (one time use)
            {
                using (HttpResponseMessage response = await client.GetAsync("http://159.203.252.197/messages"))
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
                        List<MessageItem> incomingMessages = JsonConvert.DeserializeObject<List<MessageItem>>(content_string);
                        for (int x = 0; x < incomingMessages.Count; x++)
                        {
                            ListViewItem incomingItems = new ListViewItem();
                            incomingItems.Content = incomingMessages[x].messageText();                      //Outputs desired text into the actual chat window
                            incomingItems.Tag = incomingMessages[x];                                                 //References the MessageItem object
                            chatListView.Items.Add(incomingItems);
                        }
                    }
                }
            }
        }

        // Post from inputBox to chatListView
        private async void postMessage(String input, Boolean secureStatus)
        {
            if (input != String.Empty)
            {
                //Store text from input box and associated data into MessageItem object
                String timeStamp = DateTime.Now.ToString("MM/d/yy h:mm tt");           //Should this be generated locally or on backend?
                String finalInput;
                if(secureStatus == false) {
                    finalInput = input;
                } else {
                    finalInput = Crypto.Encrypt(App.strAsymmetricAlgName, Crypto.returnPublicKey(App.secrets), input);
                }
                //Create new a instance of the newMesage Class and load it with the message data
                MessageItem newMessage = new MessageItem { conversation= "http://159.203.252.197/conversations/2/", message = finalInput, userSent = "http://159.203.252.197/users/1/", timeStamp = timeStamp, userid = App.currentUser, isEncrypted = secureStatus };
                
                //Create a Key and Pair match with object data (prepared for POST request)
                var values = new Dictionary<string, string>
                {
                    { "conversation", newMessage.returnObject_conversation(newMessage) },
                    { "encrypted", newMessage.returnObject_isEncryptedString(newMessage) },
                    { "sentTo", "http://159.203.252.197/users/1/" },
                    { "text" , newMessage.returnObject_message(newMessage) }

                };
                var theContent = new FormUrlEncodedContent(values);


                //POST request to server ("using" term creates a one time use instance of a class)
                using (HttpClient client = new HttpClient()) //using block makes the object disposable (one time use)
                {
                    //Authentication data (Currently hardcoded)
                    var byteArray = Encoding.ASCII.GetBytes("Chris:chris1234");
                    var header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    client.DefaultRequestHeaders.Authorization = header;

                    //Request and Response to server
                    using (HttpResponseMessage response = await client.PostAsync("http://159.203.252.197/messages/", theContent))

                    //View raw POST output here
                    //using (HttpResponseMessage response = await client.PostAsync("http://posttestserver.com/post.php", theContent))

                    {
                        if (App.DEBUG_MODE)
                        {
                            Debug.WriteLine("POST Status Code:  " + response.StatusCode);
                            Debug.WriteLine("POST Reason: " + response.ReasonPhrase);

                            using (HttpContent content = response.Content)
                            {
                                string content_string = await content.ReadAsStringAsync();
                                Debug.WriteLine("POST contents:  "+content_string);
                            }
                        }
                    }
                }

                //GET request to server to update chatListView with recentely POSTed message
                using (HttpClient client = new HttpClient()) //using block makes the object disposable (one time use)
                {
                    using (HttpResponseMessage response = await client.GetAsync("http://159.203.252.197/messages"))
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
                            List<MessageItem> incomingMessages = JsonConvert.DeserializeObject<List<MessageItem>>(content_string);
                            ListViewItem incomingItems = new ListViewItem();
                            int lastMessage = incomingMessages.Count - 1;
                            incomingItems.Content = incomingMessages[lastMessage].messageText();                      //Outputs desired text into the actual chat window
                            incomingItems.Tag = incomingMessages[lastMessage];                                                 //References the MessageItem object
                            chatListView.Items.Add(incomingItems);
                        }
                    }
                }

                /* //Update the Chat window by adding newMessage object to the chatListView list view (OLD) 
                ListViewItem item = new ListViewItem();
                item.Content = newMessage.messageText();                               //Outputs desired text into the actual chat window
                item.Tag = newMessage;                                                 //References the MessageItem object
                chatListView.Items.Add(item);
                inputBox.Text = "";                                                    //Reset input text field to empty */
            }
        }

        private async void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            string newMsg = inputBox.Text;
            postMessage(newMsg, App.isSecureEnabled);
        }

        private async void inputBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                string newMsg = (inputBox.Text).TrimEnd(' ','\r', '\n');
                postMessage(newMsg, App.isSecureEnabled);
            }

        }

        private void currentUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.currentUser = currentUserList.SelectedItem.ToString();
        }

        private void chatListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewItem temp = (ListViewItem) chatListView.SelectedItems[0];
            MessageItem msg = (MessageItem) temp.Tag;
            friendsList.Items.Clear();
            friendsList.Items.Add(msg.returnObject_message(msg));
            friendsList.Items.Add(msg.returnObject_timeStamp(msg));
            friendsList.Items.Add(msg.returnObject_userSent(msg));                            
            friendsList.Items.Add(msg.returnObject_isEncrypted(msg).ToString());
        }

        private void buttonSecure_Click(object sender, RoutedEventArgs e)
        {
            if(App.isSecureEnabled)
            {
                App.isSecureEnabled = false;
                chatWindowBackground.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                buttonSecure.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                buttonSecure.Content = "Secure";
            } else
            {
                App.isSecureEnabled = true;
                Debug.Write(App.isSecureEnabled);
                chatWindowBackground.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                buttonSecure.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                buttonSecure.Content = "Unsecure";
            }
        }

        private void buttonDecrypt_Click(object sender, RoutedEventArgs e)
        {
            if (chatListView.Items.Any() != false) { 
                ListViewItem temp = (ListViewItem)chatListView.SelectedItems[0];
                if (temp != null)
                {
                    MessageItem msg = (MessageItem)temp.Tag;
                    string encryptedMessage = msg.returnObject_message(msg);
                    if (msg.returnObject_isEncrypted(msg) == true)
                    {
                        String decryptedMessage = Crypto.Decrypt(App.strAsymmetricAlgName, Crypto.returnPrivateKey(App.secrets), encryptedMessage);
                        temp.Content = "(" + msg.returnObject_timeStamp(msg) + ")" + " " + msg.returnObject_userid(msg) + ": " + "**DECRYPTED MESSAGE**:  " + decryptedMessage;
                    }
                }
            }
        }
    }
}
