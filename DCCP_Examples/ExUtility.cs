using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DCCP_Examples
{
    static class ExUtility
    {
        public const double c_tolerance = 1E-6;

        public static Random rand = new Random();

        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void ResetSeed()
        {
            rand = new Random();
        }

        public static void SetSeed(int seed)
        {
            rand = new Random(seed);
        }

        public static double RandRange(double min, double max)
        {
            return rand.NextDouble() * (max - min) + min;
        }

        public static double[] Ones(int dim)
        {
            return Constant(dim, 1);
        }

        public static double[] Constant(int dim, double constant)
        {
            double[] d = new double[dim];
            for (int i = 0; i < d.Length; i++) d[i] = constant;
            return d;
        }

        public static void SubtractBy(this double[] a, double[] b)
        {
            for (int i = 0; i < a.Length; i++)
                a[i] -= b[i];
        }

        public static List<Vector> PoissonDiskSample(double max, double r, double k)
        {
            int cellCnt = (int)Math.Ceiling(max / r);

            int[,] grid = new int[cellCnt, cellCnt];

            Queue<int> active = new Queue<int>();
            List<Vector> points = new List<Vector>();

            points.Add(new Vector(RandRange(0, max - c_tolerance), RandRange(0, max - c_tolerance)));
            grid[(int)Math.Floor(points[0].X / r), (int)Math.Floor(points[0].Y / r)] = 1;
            active.Enqueue(0);

            while (active.Count > 0)
            {
                int pind = active.Peek();
                bool found = false;

                for (int i = 0; i < k; i++)
                {
                    Vector sample = points[pind] + RandomOnUnitCircle() * (r + r * rand.NextDouble());

                    int sX = (int)Math.Floor(sample.X / r);
                    int sY = (int)Math.Floor(sample.Y / r);

                    if (sX < 0 || sX >= cellCnt || sY < 0 || sY >= cellCnt) continue;
                    if (grid[sX, sY] > 0) continue;
                    if (sX + 1 < cellCnt && grid[sX + 1, sY] > 0 && (points[grid[sX + 1, sY] - 1] - sample).Length < r) continue;
                    if (sX - 1 >= 0 && grid[sX - 1, sY] > 0 && (points[grid[sX - 1, sY] - 1] - sample).Length < r) continue;
                    if (sY + 1 < cellCnt && grid[sX, sY + 1] > 0 && (points[grid[sX, sY + 1] - 1] - sample).Length < r) continue;
                    if (sY - 1 >= 0 && grid[sX, sY - 1] > 0 && (points[grid[sX, sY - 1] - 1] - sample).Length < r) continue;
                    if (sY - 1 >= 0 && sX - 1 >= 0 && grid[sX - 1, sY - 1] > 0 && (points[grid[sX - 1, sY - 1] - 1] - sample).Length < r) continue;
                    if (sY + 1 < cellCnt && sX - 1 >= 0 && grid[sX - 1, sY + 1] > 0 && (points[grid[sX - 1, sY + 1] - 1] - sample).Length < r) continue;
                    if (sY - 1 >= 0 && sX + 1 < cellCnt && grid[sX + 1, sY - 1] > 0 && (points[grid[sX + 1, sY - 1] - 1] - sample).Length < r) continue;
                    if (sY + 1 < cellCnt && sX + 1 < cellCnt && grid[sX + 1, sY + 1] > 0 && (points[grid[sX + 1, sY + 1] - 1] - sample).Length < r) continue;

                    found = true;

                    points.Add(sample);
                    grid[sX, sY] = points.Count;
                    active.Enqueue(points.Count - 1);
                }

                if (!found)
                    active.Dequeue();
            }

            grid = null;

            return points;
        }

        public static Vector RandomOnUnitCircle()
        {
            double t = RandRange(0, Math.PI * 2);
            return new Vector(Math.Cos(t), Math.Sin(t));
        }

        public static int[] RandomInts(int lb, int ub, int count)
        {
            int[] ints = new int[count];
            for (int i = 0; i < count; i++)
                ints[i] = rand.Next(lb, ub);

            return ints;
        }

        public static double[] RandomDoubles(double lb, double ub, int count)
        {
            double[] dubs = new double[count];
            for (int i = 0; i < count; i++)
                dubs[i] = RandRange(lb, ub);

            return dubs;
        }

        public static double Arg(double x, double y)
        {
            if (x > 0)
                return Math.Atan2(y, x);
            else if (x < 0 && y >= 0)
                return Math.Atan2(y, x) + Math.PI;
            else if (x < 0 && y < 0)
                return Math.Atan2(y, x) - Math.PI;
            else if (x == 0 && y > 0)
                return Math.PI / 2;
            else if (x == 0 && y < 0)
                return -Math.PI / 2;
            else
                return double.NaN;
        }

        public static double NormComplex(double x, double y)
        {
            return x * x + y * y;
        }

        // **************
        // WPF
        // **************

        public static Color Fade(this Color c, byte opacity)
        {
            return Color.FromArgb(opacity, c.R, c.G, c.B);
        }

        public static void ClearCanvas(this Canvas canvas)
        {
            canvas.Children.Clear();
        }

        public static void DrawPoints(this Canvas canvas, Color color, double ptWidth, params Point[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Ellipse e = new Ellipse();
                e.Width = ptWidth;
                e.Height = ptWidth;
                e.Fill = new SolidColorBrush(color);
                Canvas.SetTop(e, points[i].Y - ptWidth / 2);
                Canvas.SetLeft(e, points[i].X - ptWidth / 2);

                canvas.Children.Add(e);
            }
        }

        public static void DrawEdges(this Canvas canvas, Color color, double edgeWidth, params Tuple<Point, Point>[] segs)
        {
            for (int i = 0; i < segs.Length; i++)
            {
                Line l = new Line();
                l.Stroke = new SolidColorBrush(color);
                l.Y1 = segs[i].Item1.Y;
                l.Y2 = segs[i].Item2.Y;
                l.X1 = segs[i].Item1.X;
                l.X2 = segs[i].Item2.X;
                l.StrokeThickness = edgeWidth;
                canvas.Children.Add(l);
            }
        }

        public static void DrawPlane(this Canvas canvas, Vector W, double B, bool is01 = true)
        {
            double scale = is01 ? canvas.Width : 1;
            if (W.Y == 0)
                canvas.DrawPath(Brushes.LightSteelBlue.Color, Brushes.LightSteelBlue.Color, 2, 2, new Point(B / W.X * scale, 0), new Point(B / W.X * scale, canvas.Height));
            else if (W.X == 0)
                canvas.DrawPath(Brushes.LightSteelBlue.Color, Brushes.LightSteelBlue.Color, 2, 2, new Point(0, B / W.Y * scale), new Point(canvas.Width, B / W.Y * scale));
            else
            {
                Point p1 = new Point(0, B / W.Y * scale);
                Point p2 = new Point(B / W.X * scale, 0);

                canvas.DrawPath(Brushes.LightSteelBlue.Color, Brushes.LightSteelBlue.Color, 2, 2, p1, p2);
            }
        }

        public static void DrawPath(this Canvas canvas, Color ptColor, Color edgeColor, double ptWidth, double edgeWidth, params Point[] points)
        {
            for (int i = 1; i < points.Length; i++)
                canvas.DrawEdges(edgeColor, edgeWidth, new Tuple<Point, Point>(new Point(points[i - 1].X, points[i - 1].Y), new Point(points[i].X, points[i].Y)));
            canvas.DrawPoints(ptColor, ptWidth, points);
        }

        public static void DrawBounds(this Canvas canvas, double width)
        {
            Line lu = new Line();
            lu.Stroke = Brushes.LightSteelBlue;
            lu.Y1 = 0;
            lu.Y2 = 0;
            lu.X1 = 0;
            lu.X2 = width;
            lu.StrokeThickness = 1;
            canvas.Children.Add(lu);

            Line ll = new Line();
            ll.Stroke = Brushes.LightSteelBlue;
            ll.Y1 = 0;
            ll.Y2 = width;
            ll.X1 = 0;
            ll.X2 = 0;
            ll.StrokeThickness = 1;
            canvas.Children.Add(ll);

            Line lr = new Line();
            lr.Stroke = Brushes.LightSteelBlue;
            lr.Y1 = 0;
            lr.Y2 = width;
            lr.X1 = width;
            lr.X2 = width;
            lr.StrokeThickness = 1;
            canvas.Children.Add(lr);

            Line ld = new Line();
            ld.Stroke = Brushes.LightSteelBlue;
            ld.Y1 = width;
            ld.Y2 = width;
            ld.X1 = 0;
            ld.X2 = width;
            ld.StrokeThickness = 1;
            canvas.Children.Add(ld);
        }

        public static void DrawCircle(this Canvas canvas, Color color, Color stroke, double strokeThickness, Point pts, double rads)
        {
            Ellipse e = new Ellipse();
            e.Width = rads * 2;
            e.Height = rads * 2;
            e.Fill = new SolidColorBrush(color);
            e.Stroke = new SolidColorBrush(stroke);
            e.StrokeThickness = strokeThickness;
            Canvas.SetLeft(e, pts.X - rads);
            Canvas.SetTop(e, pts.Y - rads);
            canvas.Children.Add(e);
        }

        public static void DrawPolygon(this Canvas canvas, Color color, Color stroke, double strokeThickness, params Point[] points)
        {
            PointCollection pc = new PointCollection(points);

            Polygon p = new Polygon();
            p.Points = pc;
            p.Fill = new SolidColorBrush(color);
            p.Stroke = new SolidColorBrush(stroke);
            p.StrokeThickness = strokeThickness;
            p.StrokeLineJoin = PenLineJoin.Round;

            canvas.Children.Add(p);
        }
    }
}
