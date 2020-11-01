using ILOG.CPLEX;
using System;
using System.Linq;
using ILOG.Concert;

namespace DCCP
{
    public static class dc_Ops
    {

        public class Dot : dc_Func
        {
            private double[] _scale;

            public Dot(double[] scale, params dc_Func[] inputs) : base(inputs) { _scale = scale.Duplicate(); }

            protected override double LocalEvaluate(double[] input) { return _scale.Dot(input); }

            protected override double[] LocalGradient(double[] input) { return _scale; }

            protected override INumExpr LocalExpression(Cplex model, INumExpr[] input) { return model.ScalProd(_scale, Array.ConvertAll(input, p => (INumVar)p)); }
            
        }

        public class Scale : dc_Func
        {
            private double _scale;

            public Scale(double scale, dc_Func input) : base(input) { _scale = scale; }

            protected override double LocalEvaluate(double[] input) { return _scale * input[0]; }

            protected override double[] LocalGradient(double[] input) { return new double[] { _scale }; }

            protected override INumExpr LocalExpression(Cplex model, INumExpr[] input) { return model.Prod(_scale, input[0]); }
        }

        public class Affine : dc_Func
        {
            private double[] _scale;
            private double _shift;

            public Affine(double[] scale, double shift, params dc_Func[] inputs) : base(inputs) { _shift = shift; _scale = scale.Duplicate(); }
            
            protected override double LocalEvaluate(double[] input) { return _scale.Dot(input) + _shift; }

            protected override double[] LocalGradient(double[] input) { return _scale; }

            protected override INumExpr LocalExpression(Cplex model, INumExpr[] input) { return model.Sum(model.ScalProd(_scale, Array.ConvertAll(input, p => (INumVar)p)), _shift); }
            
        }

        public class Affine2 : dc_Func
        {
            private double _scale;
            private double _shift;

            public Affine2(double scale, double shift, dc_Func input) : base(input) { _shift = shift; _scale = scale; }
            
            protected override double LocalEvaluate(double[] input) { return _scale * input[0] + _shift; }

            protected override double[] LocalGradient(double[] input) { return new double[] { _scale }; }

            protected override INumExpr LocalExpression(Cplex model, INumExpr[] input) { return model.Sum(model.Prod(_scale, input[0]), _shift); }
            
        }

        public class Sum : dc_Func
        {
            private double[] _scale;

            public Sum(params dc_Func[] inputs) : base(inputs) { _scale = dc_Utility.Ones(inputs.Length); }
            
            protected override double LocalEvaluate(double[] input) { return input.Sum(); }

            protected override double[] LocalGradient(double[] input) { return _scale; }

            protected override INumExpr LocalExpression(Cplex model, INumExpr[] input) { return model.Sum(input); }
            
        }

        public class Diff : dc_Func
        {
            public Diff(dc_Func input1, dc_Func input2) : base(input1, input2) { }
            
            protected override double LocalEvaluate(double[] input) { return input[0] - input[1]; }

            protected override double[] LocalGradient(double[] input) { return new double[] { 1, -1 }; }

            protected override INumExpr LocalExpression(Cplex model, INumExpr[] input) { return model.Diff(input[0], input[1]); }
            
        }

        public class Const : dc_Func
        {
            private double _value;

            public Const(double value) { _value = value; }
            
            protected override double LocalEvaluate(double[] input) { return _value; }

            protected override double[] LocalGradient(double[] input) { return new double[] { 1 }; }

            protected override INumExpr LocalExpression(Cplex model, INumExpr[] input) { return model.Constant(_value); }
            
        }

        public class L2NormSquared : dc_Func
        {
            public L2NormSquared(params dc_Var[] inputs) : base(inputs) { }
            
            protected override double LocalEvaluate(double[] input) { return input.Norm2Square(); }

            protected override double[] LocalGradient(double[] input) { var y = input.Duplicate(); y.MultiplyBy(2.0); return y; }

            protected override INumExpr LocalExpression(Cplex model, INumExpr[] input) { var refs = Array.ConvertAll(input, p => (INumVar)p); return model.ScalProd(refs, refs); }
            
        }

        public class SumSquares : dc_Func
        {
            public SumSquares(params dc_Func[] inputs) : base(inputs) { }
            
            protected override double LocalEvaluate(double[] input) { return input.Norm2Square(); }

            protected override double[] LocalGradient(double[] input) { var y = input.Duplicate(); y.MultiplyBy(2.0); return y; }

            protected override INumExpr LocalExpression(Cplex model, INumExpr[] input) { var refs = Array.ConvertAll(input, p => model.Square(p)); return model.Sum(refs); }
            
        }

        public class L1Norm : dc_Func
        {
            public L1Norm(params dc_Func[] inputs) : base(inputs) { }
            
            protected override double LocalEvaluate(double[] input) { return input.Norm1(); }

            protected override double[] LocalGradient(double[] input) { double[] y = new double[input.Length]; for (int i = 0; i < input.Length; i++) y[i] = Math.Sign(input[i]); return y; }

            protected override INumExpr LocalExpression(Cplex model, INumExpr[] input) { var refs = Array.ConvertAll(input, p => model.Abs(p)); return model.Sum(refs); }
            
        }

        public class Square : dc_Func
        {
            public Square(dc_Func input) : base(input) { }
            
            protected override double LocalEvaluate(double[] input) { return input[0] * input[0]; }

            protected override double[] LocalGradient(double[] input) { return new double[] { 2 * input[0] }; }

            protected override INumExpr LocalExpression(Cplex model, INumExpr[] input) { return model.Square(input[0]); }
            
        }

        public class Abs : dc_Func
        {
            public Abs(dc_Func input) : base(input) { }
            
            protected override double LocalEvaluate(double[] input) { return Math.Abs(input[0]); }

            protected override double[] LocalGradient(double[] input) { return new double[] { Math.Sign(input[0]) }; }

            protected override INumExpr LocalExpression(Cplex model, INumExpr[] input) { return model.Abs(input[0]); }
            
        }
    }
}
