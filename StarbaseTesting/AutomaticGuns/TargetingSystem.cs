using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameEngine;

namespace StarbaseTesting.AutomaticGuns
{
    class MovingObject
    {
        public Vector2_64 pos { get; set; }
        public Vector2_64 dir { get; set; }
        public double speed { get; set; }
        public string name { get; set; }
    }

    class TargetingSystem
    {
        public float bulletSpeed { get; set; }
        List<MovingObject> trackedObjects = new List<MovingObject>();

        public void AddTrackedObject(MovingObject obj)
        {
            trackedObjects.Add(obj);
        }

        private double SmallestPos(double a, double b, double c)
        {
            double t1 = Mathc.QuadraticFormula(a, b, c, false);
            double t2 = Mathc.QuadraticFormula(a, b, c, true);

            if (!double.IsNaN(t1))
            {
                if (!double.IsNaN(t2))
                {
                    if (t1 < 0)
                    {
                        // Return t2 if it is positive
                        if (t2 > 0)
                            return t2;
                        // No positive answer
                        return 0;
                    }

                    // Return t1 since it is the only positive
                    if (t2 < 0)
                        return t1;
                    // Return t1 if it's the lowest of both positive answers
                    if (t1 < t2)
                        return t1;
                    // Return t2 if it's the lowest of both positive answers
                    return t2;
                }

                // Return t1 if it is positive
                return (t1 >= 0) ? t1 : 0;
            }
            // Return t2 if it is positive
            else if (!double.IsNaN(t2))
                return (t2 >= 0) ? t2 : 0;
            return 0;
        }

        public string ComputeLeadingAngles(MovingObject obj)
        {
            StringBuilder sb = new StringBuilder($"Bullet Speed: {bulletSpeed}\n");
            MovingObject normObj = new MovingObject()
            {
                pos = obj.pos.Normal(),
                dir = obj.dir,
                speed = obj.speed / obj.pos.Magnitude,
                name = $"{obj.name} (Normalized)"
            };

            ComputeLeadingAnglesFromPos(obj, sb);
            ComputeLeadingAnglesFromPosMod(obj, sb);
            ComputeLeadingAnglesActual(obj, sb);
            ComputeLeadingAnglesFromAng(obj, sb);
            ComputeLeadingAnglesFromPerp(obj, sb);
            return sb.ToString();
        }

        private string ComputeLeadingAnglesFromPos(MovingObject obj, StringBuilder sb)
        {
            Vector2_64 aimDir = new Vector2_64(0, 0);
            Vector2_64 shipPos = new Vector2_64(0, 0);

            sb.Append($"\n{obj.name}:");
            sb.Append($"\n\tPos: {obj.pos}");
            sb.Append($"\n\tDir: {obj.dir}");
            sb.Append($"\n\tSpeed: {obj.speed}");

            double a = Math.Pow(obj.dir.X, 2) + Math.Pow(obj.dir.Y, 2) - Math.Pow(bulletSpeed, 2);
            double b = 2 * ((obj.pos.X * obj.dir.X) + (obj.pos.Y * obj.dir.Y) - (shipPos.X * obj.dir.X) - (shipPos.Y * obj.dir.Y));
            double c = Math.Pow(obj.pos.X, 2) + Math.Pow(obj.pos.Y, 2) + Math.Pow(shipPos.X, 2) + Math.Pow(shipPos.Y, 2) - (2 * shipPos.X * obj.pos.X) - (2 * shipPos.Y * obj.pos.Y);

            double t = SmallestPos(a, b, c);

            if (t == 0)
            {
                sb.Append($"\n\tCannot hit target.");
                return sb.ToString();
            }

            aimDir.X = (obj.pos.X - shipPos.X + (t * obj.speed * obj.dir.X)) / (t * bulletSpeed);
            aimDir.Y = (obj.pos.Y - shipPos.Y + (t * obj.speed * obj.dir.Y)) / (t * bulletSpeed);

            sb.Append($"\n\tImpact Time: {t}");
            sb.Append($"\n\tImpact Point: {t * bulletSpeed * aimDir}");
            sb.Append($"\n\tLeading Dir: {aimDir}");
            sb.Append($"\n\tLeading Angle: {aimDir.AngleDegrees}");

            return sb.ToString();
        }
        private string ComputeLeadingAnglesFromPosMod(MovingObject obj, StringBuilder sb)
        {
            Vector2_64 aimDir = new Vector2_64(0, 0);

            sb.Append($"\n{obj.name} (Assumed 0,0):");
            sb.Append($"\n\tPos: {obj.pos}");
            sb.Append($"\n\tDir: {obj.dir}");
            sb.Append($"\n\tSpeed: {obj.speed}");

            double a = Math.Pow(obj.dir.X, 2) + Math.Pow(obj.dir.Y, 2) - Math.Pow(bulletSpeed, 2);
            double b = 2 * ((obj.pos.X * obj.dir.X) + (obj.pos.Y * obj.dir.Y));
            double c = Math.Pow(obj.pos.X, 2) + Math.Pow(obj.pos.Y, 2);

            double t = SmallestPos(a, b, c);

            if (t == 0)
            {
                sb.Append($"\n\tCannot hit target.");
                return sb.ToString();
            }

            aimDir.X = (obj.pos.X + (t * obj.speed * obj.dir.X)) / (t * bulletSpeed);
            aimDir.Y = (obj.pos.Y + (t * obj.speed * obj.dir.Y)) / (t * bulletSpeed);

            sb.Append($"\n\tImpact Time: {t}");
            sb.Append($"\n\tImpact Point: {t * bulletSpeed * aimDir}");
            sb.Append($"\n\tLeading Dir: {aimDir}");
            sb.Append($"\n\tLeading Angle: {aimDir.AngleDegrees}");

            return sb.ToString();
        }

