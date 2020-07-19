using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace another_raytracer
{
	public static class Util
	{
		public static float Map(float curr, float aStart, float aEnd, float bStart, float bEnd) => (curr - aStart) / (aEnd - aStart) * (bEnd - bStart) + bStart;

		public static float Clamp(float curr, float min, float max) => Math.Min(max, Math.Max(min, curr));
		public static Vector3 Clamp(this Vector3 v, float min, float max)
		{
			return new Vector3(
				Clamp(v.X, min, max),
				Clamp(v.Y, min, max),
				Clamp(v.Z, min, max)
			);
		}
	}
}
