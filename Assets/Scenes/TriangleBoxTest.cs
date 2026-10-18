using System.Collections.Generic;
using JetBrains.Annotations;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using static UnityEngine.UI.Image;

// Code adaptiert von https://stackoverflow.com/questions/17458562/efficient-aabb-triangle-intersection-in-c-sharp 
class IntersectionTest
{

    public static bool IsIntersecting(IAABox box, ITriangle triangle)
    {
        double triangleMin, triangleMax;
        double boxMin, boxMax;

        // Test the box normals (x-, y- and z-axes)
        var boxNormals = new IVector[] {
        new VectorImpl(new Vector3(1,0,0)),
        new VectorImpl(new Vector3(0,1,0)),
        new VectorImpl(new Vector3(0,0,1))
    };
        for (int i = 0; i < 3; i++)
        {
            Project(triangle.Vertices, boxNormals[i], out triangleMin, out triangleMax);
            if (triangleMax < box.Start.Coords[i] || triangleMin > box.End.Coords[i])
                return false; // No intersection possible.
        }

        // Test the triangle normal
        double triangleOffset = triangle.Normal.Dot(triangle.A);
        Project(box.Vertices, triangle.Normal, out boxMin, out boxMax);
        if (boxMax < triangleOffset || boxMin > triangleOffset)
            return false; // No intersection possible.

        // Test the nine edge cross-products
        IVector[] triangleEdges = new IVector[] {
        triangle.A.Minus(triangle.B),
        triangle.B.Minus(triangle.C),
        triangle.C.Minus(triangle.A)
    };
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
            {
                // The box normals are the same as it's edge tangents
                IVector axis = triangleEdges[i].Cross(boxNormals[j]);
                Project(box.Vertices, axis, out boxMin, out boxMax);
                Project(triangle.Vertices, axis, out triangleMin, out triangleMax);
                if (boxMax < triangleMin || boxMin > triangleMax)
                    return false; // No intersection possible
            }

        // No separating axis found.
        return true;
    }

    public static void Project(IEnumerable<IVector> points, IVector axis,
            out double min, out double max)
    {
        min = double.PositiveInfinity;
        max = double.NegativeInfinity;
        foreach (var p in points)
        {
            double val = axis.Dot(p);
            if (val < min) min = val;
            if (val > max) max = val;
        }
    }
}

interface IVector
{
    double X { get; }
    double Y { get; }
    double Z { get; }
    double[] Coords { get; }
    double Dot(IVector other);
    IVector Minus(IVector other);
    IVector Cross(IVector other);
}

interface IShape
{
    IEnumerable<IVector> Vertices { get; }
}

interface IAABox : IShape
{
    IVector Start { get; }
    IVector End { get; }
}

interface ITriangle : IShape
{
    IVector Normal { get; }
    IVector A { get; }
    IVector B { get; }
    IVector C { get; }
}

class IAABoxImpl : IAABox
{
    public IAABoxImpl(Vector3 origin, Vector3 topPoint)
    {
        Start = new VectorImpl(origin);
        End = new VectorImpl(topPoint);
        var temp = new List<IVector>
        {
            Start,
            End,
            new VectorImpl(new Vector3(origin.x, origin.y, topPoint.z)),
            new VectorImpl(new Vector3(origin.x, topPoint.y, origin.z)),
            new VectorImpl(new Vector3(origin.x, topPoint.y, topPoint.z)),
            new VectorImpl(new Vector3(topPoint.x, origin.y, origin.z)),
            new VectorImpl(new Vector3(topPoint.x, origin.y, topPoint.z)),
            new VectorImpl(new Vector3(topPoint.x, topPoint.y, origin.z))
        };

        Vertices = temp;
    }

    public IVector Start { get; }
    public IVector End { get; }
    public IEnumerable<IVector> Vertices { get; }
}
class TriangleImpl : ITriangle
{
    public TriangleImpl(Vector3 a, Vector3 b, Vector3 c)
    {
        A = new VectorImpl(a); B = new VectorImpl(b); C = new VectorImpl(c);
        Normal = B.Minus(A).Cross(C.Minus(A));
        Vertices = new List<IVector>
        {
            A,
            B,
            C
        };
    }
    public IVector Normal { get; }
    public IVector A { get; }
    public IVector B { get; }
    public IVector C { get; }

    public IEnumerable<IVector> Vertices { get; }
}


class VectorImpl : IVector
{
    public Vector3 vec;
    public VectorImpl(Vector3 vector)
    {
        vec = vector;
        X = vector.x; Y = vector.y; Z = vector.z;
        Coords = new double[3] { X, Y, Z };
    }

    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public double[] Coords { get; }
    public double Dot(IVector other) { return Vector3.Dot(vec, new Vector3((float)other.X, (float)other.Y, (float)other.Z)); }

    public IVector Minus(IVector other) { return new VectorImpl(vec - new Vector3((float)other.X, (float)other.Y, (float)other.Z)); }
    public IVector Cross(IVector other) { return new VectorImpl(Vector3.Cross(vec, new Vector3((float)other.X, (float)other.Y, (float)other.Z))); }

}