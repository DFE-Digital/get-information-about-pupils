using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Helpers.CSP
{
    [ExcludeFromCodeCoverage]
    public static class CSPHelper
    {
        private static string _random;

        public static string RandomCharacters 
        {
            get
            {
                if (String.IsNullOrEmpty(_random))
                {
                    _random = GenerateRandom();
                }

                return _random;
            }
        }

        private static string GenerateRandom()
        {
            var byteArray = new byte[32];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(byteArray);
            }

            return Convert.ToBase64String(byteArray);
        }
    }
}
