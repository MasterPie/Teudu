using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Teudu.InfoDisplay
{
    /// <summary>
    /// Interaction logic for BoardNavigatorControl.xaml
    /// </summary>
    public partial class BoardNavigatorControl : UserControl
    {
        public BoardNavigatorControl()
        {
            InitializeComponent();
        }

        private IBoardService boardMaster;
        public IBoardService BoardMaster
        {
            set { boardMaster = value; }
        }

        private Board prev, current, next;
        public Board Previous
        {
            set
            {
                prev = value;
            }
        }

        public Board Current
        {
            set
            {
                current = value;
            }
        }

        public Board Next
        {
            set
            {
                next = value;
            }
        }

        private void Advance(Board nextBoard)
        {
            //animation => refresh prev, next boards
            Current = next;
            //jump to current board
            Previous = current;
            Next = next;
        }

        private void Regress(Board prevBoard)
        {
            //animation, completed => refresh prev, next boards
            Current = prev;
            //jump to current board
            Next = current;
            Previous = prevBoard;
        }

        private void JumpToCenter()
        {
            ///TODO: implement
        }

        private double x, y;
        public double XOffset
        {
            set { x = value; }
        }
        public double YOffset
        {
            set { y = value; }
        }

        private bool GoPrevious()
        {
            return false;
        }

        private bool GoNext()
        {
            return false;
        }
    }
}
