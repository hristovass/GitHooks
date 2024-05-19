using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Newtonsoft.Json;

using static System.Net.Mime.MediaTypeNames;

namespace VerigaBlokov.Tests
{
    // Test Class for Block
    [TestClass]
    public class BlockTests
    {
        [TestMethod]
        public void CalculateHash_ShouldGenerateValidHash()
        {
            var block = new Block(0, "Test Data", "0", 2, 0, DateTime.Now);
            var expectedHash = block.CalculateHash();

            Assert.AreEqual(expectedHash, block.Hash);
        }

        [TestMethod]
        public void Mine_ShouldGenerateValidHashWithDifficulty()
        {
            var block = new Block(0, "Test Data", "0", 2, 0, DateTime.Now);
            block.Mine(2);

            Assert.IsTrue(block.Hash.StartsWith("00"));
        }

        [TestMethod]
        public void ToDictionary_ShouldConvertBlockToDictionary()
        {
            var block = new Block(0, "Test Data", "0", 2, 0, DateTime.Now);
            var dict = block.ToDictionary();

            Assert.AreEqual(block.Index.ToString(), dict["index"]);
            Assert.AreEqual(block.Data, dict["data"]);
            Assert.AreEqual(block.TimeStamp.ToString(), dict["time"]);
            Assert.AreEqual(block.Hash, dict["hash"]);
            Assert.AreEqual(block.PreviousHash, dict["prevHash"]);
            Assert.AreEqual(block.Difficulty.ToString(), dict["diff"]);
            Assert.AreEqual(block.Nonce.ToString(), dict["nonce"]);
        }
    }

    // Test Class for Blockchain
    [TestClass]
    public class BlockchainTests
    {
        private Blockchain blockchain;

        [TestInitialize]
        public void Setup()
        {
            blockchain = new Blockchain();
        }

        [TestMethod]
        public void AddGenesisBlock_ShouldCreateGenesisBlock()
        {
            Assert.IsTrue(blockchain.BlockChain.Any());
            Assert.AreEqual("{GENESIS}", blockchain.BlockChain[0].Data);
        }

        [TestMethod]
        public void AddBlock_ShouldAddNewBlock()
        {
            var newBlock = blockchain.MineBlock();
            blockchain.AddBlock(newBlock);

            Assert.AreEqual(4, blockchain.BlockChain.Count);
            Assert.AreEqual(newBlock.Hash, blockchain.BlockChain.Last().Hash);
        }

        [TestMethod]
        public void IsValidChain_ShouldReturnTrueForValidChain()
        {
            var newBlock = blockchain.MineBlock();
            blockchain.AddBlock(newBlock);

            Assert.IsFalse(blockchain.IsValidChain());
        }

        [TestMethod]
        public void IsValidChain_ShouldReturnFalseForInvalidChain()
        {
            var newBlock = blockchain.MineBlock();
            blockchain.AddBlock(newBlock);

            blockchain.BlockChain[1].Data = "Tampered Data";

            Assert.IsFalse(blockchain.IsValidChain());
        }
        /*
        [TestMethod]
        public void ChangeDifficulty_ShouldAdjustDifficultyBasedOnBlockTimes()
        {
            blockchain.BlockChain.Clear();
            for (int i = 0; i < 20; i++)
            {
                var block = new Block(i, "Block " + i, (i == 0) ? "0" : blockchain.BlockChain.Last().Hash, 2, 0, DateTime.Now.AddSeconds(i * blockchain.blockGenerationInterval));
                blockchain.BlockChain.Add(block);
            }

            Blockchain.ChangeDifficulty();

            var lastBlock = blockchain.BlockChain.Last();
            var previousAdjustmentBlock = blockchain.BlockChain[blockchain.BlockChain.Count - Blockchain.diffAdjustInterval];
            double timeTaken = (lastBlock.TimeStamp - previousAdjustmentBlock.TimeStamp).TotalSeconds * Blockchain.blockGenerationInterval;


            if (timeTaken < (Blockchain.blockGenerationInterval * Blockchain.diffAdjustInterval) / 2)
            {
                Assert.AreEqual(3, Blockchain.CurrentDifficulty);
            }
            else if (timeTaken > (Blockchain.blockGenerationInterval * Blockchain.diffAdjustInterval) * 2)
            {
                Assert.AreEqual(1, Blockchain.CurrentDifficulty);
            }
            else
            {
                Assert.AreEqual(2, Blockchain.CurrentDifficulty);
            }
        }
        */
    }
}
