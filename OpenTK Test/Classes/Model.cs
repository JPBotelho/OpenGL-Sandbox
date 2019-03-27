using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenTK_Test
{
	class Model
	{
		[Conditional("DEBUG")]
		[DebuggerStepThrough]
		public static void CheckLastError()
		{
			ErrorCode errorCode = GL.GetError();
			if (errorCode != ErrorCode.NoError)
			{
				throw new Exception(errorCode.ToString());
			}
		}

		public List<TextureInfo> texturesLoaded = new List<TextureInfo>();
		private List<Mesh> meshes = new List<Mesh>();
		private string directory;


		public Model(string path)
		{
			LoadModel(path);
		}

		public void Draw(Shader shader)
		{
			for (int i = 0; i < meshes.Count; i++)
				meshes[i].Draw(shader);
		}

		private void LoadModel(string path)
		{
			AssimpContext importer = new AssimpContext();
			Scene scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.CalculateTangentSpace);

			if(scene == null || (scene.SceneFlags & SceneFlags.Incomplete) == SceneFlags.Incomplete || scene.RootNode == null)
			{
				Console.WriteLine("ERROR::ASSIMP");
				return;
			}
			int index = path.LastIndexOf("/");
			directory = path.Substring(0, index);

			ProcessNode(scene.RootNode, scene);
		}

		private void ProcessNode(Node node, Scene scene)
		{
			for(int i = 0; i < node.MeshCount; i++)
			{
				Assimp.Mesh mesh = scene.Meshes[node.MeshIndices[i]];
				meshes.Add(ProcessMesh(mesh, scene));
			}

			for(int i = 0; i < node.Children.Count; i++)
			{
				ProcessNode(node.Children[i], scene);
			}
		}

		private Mesh ProcessMesh(Assimp.Mesh mesh, Scene scene)
		{
			List<Vertex> vertices = new List<Vertex>();
			List<uint> indices = new List<uint>();
			List<TextureInfo> textures = new List<TextureInfo>();

			for (int i = 0; i < mesh.VertexCount; i++)
			{
				Vertex vertex = new Vertex();
				Vector3 vector;

				vector.X = mesh.Vertices[i].X;
				vector.Y = mesh.Vertices[i].Y;
				vector.Z = mesh.Vertices[i].Z;
				vertex.Position = vector;


				vector.X = mesh.Normals[i].X;
				vector.Y = mesh.Normals[i].Y;
				vector.Z = mesh.Normals[i].Z;
				vertex.Normal = vector;

				if (mesh.HasTextureCoords(0))
				{
					Vector2 vec;
					vec.X = mesh.TextureCoordinateChannels[0][i].X;
					vec.Y = mesh.TextureCoordinateChannels[0][i].Y;
					vertex.TexCoords = vec;
				}
				else
				{
					vertex.TexCoords = Vector2.Zero;
				}

				vector.X = mesh.Tangents[i].X;
				vector.Y = mesh.Tangents[i].Y;
				vector.Z = mesh.Tangents[i].Z;
				vertex.Tangent = vector;
				// bitangent
				vector.X = mesh.BiTangents[i].X;
				vector.Y = mesh.BiTangents[i].Y;
				vector.Z = mesh.Tangents[i].Z;
				vertex.Bitangent = vector;

				vertices.Add(vertex);
			}

			for (int i = 0; i < mesh.FaceCount; i++)
			{
				Face face = mesh.Faces[i];
				// retrieve all indices of the face and store them in the indices vector
				for (int j = 0; j < face.IndexCount; j++)
				{
					indices.Add((uint)face.Indices[j]);
					//Console.WriteLine(face.Indices[j]);
				}
			}
			Material material = scene.Materials[mesh.MaterialIndex];

			List<TextureInfo> diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse, "texture_diffuse");
			textures.AddRange(diffuseMaps);
			// 2. specular maps
			List<TextureInfo> specularMaps = LoadMaterialTextures(material, TextureType.Specular, "texture_specular");
			textures.AddRange(specularMaps);
			// 3. normal maps
			List<TextureInfo> normalMaps = LoadMaterialTextures(material, TextureType.Height, "texture_normal");
			textures.AddRange(normalMaps);
			// 4. height maps
			List<TextureInfo> heightMaps = LoadMaterialTextures(material, TextureType.Ambient, "texture_height");
			textures.AddRange(heightMaps);

			return new Mesh(vertices.ToArray(), indices.ToArray(), textures.ToArray());
		}

		private List<TextureInfo> LoadMaterialTextures(Material mat, TextureType type, string typeName)
		{
			List<TextureInfo> textures = new List<TextureInfo>();
			for (int i = 0; i < mat.GetMaterialTextureCount(type); i++)
			{
				TextureSlot str;
				mat.GetMaterialTexture(type, i, out str);
				// check if texture was loaded before and if so, continue to next iteration: skip loading a new texture
				bool skip = false;
				for (int j = 0; j < texturesLoaded.Count; j++)
				{
					if (texturesLoaded[j].path == str.FilePath)
					{
						textures.Add(texturesLoaded[j]);
						skip = true; // a texture with the same filepath has already been loaded, continue to next one. (optimization)
						break;
					}
				}
				if (!skip)
				{   // if texture hasn't been loaded already, load it
					TextureInfo texture;
					texture.id = TextureFromFile(str.FilePath, directory);
					texture.type = typeName;
					texture.path = str.FilePath;
					textures.Add(texture);
					texturesLoaded.Add(texture);  // store it as texture loaded for entire model, to ensure we won't unnecesery load duplicate textures.
				}
			}
			return textures;
		}
		int tcount = 1;
		private uint TextureFromFile(string path, string directory)
		{
			string tPath = System.IO.Path.Combine(directory, path);
			Texture t = new Texture(tPath);
			Console.WriteLine(tcount + ": " + tPath +"\n");
			tcount++;

			return (uint)t.Handle;
		}
	}
}
