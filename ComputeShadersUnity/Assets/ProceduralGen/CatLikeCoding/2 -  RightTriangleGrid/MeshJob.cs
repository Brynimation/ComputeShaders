using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

namespace ProceduralMeshes
{

    /*Here, we define a burst job that will be used to generate meshes. It is responsible for passing the
     appropriate data streams to the appropriate generator*/
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct MeshJob<G, S> : IJobFor
        where G : struct, IMeshGenerator
        where S : struct, IMeshStreams
    {
        private G generator;
        [WriteOnly] //we only write to the streams when generating the mesh
        private S streams;

        public void Execute(int index)
        {
            generator.Execute(index, streams);
        }

        /*This method creates and schedules a job, returning its handle. As parameters, it will take mesh data and a JobDependency.
         This is a bit like a weird constructor.*/
        public static JobHandle ScheduleParallel(Mesh mesh, int resolution, Mesh.MeshData data, JobHandle dependency)
        {
            MeshJob<G, S> job = new MeshJob<G, S>();
            job.generator.Resolution = resolution;
            mesh.bounds = job.generator.Bounds;
            job.streams.Setup(data, job.generator.Bounds, job.generator.VertexCount, job.generator.IndexCount);
            return job.ScheduleParallel(job.generator.jobLength, resolution, dependency);
        }
    }

    //This delegate (a function that can be passed as a parameter to other functions) will allow us to select variants of MeshGenerators in the inspector
    public delegate JobHandle MeshJobScheduleDelegate(Mesh mesh, int resolution, Mesh.MeshData data, JobHandle dependency);
}
