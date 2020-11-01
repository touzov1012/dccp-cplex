using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DCCP;

namespace DCCP_Examples
{
    /// <summary>
    /// Interaction logic for Window_Collision.xaml
    /// </summary>
    public partial class Window_Collision : Window
    {
        int col_seed = 4;
        int col_T = 50;
        int col_cnt = 5;
        double col_dmin = 40;
        double col_velrng = 25;
        double col_maxpush = 50;
        double[,] col_A = new double[,] {
            { 1, 0, 0.1, 0 },
            { 0, 1, 0, 0.1 },
            { 0, 0, 0.95, 0 },
            { 0, 0, 0, 0.95 }
        };
        double[,] col_B = new double[,] {
            { 0, 0 },
            { 0, 0 },
            { 0.1, 0 },
            { 0, 0.1 }
        };
        Point[] startPos;
        Point[] endPos;
        Point[] startVel;
        Point[] endVel;

        int attempts = 3;
        double tau = 0.005;
        double tauM = 1E3;
        double mu = 1.2;
        double ceil = double.PositiveInfinity;
        int ceilby = 30;

        public Window_Collision()
        {
            InitializeComponent();
            txt_steps.Text = col_T.ToString();
            txt_nodes.Text = col_cnt.ToString();
            txt_attempts.Text = attempts.ToString();
            txt_tau.Text = tau.ToString("F3");
            txt_tauM.Text = tauM.ToString("E0");
            txt_mu.Text = mu.ToString("F3");
            txt_ceil.Text = ceil.ToString("F3");
            txt_ceilby.Text = ceilby.ToString();
            txt_seed.Text = col_seed.ToString();
            txt_mind.Text = col_dmin.ToString("F3");
            txt_rndvel.Text = col_velrng.ToString("F3");
            txt_maxpush.Text = col_maxpush.ToString("F3");
        }

        private void SetParameters()
        {
            int t_int;
            double t_double;

            col_T = int.TryParse(txt_steps.Text, out t_int) ? t_int : col_T;
            col_cnt = int.TryParse(txt_nodes.Text, out t_int) ? t_int : col_cnt;
            attempts = int.TryParse(txt_attempts.Text, out t_int) ? t_int : attempts;
            tau = double.TryParse(txt_tau.Text, out t_double) ? t_double : tau;
            tauM = double.TryParse(txt_tauM.Text, out t_double) ? t_double : tauM;
            mu = double.TryParse(txt_mu.Text, out t_double) ? t_double : mu;
            ceil = double.TryParse(txt_ceil.Text, out t_double) ? t_double : ceil;
            ceilby = int.TryParse(txt_ceilby.Text, out t_int) ? t_int : ceilby;
            col_seed = int.TryParse(txt_seed.Text, out t_int) ? t_int : col_seed;
            col_dmin = double.TryParse(txt_mind.Text, out t_double) ? t_double : col_dmin;
            col_velrng = double.TryParse(txt_rndvel.Text, out t_double) ? t_double : col_velrng;
            col_maxpush = double.TryParse(txt_maxpush.Text, out t_double) ? t_double : col_maxpush;
        }

