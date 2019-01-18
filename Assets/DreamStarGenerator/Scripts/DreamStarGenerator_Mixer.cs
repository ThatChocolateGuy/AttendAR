using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DreamStarGen.Algorithms;

namespace DreamStarGen
{
    [RequireComponent(typeof(MeshFilter))]
    public class DreamStarGenerator_Mixer : DreamStarGenerator
    {

        public AnimationCurve Curve_A;
        public AnimationCurve Curve_B;
        public AnimationCurve Curve_C;

        [HideInInspector]
        public BasicAlgorithm[] Algorithm;


        public override void Initialize()
        {
            Algorithm = GetComponents<BasicAlgorithm>();

            if (Algorithm == null) Algorithm = new BasicAlgorithm[0];

            for (int i = 0; i < Algorithm.Length; i++)
            {
                if (Algorithm[i] != null) Algorithm[i].Initialize(this);
            }
        }

        public override Vector3 StarAlgorithm(float Angle)
        {
            Vector3 output = new Vector3();
            for (int i = 0; i < Algorithm.Length; i++)
            {
                if (Algorithm[i] != null)
                {
                    output += Algorithm[i].StarAlgorithm(Angle, this);
                }
            }


            return output;
        }







    }
}