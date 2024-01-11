#ifndef CYLINDERMAPPING_CGINC
#define CYLINDERMAPPING_CGINC

float Base_Width;
float Cylinder_Depth;
float Cylinder_Angle;
float Cylinder_Radius;

float4 MapCoordinate(float4 coord)
{
	float theta = (coord.x / Base_Width) * Cylinder_Angle;
	float radius = Cylinder_Radius * Base_Width;
	float depth = Cylinder_Depth * Base_Width;
	
	coord.x = sin(theta) * radius;
	coord.z = (cos(theta) * radius) + depth;
	
	return coord;
}

#endif
