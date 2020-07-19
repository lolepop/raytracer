using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace another_raytracer
{
	interface IIntersectable
	{
		Vector3 Pos { get; set; }

		Vector3 Colour { get; set; }
		Vector3 SpecularColour { get; set; }

		// phong reflection coefficients
		float Ambient { get; set; }
		float Diffuse { get; set; }
		float Specular { get; set; }
		float Shininess { get; set; }

		(float i1, float i2) Intersect(Ray ray);
		Vector3 CalcNormal(Vector3 intersection, bool bias = false);
	}

	class Ray
	{
		public Vector3 Pos { get; set; }
		public Vector3 Dir { get; set; }

		public Ray(Vector3 pos, Vector3 dir)
		{
			Pos = pos;
			Dir = dir;
		}

		public Vector3 ToPoint(float dist) => Pos + Dir * dist;

	}

	class Light
	{
		public Vector3 Pos { get; set; }
		public float Brightness { get; set; }

		public Light(Vector3 pos, float brightness)
		{
			Pos = pos;
			Brightness = brightness;
		}

	}

	class Sphere : IIntersectable
	{
		public Vector3 Pos { get; set; }
		public float Radius { get; set; }

		public Vector3 Colour { get; set; } = new Vector3(0, .5f, 0);
		public Vector3 SpecularColour { get; set; } = new Vector3(1, 1, 1);

		public float Ambient { get; set; } = .2f;
		public float Diffuse { get; set; } = 1000f;
		public float Specular { get; set; } = 10f;
		public float Shininess { get; set; } = 1.05f;

		public Sphere(Vector3 pos, float radius)
		{
			Pos = pos;
			Radius = radius;
		}

		public Sphere(Vector3 pos, float radius, Vector3 colour, Vector3 specularColour, float ambient, float diffuse, float specular, float shininess) : this(pos, radius)
		{
			Colour = colour;
			SpecularColour = specularColour;
			Ambient = ambient;
			Diffuse = diffuse;
			Specular = specular;
			Shininess = shininess;
		}

		public (float i1, float i2) Intersect(Ray ray)
		{
			Vector3 os = Pos - ray.Pos;

			float iBisect = Vector3.Dot(ray.Dir, os);
			if (iBisect < 0) return (-1, -1);

			double distsq = os.LengthSquared - Math.Pow(iBisect, 2);
			float dist = (float)Math.Sqrt(distsq);
			if (dist > Radius) return (-1, -1);

			float halfbisect = (float)Math.Sqrt(Math.Pow(Radius, 2) - distsq);

			return (iBisect - halfbisect, iBisect + halfbisect);

		}

		// bias idea from https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/ligth-and-shadows
		public Vector3 CalcNormal(Vector3 intersection, bool bias = false) => (intersection - Pos).Normalized() * (bias ? 1e-4f : 1);

	}
}
