using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DCCP
{
    public static class dc_Utility
    {
        public const double c_tolerance = 1E-6;
        public const string c_stars = "******************************";

        public static void WriteLine(string line)
        {
            if (dc_ConsoleManager.HasConsole)
                Console.WriteLine(line);
            else
                Debug.WriteLine(line);
        }

        public static void WriteLineIf(bool condition, string line)
        {
            if (condition)
                WriteLine(line);
        }

        public static bool Approximately(this double a, double b, double tol = c_tolerance)
        {
            return Math.Abs(a - b) < tol;
        }
                                                
        public static void MultiplyBy(this double[] a, double b)
        {
            for (int i = 0; i < a.Length; i++)
                a[i] *= b;
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

        public static double[] Duplicate(this double[] a)
        {
            double[] d = new double[a.Length];
            a.CopyTo(d, 0);
            return d;
        }

        public static double Dot(this double[] a, double[] b)
        {
            double res = 0;
            for (int i = 0; i < a.Length; i++)
                res += a[i] * b[i];
            return res;
        }

        public static double Norm2Square(this double[] x)
        {
            return x.Dot(x);
        }

        public static double Norm1(this double[] x)
        {
            double res = 0;
            for (int i = 0; i < x.Length; i++)
                res += Math.Abs(x[i]);
            return res;
        }

        public static double Norm2(this double[] x)
        {
            return Math.Sqrt(x.Norm2Square());
        }

    }
}
