﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace another_raytracer
{
	class Shaders
	{
		public static string vertexShader = @"
			#version 330 core
			layout(location = 0) in vec2 position;
			layout(location = 1) in vec3 acolour;

			out vec3 colour;

			void main()
			{
				gl_Position = vec4(position, 0.0, 1.0);
				colour = acolour;
			}
		";

		public static string fragmentShader = @"
			#version 330 core
			in vec3 colour;
			out vec4 FragColor;

			void main()
			{
				FragColor = vec4(colour, 1.0f);
			}
		";

	}

}
