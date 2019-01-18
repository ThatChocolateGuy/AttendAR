using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DreamStarGen
{
    public class DreamStarGenerator_Crazy : DreamStarGenerator
    {



        public override Vector3 StarAlgorithm(float Angle)
        {
            float r = 0;

            float radiant = Angle * a;
            float alternator = Mathf.Cos(radiant / (b / 1000));


            float value1 = 1 * alternator;


            r = Radius * (value1);

            float x = r * Mathf.Cos(Angle);
            float y = r * Mathf.Sin(Angle);
            return new Vector3(x, y, 0);
        }
    }
}