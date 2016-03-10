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
using Windows.UI.Popups;
using Windows.Storage.Streams;



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

            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize = new Size { Height = 720, Width = 1280 };
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.PreferredLaunchViewSize;

            //Initialize user list from Server
            App.userManagement.initUserAndConversation(currentUserList);
            //App.userManagement.getAvailableConversations(friendsList);

            //Initialize RSA cryptography service
            App.secrets.initCrypto();
            //loadLastConversation();


        }





//NON-USER INTERFACE METHODS

        //Verify keys with client and server
        private async void verifyKeys()
        {
            //Convert client key from IBUFFER to STRING
            IBuffer clientPublicKeyBUFFER = Crypto.returnPublicKey(App.secrets);
            byte[] clientPublicKeyBYTE = new byte[512];
            WindowsRuntimeBufferExtensions.CopyTo(clientPublicKeyBUFFER, clientPublicKeyBYTE);
            string clientPublicKeySTRING = Convert.ToBase64String(clientPublicKeyBYTE);

            String serverPublicKey;

            //GET request to server (fetch new messages from server at the same time)
            string publicKeyURL = App.userManagement.getPublicKey_string();
            if (publicKeyURL != null) { 
                using (HttpClient client = new HttpClient()) //using block makes the object disposable (one time use)
                {
                    using (HttpResponseMessage response = await client.GetAsync(publicKeyURL))      //BUG:  HANDLE OFFLINE MODE
                    {
                        using (HttpContent content = response.Content)
                        {
                            string content_string = await content.ReadAsStringAsync();
                            System.Net.Http.Headers.HttpContentHeaders content_headers = content.Headers;
                            dynamic incomingJSON = JsonConvert.DeserializeObject(content_string);
                            serverPublicKey = incomingJSON.key;
                        }
                    }
                }
            } else
            {
                serverPublicKey = "-1";
            }
            if(clientPublicKeySTRING != serverPublicKey)
            {
                var dialog = new MessageDialog("Please Update your public key");
                dialog.Title = "WARNING:  Local and Server public keys are not in sync";
                dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                var res = await dialog.ShowAsync();
            }
        }







        //Loads conversation for selected user
        /*private async void loadLastConversation()
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
                            ListViewItem incomingItems = new ListViewItem();

                            //Outputs desired text into the actual chat window
                            incomingItems.Content = incomingMessages[x].messageText();         
                                         
                            //References the MessageItem object
                            incomingItems.Tag = incomingMessages[x];                                                
                            chatListView.Items.Add(incomingItems);
                        }
                    }
                }
            }
        } */








        //POST FUNCTION
        private async void postMessage(String input, Boolean secureStatus)
        {
            if (input != String.Empty && App.userManagement.conversationIsNotNull())
            {

                //Encrypt text from input box if encryption is on
                // String timeStamp = DateTime.Now.ToString("MM/d/yy h:mm tt");           
                String finalInput;

                if(secureStatus == false) {
                    finalInput = input;
                } else {
                    finalInput = Crypto.Encrypt(App.strAsymmetricAlgName, Crypto.returnPublicKey_OTHER_USER(App.secrets), input);
                }

                //Load the text and associated message data into a messageItem object
                MessageItem newMessage = new MessageItem { conversation= App.userManagement.getConversationURL_string(),
                                                           message = finalInput,
                                                           userSent = App.userManagement.getOtherUserURL_string(),
                                                           //timeStamp = timeStamp,
                                                           userid = App.userManagement.getCurrentUser_string(),
                                                           isEncrypted = secureStatus
                                                         };
                
                //Create a Key and Pair match with the new messageItem object data in preparation of Http request
                var values = new Dictionary<string, string>
                {
                    { "conversation", newMessage.returnObject_conversation(newMessage) },
                    { "encrypted", newMessage.returnObject_isEncryptedString(newMessage) },
                    { "sentTo", "http://159.203.252.197/users/1/" },
                    { "text" , newMessage.returnObject_message(newMessage) }

                };

                //Encode the key pair match in preparation of Http request
                var theContent = new FormUrlEncodedContent(values);


                //POST request to the server with the encoded data (create HttpClient)
                using (HttpClient client = new HttpClient()) //using block makes the object disposable (one time use)
                {

                    //Check HTTP data
                    if (App.DEBUG_MODE == true)
                    {
                        Debug.WriteLine("POST CREDENTIALS:  "+App.userManagement.getCurrentUser_string() + ":" + currentUserPasswordActual.Password);
                    }

                    //Load User credentials from currentUser and password field located on user interface into the client object
                    var byteArray = Encoding.ASCII.GetBytes(App.userManagement.getCurrentUser_string() + ":"+currentUserPasswordActual.Password);
                    var header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    client.DefaultRequestHeaders.Authorization = header;

                    //Post the message to the server
                    using (HttpResponseMessage response = await client.PostAsync("http://159.203.252.197/messages/", theContent)) {

                        //Check HTTP data
                        if (App.DEBUG_MODE) {
                            Debug.WriteLine("POST Status Code:  " + response.StatusCode);
                            Debug.WriteLine("POST Reason: " + response.ReasonPhrase);

                            using (HttpContent content = response.Content) {
                                string content_string = await content.ReadAsStringAsync();
                                Debug.WriteLine("POST contents:  "+content_string);
                            }
                        }

                        //Refreshes ChatListView with recently POSTed string.  If credentials are wrong, output INVALID CREDENTIALS to chatwindow
                        if (response.IsSuccessStatusCode == true) {
                            //GET request to server to update chatListView with recentely POSTed message
                            using (HttpClient client2 = new HttpClient()) //using block makes the object disposable (one time use)
                            {
                                using (HttpResponseMessage response2 = await client2.GetAsync("http://159.203.252.197/messages/"))
                                {
                                    //Check HTTP data
                                    if (App.DEBUG_MODE)
                                    {
                                        Debug.WriteLine("GET Status Code:  " + response2.StatusCode);
                                        Debug.WriteLine("GET Reason: " + response2.ReasonPhrase);
                                    }

                                    using (HttpContent content2 = response2.Content)
                                    {
                                        string content_string2 = await content2.ReadAsStringAsync();
                                        System.Net.Http.Headers.HttpContentHeaders content_headers2 = content2.Headers;

                                        //Check HTTP data
                                        if (App.DEBUG_MODE)
                                        {
                                            Debug.WriteLine("GET content:  " + content_string2);
                                            Debug.WriteLine("GET content headers:  " + content_headers2);
                                        }

                                        //TEMPORARY METHOD TO POST MESSAGES FROM SERVER TO CHAT VIEW
                                        List<MessageItem> incomingMessages = JsonConvert.DeserializeObject<List<MessageItem>>(content_string2);
                                        ListViewItem incomingItems = new ListViewItem();
                                        int lastMessage = incomingMessages.Count - 1;
                                        incomingItems.Content = incomingMessages[lastMessage].messageText();                      //Outputs desired text into the actual chat window
                                        incomingItems.Tag = incomingMessages[lastMessage];                                                 //References the MessageItem object
                                        chatListView.Items.Add(incomingItems);
                                    }
                                }
                            }
                        } else {
                            var dialog = new MessageDialog("");
                            dialog.Title = "INVALID USER/PASSWORD COMBINATION";
                            dialog.Content = "You have entered the wrong password for the selected user, please try again";
                            dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                            var res = await dialog.ShowAsync();
                        }
                    }
                }
            }
            //Clear textbox and auto scroll to the bottom
            inputBox.Text = String.Empty;
            chatListViewScroller.UpdateLayout();
            chatListViewScroller.ScrollToVerticalOffset(chatListView.ActualHeight);
        }






