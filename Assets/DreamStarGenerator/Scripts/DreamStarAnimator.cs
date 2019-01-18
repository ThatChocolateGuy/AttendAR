using UnityEngine;
using System.Collections;

namespace DreamStarGen
{
    public class DreamStarAnimator : MonoBehaviour
    {

        public DreamStarGenerator Generator;

        public float Speed;

        [Range(0, 30)]
        public int FrameSkip = 0;


        [Header("Speed Multiplier of Star Variables (A-E)")]
        public float Change_A = 0;
        public float Change_B = 0;
        public float Change_C = 0;
        public float Change_D = 0;
        public float Change_E = 0;

        private int frame = 0;

        void Start()
        {
            if (!Generator) Generator = GetComponent<DreamStarGenerator>();
        }

        void OnDrawGizmosSelected()
        {
            if (!Generator) Generator = GetComponent<DreamStarGenerator>();
        }

        void FixedUpdate()
        {
            if (!Generator) return;


            Generator.a += Speed * Change_A;
            Generator.b += Speed * Change_B;
            Generator.c += Speed * Change_C;
            Generator.d += Speed * Change_D;
            Generator.e += Speed * Change_E;

            if (frame > 0)
            {
                frame--;
                return;
            }
            frame = FrameSkip;
            Generator._GenerateStar();
        }
    }
}