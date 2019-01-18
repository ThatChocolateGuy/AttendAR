using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DreamStarGen.Algorithms
{
    public class BasicAlgorithm : MonoBehaviour
    {
        public float Impact = 1;
        public float Angle_MP = 1;

        public virtual void Initialize(DreamStarGenerator_Mixer generator)
        {

        }

        public virtual Vector3 StarAlgorithm(float Angle, DreamStarGenerator_Mixer generator)
        {

            return new Vector3();

        }



    }
}