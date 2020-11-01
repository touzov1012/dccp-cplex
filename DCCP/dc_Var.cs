using ILOG.Concert;
using ILOG.CPLEX;
using System;

namespace DCCP
{
    public class dc_Var : dc_Func
    {
        public string name;

        public double lb;
        public double ub;
        public INumVar reference;

        public dc_Var(string name, double lb, double ub, Func<double, double, double> initializer)
        {
            this.name = name;
            this.lb = lb;
            this.ub = ub;
            RandomInit(initializer);

            m_lastGradient = new double[] { 1 };
        }

        public dc_Var(string name, double lb, double ub, double value)
        {
            this.name = name;
            this.lb = lb;
            this.ub = ub;
            SetValue(value);

            m_lastGradient = new double[] { 1 };
        }
        
        public void SetValue(double value)
        {
            m_lastValue = value;
        }

        public void RandomInit(Func<double, double, double> initializer)
        {
            SetValue(initializer(lb, ub));
        }

        protected override INumExpr LocalExpression(Cplex model, INumExpr[] input)
        {
            return reference;
        }

        protected override double LocalEvaluate(double[] input)
        {
            return m_lastValue;
        }

        protected override double[] LocalGradient(double[] input)
        {
            return new double[] { 1 };
        }
        
    }
}
