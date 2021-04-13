using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TankGame
{
    public static class ModelExtends
    {
        public static Vector3 ToUnityVector(this DataModel.Vector3 vector)
        {
            return new Vector3
            {
                x = vector.X,
                y = vector.Y,
                z = vector.Z
            };
        }
    }


}
