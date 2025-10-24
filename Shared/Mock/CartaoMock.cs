using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Mock
{
    public class CartaoMock
    {
        private static string[] VISA_PREFIX_LIST = ["4539", "4556", "4916", "4532", "4929", "40240071", "4485", "4716", "4"];
        private static string[] MASTERCARD_PREFIX_LIST = ["51", "52", "53", "54", "55"];

        private static string GerarCartao(string prefix, int length)
        {
            string ccnumber = prefix;
            while (ccnumber.Length < (length - 1))
            {
                double rnd = (new Random().NextDouble() * 1.0f - 0f);
                ccnumber += Math.Floor(rnd * 10);
                Thread.Sleep(10);
            }
            var reversedCCnumberstring = ccnumber.ToCharArray().Reverse();
            var reversedCCnumberList = reversedCCnumberstring.Select(c => Convert.ToInt32(c.ToString()));
            int sum = 0;
            int pos = 0;
            int[] reversedCCnumber = reversedCCnumberList.ToArray();

            while (pos < length - 1)
            {
                int odd = reversedCCnumber[pos] * 2;

                if (odd > 9)
                    odd -= 9;

                sum += odd;

                if (pos != (length - 2))
                    sum += reversedCCnumber[pos + 1];

                pos += 2;
            }
            int checkdigit = Convert.ToInt32((Math.Floor((decimal)sum / 10) + 1) * 10 - sum) % 10;
            ccnumber += checkdigit;
            return ccnumber;
        }

        private static IEnumerable<string> GerarCartoes(string[] prefixList, int length, int howMany = 1)
        {
            var result = new Stack<string>();
            for (int i = 0; i < howMany; i++)
            {
                int randomPrefix = new Random().Next(0, prefixList.Length - 1);
                if (randomPrefix > 1)
                    randomPrefix--;

                string ccnumber = prefixList[randomPrefix];
                result.Push(GerarCartao(ccnumber, length));
            }
            return result;
        }

        /// <summary>
        /// Escolhe uma das bandeiras aleatoriamente e gera um cartao de credito valido
        /// </summary>
        /// <returns></returns>
        public static CartaoMockItem GerarCartao(string? tipo = null)
        {
            string[] tipos = { "master", "visa" };
            Random random = new Random();

            if (string.IsNullOrEmpty(tipo) || !tipos.Contains(tipo))
                tipo = tipos[random.Next(tipos.Length)];

            switch (tipo)
            {
                case "master":
                    return GerarMaster();
                case "visa":
                    return GerarVisa();
                default:
                    return GerarVisa();
            }
        }

        public static CartaoMockItem GerarMaster()
        {
            var numero = GerarCartoes(MASTERCARD_PREFIX_LIST, 16, 1).First();
            return new CartaoMockItem() { Numero = numero, Bin = numero.Substring(0, 6), Bandeira = "Master" };
        }

        public static CartaoMockItem GerarVisa()
        {
            var numero = GerarCartoes(VISA_PREFIX_LIST, 16, 1).First();
            return new CartaoMockItem() { Numero = numero, Bin = numero.Substring(0, 6), Bandeira = "Visa" };
        }
    }
}
