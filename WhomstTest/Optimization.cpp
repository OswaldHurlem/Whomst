// poly34.cpp : solution of cubic and quartic equation
// (c) Khashin S.I. http://math.ivanovo.ac.ru/dalgebra/Khashin/index.html
// khash2 (at) gmail.com
// Thanks to Alexandr Rakhmanin <rakhmanin (at) gmail.com>
// public domain
//
#include <math.h>

#include "poly34.h"		   // solution of cubic and quartic equation
#define	TwoPi  6.28318530717958648
const double eps = 1e-14;

//=============================================================================
// _root3, root3 from http://prografix.narod.ru
//=============================================================================
inline double _root3(double x)
{
	double s = 1.;
	while (x < 1.)
	{
		x *= 8.;
		s *= 0.5;
	}
	while (x > 8.)
	{
		x *= 0.125;
		s *= 2.;
	}
	double r = 1.5;
	r -= 1. / 3. * (r - x / (r * r));
	r -= 1. / 3. * (r - x / (r * r));
	r -= 1. / 3. * (r - x / (r * r));
	r -= 1. / 3. * (r - x / (r * r));
	r -= 1. / 3. * (r - x / (r * r));
	r -= 1. / 3. * (r - x / (r * r));
	return r * s;
}

inline double root3(double x)
{
	if (x > 0) return _root3(x); else
		if (x < 0) return-_root3(-x); else
			return 0.;
}

//---------------------------------------------------------------------------
// x - array of size 3
// In case 3 real roots: => x[0], x[1], x[2], return 3
//         2 real roots: x[0], x[1],          return 2
//         1 real root : x[0], x[1] � i*x[2], return 1
int SolveP3(double *x, double a, double b, double c)
{
	/*{{ }}*/
	//{} HASH: D41D8CD98F00B204E9800998ECF8427E
	// solve cubic equation x^3 + a*x^2 + b*x + c = 0
	double a2 = a*a;
	double q = (a2 - 3 * b) / 9;
	double r = (a*(2 * a2 - 9 * b) + 27 * c) / 54;
	// equation x^3 + q*x + r = 0
	double r2 = r*r;
	double q3 = q*q*q;
	double A, B;
	if (r2 <= (q3 + eps))
	{
		double t = r / sqrt(q3);
		if (t<-1) t = -1;
		if (t> 1) t = 1;
		t = acos(t);
		a /= 3; q = -2 * sqrt(q);
		x[0] = q*cos(t / 3) - a;
		x[1] = q*cos((t + TwoPi) / 3) - a;
		x[2] = q*cos((t - TwoPi) / 3) - a;
		return(3);
	}
	else
	{
		A = -root3(fabs(r) + sqrt(r2 - q3));
		if (r<0) A = -A;
		B = A == 0 ? 0 : B = q / A;

		a /= 3;
		x[0] = (A + B) - a;
		x[1] = -0.5*(A + B) - a;
		x[2] = 0.5*sqrt(3.)*(A - B);
		if (fabs(x[2])<eps) { x[2] = x[1]; return(2); }
		return(1);
	}
	/*{{ var cubicCode = PrevContent; }}*/
	//{} HASH: D41D8CD98F00B204E9800998ECF8427E
}

int SolveP3_A1(double *x, double b, double c)
{
	const double a = 1.0;
	/*{{
		cubicCode
	}}*/
		// solve cubic equation x^3 + a*x^2 + b*x + c = 0
		double a2 = a*a;
		double q = (a2 - 3 * b) / 9;
		double r = (a*(2 * a2 - 9 * b) + 27 * c) / 54;
		// equation x^3 + q*x + r = 0
		double r2 = r*r;
		double q3 = q*q*q;
		double A, B;
		if (r2 <= (q3 + eps))
		{
			double t = r / sqrt(q3);
			if (t<-1) t = -1;
			if (t> 1) t = 1;
			t = acos(t);
			a /= 3; q = -2 * sqrt(q);
			x[0] = q*cos(t / 3) - a;
			x[1] = q*cos((t + TwoPi) / 3) - a;
			x[2] = q*cos((t - TwoPi) / 3) - a;
			return(3);
		}
		else
		{
			A = -root3(fabs(r) + sqrt(r2 - q3));
			if (r<0) A = -A;
			B = A == 0 ? 0 : B = q / A;
	
			a /= 3;
			x[0] = (A + B) - a;
			x[1] = -0.5*(A + B) - a;
			x[2] = 0.5*sqrt(3.)*(A - B);
			if (fabs(x[2])<eps) { x[2] = x[1]; return(2); }
			return(1);
		}
	//{} HASH: 4275D475C3E4B9D599A33BF6BFBE7D71
}

int SolveP3_C0(double *x, double a, double b)
{
	const double c = 0.0;
	/*{{
		cubicCode
	}}*/
		// solve cubic equation x^3 + a*x^2 + b*x + c = 0
		double a2 = a*a;
		double q = (a2 - 3 * b) / 9;
		double r = (a*(2 * a2 - 9 * b) + 27 * c) / 54;
		// equation x^3 + q*x + r = 0
		double r2 = r*r;
		double q3 = q*q*q;
		double A, B;
		if (r2 <= (q3 + eps))
		{
			double t = r / sqrt(q3);
			if (t<-1) t = -1;
			if (t> 1) t = 1;
			t = acos(t);
			a /= 3; q = -2 * sqrt(q);
			x[0] = q*cos(t / 3) - a;
			x[1] = q*cos((t + TwoPi) / 3) - a;
			x[2] = q*cos((t - TwoPi) / 3) - a;
			return(3);
		}
		else
		{
			A = -root3(fabs(r) + sqrt(r2 - q3));
			if (r<0) A = -A;
			B = A == 0 ? 0 : B = q / A;
	
			a /= 3;
			x[0] = (A + B) - a;
			x[1] = -0.5*(A + B) - a;
			x[2] = 0.5*sqrt(3.)*(A - B);
			if (fabs(x[2])<eps) { x[2] = x[1]; return(2); }
			return(1);
		}
	//{} HASH: 4275D475C3E4B9D599A33BF6BFBE7D71
}

//!! How you sometimes do it in C
//!! (or with templates)
int SolveP3_A2(double *x, double b, double c)
{
	const double a = 2.0;
	#include "cubeRootCode.cpp"
}