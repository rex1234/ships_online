using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;

namespace Ships
{
    public partial class Form1 : Form
    {
        private readonly Color EmptyFieldC = Color.Lavender;
        private readonly Color EmptyFieldGuessedC = Color.SkyBlue;

        private readonly Color ShipFieldC = Color.Purple;
        private readonly Color ShipFieldGuessedC = Color.Orange;
        private readonly Color WholeShipFieldC = Color.Red;

        public static readonly IDictionary<int, int> ShipCounts = new Dictionary<int, int>()
        {
            {1, 2},
            {2, 2},
            {3, 3},
            {4, 3},
            {5, 1}
        };

        private const int Port = 45678;
        private const string RegistryKey = @"Software\ShipsGame";

        private NetworkStream networkStream;
        private bool running; 
        private bool host; // hosting / joined        

        private ShipBoardView mainBoard;
        private ShipBoardView smallBoard;

        private bool selecting = true; //selecting / guessing

        private Player player;
        private Player opponent;

        public Form1()
        {
            InitializeComponent();

            mainBoard = new ShipBoardView(this) { X = 8, Y = mainNameLabel.Location.Y + mainNameLabel.Height + 10 };
            mainBoard.FieldClicked += (x, y) =>
                {
                    if (selecting)
                    {
                        player[x, y] = player[x, y] == Player.ShipField
                            ? Player.EmptyField : Player.ShipField;
                        mainBoard[x, y] = player[x, y] == Player.ShipField
                            ? ShipFieldC : EmptyFieldC;

                        confirmButton.Enabled = !player.IsError &&
                            player.GetShipCount(1) == ShipCounts[1] &&
                            player.GetShipCount(2) == ShipCounts[2] &&
                            player.GetShipCount(3) == ShipCounts[3] &&
                            player.GetShipCount(4) == ShipCounts[4] &&
                            player.GetShipCount(5) == ShipCounts[5];
                        SetRemainingShipsText();
                    }
                    else //guessing
                    {
                        if (!opponent.Guessed[x, y])
                        {
                            opponent.Guessed[x, y] = true;
                            mainBoard.Clickable = false;
                            SendMessage("get", String.Format("{0},{1}", x, y));                            
                        }
                    }
                };
            mainBoard.Show();

            smallBoard = new ShipBoardView(this);
            smallBoard.FieldSize = 14;
            smallBoard.X = statusLabel1.Location.X;
            smallBoard.Y = mainBoard.Y + mainBoard.Height * mainBoard.FieldSize - smallBoard.Height * smallBoard.FieldSize;
            smallBoard.Show();

            NewGame();

            statusLabel1.Visible = false;
            confirmButton.Visible = false;
        }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            confirmButton.Enabled = false;
            confirmButton.Text = "Ready";

            player.Ready = true;
            mainBoard.Clickable = false;

            SendMessage("ready");
            bottomStatusLabel.Text = String.Format("Waiting for {0} to finish placing ships.", opponent.Name);

            TryToStartGame();
        }

        private void NewGame()
        {
            mainBoard.Reset();
            smallBoard.Reset();
            mainBoard.Clickable = false;
            selecting = true;

            player = new Player(12, 12);
            opponent = new Player(12, 12);
            LoadPlayerNick();
            SetRemainingShipsText();
        }

        private void PlayerConnected()
        {
            SendMessage("name", player.Name);

            statusLabel1.Visible = true;
            confirmButton.Visible = true;
            mainBoard.Clickable = true;
            bottomStatusLabel.Text = "Select your ships";
        }

        private void PlayerDisconnected()
        {
            MessageBox.Show("The other player disconnected.", "Ending game", MessageBoxButtons.OK, MessageBoxIcon.Information);
            running = false;

            bottomStatusLabel.Text = "";
            statusLabel1.Visible = false;
            confirmButton.Visible = false;

            mainNameLabel.Text = "Player 1";
            smallNameLabel.Text = "Player 2";

            NewGame();
        }

        private void TryToStartGame()
        {
            if (player.Ready && opponent.Ready)
            {
                smallBoard.Reset();
                selecting = false;

                for (int i = 0; i < player.Width; i++)
                {
                    for (int j = 0; j < player.Height; j++)
                    {
                        smallBoard[i, j] = mainBoard[i, j];
                    }
                }

                mainBoard.Reset();
                mainBoard.Clickable = host;
                confirmButton.Visible = false;

                SetRemainingShipsText();

                smallNameLabel.Text = "You";
                mainNameLabel.Text = opponent.Name;
                SetMoveText();
            }
        }

        //
        //messaging
        //

