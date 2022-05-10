using UnityEngine;

namespace Buck
{
	/// <summary>
	/// Basic collection of easing functions
	/// </summary>
	public static class Easings
	{
		/// <summary>
		/// Constant Pi.
		/// </summary>
		private const float PI = Mathf.PI;

		/// <summary>
		/// Constant Pi / 2.
		/// </summary>
		private const float HALFPI = Mathf.PI / 2.0f;

		/// <summary>
		/// Easing Functions enumeration
		/// </summary>
		public enum Functions
		{
			Linear,
			QuadraticEaseIn,
			QuadraticEaseOut,
			QuadraticEaseInOut,
			CubicEaseIn,
			CubicEaseOut,
			CubicEaseInOut,
			QuarticEaseIn,
			QuarticEaseOut,
			QuarticEaseInOut,
			QuinticEaseIn,
			QuinticEaseOut,
			QuinticEaseInOut,
			SineEaseIn,
			SineEaseOut,
			SineEaseInOut,
			CircularEaseIn,
			CircularEaseOut,
			CircularEaseInOut,
			ExponentialEaseIn,
			ExponentialEaseOut,
			ExponentialEaseInOut,
			ElasticEaseIn,
			ElasticEaseOut,
			ElasticEaseInOut,
			BackEaseIn,
			BackEaseOut,
			BackEaseInOut,
			BounceEaseIn,
			BounceEaseOut,
			BounceEaseInOut
		}

		/// <summary>
		/// Interpolate using the specified function.
		/// </summary>
		static public float Interpolate(float p, Functions function)
		{
			switch (function)
			{
				default:
				case Functions.Linear: return Linear(p);
				case Functions.QuadraticEaseOut: return QuadraticEaseOut(p);
				case Functions.QuadraticEaseIn: return QuadraticEaseIn(p);
				case Functions.QuadraticEaseInOut: return QuadraticEaseInOut(p);
				case Functions.CubicEaseIn: return CubicEaseIn(p);
				case Functions.CubicEaseOut: return CubicEaseOut(p);
				case Functions.CubicEaseInOut: return CubicEaseInOut(p);
				case Functions.QuarticEaseIn: return QuarticEaseIn(p);
				case Functions.QuarticEaseOut: return QuarticEaseOut(p);
				case Functions.QuarticEaseInOut: return QuarticEaseInOut(p);
				case Functions.QuinticEaseIn: return QuinticEaseIn(p);
				case Functions.QuinticEaseOut: return QuinticEaseOut(p);
				case Functions.QuinticEaseInOut: return QuinticEaseInOut(p);
				case Functions.SineEaseIn: return SineEaseIn(p);
				case Functions.SineEaseOut: return SineEaseOut(p);
				case Functions.SineEaseInOut: return SineEaseInOut(p);
				case Functions.CircularEaseIn: return CircularEaseIn(p);
				case Functions.CircularEaseOut: return CircularEaseOut(p);
				case Functions.CircularEaseInOut: return CircularEaseInOut(p);
				case Functions.ExponentialEaseIn: return ExponentialEaseIn(p);
				case Functions.ExponentialEaseOut: return ExponentialEaseOut(p);
				case Functions.ExponentialEaseInOut: return ExponentialEaseInOut(p);
				case Functions.ElasticEaseIn: return ElasticEaseIn(p);
				case Functions.ElasticEaseOut: return ElasticEaseOut(p);
				case Functions.ElasticEaseInOut: return ElasticEaseInOut(p);
				case Functions.BackEaseIn: return BackEaseIn(p);
				case Functions.BackEaseOut: return BackEaseOut(p);
				case Functions.BackEaseInOut: return BackEaseInOut(p);
				case Functions.BounceEaseIn: return BounceEaseIn(p);
				case Functions.BounceEaseOut: return BounceEaseOut(p);
				case Functions.BounceEaseInOut: return BounceEaseInOut(p);
			}
		}

		/// <summary>
		/// Modeled after the line y = x
		/// </summary>
		static public float Linear(float p)
		{
			return p;
		}

		/// <summary>
		/// Modeled after the parabola y = x^2
		/// </summary>
		static public float QuadraticEaseIn(float p)
		{
			return p * p;
		}

		/// <summary>
		/// Modeled after the parabola y = -x^2 + 2x
		/// </summary>
		static public float QuadraticEaseOut(float p)
		{
			return -(p * (p - 2));
		}