        private string ComputeLeadingAnglesActual(MovingObject obj, StringBuilder sb)
        {
            Vector2_64 aimDir = new Vector2_64(0, 0);
            Vector2_64 secPrevPos = (obj.speed * -obj.dir) + obj.pos;

            // Initial/last sighting
            double angle1 = secPrevPos.AngleRadians;
            double distance1 = secPrevPos.Magnitude;

            // Most recent sighting
            double angle2 = obj.pos.AngleRadians;
            double distance2 = obj.pos.Magnitude;

            Vector2_64 tPos1 = Vector2_64.FromAngle(angle1) * distance1; // Previous position of target
            Vector2_64 tPos2 = Vector2_64.FromAngle(angle2) * distance2; // Current position of target
            Vector2_64 tDir = (tPos2 - tPos1).Normal(); // Direction target is moving

            double speed = (tPos2 - tPos1).Magnitude;


            sb.Append($"\n{obj.name} (Reconstructed):");
            sb.Append($"\n\tPos: {tPos2} [Act: {obj.pos}]");
            sb.Append($"\n\tDir: {tDir} [Act: {obj.dir}]");
            sb.Append($"\n\tSpeed: {obj.speed} [Act: {obj.speed}]");

            double a = Math.Pow(tDir.X, 2) + Math.Pow(tDir.Y, 2) - Math.Pow(bulletSpeed, 2);
            double b = 2 * ((tPos2.X * tDir.X) + (tPos2.Y * tDir.Y));
            double c = Math.Pow(tPos2.X, 2) + Math.Pow(tPos2.Y, 2);

            double t = SmallestPos(a, b, c);

            if (t == 0)
            {
                sb.Append($"\n\tCannot hit target.");
                return sb.ToString();
            }

            aimDir.X = (tPos2.X + (t * obj.speed * tDir.X)) / (t * bulletSpeed);
            aimDir.Y = (tPos2.Y + (t * obj.speed * tDir.Y)) / (t * bulletSpeed);
            aimDir.Normalize();

            sb.Append($"\n\tImpact Time: {t}");
            sb.Append($"\n\tImpact Point: {t * bulletSpeed * aimDir}");
            sb.Append($"\n\tLeading Dir: {aimDir}");
            sb.Append($"\n\tLeading Angle: {aimDir.AngleDegrees}");

            return sb.ToString();
        }

