using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VerigaBlokov
{
    class Blockchain
    {
        public static TcpListener tcpListener;
        public static List<TcpClient> connected = new List<TcpClient>();
        public static List<TcpClient> connect = new List<TcpClient>();
        public static List<Block> blockList = new List<Block>();



        public static int CurrentDifficulty = 2;
        public static int diffAdjustInterval = 10; // The difficulty adjustment interval tells how often the difficulty will change.
        public static int blockGenerationInterval = 10; // The block generation interval tells in what time a new block can be found, every 10 seconds we can insert a new block into the chain

        //private Stopwatch timer = new Stopwatch();


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


        // TcpListener asynchronous operation to accept an incoming connection attempt.
        public static void ListenForConnections(string lPort)
        {
            VerigaBlokov forma = new VerigaBlokov();
            forma.btStart.Enabled = false;
            forma.tbLocalPort.Enabled = false;

            //forma.rtbAppendText(forma.rtbLog, "Listen for connections!");

            tcpListener = new TcpListener(IPAddress.Loopback, int.Parse(lPort));
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(CallbackMethod), null);
        }
        // TcpClient for asynchronous operation
        public static void CallbackMethod(IAsyncResult ar)
        {
            TcpClient tcpClient = tcpListener.EndAcceptTcpClient(ar);
            connected.Add(tcpClient);

            tcpListener.BeginAcceptTcpClient(new AsyncCallback(CallbackMethod), null);

            //rtbAppendText(rtbLog, "Client " + tcpClient.Client.RemoteEndPoint + " has connected!");

            while (tcpClient.Connected)
            {
                byte[] bRecMsg = new byte[333];
                try
                {
                    tcpClient.GetStream().Read(bRecMsg, 0, bRecMsg.Length);
                    Task.Run(() =>
                    {
                        Transaction(bRecMsg, tcpClient);
                    });
                }
                catch (Exception ex)
                {
                    if (!tcpClient.Connected)
                        tcpClient.Close();
                    else
                        MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }


            }
        }
        // Connect/Send
        public static void connectToPeer(string remotePort)
        {
            VerigaBlokov forma = new VerigaBlokov();
            forma.tbRemotePort.Enabled = false;
            forma.btConnect.Enabled = false;

            TcpClient peerTcpClient;

            try
            {
                peerTcpClient = new TcpClient(IPAddress.Loopback.ToString(), int.Parse(remotePort));
                connect.Add(peerTcpClient);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                forma.tbRemotePort.Enabled = true;
                forma.btConnect.Enabled = true;
                return;
            }

            //rtbAppendText(rtbLog, "Succesfully connected to" + remotePort + "!");

            forma.tbRemotePort.Enabled = true;
            forma.btConnect.Enabled = true;

            Dictionary<string, string> send = new Dictionary<string, string>(){
                { "length", blockList.Count.ToString() }
            };

            string json = JsonConvert.SerializeObject(send);
            byte[] bytes = Encoding.UTF8.GetBytes(json.ToCharArray(), 0, json.Length);
            peerTcpClient.GetStream().Write(bytes, 0, bytes.Length);

            Task.Run(() => receive(peerTcpClient));
        }

        // Receive
        public static void receive(TcpClient client)
        {
            while (client.Connected)
            {
                byte[] bRecMsg = new byte[333];
                try
                {
                    client.GetStream().Read(bRecMsg, 0, bRecMsg.Length);
                    Task.Run(() =>
                    {
                        Transaction(bRecMsg, client);
                    });
                }
                catch (Exception ex)
                {
                    if (!client.Connected)
                        client.Close();
                    else
                        MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
        }

        public static void Transaction(byte[] bRecMsg, TcpClient client)
        {
            VerigaBlokov forma = new VerigaBlokov();
            string read = Encoding.UTF8.GetString(bRecMsg, 0, bRecMsg.Length);
            if (!string.IsNullOrEmpty(@read))
            {
                // Pravi 2 data struktura Dictionari vo JSON format za da moze da gi smesti poadoticite prateni preku mreza
                Dictionary<string, string> msg = JsonConvert.DeserializeObject<Dictionary<string, string>>(@read);
                Dictionary<string, string> send = new Dictionary<string, string>();

                if (msg.ContainsKey("length"))
                {
                    // Koga nekoj se povrzuva na mojata porta i ako dobijam poraka so dolzina na lanec pogolema od mojata
                    // prajkam poraka da mi go preprati negoviot lanec (send)
                    if (int.Parse(msg["length"]) > blockList.Count)
                    {
                        send.Add("send", "");

                        string json = JsonConvert.SerializeObject(send);
                        byte[] bytes = Encoding.UTF8.GetBytes(json.ToCharArray(), 0, json.Length);
                        client.GetStream().Write(bytes, 0, bytes.Length);
                    }
                    // ako dobienat dolzina na lanecot e pomala od mojata prati mu go mojot lanec
                    else if (int.Parse(msg["length"]) < blockList.Count)
                    {
                        for (int i = 0; i < blockList.Count; i++)
                        {
                            send = blockList[i].ToDictionary();
                            send.Add("replace", "");

                            string json = JsonConvert.SerializeObject(send);
                            byte[] bytes = Encoding.UTF8.GetBytes(json.ToCharArray(), 0, json.Length);
                            client.GetStream().Write(bytes, 0, bytes.Length);
                        }
                    }
                }

                // ako dobijam poraka(send) deka mojot lanec e pogolem od partnerot i mu go prajkam mojot lanec block po block
                // so znamence replace
                if (msg.ContainsKey("send"))
                {
                    for (int i = 0; i < blockList.Count; i++)
                    {
                        send = blockList[i].ToDictionary();
                        send.Add("replace", "");

                        string json = JsonConvert.SerializeObject(send);
                        byte[] bytes = Encoding.UTF8.GetBytes(json.ToCharArray(), 0, json.Length);
                        client.GetStream().Write(bytes, 0, bytes.Length);
                    }
                }

                else if (msg.ContainsKey("replace"))
                {
                    Block bl = new Block(int.Parse(msg["index"]), msg["data"], msg["prevHash"], int.Parse(msg["diff"]), int.Parse(msg["nonce"]), DateTime.Parse(msg["time"]));

                    if (blockList.Count <= bl.Index)
                        blockList.Add(bl);
                    else if (blockList[bl.Index].Index == bl.Index)
                        blockList[bl.Index] = bl;
                    else
                    {
                        blockList.Add(bl);
                        blockList = blockList.OrderBy(o => o.Index).ToList();
                    }

                    Broadcast(bl, "check");

                    forma.rtbBlockchainLog();
                }
                else if (msg.ContainsKey("block")) // go poreveruva primeniot Block
                {
                    Block bl = new Block(int.Parse(msg["index"]), msg["data"], msg["prevHash"], int.Parse(msg["diff"]), int.Parse(msg["nonce"]), DateTime.Parse(msg["time"]));
                    // proveruva Hash od primeniot blok so nov sto go kreira so podatoci od primenit blok
                    if (bl.Hash.Equals(msg["hash"]))
                    {
                        if (CurrentDifficulty != int.Parse(msg["new_diff"]))
                        {
                            CurrentDifficulty = int.Parse(msg["new_diff"]);
                        }

                        //rtbAppendText(rtbLog, "Successful verification of the block sent by: " + client.Client.RemoteEndPoint);

                        Broadcast(bl, "check");
                        blockList.Add(bl);
                        /*
                        // proveri integritet pred da go vneses vo lanecot
                        if (IsValidChain())
                            blockList.Add(bl);
                        else
                            blockList.Remove(bl);
                        */
                        forma.rtbBlockchainLog();
                    }
                }
                else if (msg.ContainsKey("check"))
                {
                    Block bl = new Block(int.Parse(msg["index"]), msg["data"], msg["prevHash"], int.Parse(msg["diff"]), int.Parse(msg["nonce"]), DateTime.Parse(msg["time"]));
                    // proveren od drugite i vraten
                    if (bl.Hash.Equals(msg["hash"]))
                    {

                        if (blockList.Count < bl.Index + 1 || !blockList[bl.Index].Hash.Equals(msg["hash"]))
                        {
                            if (CurrentDifficulty != int.Parse(msg["new_diff"]))
                            {
                                CurrentDifficulty = int.Parse(msg["new_diff"]);
                            }

                            Broadcast(bl, "check");

                            blockList.Add(bl);
                            forma.rtbBlockchainLog();
                        }
                    }
                }
            }
        }

        public static void Broadcast(Block bl, string msg)
        {
            foreach (TcpClient client in connected.ToArray())
            {
                Dictionary<string, string> send = bl.ToDictionary();
                send.Add(msg, "");
                send.Add("new_diff", CurrentDifficulty.ToString());
                string json = JsonConvert.SerializeObject(send);

                byte[] bytes = Encoding.UTF8.GetBytes(json.ToCharArray(), 0, json.Length);

                client.GetStream().Write(bytes, 0, bytes.Length);
            }
        }




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

        public Block MineBlock()
        {
            int n = 0;
            DateTime dateTime = DateTime.Now;
            ChangeDifficulty();
            while (true)
            {
                // Generate block until the hash satisfies the difficulty condition
                Block bl = new Block(Blockchain.blockList.Count, "Block NO." + Blockchain.blockList.Count,
                    (Blockchain.blockList.Count == 0) ? "0" : Blockchain.blockList[Blockchain.blockList.Count - 1].Hash,
                    Blockchain.CurrentDifficulty, n, dateTime);

                string starter = "".PadLeft(Blockchain.CurrentDifficulty, '0');
                if (bl.Hash.StartsWith(starter))
                    return bl;
                else
                    n++;

                // Log mining progress periodically
                //if (n % 100000 == 0)
                //rtbAppendText(rtbLog, "Mining...");
            }
        }

        public static void ChangeDifficulty()
        {

            if (blockList.Count - diffAdjustInterval < 0) return;
            Block previousAdjustmentBlock = blockList[blockList.Count - diffAdjustInterval];
            int timeExpected = blockGenerationInterval * diffAdjustInterval;
            double timeTaken = (blockList.Last().TimeStamp - previousAdjustmentBlock.TimeStamp).TotalSeconds;

            if (timeTaken < (timeExpected / 2))
            {
                CurrentDifficulty = previousAdjustmentBlock.Difficulty + 1; // increasing difficulty 
            }
            else if (timeTaken > (timeExpected * 2))
            {
                CurrentDifficulty = previousAdjustmentBlock.Difficulty - 1; // decrease difficulty 
            }

        }

    }
}
