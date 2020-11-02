# Disciplined Convex-Concave Programming
A .NET class library for the CPLEX optimizer to support disciplined convex-concave program formulations using the vanilla DC algorithm (DCA) described [here](https://arxiv.org/abs/1604.02639).

## Usage and Examples

Consider solving the non-convex problem

<p align="center"><img src="https://latex.codecogs.com/gif.latex?\begin{array}{rrcl}&space;\min&space;&&space;-x_1^2&plus;x_2^2&plus;x_3^2&space;\\&space;s.t.&space;&&space;x_1&plus;x_2&plus;x_3&space;&&space;\le&space;&&space;2&space;\\&space;&&space;x_1-4x_2^2-x_3^2&space;&&space;\le&space;&&space;-1&space;\\&space;&&space;x_1,x_2,x_3&space;&&space;\ge&space;&&space;0&space;\end{array}" /></p>

To apply the optimizer, we need to write each function in the formulation as a difference of convex functions. For example, the objective function...

<p align="center"><img src="https://latex.codecogs.com/gif.latex?-x_1^2&plus;x_2^2&plus;x_3^2=(x_2^2&plus;x_3^2)-(x_1^2)=f-g" /></p>

So, now we can build our model and define our variables as in CPLEX.

```cs
dc_Model model = new dc_Model();
dc_Var[] X = model.AddVarArray("X", 0, 2, 3, ExUtility.RandRange);
```

Set the objective function.

```cs
model.SetObjective(model.Fn_SumSquares(X[1], X[2]), model.Fn_Square(X[0]));
```

Set the constraints.

```cs
model.AddLE(model.Fn_Sum(X), model.Fn_Const(2));
model.AddLE(model.Fn_Sum(X[0], model.Fn_Const(1)), 
            model.Fn_Sum(model.Fn_Scale(4, model.Fn_Square(X[1])), model.Fn_Square(X[2])));
```

Set the number of start attempts, and execute the model.

```cs
model.Solve(5, ExUtility.RandRange);
```

The true solution is in fact (1.25, 0.75, 0.00) with an optimal value of -1. The solver recovers this solution seen below.

```
******************************
Name: X_0 | Value: 1.2499999991451 | LB: 0 | UB: 2
Name: X_1 | Value: 0.750000000079298 | LB: 0 | UB: 2
Name: X_2 | Value: 4.02633910440121E-10 | LB: 0 | UB: 2
******************************
Optimal Value: -0.999999996117459
Is Feasible: True
******************************
```

We can also tackle some of the more complicated examples presented in the paper above.

### Sphere Packing

The sphere packing problem envolves finding a placement of arbitrary sized spheres into the smallest enclosing container without sphere overlap. The difficulty in solving the problem formulation comes from the need to lower bound the distance between points in space. However, this type of constraint is natural in the DC framework and thus the solution methods presented above may be leveraged.

<p align="center" ><img src="https://user-images.githubusercontent.com/26099083/97809185-76655c80-1c39-11eb-8c9b-553559985687.PNG" width="450"></p>

### Polygon Nesting

The problem of finding an optimal nesting of arbitrary polygons is well known due to its various applications in circuit design, CNC cutting, 3D art design, marketing, etc. In 2D this is a natural extension of the sphere packing problem above. While the formulation of the problem is a bit more involved and more efficient solution methods for this problem [exist](https://github.com/touzov1012/poly-nest), it fits into the framework of a DC program and so may be solved by DCA for a few shapes.

<p align="center"><img src="https://user-images.githubusercontent.com/26099083/97809654-250a9c80-1c3c-11eb-9e1f-e4e8952406f6.PNG" width="450"></p>

### Path Planning

The problem of finding the shortest path from some point A to point B in finite dimensional real space while also avoiding spherical obstacles may also be formulated as a DC program. In fact, the same type of non-convex constraints that are required to formulate the sphere packing problem are also used in the formulation of this problem.

<p align="center"><img src="https://user-images.githubusercontent.com/26099083/97809820-6485b880-1c3d-11eb-80e7-50a5bec43c4d.PNG" width="450"></p>

### Collision Avoidance

Given a collection of moving drones in finite dimensional real space, we can imagine the task of driving these drones to a set of destinations while minimizing the amount of spent fuel. The task in its current formulation is a simple linear program which can be solved efficiently by CPLEX itself. However, lets take a look at the solution obtained...

<p align="center"><img src="https://user-images.githubusercontent.com/26099083/97809963-7ae04400-1c3e-11eb-9285-f5520dfbea13.PNG" width="450"></p>

If each drone moves from the green to the yellow dot, then the red spheres indicate points of collision between drones. To avoid these collisions while still minimizing fuel expenditure, we can add additional DC constraints and apply the DCA above.

<p align="center"><img src="https://user-images.githubusercontent.com/26099083/97810038-0528a800-1c3f-11eb-97e7-9406072c451d.PNG" width="450"></p>

Additional applications are available in the applet with even more in the paper above. This project was completed for the special topics course STOR 893: Selected Methods for Modern Optimization in Data Analysis.