        private string ComputeLeadingAnglesFromAng(MovingObject obj, StringBuilder sb)
        {
            Vector2_64 aimDir = new Vector2_64(0, 0);
            Vector2_64 shipPos = new Vector2_64(0, 0);

            // Get the angles
            double observedAng = obj.pos.AngleRadians;
            double observedAng2 = ((obj.speed * obj.dir) + obj.pos).AngleRadians;

            // We assume that it is travelling in a straight line
            Vector2_64 tposFromAng = Vector2_64.FromAngle(observedAng);
            Vector2_64 secLaterRad = Vector2_64.FromAngle(observedAng2);
            Vector2_64 observedDir = (secLaterRad - tposFromAng);
            double observedSpeed = observedDir.Magnitude;
            observedDir.Normalize();

            sb.Append($"\n{obj.name} (From Observed Angle):");
            sb.Append($"\n\tPos: {tposFromAng}");
            sb.Append($"\n\tDir: {observedDir}");
            sb.Append($"\n\tSpeed: {observedSpeed}");

            double a = Math.Pow(observedDir.X, 2) + Math.Pow(observedDir.Y, 2) - Math.Pow(bulletSpeed, 2);
            double b = 2 * ((tposFromAng.X * observedDir.X) + (tposFromAng.Y * observedDir.Y) - (shipPos.X * observedDir.X) - (shipPos.Y * observedDir.Y));
            double c = Math.Pow(tposFromAng.X, 2) + Math.Pow(tposFromAng.Y, 2) + Math.Pow(shipPos.X, 2) + Math.Pow(shipPos.Y, 2) - (2 * shipPos.X * tposFromAng.X) - (2 * shipPos.Y - tposFromAng.Y);

            double t = SmallestPos(a, b, c);

            if (t == 0)
            {
                sb.Append($"\n\tCannot hit target.");
                return sb.ToString();
            }

            aimDir.X = (tposFromAng.X - shipPos.X + (t * observedSpeed * observedDir.X)) / (t * bulletSpeed);
            aimDir.Y = (tposFromAng.Y - shipPos.Y + (t * observedSpeed * observedDir.Y)) / (t * bulletSpeed);

            sb.Append($"\n\tImpact Time: {t}");
            sb.Append($"\n\tImpact Point: {t * bulletSpeed * aimDir}");
            sb.Append($"\n\tLeading Dir: {aimDir}");
            sb.Append($"\n\tLeading Angle: {aimDir.AngleDegrees}");
            return sb.ToString();
        }

        private string ComputeLeadingAnglesFromPerp(MovingObject obj, StringBuilder sb)
        {
            Vector2_64 aimDir = new Vector2_64(0, 0);
            Vector2_64 shipPos = new Vector2_64(0, 0);

            // We assume that it is travelling in a straight line
            Vector2_64 tposFromAng = Vector2_64.FromAngle(obj.pos.AngleRadians);
            Vector2_64 secLaterPos = Vector2_64.FromAngle(((obj.speed * obj.dir) + obj.pos).AngleRadians);
            Vector2_64 perpDir = tposFromAng.BestPerp(secLaterPos) + tposFromAng;

            Vector2_64 intersectPoint = new Vector2_64();
            Line.IntersectBoundless(shipPos, secLaterPos, tposFromAng, perpDir, out intersectPoint);

            Vector2_64 observedDir = (intersectPoint - tposFromAng);

            double observedSpeed = observedDir.Magnitude;
            observedDir.Normalize();



            sb.Append($"\n{obj.name} (Perpindicular Intersect):");
            sb.Append($"\n\tPos: {tposFromAng}");
            sb.Append($"\n\tDir: {observedDir}");
            sb.Append($"\n\tSpeed: {observedSpeed}");

            double a = Math.Pow(observedDir.X, 2) + Math.Pow(observedDir.Y, 2) - Math.Pow(bulletSpeed, 2);
            double b = 2 * ((tposFromAng.X * observedDir.X) + (tposFromAng.Y * observedDir.Y) - (shipPos.X * observedDir.X) - (shipPos.Y * observedDir.Y));
            double c = Math.Pow(tposFromAng.X, 2) + Math.Pow(tposFromAng.Y, 2) + Math.Pow(shipPos.X, 2) + Math.Pow(shipPos.Y, 2) - (2 * shipPos.X * tposFromAng.X) - (2 * shipPos.Y - tposFromAng.Y);

            double t = SmallestPos(a, b, c);

            if (t == 0)
            {
                sb.Append($"\n\tCannot hit target.");
                return sb.ToString();
            }

            aimDir.X = (tposFromAng.X - shipPos.X + (t * observedSpeed * observedDir.X)) / (t * bulletSpeed);
            aimDir.Y = (tposFromAng.Y - shipPos.Y + (t * observedSpeed * observedDir.Y)) / (t * bulletSpeed);

            sb.Append($"\n\tImpact Time: {t}");
            sb.Append($"\n\tImpact Point: {t * bulletSpeed * aimDir}");
            sb.Append($"\n\tLeading Dir: {aimDir}");
            sb.Append($"\n\tLeading Angle: {aimDir.AngleDegrees}");
            return sb.ToString();
        }
    }
}
