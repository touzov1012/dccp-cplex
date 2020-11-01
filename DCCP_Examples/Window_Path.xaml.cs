using DCCP;
using System.Windows;
using System.Windows.Media;

namespace DCCP_Examples
{
    /// <summary>
    /// Interaction logic for Window_Path.xaml
    /// </summary>
    public partial class Window_Path : Window
    {
        int blocker_segs = 20;
        int blocker_cnt = 5;
        Point[] blocker_cntrs;
        double[] blocker_rads;

        int attempts = 8;
        double tau = 0.005;
        double tauM = 1E3;
        double mu = 1.2;
        double ceil = double.PositiveInfinity;
        int ceilby = 30;

        int seed = 32;

        public Window_Path()
        {
            InitializeComponent();
            txt_blockers.Text = blocker_cnt.ToString();
            txt_segs.Text = blocker_segs.ToString();
            txt_attempts.Text = attempts.ToString();
            txt_tau.Text = tau.ToString("F3");
            txt_tauM.Text = tauM.ToString("E0");
            txt_mu.Text = mu.ToString("F3");
            txt_ceil.Text = ceil.ToString("F3");
            txt_ceilby.Text = ceilby.ToString();
            txt_seed.Text = seed.ToString();
        }

        private void btn_pathInit_Click(object sender, RoutedEventArgs e)
        {
            int t_int;
            double t_double;

            blocker_cnt = int.TryParse(txt_blockers.Text, out t_int) ? t_int : blocker_cnt;
            blocker_segs = int.TryParse(txt_segs.Text, out t_int) ? t_int : blocker_segs;
            attempts = int.TryParse(txt_attempts.Text, out t_int) ? t_int : attempts;
            tau = double.TryParse(txt_tau.Text, out t_double) ? t_double : tau;
            tauM = double.TryParse(txt_tauM.Text, out t_double) ? t_double : tauM;
            mu = double.TryParse(txt_mu.Text, out t_double) ? t_double : mu;
            ceil = double.TryParse(txt_ceil.Text, out t_double) ? t_double : ceil;
            ceilby = int.TryParse(txt_ceilby.Text, out t_int) ? t_int : ceilby;
            seed = int.TryParse(txt_seed.Text, out t_int) ? t_int : seed;

            btn_pathStep.IsEnabled = true;

            blocker_cntrs = new Point[blocker_cnt];
            blocker_rads = new double[blocker_cnt];

            mainCanvas.ClearCanvas();
            ExUtility.SetSeed(seed);
            double width = mainCanvas.Width;

            for (int i = 0; i < blocker_rads.Length; i++)
            {
                blocker_rads[i] = ExUtility.RandRange(width / 10, width / 6);
                blocker_cntrs[i] = new Point(ExUtility.RandRange(blocker_rads[i], width - blocker_rads[i]), ExUtility.RandRange(blocker_rads[i], width - blocker_rads[i]));
                mainCanvas.DrawCircle(Colors.LightSalmon, Colors.LightSalmon, 0, blocker_cntrs[i], blocker_rads[i]);
            }
        }

        private void btn_pathStep_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.ClearCanvas();
            double width = mainCanvas.Width;

            dc_Model model = new dc_Model();
            dc_Var L = model.AddVar("L", 0, width * width, ExUtility.RandRange);
            dc_Var[] px = model.AddVarArray("Px", 0, width, blocker_segs + 1, ExUtility.RandRange);
            dc_Var[] py = model.AddVarArray("Py", 0, width, blocker_segs + 1, ExUtility.RandRange);

            model.AddEQ(px[0], model.Fn_Const(0));
            model.AddEQ(py[0], model.Fn_Const(0));

            model.AddEQ(px[blocker_segs], model.Fn_Const(width));
            model.AddEQ(py[blocker_segs], model.Fn_Const(width));

            for (int j = 1; j < blocker_segs; j++)
            {
                for (int i = 0; i < blocker_cnt; i++)
                {
                    dc_Func dfx = model.Fn_Diff(px[j], model.Fn_Const(blocker_cntrs[i].X));
                    dc_Func dfy = model.Fn_Diff(py[j], model.Fn_Const(blocker_cntrs[i].Y));

                    model.AddGE(model.Fn_SumSquares(dfx, dfy), model.Fn_Const(blocker_rads[i] * blocker_rads[i]));
                }
            }

            for (int j = 1; j <= blocker_segs; j++)
            {
                dc_Func dfx = model.Fn_Diff(px[j], px[j - 1]);
                dc_Func dfy = model.Fn_Diff(py[j], py[j - 1]);

                model.AddLE(model.Fn_SumSquares(dfx, dfy), L);
                //model.AddGE(model.Fn_SumSquares(dfx, dfy), model.Fn_Const(width * Math.Sqrt(2) / (segments)));
            }

            model.SetObjective(L, model.Fn_Const(0));

            model.param_tau = tau;
            model.param_tauM = tauM;
            model.param_mu = mu;
            model.param_ceil = ceil;
            model.param_ceilby = ceilby;

            model.Solve(attempts, ExUtility.RandRange);
            model.CleanUp();

            for (int i = 0; i < blocker_rads.Length; i++)
                mainCanvas.DrawCircle(Colors.LightSalmon, Colors.LightSalmon, 0, blocker_cntrs[i], blocker_rads[i]);

            Point[] path = new Point[blocker_segs + 1];
            for (int j = 0; j <= blocker_segs; j++)
                path[j] = new Point(px[j].lastValue, py[j].lastValue);

            mainCanvas.DrawPath(Colors.DarkSlateBlue, Colors.LightSteelBlue, 6, 2, path);
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow w = new MainWindow();
            w.Left = this.Left;
            w.Top = this.Top;
            w.Show();
            this.Close();
        }
    }
}
