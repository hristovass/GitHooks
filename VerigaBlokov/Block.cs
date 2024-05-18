using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace VerigaBlokov
{
    class Block
    {
        public int Index { get; set; }
        public DateTime TimeStamp { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public string Data { get; set; }
        public int Nonce { get; set; } = 0;
        public int Difficulty { get; set; } = 2;

        public Block(int ind, string info, string prevHs, int diff, int n, DateTime dateTime)
        {
            Index = ind;
            Data = info;
            PreviousHash = prevHs;
            //TimeStamp = DateTime.Now;
            TimeStamp = dateTime;
            Difficulty = diff;
            Nonce = n;
            Hash = CalculateHash();
        }

        public string CalculateHash()
        {
            SHA256 sha256 = SHA256.Create();

            byte[] inputBytes = Encoding.UTF8.GetBytes(Index.ToString() + TimeStamp.ToString() + Data + PreviousHash + Difficulty.ToString() + Nonce.ToString());
            string output = BitConverter.ToString(sha256.ComputeHash(inputBytes)).Replace("-", "");

            return output;
        }

        public void Mine(int difficulty)
        {
            var leadingZeros = new string('0', difficulty);
            while (this.Hash == null || this.Hash.Substring(0, difficulty) != leadingZeros)
            {
                this.Nonce++;
                this.Hash = this.CalculateHash();
            }
        }

        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(){
                { "index", Index.ToString() },
                { "data", Data },
                { "time", TimeStamp.ToString() },
                { "hash", Hash },
                { "prevHash", PreviousHash },
                { "diff", Difficulty.ToString() },
                { "nonce", Nonce.ToString() }
            };
        }
    }
}
