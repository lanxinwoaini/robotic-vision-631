#region File Description
//-----------------------------------------------------------------------------
// Landscape.cs
//
// TheKnack GUI
// Matt Diehl
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Robot_Racers.XNAGame.WinForms;
using System;
#endregion

namespace Robot_Racers.XNAGame
{
    class LandscapeGraphics
    {
        GraphicsDevice device;

        public struct VertexPositionNormalColored
        {
            public Vector3 Position;
            public Color Color;
            public Vector3 Normal;

            public static int SizeInBytes = 7 * 4;
            public static VertexElement[] VertexElements = new VertexElement[]
            {
                 new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
                 new VertexElement( 0, sizeof(float) * 3, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 0 ),
                 new VertexElement( 0, sizeof(float) * 4, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0 ),
            };
        }


        int[] indices;
        VertexPositionNormalColored[] vertices;
        VertexDeclaration myVertexDeclaration;
        private static float[,] heightData;
        VertexBuffer myVertexBuffer;
        IndexBuffer myIndexBuffer;

        private float landscapeScalingFactor = 20.0f;
        // Set the effects to use
        Effect effect;

        public static Texture2D heightMap;
        //public static Sun sun;

        internal void Initialize(GraphicsDevice device)
        {
            this.device = device;

            //sun = new Sun(342f, 335f);
        }

        internal void LoadContent(ContentManager Content)
        {
            effect = Content.Load<Effect>("Effects\\effects");
            heightMap = Content.Load<Texture2D>("heightmap");

            LoadHeightData(heightMap);//needs to be run before setup vertices
            SetUpIndices();
            SetUpVertices();
            CalculateNormals();
            CopyToBuffers();
        }
        public static int terrainWidth;
        public static int terrainHeight;
        private void LoadHeightData(Texture2D heightMap)
        {
            terrainWidth = heightMap.Width;
            terrainHeight = heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainHeight];
            heightMap.GetData(heightMapColors);

            heightData = new float[terrainWidth, terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainHeight; y++)
                    heightData[x, y] = heightMapColors[x + y * terrainWidth].R / landscapeScalingFactor;
        }
        private void SetUpIndices()
        {
            indices = new int[(terrainWidth - 1) * (terrainHeight - 1) * 6];
            int counter = 0;
            for (int y = 0; y < terrainHeight - 1; y++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    int lowerLeft = x + y * terrainWidth;
                    int lowerRight = (x + 1) + y * terrainWidth;
                    int topLeft = x + (y + 1) * terrainWidth;
                    int topRight = (x + 1) + (y + 1) * terrainWidth;

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }
        }
        private void CalculateNormals()
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);
            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();
        }
        private void SetUpVertices()
        {
            // find max and min values of the data:
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainHeight; y++)
                {
                    if (heightData[x, y] < minHeight)
                        minHeight = heightData[x, y];
                    if (heightData[x, y] > maxHeight)
                        maxHeight = heightData[x, y];
                }
            }


            vertices = new VertexPositionNormalColored[terrainWidth * terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainHeight; y++)
                {
                    vertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);

                    if (heightData[x, y] < minHeight + (maxHeight - minHeight) / 4)
                        vertices[x + y * terrainWidth].Color = Color.Beige;
                    else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 2 / 4)
                        vertices[x + y * terrainWidth].Color = Color.Gray;
                    else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 3 / 4)
                        vertices[x + y * terrainWidth].Color = Color.DarkKhaki;
                    else
                        vertices[x + y * terrainWidth].Color = Color.White;
                }
            }

            myVertexDeclaration = new VertexDeclaration(device, VertexPositionNormalColored.VertexElements);
        }
        public static float GetHeightAtWorldLocation(Vector3 loc){
            float x = loc.X + terrainWidth / 2.0f; //where in the heightmap
            float y = -(loc.Z - terrainHeight / 2.0f);
            if (x <= 0 || x >= terrainWidth - 1 ||
                y <= 0 || y >= terrainHeight) return 0.0f; // if we're beyond the borders, just return 0
            float top = Math.Min(255f, Convert.ToInt32(Math.Ceiling(y)));
            float bottom = Math.Max(0f, Convert.ToInt32(Math.Floor(y)));
            float right = Math.Min(255f,Convert.ToInt32(Math.Ceiling(x)));
            float left = Math.Max(0f, Convert.ToInt32(Math.Floor(x)));

            float leftWeight = left == right ? 1.0f : (right - x);
            float bottomWeight = bottom == top ? 1.0f : (top - y);

            float topRightWeight = (1.0f - leftWeight) * (1.0f - bottomWeight) * heightData[Convert.ToInt32(right), Convert.ToInt32(top)];
            float topLeftWeight = (leftWeight) * (1.0f - bottomWeight) * heightData[Convert.ToInt32(left), Convert.ToInt32(top)];
            float bottomRightWeight = (1.0f - leftWeight) * (bottomWeight) * heightData[Convert.ToInt32(right), Convert.ToInt32(bottom)];
            float bottomLeftWeight = (leftWeight) * (bottomWeight) * heightData[Convert.ToInt32(left), Convert.ToInt32(bottom)];


            return (bottomLeftWeight + topLeftWeight + bottomRightWeight + topRightWeight);
        }
        private void CopyToBuffers()
        {
            myVertexBuffer = new VertexBuffer(device, vertices.Length * VertexPositionNormalColored.SizeInBytes, BufferUsage.WriteOnly);
            myVertexBuffer.SetData(vertices);
            myIndexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            myIndexBuffer.SetData(indices);
        }

        #region Draw
        internal void DrawLandscape(Matrix viewMatrix, Matrix projectionMatrix)
        {
            Matrix worldMatrix = Matrix.CreateTranslation(-terrainWidth / 2.0f, 0.0f, terrainHeight / 2.0f);
            xnaRoboRacerDisplay.device.RenderState.CullMode = CullMode.None;
            effect.CurrentTechnique = effect.Techniques["Colored"];
            effect.Parameters["xView"].SetValue(viewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);
            effect.Parameters["xWorld"].SetValue(worldMatrix);
            effect.Parameters["xEnableLighting"].SetValue(true);
            Vector3 lightDirection = new Vector3(1.0f, -1.0f, -1.0f);
            lightDirection.Normalize();
            effect.Parameters["xLightDirection"].SetValue(lightDirection);
            effect.Parameters["xAmbient"].SetValue(0.1f);

            //effect.CurrentTechnique = effect.Techniques["Pretransformed"];
            effect.Begin();

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                //device.RenderState.CullMode = CullMode.None;
                device.VertexDeclaration = myVertexDeclaration;
                device.Indices = myIndexBuffer;
                device.Vertices[0].SetSource(myVertexBuffer, 0, VertexPositionNormalColored.SizeInBytes);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);
                pass.End();
            }
            effect.End();
        }


        #endregion

    }
}
