using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvoDotNetClient.Util
{
    /// <summary>
    /// Helper for hashcode generation.
    /// </summary>
    public class HashCodeHelper
    {
        /// <summary>
        /// Produce the hashcode of a key.
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the hashcode</returns>
        public static Int32 GetHashCode(String key)
        {
            int hash1 = 5381;
            int hash2 = 5381;
            int c;
            char[] s = key.ToCharArray();
            for (int i = 0; i < s.Length; i++)
            {
                c = s[i];
                if (i % 2 == 0)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ c;
                }
                else
                {
                    hash2 = ((hash2 << 5) + hash2) ^ c;
                }

            }
            hash1 = hash1 + (hash2 * 1566083941);
            return hash1;
        }
        /// <summary>
        /// Produce a positive hashcode limited by a max value.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="maxNum">the max value</param>
        /// <returns>the hashcode</returns>
        public static Int32 GetPositiveHashCode(String key, Int32 maxNum)
        {
            int hash1 = 5381;
            int hash2 = 5381;
            int c;
            char[] s = key.ToCharArray();
            for (int i = 0; i < s.Length; i++)
            {
                c = s[i];
                //Console.WriteLine("" + i + " " + c);
                if (i % 2 == 0)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ c;
                }
                else
                {
                    hash2 = ((hash2 << 5) + hash2) ^ c;
                }

            }
            hash1 = hash1 + (hash2 * 1566083941);
            if (hash1 < 0) hash1 = (-1) * hash1;
            return hash1 % maxNum;
        }
    }
}
