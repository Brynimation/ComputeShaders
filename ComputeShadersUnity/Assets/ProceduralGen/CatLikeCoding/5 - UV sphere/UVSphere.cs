using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace ProceduralMeshes
{
    //We're creating a unit sphere (ie, a sphere with a radius of 1)
    public struct UVSphere : IMeshGenerator
    {
        public int Resolution { get; set; }
        int ResolutionV => 2 * Resolution;
        int ResolutionU => 4 * Resolution;

        public Bounds Bounds => new Bounds(Vector3.zero, (new Vector3(2f, 2f, 2f))); //radius of 1 = diameter of 2 = bounding volume with length 2

        //A grid with R quads using shared vertices will contain R + 1 vertices.
        public int VertexCount => (ResolutionU + 1) * (ResolutionV + 1);

        public int IndexCount => 6 * (ResolutionU) * (ResolutionV);

        public int jobLength => ResolutionU + 1;

        //The plane that we transform into a sphere will be on the xy plane instead of the xz plane. Note that this is a parametric plane and will
        //no longer be strictly aligned with the world axes. We'll use u and v instead of z and x
        //For our uv sphere, we will create columns from bottom to top instead of stacked rows. Our range is also 0-1 instead of -1/2 to 1/2
        //To turn our grid to a cylinder, we calculate the x, y and z coords of our actual positions as functions of our parametric coordinates, u and v
        //            float xPos = sin(2 * PI * uCoord / Resolution); //x starts at zero
        //            float yPos = 0;
        //            float zPos = -cos(2 * PI * uCoord / Resolution);//z starts at -1
        //To go from cylinder to sphere, we have to squeeze our cylinder at the top and bottom to produce the north
        //and south poles of our sphere. Our horizontal vertex rings no longner all have the same radius, except for
        //the poles where radius is zero
        public void Execute<S>(int uCoord, S streams) where S : struct, IMeshStreams
        {
            int vertexIndex = (ResolutionV + 1) * uCoord; //Index of the first vertex of the row current row
            int triangleIndex = 2 * ResolutionV * (uCoord - 1);


            //For our uv sphere, we will create columns from bottom to top instead of stacked rows. Our range is also 0-1 instead of -1/2 to 1/2
            
            //We set our first (south pole) and last (north pole) vertices here to avoid texture twisting at the poles
            float uPos = (float)uCoord / ResolutionU;
            float uUV = (float)(uCoord - 0.5f) / ResolutionU; //We subtract 0.5 to rotate the texture coordinate by half a step for the first vertex

            float xPos = 0;//sin(2 * PI * uCoord / Resolution); //x starts at zero
            float yPos = -1;
            float zPos = 0;// -cos(2 * PI * uCoord / Resolution);//z starts at -1

            float3 pos0 = float3(xPos, yPos, zPos);
            
            float2 uv0 = float2(uUV, 0.0f);
            float3 norm0 = pos0;

            float4 tan = float4(0, 0, 0, -1);
            tan.x = cos(2f * PI * (uCoord - 0.5f) / ResolutionU);
            tan.z= sin(2f * PI * (uCoord - 0.5f) / ResolutionU);

            streams.SetVertex(vertexIndex, new Vertex(pos0, norm0, tan, uv0));
            pos0.y = norm0.y = 1f;
            uv0.y = 1f;
            streams.SetVertex(vertexIndex + ResolutionV, new Vertex(pos0, norm0, tan, uv0));
            vertexIndex +=1;
            float2 circle = float2(sin(2f * PI * uPos), cos(2f * PI * uPos));
            tan.xz = circle.yx;
            circle.y *= -1;


            //strictly less than ResolutionV as we've already set the north pole
            for (int vCoord = 1; vCoord < ResolutionV; vCoord++, vertexIndex++, triangleIndex += 2)
            {
                //Set vertices
                float circleRadius = sin(PI * (float) vCoord / ResolutionV);
                xPos = circle.x * circleRadius;
                yPos = -cos(PI * (float) vCoord / ResolutionV);
                zPos = circle.y * circleRadius;
                float3 pos = float3(xPos, yPos, zPos);
                float3 norm = pos;
                float2 uv = float2(uUV, (float)vCoord / ResolutionV);

                streams.SetVertex(vertexIndex, new Vertex(pos, norm, tan, uv));
                //Set triangles
                //Triangle indices are shared and they are made relative to the top-right vertex
                //This means: vertex index offset for the top right index is 0, top left is -1, bottom right is res-1 and bottom left is res-2
                if (uCoord > 0) //This check ensures that we don't try to generate quads for the bottom row of vertices 
                {
                    streams.SetTriangle(triangleIndex, vertexIndex + int3(-ResolutionV - 2, -ResolutionV -1, -1));
                    streams.SetTriangle(triangleIndex + 1, vertexIndex + int3(-1, -ResolutionV -1, 0));
                }


            }
            //Set the final quad for the north pole
            if (uCoord > 0) //This check ensures that we don't try to generate quads for the bottom row of vertices 
            {
                streams.SetTriangle(triangleIndex, vertexIndex + int3(-ResolutionV - 2, 0, -1));
                streams.SetTriangle(triangleIndex + 1, vertexIndex + int3(-1, -ResolutionV - 1, 0));
            }

        }
    }
}

