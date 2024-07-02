using Godot;
using System;
using System.Collections.Generic;
using Foundation;

namespace Movement
{
    public partial class FIKSA
    {
        public List<Joint> Segments;
        public Mover Motion;
        private Joint Rot;
        private float Amplitude = 0.08f;   //org: 0.1f
        private float Frequency = 0.04f;   //org: 0.06f
        private float Angle = 0;
        public FIKSA(int numberOfSegments, Vector2 initPosition, Vector2 viewportSize)
        {
            this.Motion = new Mover(initPosition, viewportSize);
            this.Rot = new(Motion.Position, 2, Colors.Red, 2);
            this.Segments = Joint.GenerateJoints(numberOfSegments, Rot.Tail, 20, Colors.White, 5);
        }
        public void Init(Vector2? target, float sensitivity, bool isHungry)
        {
            this.Motion.Update(target, isHungry);
            this.Rot.MoveTo(Motion.Position);
            ApplyLocomotion(sensitivity);
            AlignSegments();
        }
        public List<Joint> GetSegments()
        {
            return this.Segments;
        }
        public Joint GetRot()
        {
            return this.Rot;
        } 
        private void AlignSegments()
        {
            Vector2 target = Rot.Tail;
            Segments[0].Align(Rot.CalRestPos());
            for (int i = 0; i < Segments.Count; i++)
            {
                Segments[i].MoveTo(target);
                if (i != Segments.Count - 1)
                {
                    target = Segments[i].Tail;
                    Segments[i + 1].Align(Segments[i].CalRestPos(), 3);
                }
            }
        }
        private void ApplyLocomotion(float sensitivity)
        {
            float distance = (Segments[0].Head - Segments[1].Tail).Length();
            if (distance > (Segments[0].Length + Segments[1].Length) - sensitivity)
            {
                Swimming();
            }
        }
        private void Swimming()
        {
            float x = MathF.Cos(Angle) * Amplitude;
            float y = MathF.Sin(Angle) * Amplitude;
            Vector2 Rot = new(x, y);
            Angle += Frequency;
            this.Rot.Align(Rot);
            //Increase the number of swaying joint as the speed grow  
            if(Motion.CurrentSpeed != Motion.NormalSpeed)
            {
                this.Amplitude = 0.35f;
                this.Frequency = 0.08f;
                this.Segments[0].Align(Rot);
                this.Segments[1].Align(Rot);
                this.Segments[2].Align(Rot);
                this.Segments[3].Align(Rot);
            }
            else
            {
                this.Amplitude = 0.08f;
                this.Frequency = 0.04f;
            }
        }
    }
}