        private void Collision_Test(bool avoidCollision)
        {
            SetParameters();

            double width = mainCanvas.Width;

            double[] stateLB = new double[] { 0, 0, double.MinValue, double.MinValue };
            double[] stateUB = new double[] { width, width, double.MaxValue, double.MaxValue };

            mainCanvas.ClearCanvas();
            ExUtility.SetSeed(col_seed);

            // u [obj] [time] [x,y]
            // x [obj] [time] [x,y,vx,vy]
            dc_Model model = new dc_Model();
            dc_Var[][][] u = new dc_Var[col_cnt][][]; for (int i = 0; i < col_cnt; i++) { u[i] = new dc_Var[col_T][]; for (int j = 0; j < col_T; j++) u[i][j] = model.AddVarArray("U", -col_maxpush, col_maxpush, 2, ExUtility.RandRange); }
            dc_Var[][][] x = new dc_Var[col_cnt][][]; for (int i = 0; i < col_cnt; i++) { x[i] = new dc_Var[col_T + 1][]; for (int j = 0; j <= col_T; j++) x[i][j] = model.AddVarArray("X", stateLB, stateUB, ExUtility.RandRange); }

            for (int i = 0; i < col_cnt; i++)
            {
                model.AddEQ(x[i][0][0], model.Fn_Const(startPos[i].X));
                model.AddEQ(x[i][0][1], model.Fn_Const(startPos[i].Y));
                model.AddEQ(x[i][0][2], model.Fn_Const(startVel[i].X));
                model.AddEQ(x[i][0][3], model.Fn_Const(startVel[i].Y));

                model.AddEQ(x[i][col_T][0], model.Fn_Const(endPos[i].X));
                model.AddEQ(x[i][col_T][1], model.Fn_Const(endPos[i].Y));
                model.AddEQ(x[i][col_T][2], model.Fn_Const(endVel[i].X));
                model.AddEQ(x[i][col_T][3], model.Fn_Const(endVel[i].Y));
            }

            model.SetObjective(model.Fn_Sum(Array.ConvertAll(u.SelectMany(p => p).ToArray(), p => model.Fn_L1Norm(p))), model.Fn_Const(0));
            for (int i = 0; i < col_cnt; i++)
            {
                for (int j = 0; j < col_T; j++)
                {
                    model.AddEQ(x[i][j + 1][0],
                        model.Fn_Dot(new double[] { col_A[0, 0], col_A[0, 1], col_A[0, 2], col_A[0, 3], col_B[0, 0], col_B[0, 1] },
                        x[i][j][0], x[i][j][1], x[i][j][2], x[i][j][3], u[i][j][0], u[i][j][1]));
                    model.AddEQ(x[i][j + 1][1],
                        model.Fn_Dot(new double[] { col_A[1, 0], col_A[1, 1], col_A[1, 2], col_A[1, 3], col_B[1, 0], col_B[1, 1] },
                        x[i][j][0], x[i][j][1], x[i][j][2], x[i][j][3], u[i][j][0], u[i][j][1]));
                    model.AddEQ(x[i][j + 1][2],
                        model.Fn_Dot(new double[] { col_A[2, 0], col_A[2, 1], col_A[2, 2], col_A[2, 3], col_B[2, 0], col_B[2, 1] },
                        x[i][j][0], x[i][j][1], x[i][j][2], x[i][j][3], u[i][j][0], u[i][j][1]));
                    model.AddEQ(x[i][j + 1][3],
                        model.Fn_Dot(new double[] { col_A[3, 0], col_A[3, 1], col_A[3, 2], col_A[3, 3], col_B[3, 0], col_B[3, 1] },
                        x[i][j][0], x[i][j][1], x[i][j][2], x[i][j][3], u[i][j][0], u[i][j][1]));
                }
            }

            if (avoidCollision)
            {
                for (int i = 0; i <= col_T; i++)
                {
                    for (int j = 0; j < col_cnt; j++)
                    {
                        for (int k = j + 1; k < col_cnt; k++)
                        {
                            dc_Func dfx = model.Fn_Diff(x[j][i][0], x[k][i][0]);
                            dc_Func dfy = model.Fn_Diff(x[j][i][1], x[k][i][1]);

                            model.AddGE(model.Fn_SumSquares(dfx, dfy), model.Fn_Const(col_dmin * col_dmin));
                        }
                    }
                }
            }

            model.param_tau = tau;
            model.param_tauM = tauM;
            model.param_mu = mu;
            model.param_ceil = ceil;
            model.param_ceilby = ceilby;

            model.Solve(avoidCollision ? attempts : 1, ExUtility.RandRange);
            model.CleanUp();



            Tuple<Point, double>[] pts = new Tuple<Point, double>[col_T + 1];
            for (int i = 0; i < col_cnt; i++)
            {
                for (int j = 0; j <= col_T; j++)
                {
                    double collide = -1;
                    for (int k = 0; k < col_cnt; k++)
                    {
                        if (k == i)
                            continue;
                        double dfx = x[i][j][0].lastValue - x[k][j][0].lastValue;
                        double dfy = x[i][j][1].lastValue - x[k][j][1].lastValue;

                        if (dfx * dfx + dfy * dfy < col_dmin * col_dmin - ExUtility.c_tolerance)
                            collide = Math.Sqrt(dfx * dfx + dfy * dfy);
                    }

                    pts[j] = new Tuple<Point, double>(new Point(x[i][j][0].lastValue, x[i][j][1].lastValue), collide);
                }

                for (int k = 0; k <= col_T; k++)
                    if (pts[k].Item2 >= 0)
                        mainCanvas.DrawPoints(Colors.Red.Fade(10), pts[k].Item2, pts[k].Item1);

                mainCanvas.DrawPath(Colors.Transparent, Colors.LightSteelBlue, 0, 1.5, pts.Select(p => p.Item1).ToArray());
                for (int k = 0; k <= col_T; k++)
                    mainCanvas.DrawPoints(pts[k].Item2 == -1 ? Colors.DarkSlateBlue.Fade((byte)(k / (double)col_T * 200 + 55)) : Colors.Red,
                        pts[k].Item2 == -1 ? 3 : 5, pts[k].Item1);

                Point s1 = new Point(x[i][0][0].lastValue, x[i][0][1].lastValue);
                Point e1 = new Point(s1.X + x[i][0][2].lastValue, s1.Y + x[i][0][3].lastValue);

                Point s2 = new Point(x[i][col_T][0].lastValue, x[i][col_T][1].lastValue);
                Point e2 = new Point(s2.X + x[i][col_T][2].lastValue, s2.Y + x[i][col_T][3].lastValue);

                mainCanvas.DrawEdges(Colors.ForestGreen, 2, new Tuple<Point, Point>(s1, e1));
                mainCanvas.DrawEdges(Colors.Orange, 2, new Tuple<Point, Point>(s2, e2));

                mainCanvas.DrawPoints(Colors.ForestGreen, 8, pts[0].Item1);
                mainCanvas.DrawPoints(Colors.Orange, 8, pts[col_T].Item1);
            }
        }

