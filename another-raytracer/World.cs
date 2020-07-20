using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using OpenTK;
using System.Drawing;

namespace another_raytracer
{
	
	class World
	{
		private List<IIntersectable> objects = new List<IIntersectable>();
		private List<Light> lights = new List<Light>();

		public int ScreenWidth { get; set; }
		public int ScreenHeight { get; set; }
		public Vector3 BackgroundColour { get; set; } = new Vector3(0.1f);

		public Vector3 CamPos { get; set; } = new Vector3(0, 0, -1);
		public static Vector3 CamDir { get; } = new Vector3(0, 0, 1);
		public Matrix3 CamRot { get; set; } = Matrix3.CreateRotationX(0);

		public static Vertex[] vertices;
		public object locker = new Object();

		public World(int screenWidth, int screenHeight)
		{
			ScreenWidth = screenWidth;
			ScreenHeight = screenHeight;

			objects.Add(new Sphere(new Vector3(-1, -1, 2), 1));
			objects.Add(new Sphere(new Vector3(1, 2, 2), 0.2f));
			lights.Add(new Light(new Vector3(0, 10, 1), 5000f));
			lights.Add(new Light(new Vector3(-10, -10, 1), 15000f));

			// lol
			objects[0].Colour = new Vector3(0.1f, 0, 0);
			objects[0].Reflectivity = 0.8f;

			objects[0].Pos = new Vector3(-1, 1, 2);
			objects[1].Pos = new Vector3(1, 1, 2);

		}

		public World(int screenWidth, int screenHeight, Vector3 backgroundColour) : this(screenWidth, screenHeight)
		{
			BackgroundColour = backgroundColour;
		}

		private Vector3 CastRay(Ray ray, int depth = 0)
		{
			(float i1, float i2) minIntersection = (float.MaxValue, float.MaxValue);
			IIntersectable minObj = null;

			// find closest intersection if it exists
			foreach (IIntersectable obj in objects)
			{
				(float i1, float i2) intersection = obj.Intersect(ray);
				if (intersection.i1 > 0 && intersection.i1 < minIntersection.i1)
				{
					minIntersection = intersection;
					minObj = obj;
				}
			}

			if (minObj == null)
				if (depth == 0)
					return BackgroundColour;
				else
					return new Vector3(0);

			Vector3 iPoint = ray.ToPoint(minIntersection.i1);
			Vector3 iNormal = minObj.CalcNormal(iPoint, true);

			Vector3 pointCol = minObj.Colour * minObj.Ambient;


			foreach (Light light in lights)
			{
				// cast shadow ray for each object. if collide, shadow
				Vector3 il = (light.Pos - iPoint).Normalized();
				Ray ilRay = new Ray(iPoint + iNormal, il);

				bool shadow = false;
				foreach (IIntersectable obj in objects)
				{
					if (obj.Intersect(ilRay).i1 > 0)
					{
						shadow = true;
						break;
					}
				}

				// calculate actual lighting if not in shadow
				// based on https://en.wikipedia.org/wiki/Phong_reflection_model and https://en.wikipedia.org/wiki/Blinn%E2%80%93Phong_reflection_model
				if (!shadow)
				{
					float angleCoeff = Math.Max(0f, Vector3.Dot(il, iNormal));
					Vector3 diffuse = minObj.Colour * angleCoeff * light.Brightness * minObj.Diffuse;

					Vector3 lv = il - ray.Dir;
					Vector3 h = lv / lv.Length;
					float nh = Math.Max(0f, Vector3.Dot(iNormal, h));
					Vector3 specular = minObj.SpecularColour * (float)Math.Pow(nh, minObj.Shininess) * minObj.Specular;

					pointCol += diffuse * specular;

				}

				// handle reflective materials
				if (minObj.Reflectivity > 0 && depth < 5)
				{
					Vector3 rVec = (il - 2 * Vector3.Dot(il, iNormal) * iNormal).Normalized();

					Ray subRay = new Ray(iPoint + iNormal, rVec);
					Vector3 rColour = CastRay(subRay, depth + 1);

					pointCol += rColour * minObj.Reflectivity;

				}

			}

			return pointCol;

		}

		private double i = 0;

		public void RenderWorld(double delta)
		{
			i += delta;

			// vertical rotation
			lights[0].Pos = new Vector3
			{
				X = 10 * (float)Math.Cos(2 * i),
				Y = 0,
				Z = 10 * (float)Math.Sin(2 * i)
			};

			// horizontal rotation
			lights[1].Pos = new Vector3
			{
				X = 0,
				Y = 10 * (float)Math.Sin(i),
				Z = 10 * (float)Math.Cos(i)
			};


			float aspectRatio = (float)ScreenWidth / ScreenHeight;

			var e = new CountdownEvent(World.vertices.Length);

			for (int y = 0; y < ScreenHeight; y++)
			{
				for (int x = 0; x < ScreenWidth; x++)
				{

					float yMapped = ((y + 0.5f) / ScreenHeight) * 2 - 1;
					float xMapped = (((x + 0.5f) / ScreenWidth) * 2 - 1) * aspectRatio;

					Vector3 ov = Vector3.Transform(new Vector3(xMapped, yMapped, 1), CamRot);
					Ray ray = new Ray(CamPos, ov.Normalized());

					ThreadPool.QueueUserWorkItem(new WaitCallback(state => {
						var colour = CastRay(ray).Clamp(0f, 1f);

						lock (locker)
						{
							World.vertices[(int)state].r = colour.X;
							World.vertices[(int)state].g = colour.Y;
							World.vertices[(int)state].b = colour.Z;

							e.Signal();
						}

					}), y * ScreenWidth + x);

				}
			}

			e.Wait();

		}


	}
}
