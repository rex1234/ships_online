using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ships
{
    class ShipButton : Button
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Color BorderColor 
        { 
            get { return FlatAppearance.BorderColor;}  
            set { FlatAppearance.BorderColor = value;} 
        }

        public ShipButton(int size)
        {
            FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            Size = new System.Drawing.Size(size, size);            
            FlatAppearance.BorderSize = 1;
        }
    }
}
