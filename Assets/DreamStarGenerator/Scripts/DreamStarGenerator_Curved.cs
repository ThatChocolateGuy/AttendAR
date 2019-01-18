using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DreamStarGen
{
    public class DreamStarGenerator_Curved : DreamStarGenerator
    {

        public AnimationCurve Curve;

        public override void Initialize()
        {
            if (b < 0) b = 0;
        }

        public override Vector3 StarAlgorithm(float Angle)
        {
            float r = 0;
            float radiant = Angle * Mathf.Deg2Rad * a;
            float value1 = 1;
            r = Radius * (value1);

            float x = r * Mathf.Cos(radiant);
            float y = r * Mathf.Sin(radiant);

            float curvevalue = Curve.Evaluate(Angle * b);

            float powcurvevalue = Mathf.Pow(curvevalue, c);


            return new Vector3(x, y, 0) * powcurvevalue;
        }
    }
}