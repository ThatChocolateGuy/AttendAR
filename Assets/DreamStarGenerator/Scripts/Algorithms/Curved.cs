using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DreamStarGen.Algorithms
{
    public class Curved : BasicAlgorithm
    {
     

        public override void Initialize(DreamStarGenerator_Mixer generator)
        {
            if (generator. b < 0) generator.b = 0;
        }

        public override Vector3 StarAlgorithm(float Angle, DreamStarGenerator_Mixer generator)
        {
            Angle *= Angle_MP;

            float r = 0;
            float radiant = Angle * Mathf.Deg2Rad * generator.a;
            float value1 = 1;
            r = generator.Radius * (value1);

            float x = r * Mathf.Cos(radiant);
            float y = r * Mathf.Sin(radiant);

            float curvevalue = generator.Curve_A.Evaluate(Angle * generator.b);

            float powcurvevalue = Mathf.Pow(curvevalue, generator.c);


            return new Vector3(x, y, 0) * powcurvevalue * Impact;
        }


    }
}