using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DreamStarGen
{

    public class MeshGenerators
    {

        public static void GenerateLine(ref Mesh output, Vector3 StartPoint, Vector3 EndPoint, float Width, float Overlap = 0)
        {
            if (output == null) output = new Mesh();
            output.Clear();

            Vector3 distance = EndPoint - StartPoint;
            Vector3 overlapvec = distance.normalized * Overlap;


            Vector3 perpendicularvector = Quaternion.AngleAxis(90.0f, new Vector3(0, 0, 1)) * distance;
            perpendicularvector = perpendicularvector.normalized;

            Vector3[] vertices = new Vector3[4];
            vertices[0] = StartPoint - overlapvec - perpendicularvector * Width;
            vertices[1] = StartPoint - overlapvec + perpendicularvector * Width;
            vertices[2] = EndPoint + overlapvec + perpendicularvector * Width;
            vertices[3] = EndPoint + overlapvec - perpendicularvector * Width;

            Vector2[] uvs = new Vector2[4];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(0, 1);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(1, 0);

            int[] triangles = new int[] { 0, 1, 2, 2, 3, 0 };

            output.vertices = vertices;
            output.uv = uvs;
            output.triangles = triangles;
            


        }

        public static void GenerateCurve(ref Mesh output, Vector3[] points, float Width, Transform transform, bool closed = false)
        {
            if (output == null) output = new Mesh();

            output.Clear();

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> UVs = new List<Vector2>();
            List<int> Triangles = new List<int>();

            Vector3 StartPoint;
            Vector3 EndPoint;
            Vector3 perpendicularvector = new Vector3();
            Vector3 perpendicularvector2 = new Vector3();
            Vector3 cornerpoint = new Vector3();


            if (points.Length < 2)
            {
                closed = false;
            }
            else if (points[0] != points[points.Length - 1])
            {
                closed = false;
            }

            if (points.Length > 1)
            {
                StartPoint = points[0];
                UVs.Add(new Vector2(0, 0));
                UVs.Add(new Vector2(0, 1));



                perpendicularvector = Quaternion.AngleAxis(90.0f, new Vector3(0, 0, 1)) * (points[0] - points[1]);

                if (closed && points.Length > 2)
                {
                    perpendicularvector2 = Quaternion.AngleAxis(90.0f, new Vector3(0, 0, 1)) * (points[points.Length - 2] - points[points.Length - 1]);
                    cornerpoint = -(perpendicularvector + perpendicularvector2).normalized;

                    vertices.Add(StartPoint + cornerpoint * Width);
                    vertices.Add(StartPoint - cornerpoint * Width);
                }
                else if (points.Length > 2)
                {
                    perpendicularvector2 = Quaternion.AngleAxis(90.0f, new Vector3(0, 0, 1)) * (points[1] - points[2]);


                    Vector3 e = perpendicularvector.normalized;
                    cornerpoint = (perpendicularvector + perpendicularvector2).normalized;

                    Vector3 a = cornerpoint * Width;



                    Vector3 ap = Vector3.Dot(e, a) * e;





                    vertices.Add(StartPoint - ap);
                    vertices.Add(StartPoint + ap);
                }
                else
                {
                    vertices.Add(StartPoint + cornerpoint * Width);
                    vertices.Add(StartPoint - cornerpoint * Width);
                }














            }

            #region Main
            int increment = 0;
            for (int i = 1; i < points.Length - 1; i++)
            {
                StartPoint = points[i - 1];
                EndPoint = points[i];

                Vector3 distance1 = EndPoint - StartPoint;
                if (i < points.Length - 1)
                {
                    perpendicularvector2 = Quaternion.AngleAxis(90.0f, new Vector3(0, 0, 1)) * (points[i + 1] - EndPoint);
                }
                else
                {
                    perpendicularvector2 = new Vector3();
                }

                perpendicularvector = Quaternion.AngleAxis(90.0f, new Vector3(0, 0, 1)) * distance1;

                cornerpoint = (perpendicularvector + perpendicularvector2).normalized;



                vertices.Add(EndPoint + cornerpoint * Width);
                vertices.Add(EndPoint - cornerpoint * Width);



                if (i % 2 == 0)
                {
                    UVs.Add(new Vector2(0, 0));
                    UVs.Add(new Vector2(0, 1));
                }
                else
                {
                    UVs.Add(new Vector2(1, 0));
                    UVs.Add(new Vector2(1, 1));

                }

                increment = i;

                Triangles.Add(increment + increment - 1);
                Triangles.Add(increment + increment - 2);
                Triangles.Add(increment + increment);
                Triangles.Add(increment + increment);
                Triangles.Add(increment + increment + 1);
                Triangles.Add(increment + increment - 1);
            }

            #endregion

            if (points.Length > 1)
            {
                StartPoint = points[points.Length - 2];
                EndPoint = points[points.Length - 1];

                Vector3 dist = EndPoint - StartPoint;

                perpendicularvector = Quaternion.AngleAxis(90.0f, new Vector3(0, 0, 1)) * dist;


                if (closed)
                {
                    perpendicularvector2 = Quaternion.AngleAxis(90.0f, new Vector3(0, 0, 1)) * (points[1] - points[0]);
                    cornerpoint = (perpendicularvector + perpendicularvector2).normalized;

                    vertices.Add(EndPoint + cornerpoint * Width);
                    vertices.Add(EndPoint - cornerpoint * Width);
                }
                else if (points.Length > 2)
                {
                    perpendicularvector2 = Quaternion.AngleAxis(90.0f, new Vector3(0, 0, 1)) * (points[points.Length - 2] - points[points.Length - 3]);
                    Vector3 e = perpendicularvector.normalized;
                    cornerpoint = (perpendicularvector + perpendicularvector2).normalized;

                    Vector3 a = cornerpoint * Width;



                    Vector3 ap = Vector3.Dot(e, a) * e;


                    vertices.Add(EndPoint + ap);
                    vertices.Add(EndPoint - ap);
                }
                else
                {
                    vertices.Add(EndPoint + cornerpoint * Width);
                    vertices.Add(EndPoint - cornerpoint * Width);
                }



                if ((points.Length - 1) % 2 == 0)
                {
                    UVs.Add(new Vector2(0, 0));
                    UVs.Add(new Vector2(0, 1));
                }
                else
                {
                    UVs.Add(new Vector2(1, 0));
                    UVs.Add(new Vector2(1, 1));

                }

                increment = points.Length - 1;
                Triangles.Add(increment + increment - 1);
                Triangles.Add(increment + increment - 2);
                Triangles.Add(increment + increment);
                Triangles.Add(increment + increment);
                Triangles.Add(increment + increment + 1);
                Triangles.Add(increment + increment - 1);
            }










            output.SetVertices(vertices);
            output.SetUVs(0, UVs);
            output.SetTriangles(Triangles, 0);
            output.RecalculateNormals();


        }





    }
}