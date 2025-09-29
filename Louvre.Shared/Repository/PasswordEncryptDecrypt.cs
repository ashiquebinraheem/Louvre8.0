using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Repository
{
    using System;
    using System.Security.Cryptography;

    public static class SimpleEncryptDecrypt
    {
        public static string Encrypt(string input)
        {
            // Convert the input into a char array
            char[] inputArray = input.ToCharArray();

            // Loop through the array and encrypt each character
            for (int i = 0; i < inputArray.Length; i++)
            {
                // Increment each character's ASCII value by 1
                int asciiValue = (int)inputArray[i];
                asciiValue++;
                inputArray[i] = (char)asciiValue;
            }

            // Return the encrypted string
            return new string(inputArray);
        }

        public static string Decrypt(string input)
        {
            // Convert the input into a char array
            char[] inputArray = input.ToCharArray();

            // Loop through the array and decrypt each character
            for (int i = 0; i < inputArray.Length; i++)
            {
                // Decrement each character's ASCII value by 1
                int asciiValue = (int)inputArray[i];
                asciiValue--;
                inputArray[i] = (char)asciiValue;
            }

            // Return the decrypted string
            return new string(inputArray);
        }
    }
}
