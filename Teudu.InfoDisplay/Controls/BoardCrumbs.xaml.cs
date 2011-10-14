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
    /// Interaction logic for BoardCrumbs.xaml
    /// </summary>
    public partial class BoardCrumbs : UserControl
    {
        private Dictionary<Board, Ellipse> boards;
        public BoardCrumbs()
        {
            InitializeComponent();
            boards = new Dictionary<Board, Ellipse>();
        }

        public List<Board> Boards
        {
            set
            {
                boards.Clear();
                this.EllipseContainer.Children.Clear();
                value.ForEach(x=> {
                    Ellipse circle = new Ellipse();
                    circle.Width = 10;
                    circle.Height = 10;
                    circle.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    circle.Opacity = 0.3;
                    circle.Margin = new Thickness(25, 0, 0, 0);
                    boards.Add(x, circle);
                    this.EllipseContainer.Children.Add(circle);
                });
            }
        }

        public Board Current
        {
            set
            {
                SetCurrentEllipse(value);
            }
        }

        private void SetCurrentEllipse(Board board)
        {
            boards.Values.ToList().ForEach(x => { x.Opacity = 0.3; x.Height = 10; x.Width = 10; });
            if (boards.ContainsKey(board))
            {
                Ellipse targetCrumb = boards[board];
                targetCrumb.Opacity = 1;
                targetCrumb.Width = 20;
                targetCrumb.Height = 20;
            }
        }
    }
}
