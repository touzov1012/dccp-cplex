using ILOG.CPLEX;
using ILOG.Concert;
using System;
using System.Collections.Generic;

namespace DCCP
{
    public class dc_Model
    {
        private dc_FGPair _objective;
        private List<dc_FGPair> _inequalities;
        private List<dc_FGPair> _equalities;
        private List<dc_Var> _variables;

        private Cplex model;

        public int param_timeout = 400;
        public int param_leveloff_steps = 20;
        public double param_ceil = double.PositiveInfinity;
        public int param_ceilby = 15;
        public double param_tau = 1;
        public double param_tauM = 3000;
        public double param_mu = 1.1;

        private double _term_lastValue;
        private int _term_sameCnt;

        public dc_Model()
        {
            _inequalities = new List<dc_FGPair>();
            _equalities = new List<dc_FGPair>();
            _variables = new List<dc_Var>();

            model = new Cplex();
        }

        public void CleanUp()
        {
            model.ClearModel();
            model.Dispose();
        }

        public void SetObjective(dc_Func f, dc_Func g)
        {
            _objective = new dc_FGPair(f, g);
        }

        public void AddLE(dc_Func f, dc_Func g)
        {
            _inequalities.Add(new dc_FGPair(f, g));
        }

        public void AddGE(dc_Func f, dc_Func g)
        {
            AddLE(g, f);
        }

        public void AddEQ(dc_Func f, dc_Func g)
        {
            _equalities.Add(new dc_FGPair(f, g));
        }

        public dc_Var AddVar(string name, double lb, double ub, Func<double, double, double> initializer)
        {
            _variables.Add(new dc_Var(name, lb, ub, initializer));
            return _variables[_variables.Count - 1];
        }

        public dc_Var AddVar(string name, double lb, double ub, double value)
        {
            _variables.Add(new dc_Var(name, lb, ub, value));
            return _variables[_variables.Count - 1];
        }

        public dc_Var[] AddVarArray(string name, double lb, double ub, int size, Func<double, double, double> initializer)
        {
            dc_Var[] vars = new dc_Var[size];
            for(int i = 0; i < size; i++)
            {
                vars[i] = new dc_Var(name + "_" + i, lb, ub, initializer);
                _variables.Add(vars[i]);
            }
            return vars;
        }

        public dc_Var[] AddVarArray(string name, double[] lb, double[] ub, Func<double, double, double> initializer)
        {
            dc_Var[] vars = new dc_Var[lb.Length];
            for (int i = 0; i < lb.Length; i++)
            {
                vars[i] = new dc_Var(name + "_" + i, lb[i], ub[i], initializer);
                _variables.Add(vars[i]);
            }
            return vars;
        }

        public dc_Func Fn_Dot(double[] scale, params dc_Func[] inputs) { return new dc_Ops.Dot(scale, inputs); }

        public dc_Func Fn_Scale(double scale, dc_Func input) { return new dc_Ops.Scale(scale, input); }

        public dc_Func Fn_Negative(dc_Func input) { return new dc_Ops.Scale(-1, input); }

        public dc_Func Fn_Affine(double[] scale, double shift, params dc_Func[] inputs) { return new dc_Ops.Affine(scale, shift, inputs); }

        public dc_Func Fn_Affine(double scale, double shift, dc_Func input) { return new dc_Ops.Affine2(scale, shift, input); }

        public dc_Func Fn_Sum(params dc_Func[] inputs) { return new dc_Ops.Sum(inputs); }

        public dc_Func Fn_Diff(dc_Func input1, dc_Func input2) { return new dc_Ops.Diff(input1, input2); }

        public dc_Func Fn_Const(double constant) { return new dc_Ops.Const(constant); }

        public dc_Func Fn_L2NormSquared(params dc_Var[] inputs) { return new dc_Ops.L2NormSquared(inputs); }

        public dc_Func Fn_SumSquares(params dc_Func[] inputs) { return new dc_Ops.SumSquares(inputs); }

        public dc_Func Fn_L1Norm(params dc_Func[] inputs) { return new dc_Ops.L1Norm(inputs); }

        public dc_Func Fn_Square(dc_Func input) { return new dc_Ops.Square(input); }

        public dc_Func Fn_Abs(dc_Func input) { return new dc_Ops.Abs(input); }

        public bool IsFeasible()
        {
            bool feasible = true;
            for (int i = 0; i < _equalities.Count; i++)
            {
                if (!_equalities[i].f.lastValue.Approximately(_equalities[i].g.lastValue, 1E-03))
                    feasible = false;
            }

            for (int i = 0; i < _inequalities.Count; i++)
            {
                if (_inequalities[i].f.lastValue > _inequalities[i].g.lastValue + 1E-03)
                    feasible = false;
            }

            return feasible;
        }

        public void Initialize(Func<double, double, double> initializer)
        {
            for (int j = 0; j < _variables.Count; j++)
                _variables[j].RandomInit(initializer);
        }

