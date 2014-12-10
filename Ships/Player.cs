using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ships
{
    class Player
    {
        //board codes
        public const int EmptyField = 0;
        public const int ShipField = 1;

        //network codes
        public const int Miss = 0;
        public const int Hit = 1;
        public const int HitWholeShip = 2;

        public string Name { get; set; }
        public bool Ready { get; set; }

        public int Height { get; set; }
        public int Width { get; set; }
        public bool IsError { get; private set; } //wrongly places ship

        //Ships we destroyed to the enemy
        public IDictionary<int, int> FoundShipCounts { get; private set; }

        public int RemainingShips
        {
            get 
            { 
                return Form1.ShipCounts.Select(s => s.Value).Sum() -
                    destroyedShips.Select(s => s.Value).Sum(); 
            }
        }

        public bool FoundAll
        {
            get
            {
                return FoundShipCounts.Select(s => s.Value).Sum() ==
                    Form1.ShipCounts.Select(s => s.Value).Sum();
            }
        }
        
        //which fields enemy tried to guess
        public bool[,] Guessed { get; private set; }

        private int[,] board;        
        //no. of placed ships when creating the game
        IDictionary<int, int> ships;
        //no. of ships the enemy destroyed
        IDictionary<int, int> destroyedShips;

        public int this[int i, int j]
        {
            get { return board[i, j]; }
            set 
            { 
                board[i, j] = value;
                FindShips();
            }
        }

        public Player(int width, int height)
        {
            Width = width;
            Height = height;
            board = new int[width, height];
            Guessed = new bool[width, height];
            Name = System.Environment.MachineName;

            ships = new Dictionary<int, int>();
            destroyedShips = new Dictionary<int, int>();
            FoundShipCounts = new Dictionary<int, int>();
            for (int i = 0; i <= Width; i++)
            {                
                ships[i] = 0;
                destroyedShips[i] = 0;
                FoundShipCounts[i] = 0;
            }
        }


        public int GetShipCount(int shipLength)
        {
            return ships[shipLength];
        }

        public int Guess(int x, int y)
        {
            Guessed[x, y] = true;

            if (board[x, y] == Player.ShipField)
            {
                int shipLength;
                if (IsWholeShipGuessed(x, y, out shipLength))
                {
                    destroyedShips[shipLength]++;
                    return HitWholeShip;
                }
                else
                {
                    return Hit;
                }
            }
            else
            {
                return Miss;
            }
        }

        public bool IsWholeShipGuessed(int x, int y, out int shipLength)
        {
            bool whole = true;

            //najde prvu hornu / lavu kocku lode
            while (x > 0 && board[x - 1, y] == ShipField) --x;
            while (y > 0 && board[x, y - 1] == ShipField) --y;

            int startX = x;
            int startY = y;

            shipLength = 0;

            while (whole && y < Height && board[x, y] == ShipField)
            {
                whole = whole && Guessed[x, y];
                ++y;
                ++shipLength;
            }

            x = startX;
            y = startY;
            while (whole && x < Width && board[x, y] == ShipField)
            {
                whole = whole && Guessed[x, y];
                ++x;
                ++shipLength;
            }

            --shipLength;

            return whole;
        }

        private void FindShips()
        {
            bool error = false;
            
            for (int i = 0; i < 12; i++)
            {
                ships[i] = 0;
            }

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (board[i, j] != ShipField)
                    {
                        continue;
                    }

                    int shipLength = 0;
                    bool found = true;

                    //finds horizontal ships
                    if (j == 0 || board[i, j - 1] != ShipField)
                    {
                        while (j + shipLength < Width && board[i, j + shipLength] == ShipField)
                        {
                            //check if there is only water around the ship
                            found = (i == Height - 1 || board[i + 1, j + shipLength] != ShipField) &&
                                     (i == 0 || board[i - 1, j + shipLength] != ShipField);
                            ++shipLength;
                        }
                        //corners
                        found = found &&
                            (i == 0 || j == 0 || board[i - 1, j - 1] != ShipField) &&
                            (i == Height - 1 || j == 0 || board[i + 1, j - 1] != ShipField) &&
                            (i == 0 || j + shipLength == Width || board[i - 1, j + shipLength] != ShipField) &&
                            (i == Height - 1 || j + shipLength == Width || board[i + 1, j + shipLength] != ShipField);


                        //finds vertical ships
                        if (!found)
                        {
                            found = true;
                            shipLength = 0;
                            if (i == 0 || board[i - 1, j] != ShipField)
                            {
                                while (found && i + shipLength < 12 && board[i + shipLength, j] == ShipField)
                                {
                                    //check if there is only water around the ship
                                    found = (j == Width - 1 || board[i + shipLength, j + 1] != ShipField) &&
                                             (j == 0 || board[i + shipLength, j - 1] != ShipField);
                                    ++shipLength;                                    
                                }
                                //corners
                                found = found &&
                                    (j == 0 || i == 0 || board[i - 1, j - 1] != ShipField) &&
                                    (j == Width - 1 || i == 0 || board[i - 1, j + 1] != ShipField) &&
                                    (j == 0 || i + shipLength == Height || board[i + shipLength, j - 1] != ShipField) &&
                                    (j == Width - 1 || i + shipLength == Height || board[i + shipLength, j + 1] != ShipField);
                            }
                        }

                        if (found)
                        {
                            ++ships[shipLength];
                        }
                        else
                        {
                            error = true;
                        }
                    }
                }
            }
            IsError = error;
        }

        public int GetDestroyedShipCount(int shipLength)
        {
            return destroyedShips[shipLength];
        }
    }
}
