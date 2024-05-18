using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerigaBlokov
{

    class Blockchain
    {
        public IList<Block> BlockChain { get; set; }
        public int Difficulty { set; get; } = 1;

        public Blockchain()
        {
            InitializeChain();
            AddGenesisBlock();
        }
        //Initialize
        private void InitializeChain()
        {
            BlockChain = new List<Block>();
        }
        private void AddGenesisBlock()
        {
            BlockChain.Add(CreateGenesisBlock());
        }
        public Block CreateGenesisBlock()
        {
            return new Block(0, "{GENESIS}", null, 2, 0, DateTime.Now);
        }
        //end Initialize

        public void AddBlock(Block block)
        {
            Block latestBlock = GetLatestBlock();
            block.Index = latestBlock.Index + 1;
            block.PreviousHash = latestBlock.Hash;
            //block.Mine(this.Difficulty);
            BlockChain.Add(block);
        }

        public Block GetLatestBlock()
        {
            return BlockChain[BlockChain.Count - 1];
        }

        public bool IsValidChain()
        {
            for (int i = 1; i < BlockChain.Count; i++)
            {
                Block currentBlock = BlockChain[i];
                Block previousBlock = BlockChain[i - 1];

                if (currentBlock.Hash != currentBlock.CalculateHash())
                {
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