		/// <summary>
		/// Modeled after the piecewise quadratic
		/// y = (1/2)((2x)^2)             ; [0, 0.5)
		/// y = -(1/2)((2x-1)*(2x-3) - 1) ; [0.5, 1]
		/// </summary>
		static public float QuadraticEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				return 2 * p * p;
			}
			else
			{
				return (-2 * p * p) + (4 * p) - 1;
			}
		}

		/// <summary>
		/// Modeled after the cubic y = x^3
		/// </summary>
		static public float CubicEaseIn(float p)
		{
			return p * p * p;
		}

		/// <summary>
		/// Modeled after the cubic y = (x - 1)^3 + 1
		/// </summary>
		static public float CubicEaseOut(float p)
		{
			float f = (p - 1);
			return f * f * f + 1;
		}

		/// <summary>	
		/// Modeled after the piecewise cubic
		/// y = (1/2)((2x)^3)       ; [0, 0.5)
		/// y = (1/2)((2x-2)^3 + 2) ; [0.5, 1]
		/// </summary>
		static public float CubicEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				return 4 * p * p * p;
			}
			else
			{
				float f = ((2 * p) - 2);
				return 0.5f * f * f * f + 1;
			}
		}

		/// <summary>
		/// Modeled after the quartic x^4
		/// </summary>
		static public float QuarticEaseIn(float p)
		{
			return p * p * p * p;
		}

		/// <summary>
		/// Modeled after the quartic y = 1 - (x - 1)^4
		/// </summary>
		static public float QuarticEaseOut(float p)
		{
			float f = (p - 1);
			return f * f * f * (1 - p) + 1;
		}

		/// <summary>
		// Modeled after the piecewise quartic
		// y = (1/2)((2x)^4)        ; [0, 0.5)
		// y = -(1/2)((2x-2)^4 - 2) ; [0.5, 1]
		/// </summary>
		static public float QuarticEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				return 8 * p * p * p * p;
			}
			else
			{
				float f = (p - 1);
				return -8 * f * f * f * f + 1;
			}
		}

		/// <summary>
		/// Modeled after the quintic y = x^5
		/// </summary>
		static public float QuinticEaseIn(float p)
		{
			return p * p * p * p * p;
		}

		/// <summary>
		/// Modeled after the quintic y = (x - 1)^5 + 1
		/// </summary>
		static public float QuinticEaseOut(float p)
		{
			float f = (p - 1);
			return f * f * f * f * f + 1;
		}

		/// <summary>
		/// Modeled after the piecewise quintic
		/// y = (1/2)((2x)^5)       ; [0, 0.5)
		/// y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
		/// </summary>
		static public float QuinticEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				return 16 * p * p * p * p * p;
			}
			else
			{
				float f = ((2 * p) - 2);
				return 0.5f * f * f * f * f * f + 1;
			}
		}

		/// <summary>
		/// Modeled after quarter-cycle of sine wave
		/// </summary>
		static public float SineEaseIn(float p)
		{
			return Mathf.Sin((p - 1) * HALFPI) + 1;
		}

		/// <summary>
		/// Modeled after quarter-cycle of sine wave (different phase)
		/// </summary>
		static public float SineEaseOut(float p)
		{
			return Mathf.Sin(p * HALFPI);
		}

		/// <summary>
		/// Modeled after half sine wave
		/// </summary>
		static public float SineEaseInOut(float p)
		{
			return 0.5f * (1 - Mathf.Cos(p * PI));
		}

		/// <summary>
		/// Modeled after shifted quadrant IV of unit circle
		/// </summary>
		static public float CircularEaseIn(float p)
		{
			return 1 - Mathf.Sqrt(1 - (p * p));
		}

		/// <summary>
		/// Modeled after shifted quadrant II of unit circle
		/// </summary>
		static public float CircularEaseOut(float p)
		{
			return Mathf.Sqrt((2 - p) * p);
		}

		/// <summary>	
		/// Modeled after the piecewise circular function
		/// y = (1/2)(1 - Mathf.Sqrt(1 - 4x^2))           ; [0, 0.5)
		/// y = (1/2)(Mathf.Sqrt(-(2x - 3)*(2x - 1)) + 1) ; [0.5, 1]
		/// </summary>
		static public float CircularEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				return 0.5f * (1 - Mathf.Sqrt(1 - 4 * (p * p)));
			}
			else
			{
				return 0.5f * (Mathf.Sqrt(-((2 * p) - 3) * ((2 * p) - 1)) + 1);
			}
		}

		/// <summary>
		/// Modeled after the exponential function y = 2^(10(x - 1))
		/// </summary>
		static public float ExponentialEaseIn(float p)
		{
			return (p == 0.0f) ? p : Mathf.Pow(2, 10 * (p - 1));
		}

		/// <summary>
		/// Modeled after the exponential function y = -2^(-10x) + 1
		/// </summary>
		static public float ExponentialEaseOut(float p)
		{
			return (p == 1.0f) ? p : 1 - Mathf.Pow(2, -10 * p);
		}

		/// <summary>
		/// Modeled after the piecewise exponential
		/// y = (1/2)2^(10(2x - 1))         ; [0,0.5)
		/// y = -(1/2)*2^(-10(2x - 1))) + 1 ; [0.5,1]
		/// </summary>
		static public float ExponentialEaseInOut(float p)
		{
			if (p == 0.0 || p == 1.0) return p;

			if (p < 0.5f)
			{
				return 0.5f * Mathf.Pow(2, (20 * p) - 10);
			}
			else
			{
				return -0.5f * Mathf.Pow(2, (-20 * p) + 10) + 1;
			}
		}

		/// <summary>
		/// Modeled after the damped sine wave y = sin(13pi/2*x)*Mathf.Pow(2, 10 * (x - 1))
		/// </summary>
		static public float ElasticEaseIn(float p)
		{
			return Mathf.Sin(13 * HALFPI * p) * Mathf.Pow(2, 10 * (p - 1));
		}

		/// <summary>
		/// Modeled after the damped sine wave y = sin(-13pi/2*(x + 1))*Mathf.Pow(2, -10x) + 1
		/// </summary>
		static public float ElasticEaseOut(float p)
		{
			return Mathf.Sin(-13 * HALFPI * (p + 1)) * Mathf.Pow(2, -10 * p) + 1;
		}

		/// <summary>
		/// Modeled after the piecewise exponentially-damped sine wave:
		/// y = (1/2)*sin(13pi/2*(2*x))*Mathf.Pow(2, 10 * ((2*x) - 1))      ; [0,0.5)
		/// y = (1/2)*(sin(-13pi/2*((2x-1)+1))*Mathf.Pow(2,-10(2*x-1)) + 2) ; [0.5, 1]
		/// </summary>
		static public float ElasticEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				return 0.5f * Mathf.Sin(13 * HALFPI * (2 * p)) * Mathf.Pow(2, 10 * ((2 * p) - 1));
			}
			else
			{
				return 0.5f * (Mathf.Sin(-13 * HALFPI * ((2 * p - 1) + 1)) * Mathf.Pow(2, -10 * (2 * p - 1)) + 2);
			}
		}

		/// <summary>
		/// Modeled after the overshooting cubic y = x^3-x*sin(x*pi)
		/// </summary>
		static public float BackEaseIn(float p)
		{
			return p * p * p - p * Mathf.Sin(p * PI);
		}

		/// <summary>
		/// Modeled after overshooting cubic y = 1-((1-x)^3-(1-x)*sin((1-x)*pi))
		/// </summary>	
		static public float BackEaseOut(float p)
		{
			float f = (1 - p);
			return 1 - (f * f * f - f * Mathf.Sin(f * PI));
		}

		/// <summary>
		/// Modeled after the piecewise overshooting cubic function:
		/// y = (1/2)*((2x)^3-(2x)*sin(2*x*pi))           ; [0, 0.5)
		/// y = (1/2)*(1-((1-x)^3-(1-x)*sin((1-x)*pi))+1) ; [0.5, 1]
		/// </summary>
		static public float BackEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				float f = 2 * p;
				return 0.5f * (f * f * f - f * Mathf.Sin(f * PI));
			}
			else
			{
				float f = (1 - (2 * p - 1));
				return 0.5f * (1 - (f * f * f - f * Mathf.Sin(f * PI))) + 0.5f;
			}
		}

		/// <summary>
		/// </summary>
		static public float BounceEaseIn(float p)
		{
			return 1 - BounceEaseOut(1 - p);
		}

		/// <summary>
		/// </summary>
		static public float BounceEaseOut(float p)
		{
			if (p < 4 / 11.0f)
			{
				return (121 * p * p) / 16.0f;
			}
			else if (p < 8 / 11.0f)
			{
				return (363 / 40.0f * p * p) - (99 / 10.0f * p) + 17 / 5.0f;
			}
			else if (p < 9 / 10.0f)
			{
				return (4356 / 361.0f * p * p) - (35442 / 1805.0f * p) + 16061 / 1805.0f;
			}
			else
			{
				return (54 / 5.0f * p * p) - (513 / 25.0f * p) + 268 / 25.0f;
			}
		}

		/// <summary>
		/// </summary>
		static public float BounceEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				return 0.5f * BounceEaseIn(p * 2);
			}
			else
			{
				return 0.5f * BounceEaseOut(p * 2 - 1) + 0.5f;
			}
		}
	}
}
 	
