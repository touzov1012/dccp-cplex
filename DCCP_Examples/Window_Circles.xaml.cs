using DCCP;
using System.Windows;
using System.Windows.Media;

namespace DCCP_Examples
{
    /// <summary>
    /// Interaction logic for Window_Circles.xaml
    /// </summary>
    public partial class Window_Circles : Window
    {
        int circ_cnt = 12;
        Point[] circ_cntrs;
        double[] circ_rads;

        int attempts = 50;
        double tau = 0.5;
        double tauM = 1E3;
        double mu = 1.2;
        double ceil = double.PositiveInfinity;
        int ceilby = 30;

        int seed = 1234;

        public Window_Circles()
        {
            InitializeComponent();
            txt_circleCnt.Text = circ_cnt.ToString();
            txt_attempts.Text = attempts.ToString();
            txt_tau.Text = tau.ToString("F3");
            txt_tauM.Text = tauM.ToString("E0");
            txt_mu.Text = mu.ToString("F3");
            txt_ceil.Text = ceil.ToString("F3");
            txt_ceilby.Text = ceilby.ToString();
            txt_seed.Text = seed.ToString();
        }

        private void btn_circInit_Click(object sender, RoutedEventArgs e)
        {
            int t_int;
            double t_double;

            circ_cnt = int.TryParse(txt_circleCnt.Text, out t_int) ? t_int : circ_cnt;
            attempts = int.TryParse(txt_attempts.Text, out t_int) ? t_int : attempts;
            tau = double.TryParse(txt_tau.Text, out t_double) ? t_double : tau;
            tauM = double.TryParse(txt_tauM.Text, out t_double) ? t_double : tauM;
            mu = double.TryParse(txt_mu.Text, out t_double) ? t_double : mu;
            ceil = double.TryParse(txt_ceil.Text, out t_double) ? t_double : ceil;
            ceilby = int.TryParse(txt_ceilby.Text, out t_int) ? t_int : ceilby;
            seed = int.TryParse(txt_seed.Text, out t_int) ? t_int : seed;

            circ_cntrs = new Point[circ_cnt];
            circ_rads = new double[circ_cnt];

            btn_circStep.IsEnabled = true;

            mainCanvas.ClearCanvas();
            ExUtility.SetSeed(seed);
            double width = mainCanvas.Width;

            for (int i = 0; i < circ_rads.Length; i++)
            {
                circ_rads[i] = ExUtility.RandRange(width / 20, width / 6);
                circ_cntrs[i] = new Point(ExUtility.RandRange(circ_rads[i], width - circ_rads[i]), ExUtility.RandRange(circ_rads[i], width - circ_rads[i]));
                mainCanvas.DrawCircle(Colors.LightSalmon.Fade(127), Colors.Gray, 2, circ_cntrs[i], circ_rads[i]);
            }
        }

        private void btn_circStep_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.ClearCanvas();

            double[] maxs = ExUtility.Constant(circ_cnt, mainCanvas.Width);
            maxs.SubtractBy(circ_rads);

            dc_Model model = new dc_Model();
            dc_Var z = model.AddVar("Z", 0, mainCanvas.Width, mainCanvas.Width);
            dc_Var[] x = model.AddVarArray("X", circ_rads, maxs, ExUtility.RandRange);
            dc_Var[] y = model.AddVarArray("Y", circ_rads, maxs, ExUtility.RandRange);
            model.SetObjective(z, model.Fn_Const(0));
            for (int i = 0; i < circ_cnt; i++)
            {
                model.AddLE(model.Fn_Affine(1, circ_rads[i], x[i]), z);
                model.AddLE(model.Fn_Affine(1, circ_rads[i], y[i]), z);
            }

            for (int i = 0; i < circ_cnt; i++)
            {
                for (int j = i + 1; j < circ_cnt; j++)
                {
                    dc_Func dfx = model.Fn_Diff(x[i], x[j]);
                    dc_Func dfy = model.Fn_Diff(y[i], y[j]);
                    model.AddGE(model.Fn_SumSquares(dfx, dfy), model.Fn_Const((circ_rads[i] + circ_rads[j]) * (circ_rads[i] + circ_rads[j])));
                }
            }

            model.param_tau = tau;
            model.param_tauM = tauM;
            model.param_mu = mu;
            model.param_ceil = ceil;
            model.param_ceilby = ceilby;

            double bound = model.Solve(attempts, ExUtility.RandRange);

            mainCanvas.DrawBounds(bound);
            for (int i = 0; i < circ_cnt; i++)
            {
                mainCanvas.DrawCircle(Colors.LightSalmon.Fade(127), Colors.Gray, 2, new Point(x[i].lastValue, y[i].lastValue), circ_rads[i]);
            }
            model.CleanUp();
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
