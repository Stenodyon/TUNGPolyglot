using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Polyglot
{
    class RemotePlayer
    {
        private static Mesh MakePlayerMesh()
        {
            Vector3[] vertices = {
                new Vector3(-0.5f, -0.5f, -0.5f), // 0
                new Vector3( 0.5f, -0.5f, -0.5f), // 1
                new Vector3( 0.5f, -0.5f,  0.5f), // 2
                new Vector3(-0.5f, -0.5f,  0.5f), // 3
                new Vector3(-0.5f,  0.5f, -0.5f), // 4
                new Vector3( 0.5f,  0.5f, -0.5f), // 5
                new Vector3( 0.5f,  0.5f,  0.5f), // 6
                new Vector3(-0.5f,  0.5f,  0.5f), // 7
            };

            int[] triangles = {
                // Top (4, 5, 6, 7)
                4, 6, 5,
                4, 7, 6,
                // Front (0, 1, 4, 5)
                0, 5, 1,
                0, 4, 5,
                // Back (2, 3, 6, 7)
                2, 7, 3,
                2, 6, 7,
                // Left (0, 3, 4, 7)
                3, 4, 0,
                3, 7, 4,
                // Right (1, 2, 5, 6)
                1, 6, 2,
                1, 5, 6,
                // Bottom (0, 1, 2, 3)
                0, 1, 2,
                0, 2, 3,
            };

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }

        public static GameObject MakePlayerModel()
        {
            if(playerModel == null)
            {
                playerModel = new GameObject("Remote Player");
                MeshFilter filter = playerModel.AddComponent<MeshFilter>();
                filter.mesh = MakePlayerMesh();
                MeshRenderer renderer = playerModel.AddComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Diffuse"));
                renderer.material.color = Color.white;
                playerModel.SetActive(false);
            }
            GameObject newModel = GameObject.Instantiate(playerModel);
            newModel.SetActive(true);
            return newModel;
        }

        public static GameObject playerModel { get; private set; }

        public int ID { get; private set; }

        public Vector3 Position
        {
            get { return Position; }
            set
            {
                Position = value;
                if (model != null)
                    model.transform.position = value;
            }
        }
        public Quaternion Rotation
        {
            get { return Rotation; }
            set
            {
                Rotation = value;
                if (model != null)
                    model.transform.rotation = value;
            }
        }

        public Vector3 Angles
        {
            get { return Rotation.eulerAngles; }
            set { Rotation = Quaternion.Euler(value); }
        }

        private GameObject model;

        public RemotePlayer(int id)
        {
            this.ID = id;
            this.model = MakePlayerModel();
            this.Position = this.model.transform.position;
            this.Rotation = this.model.transform.rotation;
        }

        public void Destroy()
        {
            GameObject.Destroy(this.model);
        }
    }
}
