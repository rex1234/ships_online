using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Ships
{
    class ShipBoardView
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public int FieldSize { get; set; }
        public bool Clickable { get; set; }
        public Color BackColor { get; set; }

        public Color FieldColor { get; set; }

        public Color this[int x, int y]
        {
            get { return board[x, y].BackColor; }
            set { board[x, y].BackColor = value; }
        }

        public event OnFieldClicked FieldClicked;
        public delegate void OnFieldClicked(int x, int y);

        private ShipButton[,] board;
        private Form parent;

        public ShipBoardView(Form parent)
        {
            this.parent = parent;

            //default values
            Height = 12;
            Width = 12;
            FieldSize = 28;
            BackColor = Color.Azure;
            FieldColor = Color.Lavender;
            
            board = new ShipButton[Width, Height];            
        }

        public void Show()
        {
            AddButtons();
        }

        public int Fill(int x, int y, Color fillColor)
        {
            return Fill(x, y, this[x, y], fillColor);
        }

        private int Fill(int x, int y, Color startColor, Color fillColor)
        {
            if (x >=0 && y >= 0 && x < Width && y < Height && this[x, y] == startColor)
            {
                this[x, y] = fillColor;
                return 1 + Fill(x + 1, y, startColor, fillColor) + 
                    Fill(x - 1, y, startColor, fillColor) +             
                    Fill(x, y + 1, startColor, fillColor) +
                    Fill(x, y - 1, startColor, fillColor);
            }
            return 0;
        }

        private void AddButtons()
        {
           for (int j = 0; j < Height; j++)
           {
                for (int i = 0; i < Width; i++)
                {
                    ShipButton button = new ShipButton(FieldSize) { X = i, Y = j };
                    button.Location = new System.Drawing.Point(
                        X + i * button.Width, Y + j * button.Height);                    
                    button.Click += ShipButtonClicked;
                    button.BackColor = FieldColor;
                    button.BorderColor = BackColor;

                    board[i, j] = button;
                    parent.Controls.Add(button);
                }
            }
        }

        private void ShipButtonClicked(object sender, EventArgs args)
        {
            if (Clickable)
            {
                ShipButton clicked = sender as ShipButton;
                if (FieldClicked != null)
                {
                    FieldClicked(clicked.X, clicked.Y);
                }
            }
        }

        public void Reset()
        {
            foreach (var button in board)
            {
                button.BackColor = FieldColor;
            }
        }
    }
}
