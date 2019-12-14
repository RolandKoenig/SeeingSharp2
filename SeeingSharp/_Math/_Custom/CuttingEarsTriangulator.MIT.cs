﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SeeingSharp
{
    /// <summary>
    /// A cutting ears triangulator for simple polygons with no holes. O(n^2)
    /// This algorithm is based on the implementation of the Helix Toolkit (http://helixtoolkit.codeplex.com/)
    /// </summary>
    /// <remarks>
    /// Regarding the Helix Toolkit:
    /// Based on http://www.flipcode.com/archives/Efficient_Polygon_Triangulation.shtml
    /// References
    /// http://en.wikipedia.org/wiki/Polygon_triangulation
    /// http://computacion.cs.cinvestav.mx/~anzures/geom/triangulation.php
    /// http://www.codeproject.com/KB/recipes/cspolygontriangulation.aspx
    /// </remarks>
    public static class CuttingEarsTriangulator
    {
        /// <summary>
        /// The epsilon.
        /// </summary>
        private const double Epsilon = 1e-10;

        /// <summary>
        /// Triangulate a polygon using the cutting ears algorithm.
        /// </summary>
        /// <remarks>
        /// The algorithm does not support holes.
        /// </remarks>
        /// <param name="contour">
        /// the polygon contour
        /// </param>
        /// <returns>
        /// collection of triangle Vector2s
        /// </returns>
        public static IEnumerable<int> Triangulate(IList<Vector2> contour)
        {
            // allocate and initialize list of indices in polygon
            var result = new List<int>(contour.Count * 3);

            int n = contour.Count;
            if (n < 3)
            {
                return null;
            }

            var V = new int[n];

            // we want a counter-clockwise polygon in V
            if (Area(contour) > 0)
            {
                for (int v = 0; v < n; v++)
                {
                    V[v] = v;
                }
            }
            else
            {
                for (int v = 0; v < n; v++)
                {
                    V[v] = (n - 1) - v;
                }
            }

            int nv = n;

            // remove nv-2 Vertices, creating 1 triangle every time
            int count = 2 * nv; // error detection

            for (int m = 0, v = nv - 1; nv > 2; )
            {
                // if we loop, it is probably a non-simple polygon
                if (0 >= (count--))
                {
                    // ERROR - probable bad polygon!
                    //return null;
                    return result;
                }

                // three consecutive vertices in current polygon, <u,v,w>
                int u = v;
                if (nv <= u)
                {
                    u = 0; // previous
                }

                v = u + 1;
                if (nv <= v)
                {
                    v = 0; // new v
                }

                int w = v + 1;
                if (nv <= w)
                {
                    w = 0; // next
                }

                if (Snip(contour, u, v, w, nv, V))
                {
                    int s, t;

                    // true names of the vertices
                    int a = V[u];
                    int b = V[v];
                    int c = V[w];

                    // output Triangle
                    result.Add((int)a);
                    result.Add((int)b);
                    result.Add((int)c);

                    m++;

                    // remove v from remaining polygon
                    for (s = v, t = v + 1; t < nv; s++, t++)
                    {
                        V[s] = V[t];
                    }

                    nv--;

                    // resest error detection counter
                    count = 2 * nv;
                }
            }

            return result;
        }

        // compute area of a contour/polygon
        /// <summary>
        /// Calculates the area.
        /// </summary>
        /// <param name="contour">The contour.</param>
        /// <returns>The area.</returns>
        private static double Area(IList<Vector2> contour)
        {
            int n = contour.Count;
            double A = 0.0;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                A += contour[p].X * contour[q].Y - contour[q].X * contour[p].Y;
            }

            return A * 0.5f;
        }

        /// <summary>
        /// Decide if Vector2 (Px,Py) is inside triangle defined by (Ax,Ay) (Bx,By) (Cx,Cy).
        /// </summary>
        /// <param name="Ax">
        /// The ax.
        /// </param>
        /// <param name="Ay">
        /// The ay.
        /// </param>
        /// <param name="Bx">
        /// The bx.
        /// </param>
        /// <param name="By">
        /// The by.
        /// </param>
        /// <param name="Cx">
        /// The cx.
        /// </param>
        /// <param name="Cy">
        /// The cy.
        /// </param>
        /// <param name="Px">
        /// The px.
        /// </param>
        /// <param name="Py">
        /// The py.
        /// </param>
        /// <returns>
        /// The inside triangle.
        /// </returns>
        private static bool InsideTriangle(
            double Ax, double Ay, double Bx, double By, double Cx, double Cy, double Px, double Py)
        {
            double ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            double cCROSSap, bCROSScp, aCROSSbp;

            ax = Cx - Bx;
            ay = Cy - By;
            bx = Ax - Cx;
            by = Ay - Cy;
            cx = Bx - Ax;
            cy = By - Ay;
            apx = Px - Ax;
            apy = Py - Ay;
            bpx = Px - Bx;
            bpy = Py - By;
            cpx = Px - Cx;
            cpy = Py - Cy;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return (aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f);
        }

        /// <summary>
        /// The snip.
        /// </summary>
        /// <param name="contour">The contour.</param>
        /// <param name="u">The u.</param>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <param name="n">The n.</param>
        /// <param name="V">The v.</param>
        /// <returns>The snip.</returns>
        private static bool Snip(IList<Vector2> contour, int u, int v, int w, int n, int[] V)
        {
            int p;
            double Ax, Ay, Bx, By, Cx, Cy, Px, Py;

            Ax = contour[V[u]].X;
            Ay = contour[V[u]].Y;

            Bx = contour[V[v]].X;
            By = contour[V[v]].Y;

            Cx = contour[V[w]].X;
            Cy = contour[V[w]].Y;

            if (Epsilon > (((Bx - Ax) * (Cy - Ay)) - ((By - Ay) * (Cx - Ax))))
            {
                return false;
            }

            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                {
                    continue;
                }

                Px = contour[V[p]].X;
                Py = contour[V[p]].Y;
                if (InsideTriangle(Ax, Ay, Bx, By, Cx, Cy, Px, Py))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
