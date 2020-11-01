using DCCP;
using ILOG.Concert;
using ILOG.CPLEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DCCP_Examples
{
    /// <summary>
    /// Interaction logic for Window_Cut.xaml
    /// </summary>
    public partial class Window_Cut : Window
    {
        int node_cnt = 6;
        double edge_prob = 0.4;
        bool[,] graph;
        Point[] points;
        bool[] classes;

        int attempts = 10;
        double tau = 0.5;
        double tauM = 1E3;
        double mu = 1.2;
        double ceil = double.PositiveInfinity;
        int ceilby = 30;

        int seed = 45;

        public Window_Cut()
        {
            InitializeComponent();
            txt_nodeCnt.Text = node_cnt.ToString();
            txt_edgeProb.Text = edge_prob.ToString("F3");
            txt_attempts.Text = attempts.ToString();
            txt_tau.Text = tau.ToString("F3");
            txt_tauM.Text = tauM.ToString("E0");
            txt_mu.Text = mu.ToString("F3");
            txt_ceil.Text = ceil.ToString("F3");
            txt_ceilby.Text = ceilby.ToString();
            txt_seed.Text = seed.ToString();
        }

        private void DrawGraph(bool solved)
        {
            mainCanvas.ClearCanvas();

            for (int i = 0; i < node_cnt; i++)
            {
                for (int j = i + 1; j < node_cnt; j++)
                {
                    if (!graph[i, j])
                        continue;
                    if (!solved || classes[i] == classes[j])
                        mainCanvas.DrawEdges(Colors.LightSteelBlue, 2, new Tuple<Point, Point>(points[i], points[j]));
                    else
                        mainCanvas.DrawEdges(Colors.LightSalmon, 2, new Tuple<Point, Point>(points[i], points[j]));
                }
            }

            if (!solved)
                mainCanvas.DrawPoints(Colors.DarkSlateBlue, 6, points);
            else
            {
                for (int i = 0; i < classes.Length; i++)
                    if (classes[i])
                        mainCanvas.DrawPoints(Colors.ForestGreen, 6, points[i]);
                    else
                        mainCanvas.DrawPoints(Colors.Red, 6, points[i]);
            }
        }

        private void btn_cutInit_Click(object sender, RoutedEventArgs e)
        {
            int t_int;
            double t_double;

            node_cnt = int.TryParse(txt_nodeCnt.Text, out t_int) ? t_int : node_cnt;
            edge_prob = double.TryParse(txt_edgeProb.Text, out t_double) ? t_double : edge_prob;
            attempts = int.TryParse(txt_attempts.Text, out t_int) ? t_int : attempts;
            tau = double.TryParse(txt_tau.Text, out t_double) ? t_double : tau;
            tauM = double.TryParse(txt_tauM.Text, out t_double) ? t_double : tauM;
            mu = double.TryParse(txt_mu.Text, out t_double) ? t_double : mu;
            ceil = double.TryParse(txt_ceil.Text, out t_double) ? t_double : ceil;
            ceilby = int.TryParse(txt_ceilby.Text, out t_int) ? t_int : ceilby;
            seed = int.TryParse(txt_seed.Text, out t_int) ? t_int : seed;

            graph = new bool[node_cnt, node_cnt];
            points = new Point[node_cnt];
            classes = new bool[node_cnt];

            double width = mainCanvas.Width;

            mainCanvas.ClearCanvas();
            ExUtility.SetSeed(seed);

            double prox = 30;

            List<Vector> disks = new List<Vector>();

            while (disks.Count < node_cnt)
            {
                disks = ExUtility.PoissonDiskSample(width * 0.8, prox, 30);
                disks.Shuffle();
                prox *= 0.8;
            }

            btn_cutStep.IsEnabled = true;
            btn_cutPreStep.IsEnabled = true;

            for (int i = 0; i < node_cnt; i++)
            {
                for (int j = i + 1; j < node_cnt; j++)
                {
                    if (ExUtility.rand.NextDouble() > edge_prob)
                        continue;
                    graph[i, j] = true;
                    graph[j, i] = true;
                }
                points[i] = new Point(width * 0.1 + disks[i].X, width * 0.1 + disks[i].Y);
            }

            DrawGraph(false);
        }

        private void btn_cutPreStep_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.ClearCanvas();

            double[] La = new double[node_cnt * node_cnt];
            for (int i = 0; i < node_cnt; i++)
            {
                int deg = 0;
                for (int j = 0; j < node_cnt; j++)
                {
                    La[i * node_cnt + j] = graph[i, j] ? -1 : 0;
                    deg += graph[i, j] ? 1 : 0;
                }
                La[i * node_cnt + i] = deg;
            }

            dc_Model model = new dc_Model();

            dc_Var[] x = model.AddVarArray("X", -0.05, 1.05, node_cnt, ExUtility.RandRange);
            dc_Var[] eij = model.AddVarArray("E", -0.05, 1.05, node_cnt * node_cnt, ExUtility.RandRange);

            for (int i = 0; i < node_cnt; i++)
            {
                model.AddLE(model.Fn_Square(x[i]), x[i]);
                model.AddGE(model.Fn_Square(x[i]), x[i]);
            }

            for (int i = 0; i < node_cnt * node_cnt; i++)
            {
                model.AddLE(model.Fn_Square(eij[i]), eij[i]);
                model.AddGE(model.Fn_Square(eij[i]), eij[i]);
            }

            for (int i = 0; i < node_cnt; i++)
            {
                for (int j = i; j < node_cnt; j++)
                {
                    if (i != j)
                        model.AddEQ(eij[i * node_cnt + j], eij[j * node_cnt + i]);
                    model.AddLE(eij[i * node_cnt + j], x[i]);
                    model.AddLE(eij[i * node_cnt + j], x[j]);
                    model.AddLE(model.Fn_Sum(x[i], x[j]), model.Fn_Sum(eij[i * node_cnt + j], eij[i * node_cnt + j], model.Fn_Const(1)));
                }
            }
            model.SetObjective(model.Fn_Const(0), model.Fn_Dot(La, eij));

            model.param_tau = tau;
            model.param_tauM = tauM;
            model.param_mu = mu;
            model.param_ceil = ceil;
            model.param_ceilby = ceilby;
            model.param_timeout = 100;

            model.Solve(attempts, ExUtility.RandRange);
            model.CleanUp();

            for (int i = 0; i < node_cnt; i++)
                classes[i] = x[i].lastValue > 0.5;

            int val = 0;
            for (int i = 0; i < node_cnt; i++)
                for (int j = i + 1; j < node_cnt; j++)
                    val += classes[i] != classes[j] && graph[i, j] ? 1 : 0;

            dc_Utility.WriteLine(dc_Utility.c_stars);
            dc_Utility.WriteLine("Group A size: " + x.Count(p => p.lastValue > 0.5));
            dc_Utility.WriteLine("Group B size: " + x.Count(p => p.lastValue < 0.5));
            dc_Utility.WriteLine("Max Cut: " + val);
            dc_Utility.WriteLine(dc_Utility.c_stars);


            DrawGraph(true);
        }

        private void btn_cutStep_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.ClearCanvas();

            int[] La = new int[node_cnt * node_cnt];
            for (int i = 0; i < node_cnt; i++)
            {
                int deg = 0;
                for (int j = 0; j < node_cnt; j++)
                {
                    La[i * node_cnt + j] = graph[i, j] ? -1 : 0;
                    deg += graph[i, j] ? 1 : 0;
                }
                La[i * node_cnt + i] = deg;
            }

            Cplex model = new Cplex();

            INumVar[] x = model.BoolVarArray(node_cnt);
            INumVar[] eij = model.BoolVarArray(node_cnt * node_cnt);
            for (int i = 0; i < node_cnt; i++)
            {
                for (int j = i; j < node_cnt; j++)
                {
                    if (i != j)
                        model.AddEq(eij[i * node_cnt + j], eij[j * node_cnt + i]);
                    model.AddLe(eij[i * node_cnt + j], x[i]);
                    model.AddLe(eij[i * node_cnt + j], x[j]);
                    model.AddLe(model.Sum(x[i], x[j]), model.Sum(eij[i * node_cnt + j], eij[i * node_cnt + j], model.Constant(1)));
                }
            }
            model.AddMaximize(model.ScalProd(La, eij));

            if (model.Solve())
            {
                double[] vals = model.GetValues(x);
                int val = (int)Math.Round(model.GetObjValue());
                dc_Utility.WriteLine("");
                dc_Utility.WriteLine(dc_Utility.c_stars);
                dc_Utility.WriteLine("Group A size: " + vals.Count(p => p > 0.5));
                dc_Utility.WriteLine("Group B size: " + vals.Count(p => p < 0.5));
                dc_Utility.WriteLine("Max Cut: " + val);
                dc_Utility.WriteLine(dc_Utility.c_stars);
                dc_Utility.WriteLine("");

                for (int i = 0; i < node_cnt; i++)
                    classes[i] = vals[i] > 0.5;
                DrawGraph(true);
            }

            model.Dispose();
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
