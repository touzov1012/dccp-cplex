using System.Linq;
using ILOG.CPLEX;
using ILOG.Concert;

namespace DCCP
{
    public abstract class dc_Func
    {
        protected dc_Func[] m_inputs;

        protected double m_lastValue;
        protected double[] m_lastGradient;

        public double lastValue { get { return m_lastValue; } }

        public dc_Func[] inputs { get { return m_inputs; } }

        public dc_Func(params dc_Func[] inputs)
        {
            m_inputs = inputs;
            m_lastGradient = new double[m_inputs.Length];
        }

        public void Update()
        {
            if (m_inputs == null || m_inputs.Length == 0)
            {
                m_lastValue = LocalEvaluate(null);
                m_lastGradient = LocalGradient(null);
                return;
            }

            for (int i = 0; i < m_inputs.Length; i++)
                m_inputs[i].Update();

            double[] input = m_inputs.Select(p => p.lastValue).ToArray();

            m_lastValue = LocalEvaluate(input);
            m_lastGradient = LocalGradient(input);
        }

        protected virtual double LocalEvaluate(double[] input) { return 0; }

        protected virtual double[] LocalGradient(double[] input) { return null; }

        protected virtual INumExpr LocalExpression(Cplex model, INumExpr[] input) { return null; }

        public INumExpr AddExpression(Cplex model)
        {
            if (m_inputs == null || m_inputs.Length == 0)
                return LocalExpression(model, null);

            INumExpr[] input = new INumExpr[m_inputs.Length];
            for (int i = 0; i < m_inputs.Length; i++)
                input[i] = m_inputs[i].AddExpression(model);

            return LocalExpression(model, input);
        }

        public INumExpr Minorize(Cplex model, double delta)
        {
            if (m_inputs == null || m_inputs.Length == 0)
                return model.Prod(delta, model.Diff(AddExpression(model), m_lastValue));

            INumExpr[] exps = new INumExpr[m_inputs.Length];

            for (int i = 0; i < m_inputs.Length; i++)
                exps[i] = m_inputs[i].Minorize(model, delta * m_lastGradient[i]);

            return model.Sum(exps);
        }

        public INumExpr Minorize(Cplex model)
        {
            return model.Sum(m_lastValue, Minorize(model, 1));
        }
    }
}
