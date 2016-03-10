using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;

namespace SecureChat
{
    public class Crypto
    {
        Boolean keyPairExists;
        String strAsymmetricAlgName;
        UInt32 asymmetricKeyLength;
        IBuffer buffPublicKey_OTHER_USER;
        IBuffer buffPublicKey;
        IBuffer buffPrivateKeyStorage;
        byte[] publicKeyByteVersion = new byte[512];

        //Initialize the new Crypto object (initialized only once per app startup)
        public async void initCrypto()
        {
            this.strAsymmetricAlgName = AsymmetricAlgorithmNames.RsaPkcs1;
            this.asymmetricKeyLength = 512;

            //Checks SecureChat's folder if a key pair already exists and set keyPairExists boolean
            Windows.Storage.StorageFolder localAppFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            string cryptoFilePrivate = "SecureChatPrivateKeys.sckey";  //STORED AS BYTE DATA
            string cryptoFilePublic = "SecureChatPublicKey.sckey";     //STORED AS TEXT DATA
            if ((await localAppFolder.TryGetItemAsync(cryptoFilePublic) != null) && (await localAppFolder.TryGetItemAsync(cryptoFilePrivate) != null)) {
                this.keyPairExists = true;
            } else {
                this.keyPairExists = false;
            }
            //Load Keys depending on keyPairExists value
            if (this.keyPairExists == true) {
                //DIRECT IBUFFER
                //StorageFile loadedCryptoFilePublic = await localAppFolder.GetFileAsync(cryptoFilePublic);
                //this.buffPublicKey = await FileIO.ReadBufferAsync(loadedCryptoFilePublic);

                //FROM BYTE
                //StorageFile loadedCryptoFilePublic = await localAppFolder.GetFileAsync("BytePubKey.sckey");
                //this.buffPublicKey = await FileIO.ReadBufferAsync(loadedCryptoFilePublic);

                //Open Public Key File.  Convert key from STRING to BYTE and then convert to IBUFFER
                StorageFile loadedCryptoFilePublic = await localAppFolder.GetFileAsync(cryptoFilePublic);
                String publicKeyStringVersion = await FileIO.ReadTextAsync(loadedCryptoFilePublic);
                this.publicKeyByteVersion = Convert.FromBase64String(publicKeyStringVersion);
                this.buffPublicKey = this.publicKeyByteVersion.AsBuffer();

                //Open Private Key File
                StorageFile loadedCryptoFilePrivate = await localAppFolder.GetFileAsync(cryptoFilePrivate);
                this.buffPrivateKeyStorage = await FileIO.ReadBufferAsync(loadedCryptoFilePrivate);

            } else {
                //Generate new key pair
                CryptographicKey temp = this.CreateAsymmetricKeyPair(strAsymmetricAlgName, asymmetricKeyLength, out buffPublicKey, out buffPrivateKeyStorage);

                //Convert public key from IBUFFER type to BYTE type.  Convert from BYTE type to STRING type
                WindowsRuntimeBufferExtensions.CopyTo(this.buffPublicKey, this.publicKeyByteVersion);
                string publicKeyStringVersion = Convert.ToBase64String(this.publicKeyByteVersion);

                //Store keys in appropriate files (Public as PLAIN TEXT, Private as IBUFFER)
                await FileIO.WriteTextAsync((await localAppFolder.CreateFileAsync(cryptoFilePublic)), publicKeyStringVersion);
                await FileIO.WriteBufferAsync((await localAppFolder.CreateFileAsync(cryptoFilePrivate)), this.buffPrivateKeyStorage);
            }
        }







// Class functions
        public void loadOtherUserPublicKey(string otherUserKey)
        {
            Debug.WriteLine("OTHER USER'S PUBLIC KEY:  "+otherUserKey);
            byte[] tempByte = new byte[512];
            tempByte = Convert.FromBase64String(otherUserKey);
            this.buffPublicKey_OTHER_USER = tempByte.AsBuffer();
        }

        public static IBuffer returnPublicKey_OTHER_USER(Crypto cryptoHolder)
        {
            return cryptoHolder.buffPublicKey_OTHER_USER;
        }

        public static IBuffer returnPublicKey(Crypto cryptoHolder)
        {
            return cryptoHolder.buffPublicKey;
        }

        public static IBuffer returnPrivateKey(Crypto cryptoHolder)
        {
            return cryptoHolder.buffPrivateKeyStorage;
        }


        //Encrypt Data (temporarily encrpyting with my own public key)
       // public static string Encrypt(String strAsymmetricAlgName, IBuffer buffPublicKey, string message
        public static string Encrypt(String strAsymmetricAlgName, IBuffer buffPublicKey_OTHER_USER, string message)
        {
            //Load RSA.pkcs algorithm
            AsymmetricKeyAlgorithmProvider objAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Import the public key from a buffer.
            CryptographicKey publicKey = objAlgProv.ImportPublicKey(buffPublicKey_OTHER_USER);

            //Convert message String to IBuffer
            byte[] plainText = Encoding.UTF8.GetBytes(message); // Data to encrypt

            // Encrypt the session key by using the public key.
            IBuffer buffEncryptedMessage = CryptographicEngine.Encrypt(publicKey, CryptographicBuffer.CreateFromByteArray(plainText), null);

            //Return encrypted message as a string
            return CryptographicBuffer.EncodeToBase64String(buffEncryptedMessage);
        }



        //Decrypts Data (Always using private key)
        public static string Decrypt(String strAsymmetricAlgName, IBuffer buffPrivateKeyStorage, string encryptedMessage)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAsymmAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Import the public key from a buffer. You should keep your private key
            // secure. For the purposes of this example, however, the private key is
            // just stored in a static class variable.
            CryptographicKey keyPair = objAsymmAlgProv.ImportKeyPair(buffPrivateKeyStorage);

            //Convert message String to IBuffer
            IBuffer convertedString = CryptographicBuffer.DecodeFromBase64String(encryptedMessage);
            IBuffer buffDecryptedMessage;
            try {
                // Use the private key embedded in the key pair to decrypt the session key.
                buffDecryptedMessage = CryptographicEngine.Decrypt(keyPair, convertedString, null);
            } catch (System.ArgumentException) {
                string invalidDecryptionMessage = "INVALID DECRYPTION KEY";
                return invalidDecryptionMessage;
            }

            //Return decrpyted message as a string
            string decryptedMessage = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffDecryptedMessage);
            return decryptedMessage;
        }
        

        //Generates a new asymmetric key pair if keypair file does not exist (Credit to MDSN Microsoft Libraries)
        public CryptographicKey CreateAsymmetricKeyPair(String strAsymmetricAlgName, UInt32 keyLength, out IBuffer buffPublicKey, out IBuffer buffPrivateKeyStorage)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Demonstrate use of the AlgorithmName property (not necessary to create a key pair).
            String strAlgName = objAlgProv.AlgorithmName;

            // Create an asymmetric key pair.
            CryptographicKey keyPair = objAlgProv.CreateKeyPair(keyLength);

            // Export the public key to a buffer for use by others.
            buffPublicKey = keyPair.ExportPublicKey();

            // You should keep your private key (embedded in the key pair) secure. For  
            // the purposes of this example, however, we're just copying it into a
            // static class variable for later use during decryption.
            buffPrivateKeyStorage = keyPair.Export();
            return keyPair;
        }


    }
}
