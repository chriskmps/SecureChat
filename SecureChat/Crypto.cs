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
    public class Crypto
    {
        Boolean keyPairExists;
        String strAsymmetricAlgName;
        UInt32 asymmetricKeyLength;
        IBuffer buffPublicKey;
        IBuffer buffPrivateKeyStorage;

        //Initialize the new Crypto object (initialized only once per app startup)
        public void initCrypto()
        {
            this.strAsymmetricAlgName = AsymmetricAlgorithmNames.RsaPkcs1;
            this.asymmetricKeyLength = 512;

            //Checks SecureChat's folder if a key pair already exists and set keyPairExists boolean
            this.keyPairExists = false;
            if (this.keyPairExists == true)
            {
                // set object to existing data
            } else
            {
                this.CreateAsymmetricKeyPair(strAsymmetricAlgName, asymmetricKeyLength, out buffPublicKey, out buffPrivateKeyStorage);
            }
        }






// Class functions
        public static IBuffer returnPublicKey(Crypto cryptoHolder)
        {
            return cryptoHolder.buffPublicKey;
        }

        public static IBuffer returnPrivateKey(Crypto cryptoHolder)
        {
            return cryptoHolder.buffPrivateKeyStorage;
        }


        //Encrypt Data (temporarily encrpyting with my own public key)
        public static string Encrypt(String strAsymmetricAlgName, IBuffer buffPublicKey, string message)
        {
            //Load RSA.pkcs algorithm
            AsymmetricKeyAlgorithmProvider objAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Import the public key from a buffer.
            CryptographicKey publicKey = objAlgProv.ImportPublicKey(buffPublicKey);

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

            // Use the private key embedded in the key pair to decrypt the session key.
            IBuffer buffDecryptedMessage = CryptographicEngine.Decrypt(keyPair, convertedString, null);

            //Return decrpyted message as a string
            string decryptedMessage = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffDecryptedMessage);
            return decryptedMessage;
        }
        

        //Generates a new asymmetric key pair if keypair file does not exist (Credit to MDSN Microsoft Libraries)
        public void CreateAsymmetricKeyPair(String strAsymmetricAlgName, UInt32 keyLength, out IBuffer buffPublicKey, out IBuffer buffPrivateKeyStorage)
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
        }
    }
}
