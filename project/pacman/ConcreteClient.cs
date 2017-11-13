using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Server;
using System.Drawing;
using System.Runtime.Serialization;
using System.Threading;

namespace pacman
{


    public class ConcreteClient : MarshalByRefObject, IClient
    {
        public static ClientManager ClientManager;
        public static FormWelcome WelcomeForm;
        public FormStage StageForm;

        public List<string> messages;
        public List<IClient> Clients { get; set; }
        public string Username { get; set; }
        public string Address { get; set; }
        public int Round { get; set; }

        private Dictionary<int, PictureBox> stageObjects;
        private Dictionary<int, string> stageObjectsType;

        private bool hasGameStarted;
        static volatile Mutex mutex = new Mutex(false);

        public ConcreteClient()
        {
            this.Round = 0;
            this.hasGameStarted = false;
            this.Clients = new List<IClient>();
            this.messages = new List<string>();
        }

        public void Start(IStage stage)
        {
            // this check prevents some server to restart the game
            if (hasGameStarted)
                return;

            stageObjects = new Dictionary<int, PictureBox>();
            stageObjectsType = new Dictionary<int, string>();

            new Thread(() =>
            {
                WelcomeForm.Invoke(new System.Action(() => {
                    mutex.WaitOne();
                    WelcomeForm.Hide();
                    StageForm = new FormStage(ClientManager);
                    StageForm.Show();
                    buildWalls(stage);
                    buildCoins(stage);
                    buildMonsters(stage);
                    buildPlayers(stage);
                    mutex.ReleaseMutex();
                }));
                
                
            }).Start();

            hasGameStarted = true;

        }

        public void SendRoundStage(List<Shared.Action> actions, int score, int round)
        {
            //check if game has already started
            if (!hasGameStarted)
                return;

            mutex.WaitOne();
            foreach (Shared.Action action in actions)
            {
                PictureBox pictureBox;
                switch (action.action)
                {
                    case Shared.Action.ActionTaken.MOVE:
                        pictureBox = stageObjects[action.ID];
                        int x = pictureBox.Location.X + action.displacement.X;
                        int y = pictureBox.Location.Y + action.displacement.Y;
                        StageForm.Invoke(new System.Action(() =>
                        {
                            if(stageObjectsType[action.ID] == "player")
                            {
                                switch(action.direction)
                                {
                                    case Shared.Action.Direction.DOWN:
                                        pictureBox.Image = global::pacman.Properties.Resources.down;
                                        break;
                                    case Shared.Action.Direction.LEFT:
                                        pictureBox.Image = global::pacman.Properties.Resources.Left;
                                        break;
                                    case Shared.Action.Direction.UP:
                                        pictureBox.Image = global::pacman.Properties.Resources.Up;
                                        break;
                                    case Shared.Action.Direction.RIGHT:
                                        pictureBox.Image = global::pacman.Properties.Resources.Right;
                                        break;
                                }
                               
                            }
                                
                            pictureBox.Location = new Point(x, y);
                        }));
                        break;
                    case Shared.Action.ActionTaken.REMOVE:
                        pictureBox = stageObjects[action.ID];
                        StageForm.Invoke(new System.Action(() => StageForm.PanelGame.Controls.Remove(pictureBox)));
                        break;

                }
            }
            //todo: score update
            Round = round;
            mutex.ReleaseMutex();
        }

        public void End(IPlayer player)
        {
            // make it appear a button saying: play again?
            // close stage form.
            // do something else
        }

        public void GameOver()
        {

        }

        public void LobbyInfo(string message)
        {
            // Create threats is expensive, a great ideia is do have a pool of threads and assign work to them
            Thread t = new Thread(delegate ()
                        {
                            WelcomeForm.Invoke(new System.Action(() =>
                            {
                                WelcomeForm.LabelError.Text = message;
                                WelcomeForm.LabelError.Visible = true;
                            }));
                        });
            t.Start();
        }

//PRIVATE:

        private PictureBox createControl(string name, Point position, int width, int height)
        {
            PictureBox pictureBox = new PictureBox();
            pictureBox.BackColor = Color.Transparent;
            pictureBox.Location = new Point(position.X - width / 2, position.Y - height / 2);
            pictureBox.Margin = new Padding(0);
            pictureBox.Name = name;
            pictureBox.Size = new Size(width, height);
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.TabIndex = 4;
            pictureBox.TabStop = false;
            return pictureBox;
        }

        private void buildWalls(IStage stage)
        {
            PictureBox newWall;
            int i = 1;
            foreach (IWall wall in stage.GetWalls())
            {
                newWall = createControl("wall" + i++, wall.Position, Wall.WIDTH, Wall.HEIGHT);
                newWall.BackColor = Color.Blue;
                stageObjects.Add(wall.ID, newWall);
                stageObjectsType.Add(wall.ID, "wall");
                StageForm.Invoke(new System.Action(() =>  StageForm.PanelGame.Controls.Add(newWall)));
            }
        }

