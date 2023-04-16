using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*This interface defines the behaviour of an object that takes care of mesh generation. It defines the code that will be executed by a 
 job. It therefore needs an Execute method with an index parameter. It must also receive a streams parameter for storage. This type of this
struct will be generic; any struct that implements IMeshStreams*/
namespace ProceduralMeshes {
    public interface IMeshGenerator
    {
        int Resolution { get; set;} //defines the width and height of our grid
        Bounds Bounds { get; }
        int VertexCount {get;}

        int IndexCount {get;}

        //The length of a job must also be known when scheduling it
        int jobLength { get; }
        void Execute<S>(int index, S streams) where S : struct, IMeshStreams;
    
    }
}

