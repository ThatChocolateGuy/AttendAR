using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DreamStarGen
{
    public class MeshExporter : MonoBehaviour
    {

        public void MeshToFile(Mesh mf, Material[] mats, string name, string path)
        {
            StreamWriter writer = new StreamWriter(path + name);
            writer.Write(MeshToString(mf, mats, name));
            writer.Close();
        }

        private string MeshToString(Mesh mesh, Material[] Mats, string name)
        {
            Mesh m = mesh;
            Material[] mats = Mats;

            StringBuilder sb = new StringBuilder();

            sb.Append("g ").Append(name).Append("\n");
            foreach (Vector3 v in m.vertices)
            {
                sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 v in m.normals)
            {
                sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 v in m.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }
            for (int material = 0; material < m.subMeshCount; material++)
            {
                sb.Append("\n");
                sb.Append("usemtl ").Append(mats[material].name).Append("\n");
                sb.Append("usemap ").Append(mats[material].name).Append("\n");

                int[] triangles = m.GetTriangles(material);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                        triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
                }
            }
            return sb.ToString();
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(MeshExporter))]
    public class MeshExporterEditor : Editor
    {
        string savePath = "Assets/";
        string meshName = "ExportedStar";

        public override void OnInspectorGUI()
        {


            MeshExporter myTarget = (MeshExporter)target;
            MeshFilter mf = myTarget.GetComponent<MeshFilter>();

            if (!mf)
            {
                EditorGUILayout.LabelField("Meshfilter is missing!");
                return;
            }

            if (!mf.sharedMesh)
            {
                EditorGUILayout.LabelField("Meshfilter has nothing to export!");
                return;
            }

            meshName = EditorGUILayout.TextField("Export Name", meshName);
            savePath = EditorGUILayout.TextField("Path", savePath);

            if (GUILayout.Button("Export to .asset"))
            {


                if (mf)
                {

                    string Path = savePath + meshName + ".asset";
                    Debug.Log("Saved Mesh to:" + Path);
                    AssetDatabase.CreateAsset(Instantiate<Mesh>(mf.sharedMesh), Path);

                }
                AssetDatabase.Refresh();
            }

            MeshRenderer re = mf.GetComponent<MeshRenderer>();
            if (!re)
            {
                EditorGUILayout.LabelField("MeshRenderer is needed to export to .Obj!");
                return;
            }


            if (GUILayout.Button("Export to .obj"))
            {

                if (mf && re)
                {
                    myTarget.MeshToFile(mf.sharedMesh, re.sharedMaterials, meshName + ".obj", savePath);
                    string Path = savePath + meshName + ".obj";
                    Debug.Log("Saved Mesh to:" + Path);
                }
                AssetDatabase.Refresh();
            }




            EditorUtility.SetDirty(target);
        }

    }
#endif
}