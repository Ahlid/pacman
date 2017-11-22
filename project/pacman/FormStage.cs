﻿using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace pacman {
    public partial class FormStage : Form {

        // arranjar maneira de declarar os eventos à mão para key down e key up para quando o jogo for automatico 
        // nao dar para jogar!, apenas para falar no chat

        private Play play = Play.NONE;

        private Dictionary<int, PictureBox> stageObjects;
        private Dictionary<int, Point> stageObjectsOriginalPositions;
        private Dictionary<int, string> stageObjectsType;
        private Dictionary<int, string> roundState;

        static volatile Mutex mutex = new Mutex(false);

        private Hub hub;

        public FormStage(Hub hub, IStage stage) {

            InitializeComponent();
            this.hub = hub;

            stageObjects = new Dictionary<int, PictureBox>();
            stageObjectsType = new Dictionary<int, string>();
            roundState = new Dictionary<int, string>();
            stageObjectsOriginalPositions = new Dictionary<int, Point>();




            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
            }

            this.Invoke(new System.Action(() => {
                mutex.WaitOne();
                buildWalls(stage);
                buildCoins(stage);
                buildMonsters(stage);
                buildPlayers(stage);
                mutex.ReleaseMutex();
            }));

            hub.OnRoundReceived += SendRound;


            /*
             * activate keyisdown and key is up, only on simple game
             * // this has to set delegates for key is down and key is up
             * 
             * // and on the automated game the chat is available 
             * 
             * // make the chat as the one on lol -> enter with no text "minimizes the chat bar"
             * 
             */ 
        }


        private void keyIsDown(object sender, KeyEventArgs e) {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    play = Play.LEFT;
                    break;
                case Keys.Right:
                    play = Play.RIGHT;
                    break;
                case Keys.Up:
                    play = Play.UP;
                    break;
                case Keys.Down:
                    play = Play.DOWN;
                    break;
                case Keys.Enter:
                    this.textBoxMessage.Enabled = true;
                    this.textBoxMessage.Focus();
                    this.textBoxMessage.Select();
                    break;
            }

        }

        private void keyIsUp(object sender, KeyEventArgs e) {


            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    play = Play.NONE;
                    break;
            }
        }


        //Running every 20 ms
        private void timer1_Tick(object sender, EventArgs e) {

            /*
             *  a cada 20 ms vai buscar a jogada que está no jogo
             *  
             */ 

            if(play != Play.NONE)
            {
                //hub.SetPlay(play);
                //hub.SetPlay(this.hub.CurrentSession.game.Move);
                this.hub.CurrentSession.game.Move = play;
                // aqui faz so set da jogada 
            }
        }

        private Point fit(Point point)
        {
            int x = point.X * panelGame.Size.Width / Stage.WIDTH;
            int y = point.Y * panelGame.Size.Height / Stage.HEIGHT;
            return new Point(x, y);
        }

        public void SendRound(List<Shared.Action> actions, int score, int round)
        {

            mutex.WaitOne();
            foreach (Shared.Action action in actions)
            {
                PictureBox pictureBox;
                switch (action.action)
                {
                    case Shared.Action.ActionTaken.MOVE:
                        pictureBox = stageObjects[action.ID];
                        Point originalPos = stageObjectsOriginalPositions[action.ID];
                        originalPos = new Point(originalPos.X + action.displacement.X, originalPos.Y + action.displacement.Y);
                        stageObjectsOriginalPositions[action.ID] = originalPos;
                        Point pos = fit(originalPos);
                        int x = pos.X;
                        int y = pos.Y;
      
                        Invoke(new System.Action(() =>
                        {
                            if (stageObjectsType[action.ID] == "player")
                            {
                                switch (action.direction)
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
                        Invoke(new System.Action(() => panelGame.Controls.Remove(pictureBox)));
                        break;

                }
            }
            //todo: score update

            this.roundState.Add(round, GetState());
            mutex.ReleaseMutex();
        }

        private void textBoxMessage_OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                this.hub.PublishMessage(this.textBoxMessage.Text);
                this.textBoxMessage.Clear();
                this.textBoxMessage.Enabled = false;
                this.Select();
            }
        }

        public string GetState(int round)
        {
            if (roundState.ContainsKey(round))
            {
                return roundState[round];
            }

            return null;
        }

        private string GetState()
        {
            int pacmanCount = 1;
            int monsterCount = 1;
            int coinCount = 1;
            string state = "";
            foreach (int key in stageObjects.Keys)
            {
                switch (stageObjectsType[key])
                {
                    case "player":
                        //todo P or L
                        state += "P" + (pacmanCount++) + ", P ";
                        break;
                    case "monster":
                        state += "M" + (monsterCount++) + ", ";
                        break;
                    case "coin":
                        state += "C" + (coinCount++) + ", ";
                        break;
                }

                if (stageObjectsType[key] == "player" || stageObjectsType[key] == "monster" || stageObjectsType[key] == "coin")
                    state += stageObjects[key].Left + ", " + stageObjects[key].Top + "\n";

            }
            return state;
        }

        private PictureBox createControl(string name, Point position, int width, int height)
        {
            PictureBox pictureBox = new PictureBox();
            
            Point size = fit(new Point(width, height));
            pictureBox.Size = new Size(size.X, size.Y);

            Point pos = fit(position);

            pictureBox.BackColor = Color.Transparent;
            pictureBox.Location = fit(new Point(pos.X - size.X / 2, pos.Y - size.Y / 2)); 
            pictureBox.Margin = new Padding(0);
            pictureBox.Name = name;
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
                stageObjectsOriginalPositions.Add(wall.ID, wall.Position);
                stageObjectsType.Add(wall.ID, "wall");
                Invoke(new System.Action(() => panelGame.Controls.Add(newWall)));
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
                stageObjectsOriginalPositions.Add(player.ID, player.Position);
                stageObjectsType.Add(player.ID, "player");
                Invoke(new System.Action(() => {
                    panelGame.Controls.Add(newPlayer);
                    panelGame.Controls.SetChildIndex(newPlayer, 0);
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
                newCoin.Image = global::pacman.Properties.Resources.cccc;
                newCoin.SizeMode = PictureBoxSizeMode.StretchImage;
                stageObjects.Add(coin.ID, newCoin);
                stageObjectsOriginalPositions.Add(coin.ID, coin.Position);
                stageObjectsType.Add(coin.ID, "coin");
                Invoke(new System.Action(() => panelGame.Controls.Add(newCoin)));
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
                stageObjectsOriginalPositions.Add(monster.ID, monster.Position);
                stageObjectsType.Add(monster.ID, "monster");
                Invoke(new System.Action(() => panelGame.Controls.Add(newMonster)));
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

    }
}