//USER INTERFECT METHODS

        //INPUT WINDOW CLICK POST
        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            string newMsg = inputBox.Text;
            postMessage(newMsg, App.isSecureEnabled);
        }






        //INPUT WINDOW ENTER POST
        private void inputBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string newMsg = (inputBox.Text).TrimEnd(' ','\r', '\n');
                postMessage(newMsg, App.isSecureEnabled);
            }

        }





        //CURRENT USER SELECTION:  UPDATE CURRENT USER
        private void currentUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewItem temp = (ListViewItem) currentUserList.SelectedItems[0];
            friendsList.Items.Clear();
            App.userManagement.setCurrentUser((UserDetails)temp.Tag);
            App.userManagement.getAvailableConversations(friendsList);
            string selectedUser = (string) temp.Content;
            verifyKeys();
            chatListView.Items.Clear();
        }






        //CHATLISTVIEW SELECTION:  (DEBUG MODE)  OUTPUT CONTENTS OF CHAT OBJECT TO FRIENDS
        private void chatListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try {
                ListViewItem temp = (ListViewItem)chatListView.SelectedItems[0];
                MessageItem msg = (MessageItem)temp.Tag;
                if (App.DEBUG_MODE == true)
                {
                    friendsList.Items.Clear();
                    friendsList.Items.Add("==DEBUG MODE OUTPUT==");
                    friendsList.Items.Add(msg.returnObject_message(msg));
                    friendsList.Items.Add(msg.returnObject_timeStamp(msg));
                    friendsList.Items.Add(msg.returnObject_userSent(msg));
                    friendsList.Items.Add(msg.returnObject_isEncrypted(msg).ToString());
                }
            }          
            catch (System.Runtime.InteropServices.COMException)
            {
                //Band aid 
            }
        }




        //BUTTON:  GO SECURE
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




        //BUTTON:  DECRYPT
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


        //FRIENDS SELECTION:  LOAD CONVERSATION
        private async void friendsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (friendsList.Items.Count > 0)
            {

                string conversationURL = (string)friendsList.SelectedItems[0];
                await App.userManagement.parseSelectedConversation(conversationURL);
                if (App.DEBUG_MODE == true)
                {
                    Debug.WriteLine(conversationURL);
                }
                await App.userManagement.loadLastConversation(chatListView);
            }
            chatListViewScroller.UpdateLayout();
            chatListViewScroller.ScrollToVerticalOffset(chatListView.ActualHeight);
        }



        //BUTTON:  UPDATE CRYPTO KEY
        private async void updatePublicKey_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Updating your current key will cause previous messages to no longer be decryptable");
            dialog.Title = "Are you sure?";
            dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
            dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
            var res = await dialog.ShowAsync();

            IBuffer clientPublicKeyBUFFER = Crypto.returnPublicKey(App.secrets);
            byte[] clientPublicKeyBYTE = new byte[512];
            WindowsRuntimeBufferExtensions.CopyTo(clientPublicKeyBUFFER, clientPublicKeyBYTE);
            string clientPublicKeySTRING = Convert.ToBase64String(clientPublicKeyBYTE);

            //Create a Key and Pair match with object data (prepared for POST request)
            var values = new Dictionary<string, string>
                {
                    { "key", clientPublicKeySTRING },
                    { "user", App.userManagement.getURL_string() }

                };
            var theContent = new FormUrlEncodedContent(values);

            if ((int)res.Id == 0)
            {
                //PUT REQUEST
                string publicKeyURL = App.userManagement.getPublicKey_string();
                using (HttpClient client = new HttpClient()) //using block makes the object disposable (one time use)
                {
                    var byteArray = Encoding.ASCII.GetBytes(App.userManagement.getCurrentUser_string() + ":" + currentUserPasswordActual.Password);
                    var header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    client.DefaultRequestHeaders.Authorization = header;
                    using (HttpResponseMessage response = await client.PutAsync(publicKeyURL, theContent))       //BUG:  HANDLE OFFLINE MODE
                    {
                        using (HttpContent content = response.Content)
                        {
                            string content_string = await content.ReadAsStringAsync();
                            System.Net.Http.Headers.HttpContentHeaders content_headers = content.Headers;
                        }
                    }
                }
                MessageDialog msgbox2 = new MessageDialog("Overwriting serverside key");
                await msgbox2.ShowAsync();
            }

            if ((int)res.Id == 1)
            {
                //DO NOTHING
                MessageDialog msgbox2 = new MessageDialog("Cancelling request");
                await msgbox2.ShowAsync();
            }
        }

        private async void refersh_Click(object sender, RoutedEventArgs e)
        {
            if (App.userManagement.conversationIsNotNull())
            {
                chatListView.Items.Clear();
                await App.userManagement.loadLastConversation(chatListView);
                chatListViewScroller.UpdateLayout();
                chatListViewScroller.ScrollToVerticalOffset(chatListView.ActualHeight);
            }
        }
    }
}
