using Godot;
using Foundation;
using System;

namespace Movement
{
    public partial class Mover : Node
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; } = Vector2.Zero;
        public Vector2 Acceleration { get; set; } = Vector2.Zero;
        public float NormalSpeed { get; } = 0.6f;
        private float SpeedTimes { get; set; } = 1;
        public float CurrentSpeed { get; set; }
        public float MaxForce { get; set; } = 0.02f;
        public float WanderRange { get; set; } = 0.02f;
        public float? WanderTheta { get; set; } = null;

        //test containment
        public Vector2 ViewPortSize { get; set; }
        public Mover(Vector2 position, Vector2 viewportSize) 
        {
            this.Position = position;
            this.WanderTheta = (float)GD.RandRange(Position.Angle(), Velocity.Angle());
            this.ViewPortSize = viewportSize;
        }
        public void Update(Vector2? target, bool hungerStats)
        {
            this.Velocity += Acceleration;
            this.Position += Velocity;
            this.Acceleration = Vector2.Zero;
            this.CurrentSpeed = NormalSpeed * SpeedTimes;
            if(OverShootViewport(0))
            {
                RemoveWanderTheta();
                Containment(0);
            }
            else
            {
                BehaviourSelect(target, hungerStats);
            }
        }
        public Vector2 ProjectDirection(float distance)
        {
            Vector2 projectedDirection = Position + Velocity;
            Vector2 unitVec = Joint.GetUnitVector(Position, projectedDirection);
            return Position + unitVec * distance;
        }
        public void Seek(Vector2 target)
        {
            Vector2 desired = Joint.GetUnitVector(Position, target) * CurrentSpeed;
            Vector2 steering = Joint.GetUnitVector(Velocity, desired) * MaxForce;
            this.Acceleration += steering;
        }
        private void SeekAndArrive(Vector2 target)
        {
            float distance = (target - Position).Length();
            float radius = 100;
            Vector2 desired = Joint.GetUnitVector(Position, target);
            if(distance < radius)
            {
                float decelerate = Mathf.Remap(distance, 0, radius, 0, NormalSpeed);
                desired *= decelerate;
            }
            else
            {
                desired *= NormalSpeed;
            }
            Vector2 steering = Joint.GetUnitVector(Velocity, desired) * MaxForce;
            this.Acceleration += steering;
            Seek(desired);
        }
        public void Flee(Vector2 target)
        {
            Seek(target * -1);
        }
        private void Wander()
        {
            //project velocity at a certain distance
            Vector2 wanderPoint = ProjectDirection(100);
            //create a radius circle around wanderPoint
            float wanderRadius = 50;
            //create an offset point / Vector on the wander radius
            float theta = WanderTheta.Value + Joint.GetUnitVector(Velocity, Position).Angle();
            float x = MathF.Cos(theta) * wanderRadius;
            float y = MathF.Sin(theta) * wanderRadius;
            wanderPoint += new Vector2(x, y);
            //apply steering force
            Seek(wanderPoint);
            //randomize wander theta to get the random moving direction
            this.WanderTheta += (float) GD.RandRange(-WanderRange, WanderRange);
        }
        private void BehaviourSelect(Vector2? target, bool hungry)
        {
            if (target.HasValue && hungry)
            {
                RemoveWanderTheta();
                this.SpeedTimes = 5;
                Seek(target.Value);
            }
            else
            {
                SetWanderTheta();
                this.SpeedTimes = 1;
                Wander();
            }
        }
        private void Containment(float offset)
        {
            Vector2? desired = null;
            if(this.Position.X < offset)
            {
                desired = new(this.CurrentSpeed, this.Position.Y);
            }
            else if(this.Position.X > ViewPortSize.X - offset)
            {
                desired = new(-this.CurrentSpeed, this.Position.Y);
            }
            if(this.Position.Y < offset)
            {
                desired = new(this.Position.X, this.CurrentSpeed);
            }
            else if (this.Position.Y > ViewPortSize.Y - offset) 
            {
                desired = new(this.Position.X, -this.CurrentSpeed);
            }
            //apply force
            if(desired.HasValue)
            {
                Seek(desired.Value);
            }
        }
        private bool OverShootViewport(float offset)
        {
            return Position.X < offset || Position.X > ViewPortSize.X - offset || Position.Y < offset || Position.Y > ViewPortSize.Y - offset;
        }
        private void RemoveWanderTheta()
        {
            this.WanderTheta = null;
        }
        private void SetWanderTheta()
        {
            if (WanderTheta == null)
            {
                this.WanderTheta = Velocity.Angle();
            }
        }
    }
}
