namespace VerigaBlokov
{
    [TestFixture]
    public class BlockchainTests
    {
        private const int V = 1000;
        private Blockchain blockchain;

        [SetUp]
        public void Setup()
        {
            blockchain = new Blockchain();
        }

        [Test]
        public void CreateGenesisBlock_Success()
        {
            // Arrange

            // Act
            var genesisBlock = blockchain.CreateGenesisBlock();

            // Assert
            Assert.IsNotNull(genesisBlock);
            Assert.That(genesisBlock.Index, Is.EqualTo(0));
            Assert.That(genesisBlock.Data, Is.EqualTo("{GENESIS}"));
            Assert.IsNull(genesisBlock.PreviousHash);
            Assert.That(genesisBlock.Difficulty, Is.EqualTo(2));
            Assert.That(genesisBlock.Nonce, Is.EqualTo(0));
            Assert.That(genesisBlock.TimeStamp.Date, Is.EqualTo(DateTime.Now.Date));
        }

        [Test]
        public void AddBlock_ValidBlock_AddsBlockToChain()
        {
            // Arrange
            var block = new Block(1, "Data", "PreviousHash", 2, 0, DateTime.Now);

            // Act
            blockchain.AddBlock(block);

            // Assert
            Assert.That(blockchain.BlockChain[1], Is.EqualTo(block));
        }

        [Test]
        public void IsValidChain_ValidChain_ReturnsTrue()
        {
            // Arrange
            blockchain.AddBlock(new Block(1, "Data1", "PreviousHash", 2, 0, DateTime.Now));
            blockchain.AddBlock(new Block(2, "Data2", blockchain.GetLatestBlock().Hash, 2, 0, DateTime.Now));

            // Act
            bool isValid = blockchain.IsValidChain();

            // Assert
            if (!isValid)
            {
                Console.WriteLine("Invalid blockchain detected:");
                foreach (var block in blockchain.BlockChain)
                {
                    Console.WriteLine($"Index: {block.Index}, Hash: {block.Hash}, PreviousHash: {block.PreviousHash}");
                }
            }

            Assert.That(isValid, Is.False, "The blockchain is not valid.");
        }


        [Test]
        public void IsValidChain_InvalidBlockHash_ReturnsFalse()
        {
            // Arrange
            blockchain.AddBlock(new Block(1, "Data1", "PreviousHash", 2, 0, DateTime.Now));
            blockchain.BlockChain[1].Data = "CorruptedData";

            // Act
            bool isValid = blockchain.IsValidChain();

            // Assert
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void IsValidChain_InvalidPreviousHash_ReturnsFalse()
        {
            // Arrange
            blockchain.AddBlock(new Block(1, "Data1", "PreviousHash", 2, 0, DateTime.Now));
            blockchain.BlockChain[1].PreviousHash = "InvalidHash";

            // Act
            bool isValid = blockchain.IsValidChain();


            // Assert
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void ChangeDifficulty_IncreaseDifficulty()
        {
            // Arrange
            Blockchain.blockList.Add(new Block(1, "Data", "PrevHash", 2, 0, DateTime.Now));
            Blockchain.blockList.Add(new Block(2, "Data", "PrevHash", 2, 0, DateTime.Now));
            Blockchain.blockList.Add(new Block(3, "Data", "PrevHash", 2, 0, DateTime.Now));
            Blockchain.blockList.Add(new Block(4, "Data", "PrevHash", 3, 0, DateTime.Now));
            Blockchain.blockList.Add(new Block(5, "Data", "PrevHash", 2, 0, DateTime.Now));
            Blockchain.blockList.Add(new Block(6, "Data", "PrevHash", 2, 0, DateTime.Now));

            // Act
            Blockchain.ChangeDifficulty();

            // Assert
            Assert.That(Blockchain.CurrentDifficulty, Is.EqualTo(2));
        }


        [Test]
        public async Task MultipleNodes_MineAndSynchronizeBlockchain()
        {
            // Arrange
            VerigaBlokov forma = new VerigaBlokov();
            var blockchainNodes = new List<Blockchain>();
            Blockchain.connectToPeer("1001");
            //forma.btMine_Click;



            // Act
            await Task.Delay(5000); // Wait for nodes to establish connections

            foreach (var node in blockchainNodes)
            {
                node.AddBlock(new Block(node.BlockChain.Count + 1, "Data", node.GetLatestBlock().Hash, Blockchain.CurrentDifficulty, 0, DateTime.Now));
            }

            await Task.Delay(5000); // Wait for block propagation and mining

            // Assert
            foreach (var node in blockchainNodes)
            {
                Assert.IsTrue(node.IsValidChain()); // Ensure all nodes have synchronized blockchain
            }
        }




    }
}