using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DreamStarGen
{
    [RequireComponent(typeof(MeshFilter))]
    public class DreamStarGenerator : MonoBehaviour
    {

        public MeshFilter meshfilter;

        [Header("Base Parameters:")]
        public float Radius = 1;
        [Range(0.1f, 360)]
        public float Density = 0.1f;
        public float Width = 1;

        [Header("Star Parameters:")]
        public float a;
        public float b;
        public float c;
        public float d;
        public float e;








        private int instanceID;

        public virtual void Initialize()
        {

        }

        public virtual Vector3 StarAlgorithm(float Angle)
        {
            float r = 0;
            float radiant = Angle * Mathf.Deg2Rad * a;
            float value1 = 1;
            r = Radius * (value1);

            float x = r * Mathf.Cos(radiant);
            float y = r * Mathf.Sin(radiant);
            return new Vector3(x, y, 0);
        }






        void OnDrawGizmosSelected()
        {
            if (hasErrors()) return;
            OnDuplicate();
            _GenerateStar();
        }


        public void _GenerateStar()
        {
            if (hasErrors()) return;

            Render();
        }

        private void Render()
        {

            Initialize();
            List<Vector3> points = new List<Vector3>();
            for (float i = -Density; i < 360; i += Density)
            {
                Vector3 point = StarAlgorithm(i);
                if (float.IsNaN(point.x + point.y + point.z))
                {
                    points.Add(new Vector3(0, 0, 0));
                }
                else points.Add(StarAlgorithm(i));

            }
            Mesh mesh = meshfilter.sharedMesh;
            MeshGenerators.GenerateCurve(ref mesh, points.ToArray(), Width, transform, true);
            meshfilter.sharedMesh = mesh;
        }

        public bool hasErrors()
        {

            if (!meshfilter)
            {
                meshfilter = GetComponent<MeshFilter>();
                if (!meshfilter) meshfilter = gameObject.AddComponent<MeshFilter>();
            }

            return false;
        }

        public void OnDuplicate()
        {
#if (UNITY_EDITOR)
            if (!Application.isPlaying)//if in the editor
            {

                //if the instance ID doesnt match then this was copied!
                if (instanceID != gameObject.GetInstanceID())
                {
                    if (meshfilter)
                    {
                        meshfilter.sharedMesh = null;
                    }

                }
                instanceID = gameObject.GetInstanceID();
            }
#endif
        }
    }
}