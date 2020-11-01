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
    /// Interaction logic for Window_Polys.xaml
    /// </summary>
    public partial class Window_Polys : Window
    {
        class Triangle
        {
            public Point[] points;
            public Triangle(Point A, Point B, Point C)
            {
                points = new Point[3];
                points[0] = A;
                points[1] = B;
                points[2] = C;
            }
        }

        class Mesh
        {
            public Triangle[] tris;
            public Mesh(params Triangle[] tris)
            {
                this.tris = tris;
            }
        }

        int poly_cnt = 2;
        int poly_tri_cnt = 4;
        Mesh[] polys;

        int attempts = 10;
        double tau = 0.005;
        double tauM = 1E3;
        double mu = 1.2;
        double ceil = 0;
        int ceilby = 100;

        int seed = 34;

        public Window_Polys()
        {
            InitializeComponent();
            txt_polyCnt.Text = poly_cnt.ToString();
            txt_polyTriCnt.Text = poly_tri_cnt.ToString();
            txt_attempts.Text = attempts.ToString();
            txt_tau.Text = tau.ToString("F3");
            txt_tauM.Text = tauM.ToString("E0");
            txt_mu.Text = mu.ToString("F3");
            txt_ceil.Text = ceil.ToString("F3");
            txt_ceilby.Text = ceilby.ToString();
            txt_seed.Text = seed.ToString();
        }

        private void btn_polyInit_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.ClearCanvas();

            int t_int;
            double t_double;

            poly_cnt = int.TryParse(txt_polyCnt.Text, out t_int) ? t_int : poly_cnt;
            poly_tri_cnt = int.TryParse(txt_polyTriCnt.Text, out t_int) ? t_int : poly_tri_cnt;
            attempts = int.TryParse(txt_attempts.Text, out t_int) ? t_int : attempts;
            tau = double.TryParse(txt_tau.Text, out t_double) ? t_double : tau;
            tauM = double.TryParse(txt_tauM.Text, out t_double) ? t_double : tauM;
            mu = double.TryParse(txt_mu.Text, out t_double) ? t_double : mu;
            ceil = double.TryParse(txt_ceil.Text, out t_double) ? t_double : ceil;
            ceilby = int.TryParse(txt_ceilby.Text, out t_int) ? t_int : ceilby;
            seed = int.TryParse(txt_seed.Text, out t_int) ? t_int : seed;

            btn_polyStep.IsEnabled = true;

            double width = mainCanvas.Width;

            polys = new Mesh[poly_cnt];

            ExUtility.SetSeed(seed);
            for (int i = 0; i < poly_cnt; i++)
            {
                Triangle[] ts = new Triangle[poly_tri_cnt];
                ts[0] = new Triangle(new Point(ExUtility.RandRange(0, width), ExUtility.RandRange(0, width)),
                    new Point(ExUtility.RandRange(0, width), ExUtility.RandRange(0, width)),
                    new Point(ExUtility.RandRange(0, width), ExUtility.RandRange(0, width)));
                for (int j = 1; j < ts.Length; j++)
                {
                    ts[j] = new Triangle(new Point(ts[j - 1].points[1].X, ts[j - 1].points[1].Y),
                        new Point(ts[j - 1].points[2].X, ts[j - 1].points[2].Y),
                        new Point(ExUtility.RandRange(0, width), ExUtility.RandRange(0, width)));
                }

                polys[i] = new Mesh(ts);
            }

            ExUtility.SetSeed(245);
            for (int i = 0; i < polys.Length; i++)
            {
                int[] cs = ExUtility.RandomInts(0, 255, 3);
                DrawMeshes(mainCanvas, Color.FromRgb((byte)cs[0], (byte)cs[1], (byte)cs[2]), Color.FromRgb((byte)cs[0], (byte)cs[1], (byte)cs[2]), 1, polys[i]);
            }
        }

        private void btn_polyStep_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.ClearCanvas();

            double width = mainCanvas.Width;
            int pairs = 0;
            for (int i = 0; i < poly_cnt; i++)
            {
                for (int j = i + 1; j < poly_cnt; j++)
                {
                    pairs += polys[i].tris.Length * polys[j].tris.Length;
                }
            }

            dc_Model model = new dc_Model();
            dc_Var z = model.AddVar("Z", 0, 200, ExUtility.RandRange);
            dc_Var[][] r = new dc_Var[poly_cnt][]; for (int i = 0; i < poly_cnt; i++) r[i] = model.AddVarArray("RotS_" + i, -10, 10, 2, ExUtility.RandRange);
            dc_Var[][] t = new dc_Var[poly_cnt][]; for (int i = 0; i < poly_cnt; i++) t[i] = model.AddVarArray("Trans_" + i, -width, width, 2, ExUtility.RandRange);
            dc_Var[][][] px = new dc_Var[poly_cnt][][]; for (int i = 0; i < poly_cnt; i++) { px[i] = new dc_Var[polys[i].tris.Length][]; for (int j = 0; j < polys[i].tris.Length; j++) px[i][j] = model.AddVarArray("Poly_" + i + "_Tri_" + j + "_Ptx", 0, width, 3, ExUtility.RandRange); }
            dc_Var[][][] py = new dc_Var[poly_cnt][][]; for (int i = 0; i < poly_cnt; i++) { py[i] = new dc_Var[polys[i].tris.Length][]; for (int j = 0; j < polys[i].tris.Length; j++) py[i][j] = model.AddVarArray("Poly_" + i + "_Tri_" + j + "_Pty", 0, width, 3, ExUtility.RandRange); }
            dc_Var[][] w = new dc_Var[pairs][]; for (int i = 0; i < pairs; i++) w[i] = model.AddVarArray("W_" + i, -1, 1, 2, ExUtility.RandRange);
            dc_Var[] b = new dc_Var[pairs]; for (int i = 0; i < pairs; i++) b[i] = model.AddVar("b_" + i, -width * Math.Sqrt(2), width * Math.Sqrt(2), ExUtility.RandRange);

            dc_Func[] bp = new dc_Func[pairs];
            dc_Func[] bm = new dc_Func[pairs];

            model.SetObjective(model.Fn_Const(0), z);
            for (int i = 0; i < poly_cnt; i++)
            {
                model.AddLE(z, model.Fn_L2NormSquared(r[i]));

                for (int k = 0; k < polys[i].tris.Length; k++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        model.AddEQ(px[i][k][j], model.Fn_Dot(new double[] { polys[i].tris[k].points[j].X, -polys[i].tris[k].points[j].Y, 1 }, r[i][0], r[i][1], t[i][0]));
                        model.AddEQ(py[i][k][j], model.Fn_Dot(new double[] { polys[i].tris[k].points[j].X, polys[i].tris[k].points[j].Y, 1 }, r[i][1], r[i][0], t[i][1]));
                    }
                }
            }

            for (int i = 0; i < pairs; i++)
            {
                bp[i] = model.Fn_Affine(-2, -2, b[i]);
                bm[i] = model.Fn_Affine(2, -2, b[i]);
            }

            int pcnt = 0;
            for (int j = 0; j < poly_cnt; j++)
            {
                for (int k = j + 1; k < poly_cnt; k++)
                {
                    for (int m = 0; m < polys[j].tris.Length; m++)
                    {
                        for (int n = 0; n < polys[k].tris.Length; n++)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                dc_Func[] pPw = new dc_Func[2];
                                dc_Func[] qPw = new dc_Func[2];

                                pPw[0] = model.Fn_Sum(px[j][m][i], w[pcnt][0]);
                                pPw[1] = model.Fn_Sum(py[j][m][i], w[pcnt][1]);
                                qPw[0] = model.Fn_Sum(px[k][n][i], w[pcnt][0]);
                                qPw[1] = model.Fn_Sum(py[k][n][i], w[pcnt][1]);

                                model.AddLE(model.Fn_SumSquares(pPw), model.Fn_Sum(bm[pcnt], model.Fn_L2NormSquared(w[pcnt][0], w[pcnt][1], px[j][m][i], py[j][m][i])));
                                model.AddLE(model.Fn_L2NormSquared(w[pcnt][0], w[pcnt][1], px[k][n][i], py[k][n][i]), model.Fn_Sum(bp[pcnt], model.Fn_SumSquares(qPw[0], qPw[1])));

                            }

                            pcnt++;
                        }
                    }
                }
            }

            model.param_tau = tau;
            model.param_tauM = tauM;
            model.param_mu = mu;
            model.param_ceil = ceil;
            model.param_ceilby = ceilby;

            model.Solve(attempts, ExUtility.RandRange);

            dc_Utility.WriteLine("");
            dc_Utility.WriteLine(dc_Utility.c_stars);
            double scale = double.PositiveInfinity;
            for (int i = 0; i < poly_cnt; i++)
            {
                double news = Math.Sqrt(r[i][0].lastValue * r[i][0].lastValue + r[i][1].lastValue * r[i][1].lastValue);
                dc_Utility.WriteLine("Poly. " + i + " growth: " + news);
                scale = Math.Min(scale, news);
            }
            dc_Utility.WriteLine(dc_Utility.c_stars);

            double bound = 0;
            for (int i = 0; i < poly_cnt; i++)
            {
                for (int k = 0; k < polys[i].tris.Length; k++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        polys[i].tris[k].points[j].X = px[i][k][j].lastValue / scale;
                        polys[i].tris[k].points[j].Y = py[i][k][j].lastValue / scale;

                        bound = Math.Max(polys[i].tris[k].points[j].Y, Math.Max(polys[i].tris[k].points[j].X, bound));
                    }
                }
            }
            mainCanvas.DrawBounds(bound);

            //for(int i = 0; i < pairs; i++)
            //{
            //    DrawPlane(w[i][0].value, w[i][1].value, b[i].value);
            //}

            ExUtility.SetSeed(245);
            for (int i = 0; i < polys.Length; i++)
            {
                int[] cs = ExUtility.RandomInts(0, 255, 3);
                DrawMeshes(mainCanvas, Color.FromRgb((byte)cs[0], (byte)cs[1], (byte)cs[2]), Color.FromRgb((byte)cs[0], (byte)cs[1], (byte)cs[2]), 1, polys[i]);
            }
            model.CleanUp();
            //Utility.ResetSeed();
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow w = new MainWindow();
            w.Left = this.Left;
            w.Top = this.Top;
            w.Show();
            this.Close();
        }

        void DrawMeshes(Canvas canvas, Color color, Color stroke, double strokeThickness, params Mesh[] meshes)
        {
            for (int i = 0; i < meshes.Length; i++)
                DrawTriangles(canvas, color, stroke, strokeThickness, meshes[i].tris);
        }

        void DrawTriangles(Canvas canvas, Color color, Color stroke, double strokeThickness, params Triangle[] tris)
        {
            for (int i = 0; i < tris.Length; i++)
                canvas.DrawPolygon(color, stroke, strokeThickness, tris[i].points);
        }
    }
}
