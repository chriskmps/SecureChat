using System;
using System.Collections.Generic;
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
            foreach (string x in App.Users)
            {
                currentUserList.Items.Add(x);
            }


        }


        // Post from inputBox to chatListView
        private void postMessage(String input, Boolean secureStatus)
        {
            if (input != String.Empty)
            {
                //Store text from input box and associated data into MessageItem object
                MessageItem newMessage = new MessageItem();
                String timeStamp = DateTime.Now.ToString("MM/d/yy h:mm tt");           //Should this be generated locally or on backend?
                newMessage.storeData(input, App.currentUser, timeStamp, secureStatus, Crypto.returnPublicKey(App.secrets));

                //Store that MessageItem object into chatListView list using a ListViewItem object
                ListViewItem item = new ListViewItem();
                item.Content = newMessage.messageText();                               //Outputs desired text into the actual chat window
                item.Tag = newMessage;                                                 //References the MessageItem object
                chatListView.Items.Add(item);
                inputBox.Text = "";                                                    //Reset input text field to empty

                
            }
        }

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            string newMsg = inputBox.Text;
            postMessage(newMsg, App.isSecureEnabled);
        }

        private void inputBox_KeyDown(object sender, KeyRoutedEventArgs e)
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
            friendsList.Items.Add(msg.returnObject_userid(msg));
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
