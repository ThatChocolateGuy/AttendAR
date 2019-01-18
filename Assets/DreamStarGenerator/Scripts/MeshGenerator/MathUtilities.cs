using UnityEngine;
using System.Collections;

namespace DreamStarGen
{
    public static class MathUtilities
    {

        /// <summary>
        /// Checks, if the distance of 2 Vectors is smaller than a given Lenght 
        /// </summary>
        /// <param name="start">Startpoint</param>
        /// <param name="end">Endpoint</param>
        /// <param name="Lenght">The Value to Compare</param>
        /// <returns></returns>
        public static bool IsInRange(Vector2 start, Vector2 end, float Lenght)
        {
            Vector2 dist = start - end;
            if (dist.sqrMagnitude < Lenght * Lenght) return true;

            return false;

        }

        /// <summary>
        /// Returns position on UnitCircle
        /// </summary>
        /// <param name="Winkel">Angle in degree</param>
        /// <returns></returns>
        public static Vector2 kreisPosition(float Angle)
        {
            while (Angle > 360)
            {
                Angle -= 360;
            }

            Vector2 output = new Vector2();
            output.y = Mathf.Sin(Angle * Mathf.PI / 180);
            output.x = Mathf.Cos(Angle * Mathf.PI / 180);
            return output;
        }

        public static Vector3 spherePosition(float AngleX, float AngleY)
        {
            AngleX *= Mathf.Deg2Rad;
            AngleY *= Mathf.Deg2Rad;

            Vector3 output = new Vector3();
            output.z = Mathf.Cos(AngleX) * Mathf.Sin(AngleY);
            output.x = Mathf.Sin(AngleX) * Mathf.Sin(AngleY);
            output.y = Mathf.Cos(AngleY);

            return output;
        }

    }
}