        public double Solve(int attempts, Func<double, double, double> initializer, bool suppress = false)
        {
            double[] best = new double[_variables.Count];
            double bestValue = double.PositiveInfinity;

            for (int i = 0; i < attempts; i++)
            {
                Initialize(initializer);

                double newValue = Solve(true);

                dc_Utility.WriteLineIf(!suppress, string.Format("Attempt: {0} / {1} | Value: {2}", i + 1, attempts, newValue));

                if (newValue < bestValue)
                {
                    for (int j = 0; j < _variables.Count; j++)
                        best[j] = _variables[j].lastValue;
                    bestValue = newValue;
                }
            }

            if (attempts < 1)
                return bestValue;

            for (int j = 0; j < _variables.Count; j++)
                _variables[j].SetValue(best[j]);

            if (!suppress)
                DebugSolution(bestValue);

            return bestValue;
        }

        public double Solve(bool suppress = false)
        {
            double value = double.PositiveInfinity;
            double tau = param_tau;

            int cnt = 0;
            while(!Terminate(cnt++, value))
            {
                double newval;
                if (SolveAtCurrentPoint(tau, suppress, out newval))
                    value = newval;

                tau = Math.Min(param_mu * tau, param_tauM);
            }

            if (!suppress)
                DebugSolution(value);

            return value;
        }

        private bool SolveAtCurrentPoint(double tau, bool suppress, out double optVal)
        {
            // values should be set in each variable by this point, for starting
            model.ClearModel();

            // update values and gradients for all functions based on last set var values
            UpdateFuncGraph();

            // add variables to model
            for (int j = 0; j < _variables.Count; j++)
                AddCplexVar(model, _variables[j]);

            // create slacks for each inequality constraint
            INumVar[] si = model.NumVarArray(_inequalities.Count, 0, double.MaxValue, NumVarType.Float);

            // set up objective
            INumExpr f0 = _objective.f.AddExpression(model);
            INumExpr g0 = _objective.g.Minorize(model);
            INumExpr slc = model.Prod(tau, model.Sum(si));

            model.AddMinimize(model.Sum(model.Diff(f0, g0), slc));

            // add equality/inequality constraints
            for (int i = 0; i < _equalities.Count; i++)
                AddCplexEqual(model, _equalities[i]);

            for (int i = 0; i < _inequalities.Count; i++)
                AddCplexLEqual(model, _inequalities[i], si[i]);

            // solve the simplified model
            optVal = double.PositiveInfinity;
            bool solved = true;
            model.SetOut(null);
            if (model.Solve())
            {
                optVal = model.GetObjValue();
                dc_Utility.WriteLineIf(!suppress, "");
                dc_Utility.WriteLineIf(!suppress, "Solved on Simple Model! Value: " + optVal);

                // set new values
                for (int i = 0; i < _variables.Count; i++)
                    _variables[i].SetValue(model.GetValue(_variables[i].reference));
            }
            else
            {
                solved = false;
                dc_Utility.WriteLineIf(!suppress, "");
                dc_Utility.WriteLineIf(!suppress, "Simple Model Failed...");
            }

            // clear cplex references
            for (int i = 0; i < _variables.Count; i++)
                _variables[i].reference = null;

            // clean up
            model.ClearModel();

            return solved;
        }

        private bool Terminate(int stepsComplete, double value)
        {
            if (stepsComplete >= param_timeout)
                return true;

            if (stepsComplete <= 0)
            {
                _term_lastValue = double.PositiveInfinity;
                _term_sameCnt = 0;
            }

            _term_sameCnt = value.Approximately(_term_lastValue) ? _term_sameCnt + 1 : 0;
            _term_lastValue = value;

            if (_term_sameCnt >= param_leveloff_steps)
                return true;

            if (stepsComplete == param_ceilby && value > param_ceil)
                return true;

            return false;
        }

        private void UpdateFuncGraph()
        {
            _objective.f.Update();
            _objective.g.Update();

            for (int i = 0; i < _equalities.Count; i++)
            {
                _equalities[i].f.Update();
                _equalities[i].g.Update();
            }

            for (int i = 0; i < _inequalities.Count; i++)
            {
                _inequalities[i].f.Update();
                _inequalities[i].g.Update();
            }
        }

        private void AddCplexVar(Cplex model, dc_Var var)
        {
            var.reference = model.NumVar(var.lb, var.ub, var.name);
        }

        private void AddCplexEqual(Cplex model, dc_FGPair fg)
        {
            model.AddEq(fg.f.AddExpression(model), fg.g.AddExpression(model));
        }

        private void AddCplexLEqual(Cplex model, dc_FGPair fg, INumVar slack)
        {
            INumExpr fi = fg.f.AddExpression(model);
            INumExpr gi = fg.g.Minorize(model);
            model.AddLe(fi, model.Sum(gi, slack));
        }

        public void DebugSolution(double value)
        {
            UpdateFuncGraph();

            dc_Utility.WriteLine("");
            dc_Utility.WriteLine(dc_Utility.c_stars);
            for (int i = 0; i < _variables.Count; i++)
                dc_Utility.WriteLine(string.Format("Name: {0} | Value: {1} | LB: {2} | UB: {3}", _variables[i].name, _variables[i].lastValue, _variables[i].lb, _variables[i].ub));
            dc_Utility.WriteLine(dc_Utility.c_stars);
            dc_Utility.WriteLine("Optimal Value: " + value);
            dc_Utility.WriteLine("Is Feasible: " + IsFeasible());
            dc_Utility.WriteLine(dc_Utility.c_stars);
            dc_Utility.WriteLine("");
        }
        
        
    }
}
