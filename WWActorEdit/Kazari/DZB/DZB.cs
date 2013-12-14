#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

#endregion

namespace WWActorEdit.Kazari.DZB
{
    public class DZB
    {
        #region Variables

        public string Name { get; private set; }
        public RARC.FileEntry ParentFile { get; set; }
        public Vector3 Translation;
        public Vector3 oldTranslation;
        public Vector3 Rotation;
        public Vector3 oldRotation;
        public Vector3 Center;
        byte[] Data;
        FileHeader Header;
        public List<Vertex> Vertices = new List<Vertex>();
        List<Triangle> Triangles = new List<Triangle>();
        List<Type> Types = new List<Type>();

        TreeNode Root;
        int GLID;

        #endregion

        #region Constructors, DZB Loader, Rendering

        public DZB(RARC.FileEntry FE, TreeNode TN)
        {
            Root = TN;
            Name = FE.FileName;
            ParentFile = FE;
            Load(FE.GetFileData());
        }

        public void ChangeTranslation(Vector3 trans)
        {
            Translation = trans;
            foreach (Vertex V in Vertices) V.Translation = trans;
        }

        public Vector3 ChangeRotation(Vector3 rot)
        {
            Rotation = rot;
            foreach (Vertex V in Vertices)
            {
                V.Rotation = rot;
            }
            return Center;
        }

        public void Clear()
        {
            if (GL.IsList(GLID) == true) GL.DeleteLists(GLID, 1);
        }

        public void Load(byte[] DataArray)
        {
            Data = DataArray;

            Header = new FileHeader(Data);

            UInt32 ReadOffset = Header.VertexOffset;
            for (int i = 0; i < Header.VertexCount; i++)
                Vertices.Add(new Vertex(Data, ref ReadOffset, ParentFile));

            ReadOffset = Header.TriangleOffset;
            for (int i = 0; i < Header.TriangleCount; i++)
                Triangles.Add(new Triangle(Data, ref ReadOffset));

            ReadOffset = Header.TypeOffset;
            for (int i = 0; i < Header.TypeCount; i++)
                Types.Add(new Type(Data, ref ReadOffset));

            //gotta find the center of the room
            float minX = -1, minY = -1, minZ = -1, maxX = -1, maxY = -1, maxZ = -1;
            foreach (Vertex V in Vertices)
            {
                if (minX == -1) minX = V.Position.X;
                else if (minX > V.Position.X) minX = V.Position.X;

                if (minY == -1) minY = V.Position.Y;
                else if (minY > V.Position.Y) minY = V.Position.Y;

                if (minZ == -1) minZ = V.Position.Z;
                else if (minZ > V.Position.Z) minZ = V.Position.Z;

                if (maxX == -1) maxX = V.Position.X;
                else if (maxX < V.Position.X) maxX = V.Position.X;

                if (maxY == -1) maxY = V.Position.Y;
                else if (maxY < V.Position.Y) maxY = V.Position.Y;

                if (maxZ == -1) maxZ = V.Position.Z;
                else if (maxZ < V.Position.Z) maxZ = V.Position.Z;
            }
            Center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
            foreach (Vertex V in Vertices) V.Center = Center;
            Root.Nodes.Add(Helpers.CreateTreeNode(Name, this, string.Format("Size: {0:X6}", Data.Length)));

            Prepare();
        }

        public void Render()
        {
            if (Translation != oldTranslation || Rotation != oldRotation)
            {
                Prepare();
                oldRotation = Rotation;
                oldTranslation = Translation;
            }
            if (GL.IsList(GLID) == true) GL.CallList(GLID);
        }

