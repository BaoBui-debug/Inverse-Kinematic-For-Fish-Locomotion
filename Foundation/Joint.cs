using Godot;
using System.Collections.Generic;

namespace Foundation 
{
    public partial class Joint
    {
        public Vector2 Head { get; set; }
        public Vector2 Middle { get; set; }
        public Vector2 Tail { get; set; }
        public float Length { get; set; }
        public Color Color { get; set; }
        public float Thickness { get; set; }
        public Joint(Vector2 head, float jointLength, Color color, float thickness)
        {
            this.Head = head;
            this.Tail = new(head.X - jointLength, head.Y);
            this.Middle = (Head + Tail) / 2;
            this.Length = jointLength;
            this.Color = color;
            this.Thickness = thickness;
        }
        public void MoveTo(Vector2 destination)
        {
            Vector2 newPos = Tail - destination;
            float distant = newPos.Length();
            float scale = Length / distant;
            Vector2 newHeadPos = destination;
            Vector2 newTailPos = destination + newPos * scale;
            this.Head = newHeadPos;
            this.Tail = newTailPos;
            this.Middle = (newHeadPos + newTailPos) / 2;
        }
        public Vector2 CalRestPos()
        {
            Vector2 direction = GetUnitVector(Head, Tail);
            Vector2 desirePos = Tail + direction * Length;
            return GetUnitVector(Tail, desirePos);
        }
        public void Align(Vector2 restPosition, float rigidNess = 1)
        {
            this.Tail += restPosition * rigidNess;
        }
        public float GetThisAngle()
        {
            return GetUnitVector(Tail, Head).Angle();
        }
        //--   STATIC METHODs   --
        public static Vector2 GetUnitVector(Vector2 from, Vector2 to)
        {
            return (to - from).Normalized();
        }
        public static List<Joint> GenerateJoints(int numberOfJoints, Vector2 position, float length, Color color, float thickness)
        {
            List<Joint> result = new(numberOfJoints);
            for (int i = 0; i < numberOfJoints; i++)
            {
                result.Add(new(position, length, color, thickness));
                position = new(position.X - length, position.Y);
            }
            return result;
        }
    }
}
