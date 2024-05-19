using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace VerigaBlokov
{
    public partial class VerigaBlokov : Form
    {
        private TcpListener tcpListener;
        private List<TcpClient> connected = new List<TcpClient>();
        private List<TcpClient> connect = new List<TcpClient>();
        private List<Block> blockList = new List<Block>();

        delegate void appendTextCallback(RichTextBox tb, string text);
        delegate void refreshCallback();

        private int CurrentDifficulty = 2;
        private int diffAdjustInterval = 10; // The difficulty adjustment interval tells how often the difficulty will change.
        private int blockGenerationInterval = 10; // The block generation interval tells in what time a new block can be found, every 10 seconds we can insert a new block into the chain

        private Stopwatch timer = new Stopwatch();

        public VerigaBlokov()
        {
            InitializeComponent();
        }
        // dodava tekst vo dvata RichTextBox
        private void rtbAppendText(RichTextBox tb, string text)
        {
            if (tb.InvokeRequired)
                tb.Invoke(new appendTextCallback(rtbAppendText), new object[] { tb, text });
            else
            {
                tb.Focus();
                tb.AppendText(text + "\r\n");
            }

        }
        // ja refreshira sodrzinata
        public void rtbBlockchainLog()
        {
            if (rtbBlockChain.InvokeRequired)
                rtbBlockChain.Invoke(new refreshCallback(rtbBlockchainLog), new object[] { });
            else
            {
                rtbBlockChain.Clear();
                rtbBlockChain.Focus();

                for (int i = 0; i < blockList.Count; i++)
                    rtbAppendText(rtbBlockChain, "Indeks: " + blockList[i].Index.ToString() + "\r\nPodatek: " + blockList[i].Data + "\r\nČas: " + blockList[i].TimeStamp.ToString() + "\r\nHash: " + blockList[i].Hash + "\r\nPrejšnji hash: " + blockList[i].PreviousHash + "\r\nDiff: " + blockList[i].Difficulty + "\r\nNonce: " + blockList[i].Nonce + "\r\n");
            }
        }
        // TcpListener asynchronous operation to accept an incoming connection attempt.
        public void ListenForConnections(string lPort)
        {
            btStart.Enabled = false;
            tbLocalPort.Enabled = false;

            rtbAppendText(rtbLog, "Listen for connections!");

            tcpListener = new TcpListener(IPAddress.Loopback, int.Parse(lPort));
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(CallbackMethod), null);
        }
        // TcpClient for asynchronous operation
        private void CallbackMethod(IAsyncResult ar)
        {
            TcpClient tcpClient = tcpListener.EndAcceptTcpClient(ar);
            connected.Add(tcpClient);

            tcpListener.BeginAcceptTcpClient(new AsyncCallback(CallbackMethod), null);

            rtbAppendText(rtbLog, "Client " + tcpClient.Client.RemoteEndPoint + " has connected!");

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
        private void connectToPeer(string remotePort)
        {
            tbRemotePort.Enabled = false;
            btConnect.Enabled = false;

            TcpClient peerTcpClient;

            try
            {
                peerTcpClient = new TcpClient(IPAddress.Loopback.ToString(), int.Parse(remotePort));
                connect.Add(peerTcpClient);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbRemotePort.Enabled = true;
                btConnect.Enabled = true;
                return;
            }

            rtbAppendText(rtbLog, "Succesfully connected to" + remotePort + "!");

            tbRemotePort.Enabled = true;
            btConnect.Enabled = true;

            Dictionary<string, string> send = new Dictionary<string, string>(){
                { "length", blockList.Count.ToString() }
            };

            string json = JsonConvert.SerializeObject(send);
            byte[] bytes = Encoding.UTF8.GetBytes(json.ToCharArray(), 0, json.Length);
            peerTcpClient.GetStream().Write(bytes, 0, bytes.Length);

            Task.Run(() => receive(peerTcpClient));
        }
        // Receive
        private void receive(TcpClient client)
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

        private void Transaction(byte[] bRecMsg, TcpClient client)
        {
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

                    rtbBlockchainLog();
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

                        rtbAppendText(rtbLog, "Successful verification of the block sent by: " + client.Client.RemoteEndPoint);

                        Broadcast(bl, "check");
                        blockList.Add(bl);
                        /*
                        // proveri integritet pred da go vneses vo lanecot
                        if (IsValidChain())
                            blockList.Add(bl);
                        else
                            blockList.Remove(bl);
                        */
                        rtbBlockchainLog();
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
                            rtbBlockchainLog();
                        }
                    }
                }
            }
        }

        private void Broadcast(Block bl, string msg)
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

        private void btStart_Click(object sender, EventArgs e)
        {
            if (tbLocalPort.Text == "")
                MessageBox.Show("Invalid PORT!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                ListenForConnections(tbLocalPort.Text);
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            if (tbRemotePort.Text == "")
                MessageBox.Show("Invalid PORT!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                connectToPeer(tbRemotePort.Text);
        }

        private void btMine_Click(object sender, EventArgs e)
        {
            btMine.Enabled = false;

            int n = 0;
            Block bl;
            DateTime dateTime = DateTime.Now;
            ChangeDifficulty();
            while (true)
            {
                // generira block se dodeka ne se zadovoli uslovot x broj na 0 vo heshot
                bl = new Block(blockList.Count, "Block NO." + blockList.Count, (blockList.Count == 0) ? "0" : blockList[blockList.Count - 1].Hash, CurrentDifficulty, n, dateTime);

                string starter = "".PadLeft(CurrentDifficulty, '0');
                if (bl.Hash.StartsWith(starter))
                    break;
                else
                    n++;
                // na sekoj x generiranja na blokovi isfrla infrmacija deka ne go pogosil hesot, za da ne se misli deka blokiral programot
                if (n % 100000 == 0)
                    rtbAppendText(rtbLog, "Mining...");
            }
            btMine.Enabled = true;

            rtbAppendText(rtbLog, "Successful hash: " + bl.Hash);


            blockList.Add(bl);
            // proveri integritet

            rtbBlockchainLog();

            Broadcast(bl, "block");
        }

        public void ChangeDifficulty()
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

        public bool IsValidChain()
        {
            for (int i = 1; i < blockList.Count; i++)
            {
                Block currentBlock = blockList[i];
                Block previousBlock = blockList[i - 1];

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
