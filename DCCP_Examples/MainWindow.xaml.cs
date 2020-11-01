using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DCCP;

namespace DCCP_Examples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //DC_Debug();

            dc_Utility.WriteLine("Initialized.");
        }

        private void DC_Debug()
        {
            dc_Model model = new dc_Model();

            dc_Var[] P1 = model.AddVarArray("Point_1", -5, 5, 2, ExUtility.RandRange);
            dc_Var[] P2 = model.AddVarArray("Point_2", -5, 5, 2, ExUtility.RandRange);

            model.SetObjective(model.Fn_SumSquares(model.Fn_Diff(P1[0], P2[0]), model.Fn_Diff(P1[1], P2[1])),
                model.Fn_Const(0));

            model.AddLE(model.Fn_SumSquares(P1[0], model.Fn_Scale(0.5, P1[1])),
                model.Fn_Const(1));

            model.AddLE(model.Fn_Sum(P2[1], model.Fn_Const(9)),
                model.Fn_Square(model.Fn_Diff(P2[0], model.Fn_Const(0.5))));

            model.Solve(5, ExUtility.RandRange);
            model.CleanUp();
        }

        private void b_circles_Click(object sender, RoutedEventArgs e)
        {
            Window_Circles w = new Window_Circles();
            w.Left = this.Left;
            w.Top = this.Top;
            w.Show();
            this.Close();
        }

        private void b_polys_Click(object sender, RoutedEventArgs e)
        {
            Window_Polys w = new Window_Polys();
            w.Left = this.Left;
            w.Top = this.Top;
            w.Show();
            this.Close();
        }

        private void b_path_Click(object sender, RoutedEventArgs e)
        {
            Window_Path w = new Window_Path();
            w.Left = this.Left;
            w.Top = this.Top;
            w.Show();
            this.Close();
        }

        private void b_collision_Click(object sender, RoutedEventArgs e)
        {
            Window_Collision w = new Window_Collision();
            w.Left = this.Left;
            w.Top = this.Top;
            w.Show();
            this.Close();
        }

        private void b_phase_Click(object sender, RoutedEventArgs e)
        {
            Window_Phase w = new Window_Phase();
            w.Left = this.Left;
            w.Top = this.Top;
            w.Show();
            this.Close();
        }

        private void b_cut_Click(object sender, RoutedEventArgs e)
        {
            Window_Cut w = new Window_Cut();
            w.Left = this.Left;
            w.Top = this.Top;
            w.Show();
            this.Close();
        }

        private void b_console_Click(object sender, RoutedEventArgs e)
        {
            dc_ConsoleManager.Toggle();
        }
    }
}