        private void buildPlayers(IStage stage)
        {
            PictureBox newPlayer;
            int i = 1;
            foreach (IPlayer player in stage.GetPlayers())
            {
                newPlayer = createControl("pacman" + i++, player.Position, Player.WIDTH, Player.HEIGHT);
                newPlayer.Image = global::pacman.Properties.Resources.Left;
                stageObjects.Add(player.ID, newPlayer);
                stageObjectsType.Add(player.ID, "player");
                StageForm.Invoke(new System.Action(() => {
                    StageForm.PanelGame.Controls.Add(newPlayer);
                    StageForm.PanelGame.Controls.SetChildIndex(newPlayer, 0);
               }));
                
            }
        }

        private void buildCoins(IStage stage)
        {
            PictureBox newCoin;
            int i = 1;
            foreach (ICoin coin in stage.GetCoins())
            {
                newCoin = createControl("coin" + i++, coin.Position, Coin.WIDTH, Coin.HEIGHT);
                newCoin.Image = global::pacman.Properties.Resources.coin;
                stageObjects.Add(coin.ID, newCoin);
                stageObjectsType.Add(coin.ID, "coin");
                StageForm.Invoke(new System.Action(() => StageForm.PanelGame.Controls.Add(newCoin)));
            }
        }

        private void buildMonsters(IStage stage)
        {
            PictureBox newMonster;
            int i = 1;
            foreach (IMonster monster in stage.GetMonsters())
            {
                newMonster = createControl("monster" + i++, monster.Position, MonsterAware.WIDTH, MonsterAware.HEIGHT);
                if (i % 3 == 0)
                {
                    newMonster.Image = global::pacman.Properties.Resources.pink_guy;
                }
                else if (i % 3 == 1)
                {
                    newMonster.Image = global::pacman.Properties.Resources.red_guy;
                }
                else if (i % 3 == 2)
                {
                    newMonster.Image = global::pacman.Properties.Resources.yellow_guy;
                }
                stageObjects.Add(monster.ID, newMonster);
                stageObjectsType.Add(monster.ID, "monster");
                StageForm.Invoke(new System.Action(() => StageForm.PanelGame.Controls.Add(newMonster)));

            }
        }

        public void sendPlayersOnGame(Dictionary<string, string> clients)
        {
            foreach (KeyValuePair<string, string> entry in clients)
            {
                IClient client = (IClient)Activator.GetObject(
                    typeof(IClient),
                    entry.Value);
                client.Username = entry.Key;
                this.Clients.Add(client);
            }
        }

        public void sendNewPlayer(string username, string address)
        {
            IClient client = (IClient)Activator.GetObject(
                    typeof(IClient),
                    address);
            client.Username = username;
            this.Clients.Add(client);
        }


        // make it thread safe
        public void SendTextMessage(string username, string message)
        {
            //string msg = username + ": " + message;
            this.messages.Add(username + ": " + message);
            IClient client;
            for (int i = 0; i < this.Clients.Count; i++)
            {
                try
                {
                    client = this.Clients[i];
                    //if (client.Username != username)
                    //{
                        client.MessageToAnotherPeer(this.messages[this.messages.Count - 1]);
                   // }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to send message to another peer. Removing client from my list. " + e.Message);
                    this.Clients.RemoveAt(i);
                }

            }
            //new Thread(broadcastMessage).Start(username);
        }

        // delete?
        private void broadcastMessage(object obj)
        {
            string messageToSend;
            int messageIndex = this.messages.Count - 1; // last message
            string username = (string)obj;
            mutex.WaitOne();
            messageToSend = this.messages[messageIndex];
            mutex.ReleaseMutex();
            // create a thread to send the messages
            IClient client;
            for (int i = 0; i < this.Clients.Count; i++)
            {
                try
                {
                    client = this.Clients[i];
                    if (client.Username != username)
                    {
                        client.MessageToAnotherPeer(messageToSend);
                    }
                }catch(Exception e)
                {
                    Console.WriteLine("Failed to send message to another peer. Removing client from my list. " + e.Message);
                    this.Clients.RemoveAt(i);
                }
                
            }
        }

        public void MessageToAnotherPeer(string message)
        {
            new Thread(() =>
            {
                StageForm.Invoke(new System.Action(() => {
                    mutex.WaitOne();

                    StageForm.TextBoxChatHistory.AppendText("\r\n" + message);
                    mutex.ReleaseMutex();
                }));
            }).Start();
        }

        public void SendClients(Dictionary<string,string> clients)
        {
            foreach (string key in clients.Keys)
            {
                string address = clients[key];
                // ao enviar os dados dos clients para um cliente devem enviar o endereço e o username.
                IClient client = (IClient)Activator.GetObject(
                    typeof(IClient),
                    address);

                client.Username = key;
                client.Address = address;
            
                this.Clients.Add(client);
            }
        }

        
    }
}
