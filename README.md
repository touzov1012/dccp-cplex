# Disciplined Convex-Concave Programming
A .NET class library for the CPLEX optimizer to support disciplined convex-concave program formulations using the vanilla DC algorithm (DCA) described [here](https://arxiv.org/abs/1604.02639).

Difference of convex (DC) programs are a natural extension of convex conic optimization problems to the realm of non-convex optimization. Included in the solution is a WPF applet demonstrating a few interesting applications of DC programs, a number of which are covered in the above paper. It is well known that the DC algorithm does not give us any guarantees on global optimality of solutions; however, as in the examples, local solutions may be sufficient for specific classes of problems.

### Sphere Packing

The sphere packing problem envolves finding a placement of arbitrary sized spheres into the smallest enclosing container without sphere overlap. The difficulty in solving the problem formulation comes from the need to lower bound the distance between points in space. However, this type of constraint is natural in the DC framework and thus the solution methods presented above may be leveraged.

<p align="center"><img src="https://user-images.githubusercontent.com/26099083/97809185-76655c80-1c39-11eb-8c9b-553559985687.PNG"></p>

### Polygon Nesting

The problem of finding an optimal nesting of arbitrary polygons is well known due to its various applications in circuit design, CNC cutting, 3D art design, marketing, etc. In 2D this is a natural extension of the sphere packing problem above. While the formulation of the problem is a bit more involved and more efficient solution methods for this problem exist, it fits into the framework of a DC program and so may be solved by DCA for a few shapes.

<p align="center"><img src="https://user-images.githubusercontent.com/26099083/97809654-250a9c80-1c3c-11eb-9e1f-e4e8952406f6.PNG"></p>

### Path Planning

The problem of finding the shortest path from some point A to point B in finite dimensional real space while also avoiding spherical obstacles may also be formulated as a DC program. In fact, the same type of non-convex constraints that are required to formulate the sphere packing problem are also used in the formulation of this problem.

<p align="center"><img src="https://user-images.githubusercontent.com/26099083/97809820-6485b880-1c3d-11eb-80e7-50a5bec43c4d.PNG"></p>

### Collision Avoidance

Given a collection of moving drones in finite dimensional real space, we can imagine the task of driving these drones to a set of destinations while minimizing the amount of spent fuel. The task in its current formulation is a simple linear program which can be solved efficiently by CPLEX itself. However, lets take a look at the solution obtained...

