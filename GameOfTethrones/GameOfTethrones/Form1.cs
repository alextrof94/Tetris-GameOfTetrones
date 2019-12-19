using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GameOfTethrones
{
    public partial class Form1 : Form
	{
        bool gameStarted = false;
        Field field;
        int blockSize = 36;
        List<Image> numbers;
        List<Figure> figuresScore;
        List<Point> figuresCountsPoss;
        List<int> figuresCounts;
        public Form1()
		{
			InitializeComponent();
            pbBack.Image = Properties.Resources.Splash;
            numbers = new List<Image>();
            numbers.Add(Properties.Resources.numbers_0);
            numbers.Add(Properties.Resources.numbers_1);
            numbers.Add(Properties.Resources.numbers_2);
            numbers.Add(Properties.Resources.numbers_3);
            numbers.Add(Properties.Resources.numbers_4);
            numbers.Add(Properties.Resources.numbers_5);
            numbers.Add(Properties.Resources.numbers_6);
            numbers.Add(Properties.Resources.numbers_7);
            numbers.Add(Properties.Resources.numbers_8);
            numbers.Add(Properties.Resources.numbers_9);
            figuresCountsPoss = new List<Point>();
            figuresCountsPoss.Add(pbFigureScore1.Location);
            figuresCountsPoss.Add(pbFigureScore2.Location);
            figuresCountsPoss.Add(pbFigureScore3.Location);
            figuresCountsPoss.Add(pbFigureScore4.Location);
            figuresCountsPoss.Add(pbFigureScore5.Location);
            figuresCountsPoss.Add(pbFigureScore6.Location);
            figuresCountsPoss.Add(pbFigureScore7.Location);
        }

        [Flags]
        private enum KeyStates
        {
            None = 0,
            Down = 1,
            Toggled = 2
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);

        private static KeyStates GetKeyState(Keys key)
        {
            KeyStates state = KeyStates.None;

            short retVal = GetKeyState((int)key);

            //If the high-order bit is 1, the key is down
            //otherwise, it is up.
            if ((retVal & 0x8000) == 0x8000)
                state |= KeyStates.Down;

            //If the low-order bit is 1, the key is toggled.
            if ((retVal & 1) == 1)
                state |= KeyStates.Toggled;

            return state;
        }

        public static bool IsKeyDown(Keys key)
        {
            return KeyStates.Down == (GetKeyState(key) & KeyStates.Down);
        }

        public static bool IsKeyToggled(Keys key)
        {
            return KeyStates.Toggled == (GetKeyState(key) & KeyStates.Toggled);
        }

        private void ActionPressed()
        {
            if (!gameStarted)
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.MaxScore))
                    topscore = int.Parse(Properties.Settings.Default.MaxScore);
                field = new Field(10, 13);
                animated = new Figure(field);
                figuresScore = new List<Figure>();
                figuresScore.Add(new Figure(field, 1).Rotate().Rotate().Rotate());
                figuresScore.Add(new Figure(field, 2));
                figuresScore.Add(new Figure(field, 3).Rotate().Rotate().Rotate());
                figuresScore.Add(new Figure(field, 4));
                figuresScore.Add(new Figure(field, 5).Rotate().Rotate().Rotate());
                figuresScore.Add(new Figure(field, 6).Rotate().Rotate().Rotate());
                figuresScore.Add(new Figure(field, 7).Rotate().Rotate().Rotate());
                pbBack.Image = Properties.Resources.Level;
                figuresCounts = new List<int>();
                for (int i = 0; i < 7; i++)
                    figuresCounts.Add(0);
                gameStarted = true;
                girlShow = true;
                pbPressStart.Visible = false;
                score = 0;

                millis = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                endAnimation = millis + 1000 / figureSpeed;
                animationInterval = (endAnimation - millis) / animationCount;
                nextAnimatedFrame = millis + animationInterval;
                animationPos = pbNextFigurefield.Location;
                animationPosInterval = new Point(
                            (pbNextFigurefield.Location.X - pbAnimateTo.Location.X) / animationCount,
                            (pbNextFigurefield.Location.Y - pbAnimateTo.Location.Y) / animationCount);
                manState = 1;
                heSawUsShow = true;

                return;
            }

            if (field.nowFigure != null)
                field.nowFigure.Rotate();
            updateFrame();
        }
        private void DownPressed()
        {
            if (field.nowFigure != null)
                field.nowFigure.Move(0, 1);
            updateFrame();
        }
        private void LeftPressed()
        {
            if (field.nowFigure != null)
                field.nowFigure.Move(-1, 0);
            updateFrame();
        }
        private void RightPressed()
        {
            if (field.nowFigure != null)
                field.nowFigure.Move(1, 0);
            updateFrame();
        }
        private void EscPressed()
        {
            Close();
        }

        int manState = 0; // nothing, throw, none
        bool girlShow = false;
        bool heSawUsShow = false;
        private void updateFrame()
        {            
            // draw frame
            // draw back
            pbBack.Image = Properties.Resources.Level;

            // before draw transparent
            Graphics g = Graphics.FromImage(pbBack.Image);

            // draw girl
            if (girlShow)
                g.DrawImage(Properties.Resources.girl, pbGirl.Location);

            // draw man
            switch (manState)
            {
                case 0:
                    g.DrawImage(Properties.Resources.man_1, pbMan.Location);
                    break;
                case 1:
                    g.DrawImage(Properties.Resources.man_2, pbMan.Location);
                    break;
            }

            // draw he saw us
            if (heSawUsShow)
                g.DrawImage(Properties.Resources.he_saw_us, pbHeSawUs.Location);

            // draw blocks static
            for (int i = 0; i < field.blocksStatic.Count; i++)
            {
                g.DrawImage(field.blocksStatic[i].img,
                        (field.blocksStatic[i].posX) * blockSize + pbField.Location.X,
                        (field.blocksStatic[i].posY) * blockSize + pbField.Location.Y);
            }

            // draw now figure
            if (field.nowFigure != null)
                for (int i = 0; i < field.nowFigure.blocks.Count; i++)
                {
                    g.DrawImage(field.nowFigure.blocks[i].img,
                            (field.nowFigure.posX + field.nowFigure.blocks[i].posX) * blockSize + pbField.Location.X,
                            (field.nowFigure.posY + field.nowFigure.blocks[i].posY) * blockSize + pbField.Location.Y);
                }

            // draw next figure
            if (field.nextFigure != null)
                for (int i = 0; i < field.nextFigure.blocks.Count; i++)
                {
                    g.DrawImage(field.nextFigure.blocks[i].img,
                            field.nextFigure.blocks[i].posX * blockSize + pbNextFigurefield.Location.X,
                            field.nextFigure.blocks[i].posY * blockSize + pbNextFigurefield.Location.Y);
                }

            // draw figures score
            foreach (Block b in figuresScore[0].blocks)
                g.DrawImage(b.img,
                        b.posX * blockSize + pbFigure1.Location.X,
                        b.posY * blockSize + pbFigure1.Location.Y);
            foreach (Block b in figuresScore[1].blocks)
                g.DrawImage(b.img,
                        b.posX * blockSize + pbFigure2.Location.X,
                        b.posY * blockSize + pbFigure2.Location.Y);
            foreach (Block b in figuresScore[2].blocks)
                g.DrawImage(b.img,
                        b.posX * blockSize + pbFigure3.Location.X,
                        b.posY * blockSize + pbFigure3.Location.Y);
            foreach (Block b in figuresScore[3].blocks)
                g.DrawImage(b.img,
                        b.posX * blockSize + pbFigure4.Location.X,
                        b.posY * blockSize + pbFigure4.Location.Y);
            foreach (Block b in figuresScore[4].blocks)
                g.DrawImage(b.img,
                        b.posX * blockSize + pbFigure5.Location.X,
                        b.posY * blockSize + pbFigure5.Location.Y);
            foreach (Block b in figuresScore[5].blocks)
                g.DrawImage(b.img,
                        b.posX * blockSize + pbFigure6.Location.X,
                        b.posY * blockSize + pbFigure6.Location.Y);
            foreach (Block b in figuresScore[6].blocks)
                g.DrawImage(b.img,
                        b.posX * blockSize + pbFigure7.Location.X,
                        b.posY * blockSize + pbFigure7.Location.Y);

            // draw score labels
            g.DrawImage(Properties.Resources.score, pbScore.Location);
            g.DrawImage(Properties.Resources.top, pbTop.Location);
            // draw score
            for (int i = 0; i < 6; i++)
            {
                int ch = (int)(score / Math.Pow(10, i) % 10);
                g.DrawImage(numbers[ch],
                        (6 - i - 1) * blockSize + pbScore.Location.X,
                        pbScore.Location.Y + blockSize);
            }
            // draw top
            for (int i = 0; i < 6; i++)
            {
                int ch = (int)(topscore / Math.Pow(10, i) % 10);
                g.DrawImage(numbers[ch],
                        (6 - i - 1) * blockSize + pbTop.Location.X,
                        pbTop.Location.Y + blockSize);
            }

            // draw figures count
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 2; j++)
                {
                    int ch = (int)(figuresCounts[i] / Math.Pow(10, j) % 10);
                    g.DrawImage(numbers[ch], 
                            figuresCountsPoss[i].X + blockSize - j * blockSize,
                            figuresCountsPoss[i].Y);
                }
            // draw animated
            if (animated != null)
                for (int i = 0; i < animated.blocks.Count; i++)
                {
                    g.DrawImage(animated.blocks[i].img,
                            animated.blocks[i].posX * blockSize + animationPos.X,
                            animated.blocks[i].posY * blockSize + animationPos.Y);
                }

            // drawing end
            g.Dispose();
        }

        int score = 0;
        int topscore = 0;
        Figure animated;

        bool upReaded = false;
        bool leftReaded = false;
        bool rightReaded = false;
        bool downReaded = false;
        bool escReaded = false;
        long millis = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        long pressStartNextFrame = 0;
        // figure moving
        long figureNextFrame = 0;
        long figureSpeed = 2; // moves per second
        // animating
        long nextAnimatedFrame = long.MaxValue;
        long endAnimation = long.MaxValue;
        int animationCount = 5;
        long animationInterval = long.MaxValue;
        Point animationPosInterval = new Point(0,0);
        Point animationPos = new Point(0, 0);
        // if he_saw_us at half start man  
        // at end he_saw_us end man - start fugure
        private void tiMain_Tick(object sender, EventArgs e)
        {
            millis = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if (gameStarted)
            {
                if (animated != null)
                {
                    if (millis >= nextAnimatedFrame)
                    {
                        manState = 0;
                        animated.Rotate();
                        animationPos.X -= animationPosInterval.X;
                        animationPos.Y -= animationPosInterval.Y;
                        nextAnimatedFrame = millis + animationInterval;
                        updateFrame();
                    }
                    if (millis >= endAnimation)
                    {
                        field.nowFigure = new Figure(animated);
                        animated = null;
                        field.nextFigure = new Figure(field);
                        manState = 0;
                        heSawUsShow = false;
                    }
                }
                if (field.nowFigure != null)
                {
                    if (millis >= figureNextFrame)
                    {
                        figureNextFrame = millis + 1000 / figureSpeed;
                        if (field.nowFigure.Move(0, 1))
                        {
                            figuresCounts[field.nowFigure.type - 1]++;
                            field.addFigureToField();
                            field.nowFigure = null;
                            endAnimation = millis + 1000 / figureSpeed;
                            animationInterval = (endAnimation - millis) / animationCount;
                            nextAnimatedFrame = millis + animationInterval;
                            animationPos = pbNextFigurefield.Location;
                            animationPosInterval = new Point(
                                        (pbNextFigurefield.Location.X - pbAnimateTo.Location.X) / animationCount,
                                        (pbNextFigurefield.Location.Y - pbAnimateTo.Location.Y) / animationCount);

                            animated = new Figure(field.nextFigure);
                            field.nextFigure = null;
                            score += field.checkLines();
                            heSawUsShow = true;
                            manState = 1;
                            if (field.checkGameOver())
                            {
                                gameStarted = false;
                                if (score > topscore)
                                {
                                    topscore = score;
                                    Properties.Settings.Default.MaxScore = topscore.ToString();
                                    Properties.Settings.Default.Save();
                                }
                            }
                        }
                        updateFrame();
                    }
                }
            }
            else
            {
                if (millis > pressStartNextFrame)
                {
                    pbPressStart.Visible = !pbPressStart.Visible;
                    pressStartNextFrame = millis + 1000 / figureSpeed;
                }
            }

            CheckKeys();
        }

        private void CheckKeys()
        {
            if (IsKeyDown(Keys.Up) | IsKeyDown(Keys.W) | IsKeyDown(Keys.NumPad8) | IsKeyDown(Keys.Space) | IsKeyDown(Keys.Enter))
            {
                if (!upReaded)
                {
                    upReaded = true;
                    ActionPressed();
                }
            }
            else
                upReaded = false;

            if (IsKeyDown(Keys.Left) | IsKeyDown(Keys.A) | IsKeyDown(Keys.NumPad4))
            {
                if (!leftReaded)
                {
                    leftReaded = true;
                    LeftPressed();
                }
            }
            else
                leftReaded = false;

            if (IsKeyDown(Keys.Right) | IsKeyDown(Keys.D) | IsKeyDown(Keys.NumPad6))
            {
                if (!rightReaded)
                {
                    rightReaded = true;
                    RightPressed();
                }
            }
            else
                rightReaded = false;

            if (IsKeyDown(Keys.Down) | IsKeyDown(Keys.S) | IsKeyDown(Keys.NumPad5) | IsKeyDown(Keys.NumPad2))
            {
                if (!downReaded)
                {
                    downReaded = true;
                    DownPressed();
                }
            }
            else
                downReaded = false;

            if (IsKeyDown(Keys.Escape))
            {
                if (!escReaded)
                {
                    escReaded = true;
                    EscPressed();
                }
            }
            else
                escReaded = false;
        }
        
    }

    class Figure
    {
        public Field parentField;
        public int posX { get; set; }
        public int posY { get; set; }
        public List<Block> blocks;
        public int type;

        public Figure Rotate()
        {
            List<Block> buf = new List<Block>();
            for (int i = 0; i < blocks.Count; i++)
            {
                Block b = new Block(-blocks[i].posY, blocks[i].posX, new Bitmap(blocks[i].img));
                b.img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                buf.Add(b);
            }

            if (!parentField.checkBlocksCollisions(posX, posY, buf))
            {
                blocks = buf;
            }
            return this;
        }

        public bool Move(int x, int y)
        {
            Figure buf = new Figure(this);
            buf.posX += x;
            buf.posY += y;

            if (!parentField.checkBlocksCollisions(buf.posX, buf.posY, buf.blocks))
            {
                posX += x;
                posY += y;
                return false;
            }
            return true;
        }

        public Figure(Figure from)
        {
            this.parentField = from.parentField;
            this.posX = from.posX;
            this.posY = from.posY;
            this.blocks = from.blocks;
            this.type = from.type;
        }

        public Figure(Field parentField, int type = 0) // 0 - random, I, L, LM, O, Z, ZM, T
        {
            this.parentField = parentField;
            if (type == 0)
                type = (new Random().Next(1, 8));
            if (type > 7) type = 7;
            this.type = type;
            blocks = new List<Block>();
            posX = 4;
            posY = -5;
            switch (type)
            {
                case 1:
                    blocks.Add(new Block(0, 0, Properties.Resources.I1));
                    blocks.Add(new Block(0, 1, Properties.Resources.I2));
                    blocks.Add(new Block(0, 2, Properties.Resources.I3));
                    break;
                case 2:
                    blocks.Add(new Block(2, 0, Properties.Resources.L1));
                    blocks.Add(new Block(0, 1, Properties.Resources.L2));
                    blocks.Add(new Block(1, 1, Properties.Resources.L3));
                    blocks.Add(new Block(2, 1, Properties.Resources.L4));
                    break;
                case 3:
                    blocks.Add(new Block(0, 0, Properties.Resources.LM1));
                    blocks.Add(new Block(1, 0, Properties.Resources.LM2));
                    blocks.Add(new Block(0, 1, Properties.Resources.LM3));
                    blocks.Add(new Block(0, 2, Properties.Resources.LM4));
                    break;
                case 4:
                    blocks.Add(new Block(0, 0, Properties.Resources.o1));
                    blocks.Add(new Block(1, 0, Properties.Resources.o2));
                    blocks.Add(new Block(0, 1, Properties.Resources.o3));
                    blocks.Add(new Block(1, 1, Properties.Resources.o4));
                    break;
                case 5:
                    blocks.Add(new Block(1, 0, Properties.Resources.Z1));
                    blocks.Add(new Block(0, 1, Properties.Resources.Z2));
                    blocks.Add(new Block(1, 1, Properties.Resources.Z3));
                    blocks.Add(new Block(0, 2, Properties.Resources.Z4));
                    break;
                case 6:
                    blocks.Add(new Block(0, 0, Properties.Resources.ZM1));
                    blocks.Add(new Block(0, 1, Properties.Resources.ZM2));
                    blocks.Add(new Block(1, 1, Properties.Resources.ZM3));
                    blocks.Add(new Block(1, 2, Properties.Resources.ZM4));
                    break;
                case 7:
                    blocks.Add(new Block(0, 0, Properties.Resources.T1));
                    blocks.Add(new Block(0, 1, Properties.Resources.T2));
                    blocks.Add(new Block(1, 1, Properties.Resources.T3));
                    blocks.Add(new Block(0, 2, Properties.Resources.T4));
                    break;
            }
        }
    }

    class Block
    {
        public int posX { get; set; } // relative
        public int posY { get; set; } // relative
        public Image img { get; set; }

        public Block (int posX, int posY, Image img)
        {
            this.posX = posX;
            this.posY = posY;
            this.img = img;
        }
    }

    class Field
    {
        public List<Block> blocksStatic;
        public Figure nowFigure;
        public Figure nextFigure;
        public int width { get; set; }
        public int height { get; set; }

        public void addFigureToField()
        {
            for (int i = 0; i < nowFigure.blocks.Count; i++)
            {
                Block b = new Block(nowFigure.blocks[i].posX + nowFigure.posX, nowFigure.blocks[i].posY + nowFigure.posY, nowFigure.blocks[i].img);
                blocksStatic.Add(b);
            }
            nowFigure = null;
        }

        public bool checkBlocksCollisions(int figurePosX, int figurePosY, List<Block> bl)
        {
            foreach (Block b in blocksStatic)
            {
                foreach (Block ch in bl)
                {
                    if (b.posX == figurePosX + ch.posX && b.posY == figurePosY + ch.posY)
                        return true;
                }
            }

            for (int i = 0; i < bl.Count; i++)
            {
                if (bl[i].posX + figurePosX < 0)
                    return true;
                if (bl[i].posX + figurePosX >= width)
                    return true;
                if (bl[i].posY + figurePosY > height)
                    return true;
            }
            return false;
        }

        public bool checkGameOver()
        {
            foreach (Block b in blocksStatic)
                if (b.posY < 0)
                    return true;
            return false;
        }

        public int checkLines()
        {
            int res = 0;
            for (int y = 0; y < height + 1; y++)
            {
                List<Block> line = new List<Block>();
                foreach (Block b in blocksStatic)
                {
                    if (b.posY == y)
                        line.Add(b);
                }
                if (line.Count == width)
                {
                    blocksStatic.RemoveAll(x => line.Contains(x));
                    foreach (Block b in blocksStatic)
                    {
                        if (b.posY < y)
                            b.posY++;
                    }
                    y--;
                    res++;
                }
            }

            return res;
        }

        public Field(int width, int height)
        {
            this.width = width;
            this.height = height;
            blocksStatic = new List<Block>();
            nextFigure = new Figure(this);
        }
    }
}