        public void Prepare()
        {
            if (GL.IsList(GLID) == true) GL.DeleteLists(GLID, 1);

            GLID = GL.GenLists(1);

            GL.NewList(GLID, ListMode.Compile);
            {
                GL.PushAttrib(AttribMask.AllAttribBits);

                GL.Disable(EnableCap.Texture2D);

                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.Enable(EnableCap.Blend);
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
                GL.FrontFace(FrontFaceDirection.Ccw);

                GL.Enable(EnableCap.PolygonOffsetFill);
                GL.PolygonOffset(1.0f, 1.0f);

                GL.Color4(1.0f, 1.0f, 1.0f, 0.5f);

                foreach (Triangle Tri in Triangles)
                {
                    GL.Begin(BeginMode.Triangles);
                    GL.Vertex3(Helpers.RotateAroundCenter(Vertices[Tri.Vertices[0]].Position, Center, Rotation) + Translation);
                    GL.Vertex3(Helpers.RotateAroundCenter(Vertices[Tri.Vertices[1]].Position, Center, Rotation) + Translation);
                    GL.Vertex3(Helpers.RotateAroundCenter(Vertices[Tri.Vertices[2]].Position, Center, Rotation) + Translation);
                    GL.End();
                }

                GL.Disable(EnableCap.PolygonOffsetFill);

                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                GL.Disable(EnableCap.Blend);
                GL.Color4(0.0f, 0.0f, 0.0f, 1.0f);
                foreach (Triangle Tri in Triangles)
                {
                    GL.Begin(BeginMode.Triangles);
                    GL.Vertex3(Helpers.RotateAroundCenter(Vertices[Tri.Vertices[0]].Position, Center, Rotation) + Translation);
                    GL.Vertex3(Helpers.RotateAroundCenter(Vertices[Tri.Vertices[1]].Position, Center, Rotation) + Translation);
                    GL.Vertex3(Helpers.RotateAroundCenter(Vertices[Tri.Vertices[2]].Position, Center, Rotation) + Translation);
                    GL.End();
                }
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

                GL.PopAttrib();
            }
            GL.EndList();
        }

        #endregion

        #region Classes

        class FileHeader
        {
            public const int Size = 48;

            public UInt32 VertexCount;
            public UInt32 VertexOffset;
            public UInt32 TriangleCount;
            public UInt32 TriangleOffset;
            public UInt32 Unknown1Count;
            public UInt32 Unknown1Offset;
            public UInt32 Unknown2Count;
            public UInt32 Unknown2Offset;
            public UInt32 TypeCount;
            public UInt32 TypeOffset;
            public UInt32 Unknown3Count;
            public UInt32 Unknown3Offset;

            public FileHeader(byte[] Data)
            {
                VertexCount = Helpers.Read32(Data, 0);
                VertexOffset = Helpers.Read32(Data, 4);
                TriangleCount = Helpers.Read32(Data, 8);
                TriangleOffset = Helpers.Read32(Data, 12);
                Unknown1Count = Helpers.Read32(Data, 16);
                Unknown1Offset = Helpers.Read32(Data, 20);
                Unknown2Count = Helpers.Read32(Data, 24);
                Unknown2Offset = Helpers.Read32(Data, 28);
                TypeCount = Helpers.Read32(Data, 32);
                TypeOffset = Helpers.Read32(Data, 36);
                Unknown3Count = Helpers.Read32(Data, 40);
                Unknown3Offset = Helpers.Read32(Data, 44);
            }

            public override string ToString()
            {
                return String.Format(
                    "VertexCount: {0}, VertexOffset: {1:X8}, TriangleCount: {2}, TriangleOffset: {3:X8}, Unknown1Count: {4}, Unknown1Offset: {5:X8}, " +
                    "Unknown2Count: {6}, Unknown2Offset: {7:X8}, TypeCount: {8}, TypeOffset: {9:X8}, Unknown3Count: {10}, Unknown3Offset: {11:X8}",
                    VertexCount, VertexOffset, TriangleCount, TriangleOffset, Unknown1Count, Unknown1Offset,
                    Unknown2Count, Unknown2Offset, TypeCount, TypeOffset, Unknown3Count, Unknown3Offset);
            }
        }

        public class Vertex
        {
            public const int Size = 12;

            public Vector3 Position { get { return _Position; } set { _Position = value; } }

            Vector3 _Position;

            public int ThisOffset;

            RARC.FileEntry ParentFile;

            public Vector3 Translation = new Vector3(0, 0, 0);
            public Vector3 Rotation = new Vector3(0, 0, 0);
            public Vector3 Center = new Vector3(0, 0, 0);

            public Vertex(byte[] Data, ref UInt32 Offset, RARC.FileEntry FE)
            {
                ParentFile = FE;
                ThisOffset = (int)Offset;
                _Position = new Vector3(
                    Helpers.ConvertIEEE754Float(Helpers.Read32(Data, (int)Offset)),
                    Helpers.ConvertIEEE754Float(Helpers.Read32(Data, (int)Offset + 4)),
                    Helpers.ConvertIEEE754Float(Helpers.Read32(Data, (int)Offset + 8)));
                Offset += Size;
            }