        private void btn_collideInit_Click(object sender, RoutedEventArgs e)
        {
            SetParameters();

            btn_collidePreStep.IsEnabled = true;
            btn_collideStep.IsEnabled = true;

            double width = mainCanvas.Width;

            mainCanvas.ClearCanvas();
            ExUtility.SetSeed(col_seed);

            startPos = new Point[col_cnt];
            endPos = new Point[col_cnt];
            startVel = new Point[col_cnt];
            endVel = new Point[col_cnt];

            List<Vector> disks = ExUtility.PoissonDiskSample(width * 0.8, col_dmin + 2, 30);
            disks.Shuffle();

            if (disks.Count < 2 * col_cnt)
                return;

            int cnt = 0;
            // x [obj] [time] [x,y,vx,vy]
            for (int i = 0; i < col_cnt; i++)
            {
                startPos[i] = new Point(width * 0.1 + disks[cnt].X, width * 0.1 + disks[cnt].Y); cnt++;
                endPos[i] = new Point(width * 0.1 + disks[cnt].X, width * 0.1 + disks[cnt].Y); cnt++;
                startVel[i] = new Point(ExUtility.RandRange(-col_velrng, col_velrng), ExUtility.RandRange(-col_velrng, col_velrng));
                endVel[i] = new Point(ExUtility.RandRange(-col_velrng, col_velrng), ExUtility.RandRange(-col_velrng, col_velrng));

                Point s1 = startPos[i];
                Point e1 = new Point(s1.X + startVel[i].X, s1.Y + startVel[i].Y);

                Point s2 = endPos[i];
                Point e2 = new Point(s2.X + endVel[i].X, s2.Y + endVel[i].Y);

                mainCanvas.DrawEdges(Colors.ForestGreen, 2, new Tuple<Point, Point>(s1, e1));
                mainCanvas.DrawEdges(Colors.Orange, 2, new Tuple<Point, Point>(s2, e2));

                mainCanvas.DrawPoints(Colors.ForestGreen, 8, startPos[i]);
                mainCanvas.DrawPoints(Colors.Orange, 8, endPos[i]);
            }
        }

        private void btn_collidePreStep_Click(object sender, RoutedEventArgs e)
        {
            Collision_Test(false);
        }

        private void btn_collideStep_Click(object sender, RoutedEventArgs e)
        {
            Collision_Test(true);
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