        private void MessageReceived(String message)
        {
            string command = message.Split(';')[0];
            string data = message.Split(';')[1];

            if (command == "name")
            {
                opponent.Name = data;

                smallNameLabel.Text = opponent.Name;
                mainNameLabel.Text = "You";
            }

            else if (command == "ready")
            {
                opponent.Ready = true;
                TryToStartGame();
            }

            else if (command == "get")
            {
                string[] request = data.Split(',');
                int x = int.Parse(request[0]);
                int y = int.Parse(request[1]);
                int status = player.Guess(x, y);

                if (player[x, y] == Player.EmptyField)
                {
                    smallBoard[x, y] = EmptyFieldGuessedC;
                }
                else
                {
                    smallBoard[x, y] = ShipFieldGuessedC;
                    if (status == Player.HitWholeShip)
                    {
                        smallBoard.Fill(x, y, WholeShipFieldC);
                    }
                }

                if (status == Player.Miss)
                {
                    mainBoard.Clickable = true;
                    SetMoveText();
                }

                SendMessage("reply", String.Format("{0},{1},{2}", x, y, status));

                SetRemainingShipsText();
                if (player.RemainingShips == 0)
                {
                    MessageBox.Show(opponent.Name + " won the game!", "Game over");
                    NewGame();
                    PlayerConnected();
                }

            }

            else if (command == "reply")
            {
                string[] reply = data.Split(',');
                int x = int.Parse(reply[0]);
                int y = int.Parse(reply[1]);
                int status = int.Parse(reply[2]);

                if (status == Player.Hit)
                {
                    mainBoard.Clickable = true;
                    mainBoard[x, y] = ShipFieldGuessedC;
                }
                else if (status == Player.HitWholeShip)
                {
                    mainBoard.Clickable = true;
                    mainBoard[x, y] = ShipFieldGuessedC;
                    player.FoundShipCounts[
                        mainBoard.Fill(x, y, WholeShipFieldC)]++;
                    SetRemainingShipsText();
                }
                else if (status == Player.Miss)
                {
                    SetMoveText();
                    mainBoard[x, y] = EmptyFieldGuessedC;
                }

                SetRemainingShipsText();
                if (player.FoundAll)
                {
                    MessageBox.Show("You won!", "Congratulations");
                    NewGame();
                    PlayerConnected();
                }
            }
        }

        private void SendMessage(string command)
        {
            SendMessage(command, "");
        }

        private void SendMessage(string command, string message)
        {
            Task.Run(() =>
                {
                    Byte[] data = System.Text.Encoding.UTF8.GetBytes(String.Format("{0};{1}", command, message));
                    networkStream.Write(data, 0, data.Length);
                });
        }

        //
        //networking
        //

        private void hostGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (running)
            {
                return;
            }

            running = true;
            host = true;
            bottomStatusLabel.Text = "Waiting for other player to join. Your IP address is " + GetLocalIPAddress();
            
            Task.Run(() =>
                {
                    try
                    {
                        TcpListener server = new TcpListener(IPAddress.Any, Port);
                        host = true;
                        server.Start();                            
                        TcpClient client = server.AcceptTcpClient();
                        using(networkStream = client.GetStream())
                        {
                            ReadNetworkStream();
                        }                        
                    }
                    catch (SocketException ex)
                    {                        
                        Invoke((MethodInvoker)delegate
                        {
                            running = false;
                            bottomStatusLabel.Text = "";
                            MessageBox.Show("Failed to start the server\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        });
                    }
                }
            );
        }

        private void joinGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (running)
            {
                return;
            }

            JoinGameDialog dialog = new JoinGameDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                running = true;
                bottomStatusLabel.Text = "Joining...";
                Task.Run(() =>
                    {
                        try
                        {
                            using (TcpClient client = new TcpClient(dialog.Ip.ToString(), Port))
                            {
                                host = false;
                                networkStream = client.GetStream();                                
                                ReadNetworkStream();
                            }
                        }
                        catch (SocketException ex)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                running = false;
                                bottomStatusLabel.Text = "";
                                MessageBox.Show("Failed to join the other player.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            });
                        }
                    });
            }
        }

        private void ReadNetworkStream()
        {
            Invoke((MethodInvoker)delegate
            {
                PlayerConnected();
            });

            int length;
            byte[] bytes = new byte[1024];
            try
            {
                while ((length = networkStream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        string data = System.Text.Encoding.UTF8.GetString(bytes, 0, length);
                        MessageReceived(data);
                    });
                }
            }
            catch (IOException)
            {
                Invoke((MethodInvoker)delegate
                {
                    PlayerDisconnected();
                });
            }
        }

        private string GetLocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "";
        }


        //
        //Nick settings
        //

        private void setNickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NickDialog dialog = new NickDialog(player.Name);
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                player.Name = dialog.PlayerName;
                SavePlayerNick();
            }
        }
        
        private void LoadPlayerNick()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryKey,
                RegistryKeyPermissionCheck.ReadSubTree))
            {
                string playerName = key.GetValue("playerNick") as String;
                if (playerName != null)
                {
                    player.Name = playerName;
                }
            }
        }

        private void SavePlayerNick()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryKey,
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue("playerNick", player.Name);
            }
        }

        //
        //other UI
        //

        private void SetMoveText()
        {
            bottomStatusLabel.Text = mainBoard.Clickable ? "You are on the move." : (opponent.Name + " is on the move.");
        }

        private void SetRemainingShipsText()
        {
            if (selecting)
            {
                statusLabel1.ForeColor = player.IsError ? Color.Red : Color.Black;
                statusLabel1.Text = String.Format(
                            "Select ships:\n {0}/2 ■\n {1}/2 ■■\n {2}/3 ■■■\n {3}/3 ■■■■\n {4}/1 ■■■■■\n",
                            player.GetShipCount(1),
                            player.GetShipCount(2),
                            player.GetShipCount(3),
                            player.GetShipCount(4),
                            player.GetShipCount(5));
            }
            else
            {
                statusLabel1.Text = String.Format(
                            "Found ships:\n {0}/2 ■\n {1}/2 ■■\n {2}/3 ■■■\n {3}/3 ■■■■\n {4}/1 ■■■■■\nRemaining ships: {5}",
                            player.FoundShipCounts[1],
                            player.FoundShipCounts[2],
                            player.FoundShipCounts[3],
                            player.FoundShipCounts[4],
                            player.FoundShipCounts[5],
                            player.RemainingShips);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Battleships online\nVersion 1.0.0\n(c) 2014 Ján Mjartan corporation\nAll rights reserved", "Info");
        }
    }
}
