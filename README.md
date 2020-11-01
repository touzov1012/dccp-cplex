# Disciplined Convex-Concave Programming
A .NET class library for the CPLEX optimizer to support disciplined convex-concave program formulations using the vanilla DC algorithm described [here](https://arxiv.org/abs/1604.02639).

Difference of convex (DC) programs are a natural extension of convex conic optimization problems to the realm of non-convex optimization. Included in the solution is a WPF applet demonstrating a few interesting applications of DC programs, a number of which are covered in the above paper. It is well known that the DC algorithm does not give us any guarantees on global optimality of solutions; however, as in the examples, local solutions may be sufficient for specific classes of problems.

### Sphere packing

The sphere packing problem envolves finding a placement of arbitrary sized spheres into the smallest enclosing container without sphere overlap. The difficulty in solving the problem formulation comes from the need to lower bound the distance between points in space. However, this type of constraint is natural in the DC framework and thus the solution methods presented above may be leveraged.
