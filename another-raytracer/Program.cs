﻿using System;
using System.Threading;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace another_raytracer
{
	public struct Vertex
		{
			public float x, y, r, g, b;
		}

	public class Game : GameWindow
	{
		private World world;
		private int vbo, vao, frag, vertex, program;

		const float pi2 = (float)Math.PI / 2;
		const float camspeed = 0.05f;

		//[StructLayout(LayoutKind.Sequential, Pack = 1)]

		//private Vertex[] vertices;

		public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
		{
			Vertex[] vertices = new Vertex[width * height];
			world = new World(width, height);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					vertices[y * width + x].x =  x / (float)width * 2 - 1;
					vertices[y * width + x].y = y / (float)height * 2 - 1;

					vertices[y * width + x].r = 0f;
					vertices[y * width + x].g = 0f;
					vertices[y * width + x].b = 0f;

				}
			}

			World.vertices = vertices;

		}

		protected override void OnLoad(EventArgs e)
		{
			InitGL();

			base.OnLoad(e);
		}

		private void Update(double delta)
		{
			Title = $"ray tracer - {1/delta:F2} fps";
			world.RenderWorld(delta);
		}

		private void HandleInput()
		{
			KeyboardState input = Keyboard.GetState();

			if (input.IsKeyDown(Key.Escape))
				Exit();

			if (input.IsKeyDown(Key.Space))
				world.CamPos = new Vector3(world.CamPos.X, world.CamPos.Y + camspeed, world.CamPos.Z);
			else if (input.IsKeyDown(Key.LControl))
				world.CamPos = new Vector3(world.CamPos.X, world.CamPos.Y - camspeed, world.CamPos.Z);

			if (input.IsKeyDown(Key.W))
				world.CamPos = world.CamPos + Vector3.Transform(World.CamDir, world.CamRot) * camspeed;
			else if (input.IsKeyDown(Key.S))
				world.CamPos = world.CamPos - Vector3.Transform(World.CamDir, world.CamRot) * camspeed;
			if (input.IsKeyDown(Key.A))
				world.CamPos = world.CamPos + Vector3.Transform(World.CamDir, world.CamRot * Matrix3.CreateRotationY(-pi2)) * camspeed;
			else if (input.IsKeyDown(Key.D))
				world.CamPos = world.CamPos + Vector3.Transform(World.CamDir, world.CamRot * Matrix3.CreateRotationY(pi2)) * camspeed;

			if (input.IsKeyDown(Key.Left))
				world.CamRot *= Matrix3.CreateRotationY(-0.174533f);
			else if (input.IsKeyDown(Key.Right))
				world.CamRot *= Matrix3.CreateRotationY(0.174533f);

		}

		private void LoadShader(int handle, string shader)
		{
			GL.ShaderSource(handle, shader);
			GL.CompileShader(handle);

			string infoLogVert = GL.GetShaderInfoLog(handle);
			if (infoLogVert != String.Empty)
				Console.WriteLine(infoLogVert);
		}

		private void InitGL()
		{
			// vbo
			vbo = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
			unsafe
			{
				GL.BufferData(BufferTarget.ArrayBuffer, World.vertices.Length * sizeof(Vertex), World.vertices, BufferUsageHint.StaticDraw);
			}

			// load shaders
			vertex = GL.CreateShader(ShaderType.VertexShader);
			LoadShader(vertex, Shaders.vertexShader);

			frag = GL.CreateShader(ShaderType.FragmentShader);
			LoadShader(frag, Shaders.fragmentShader);

			program = GL.CreateProgram();
			GL.AttachShader(program, vertex);
			GL.AttachShader(program, frag);

			GL.LinkProgram(program);
			GL.UseProgram(program);

			// vao
			vao = GL.GenVertexArray();
			GL.BindVertexArray(vao);

			unsafe
			{
				GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), 0);
				GL.EnableVertexAttribArray(0);

				GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), 2 * sizeof(float));
				GL.EnableVertexAttribArray(1);
			}
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			Update(e.Time);

			GL.Clear(ClearBufferMask.ColorBufferBit);

			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
			unsafe
			{
				GL.BufferData(BufferTarget.ArrayBuffer, World.vertices.Length * sizeof(Vertex), World.vertices, BufferUsageHint.StaticDraw);
			}

			GL.BindVertexArray(vao);
			GL.UseProgram(program);

			GL.DrawArrays(PrimitiveType.Points, 0, World.vertices.Length);

			Context.SwapBuffers();

			HandleInput();
			base.OnUpdateFrame(e);
		}

		protected override void OnUnload(EventArgs e)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.DeleteBuffer(vbo);
			GL.DeleteBuffer(vao);

			GL.DetachShader(program, vertex);
			GL.DetachShader(program, frag);
			GL.DeleteShader(vertex);
			GL.DeleteShader(frag);

			GL.DeleteProgram(program);

			base.OnUnload(e);
		}

	}


	class Program
	{
		static void Main(string[] args)
		{
			//ThreadPool.SetMaxThreads(20, 20);
			(new Game(400, 225, "ray tracing")).Run(60.0);
			//(new Game(640, 480, "ray tracing")).Run(60.0);
		}
	}
}