            public void StoreChanges()
            {
                byte[] Data = ParentFile.GetFileData();

                Helpers.Overwrite32(ref Data, ThisOffset, BitConverter.ToUInt32(BitConverter.GetBytes(Helpers.RotateAroundCenter(_Position,Center,Rotation).X + Translation.X), 0));
                Helpers.Overwrite32(ref Data, ThisOffset + 4, BitConverter.ToUInt32(BitConverter.GetBytes(Helpers.RotateAroundCenter(_Position, Center, Rotation).Y + Translation.Y), 0));
                Helpers.Overwrite32(ref Data, ThisOffset + 8, BitConverter.ToUInt32(BitConverter.GetBytes(Helpers.RotateAroundCenter(_Position,Center,Rotation).Z + Translation.Z), 0));

                ParentFile.SetFileData(Data);
            }

            public override string ToString()
            {
                return String.Format("Position: {0}, ", Position.ToString());
            }
        }

        class Triangle
        {
            public const int Size = 10;

            public UInt16[] Vertices = new UInt16[3];
            public UInt16 Unknown1, Unknown2;

            public Triangle(byte[] Data, ref UInt32 Offset)
            {
                Vertices[0] = Helpers.Read16(Data, (int)Offset);
                Vertices[1] = Helpers.Read16(Data, (int)Offset + 2);
                Vertices[2] = Helpers.Read16(Data, (int)Offset + 4);
                Unknown1 = Helpers.Read16(Data, (int)Offset + 6);
                Unknown2 = Helpers.Read16(Data, (int)Offset + 8);
                Offset += Size;
            }

            public override string ToString()
            {
                return String.Format("Vertices: {0}, Unknown1: {1:X4}, Unknown2: {2:X4}", Vertices.GetContentString(), Unknown1, Unknown2);
            }
        }

        class Type
        {
            public const int Size = 52;

            public UInt32 NameOffset;
            public Vector3 Unknown1;
            public UInt32 Unknown2, Unknown3;
            public Vector3 Unknown4;
            public UInt32 Unknown5, Unknown6, Unknown7, Unknown8;

            public string Name = string.Empty;

            public Type(byte[] Data, ref UInt32 Offset)
            {
                NameOffset = Helpers.Read32(Data, (int)Offset);
                Unknown1 = new Vector3(
                    Helpers.ConvertIEEE754Float(Helpers.Read32(Data, (int)Offset + 4)),
                    Helpers.ConvertIEEE754Float(Helpers.Read32(Data, (int)Offset + 8)),
                    Helpers.ConvertIEEE754Float(Helpers.Read32(Data, (int)Offset + 12)));
                Unknown2 = Helpers.Read32(Data, (int)Offset + 16);
                Unknown3 = Helpers.Read32(Data, (int)Offset + 20);
                Unknown4 = new Vector3(
                    Helpers.ConvertIEEE754Float(Helpers.Read32(Data, (int)Offset + 24)),
                    Helpers.ConvertIEEE754Float(Helpers.Read32(Data, (int)Offset + 28)),
                    Helpers.ConvertIEEE754Float(Helpers.Read32(Data, (int)Offset + 32)));
                Unknown5 = Helpers.Read32(Data, (int)Offset + 36);
                Unknown6 = Helpers.Read32(Data, (int)Offset + 40);
                Unknown7 = Helpers.Read32(Data, (int)Offset + 44);
                Unknown8 = Helpers.Read32(Data, (int)Offset + 48);

                Name = Helpers.ReadString(Data, (int)NameOffset);

                Offset += Size;
                Console.WriteLine(String.Format(
    "NameOffset: {0:X8}, Unknown1: {1}, Unknown2: {2:X8}, Unknown3: {3:X8}, Unknown4: {4}, Unknown5: {5:X8}, Unknown6: {6:X8}, Unknown7: {7:X8}, Unknown8: {8:X8}",
    NameOffset, Unknown1, Unknown2, Unknown3, Unknown4, Unknown5, Unknown6, Unknown7, Unknown8));
            }

            public override string ToString()
            {
                return String.Format(
                    "NameOffset: {0:X8}, Unknown1: {1}, Unknown2: {2:X8}, Unknown3: {3:X8}, Unknown4: {4}, Unknown5: {5:X8}, Unknown6: {6:X8}, Unknown7: {7:X8}, Unknown8: {8:X8}",
                    NameOffset, Unknown1, Unknown2, Unknown3, Unknown4, Unknown5, Unknown6, Unknown7, Unknown8);
            }
        }

        #endregion
    }
}
