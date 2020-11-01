using DCCP;
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
using System.Windows.Shapes;

namespace DCCP_Examples
{
    /// <summary>
    /// Interaction logic for Window_Phase.xaml
    /// </summary>
    public partial class Window_Phase : Window
    {
        int phase_length = 40;
        double phase_obsmult = 2.7;
        Point[] phase_signal;

        int attempts = 5;
        double tau = 0.005;
        double tauM = 1E3;
        double mu = 1.2;
        double ceil = 300;
        int ceilby = 30;

        int seed = 3;

        public Window_Phase()
        {
            InitializeComponent();
            txt_len.Text = phase_length.ToString();
            txt_fact.Text = phase_obsmult.ToString("F3");
            txt_attempts.Text = attempts.ToString();
            txt_tau.Text = tau.ToString("F3");
            txt_tauM.Text = tauM.ToString("E0");
            txt_mu.Text = mu.ToString("F3");
            txt_ceil.Text = ceil.ToString("F3");
            txt_ceilby.Text = ceilby.ToString();
            txt_seed.Text = seed.ToString();
        }

        private void btn_phaseInit_Click(object sender, RoutedEventArgs e)
        {
            int t_int;
            double t_double;

            phase_length = int.TryParse(txt_len.Text, out t_int) ? t_int : phase_length;
            phase_obsmult = double.TryParse(txt_fact.Text, out t_double) ? t_double : phase_obsmult;
            attempts = int.TryParse(txt_attempts.Text, out t_int) ? t_int : attempts;
            tau = double.TryParse(txt_tau.Text, out t_double) ? t_double : tau;
            tauM = double.TryParse(txt_tauM.Text, out t_double) ? t_double : tauM;
            mu = double.TryParse(txt_mu.Text, out t_double) ? t_double : mu;
            ceil = double.TryParse(txt_ceil.Text, out t_double) ? t_double : ceil;
            ceilby = int.TryParse(txt_ceilby.Text, out t_int) ? t_int : ceilby;
            seed = int.TryParse(txt_seed.Text, out t_int) ? t_int : seed;

            btn_phaseStep.IsEnabled = true;

            mainCanvas.ClearCanvas();
            double width = mainCanvas.Width;
            ExUtility.SetSeed(seed);

            phase_signal = new Point[phase_length];
            for (int i = 0; i < phase_length; i++)
                phase_signal[i] = new Point(ExUtility.RandRange(-1, 1), ExUtility.RandRange(-1, 1));

            double maxN = phase_signal.Max(p => ExUtility.NormComplex(p.X, p.Y));

            Point[] pts = new Point[phase_length];
            for (int i = 0; i < phase_length; i++)
                pts[i] = new Point(i / (double)(phase_length - 1) * width, (ExUtility.NormComplex(phase_signal[i].X, phase_signal[i].Y) - maxN / 2) * width * 0.25 + width / 2);

            mainCanvas.DrawPath(Colors.DarkSlateBlue, Colors.LightSteelBlue, 5, 2, pts);
        }

        private void btn_phaseStep_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.ClearCanvas();
            double width = mainCanvas.Width;

            dc_Model model = new dc_Model();
            dc_Var[][] x = new dc_Var[phase_length][]; for (int i = 0; i < phase_length; i++) x[i] = model.AddVarArray("X", -1, 1, 2, ExUtility.RandRange);

            int phase_obs = (int)(phase_length * phase_obsmult);

            for (int i = 0; i < phase_obs; i++)
            {
                double[] aix = ExUtility.RandomDoubles(-1, 1, phase_length);
                double[] aiy = ExUtility.RandomDoubles(-1, 1, phase_length);

                double yx = 0;
                double yy = 0;
                for (int j = 0; j < phase_length; j++)
                {
                    yx += phase_signal[j].X * aix[j] + phase_signal[j].Y * aiy[j];
                    yy += -phase_signal[j].X * aiy[j] + phase_signal[j].Y * aix[j];
                }

                double y = yx * yx + yy * yy;

                dc_Func[] zx = new dc_Func[phase_length];
                dc_Func[] zy = new dc_Func[phase_length];
                for (int j = 0; j < phase_length; j++)
                {
                    zx[j] = model.Fn_Dot(new double[] { aix[j], aiy[j] }, x[j]);
                    zy[j] = model.Fn_Dot(new double[] { -aiy[j], aix[j] }, x[j]);
                }

                dc_Func zxsum = model.Fn_Sum(zx);
                dc_Func zysum = model.Fn_Sum(zy);

                model.AddGE(model.Fn_Negative(model.Fn_SumSquares(zxsum, zysum)), model.Fn_Const(-y));
                model.AddGE(model.Fn_SumSquares(zxsum, zysum), model.Fn_Const(y));
            }

            model.SetObjective(model.Fn_Const(0), model.Fn_Const(0));

            model.param_tau = tau;
            model.param_tauM = tauM;
            model.param_mu = mu;
            model.param_ceil = ceil;
            model.param_ceilby = ceilby;

            model.Solve(attempts, ExUtility.RandRange);
            model.CleanUp();

            double maxN = phase_signal.Max(p => ExUtility.NormComplex(p.X, p.Y));

            Point[] pts = new Point[phase_length];
            for (int i = 0; i < phase_length; i++)
                pts[i] = new Point(i / (double)(phase_length - 1) * width, (ExUtility.NormComplex(phase_signal[i].X, phase_signal[i].Y) - maxN / 2) * width * 0.25 + width / 2);

            mainCanvas.DrawPath(Colors.DarkSlateBlue, Colors.LightSteelBlue, 5, 2, pts);

            for (int i = 0; i < phase_length; i++)
                pts[i] = new Point(i / (double)(phase_length - 1) * width, (ExUtility.NormComplex(x[i][0].lastValue, x[i][1].lastValue) - maxN / 2) * width * 0.25 + width / 2);

            mainCanvas.DrawPath(Colors.Red, Colors.LightSalmon, 5, 2, pts);
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
