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
        
        //Generates new key pair if it does not (Credit to MDSN Microsoft Libraries)
        // Create an asymmetric key pair.

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
