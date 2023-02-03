using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
{
    public class Line
    {
        public double m { get; private set; }
        public double b { get; private set; }

        public Vector2_64 start { get; private set; }
        public Vector2_64 end { get; private set; }

        public Line(Line l)
        {
            start = l.start;
            end = l.end;
            CalcLine();
        }
        public Line(Vector2_64 start, Vector2_64 end)
        {
            this.start = start;
            this.end = end;
            CalcLine();
        }
        public Line(double x1, double y1, double x2, double y2)
        {
            start = new Vector2_64(x1, y1);
            end = new Vector2_64(x2, y2);
            CalcLine();
        }

        public void SetStartPoint(Vector2_64 point)
        {
            start = point;
            CalcLine();
        }
        public void SetEndPoint(Vector2_64 point)
        {
            end = point;
            CalcLine();
        }

        void CalcLine()
        {
            if(start.X > end.X)
            {
                Vector2_64 temp = end;
                end = start;
                start = temp;
            }
            m = GetMValue(start, end);
            b = GetBValue(start, end);
        }

        /// <summary>
        ///  Returns if two lines, defined by points, intersect.
        /// </summary>
        /// <param name="p1">Start of first line</param>
        /// <param name="p2">End of first line</param>
        /// <param name="p3">Start of second line</param>
        /// <param name="p4">end of second line</param>
        /// <param name="intersectPoint">The point of intersection. <para>Will be (0, 0) if this returns false.</para>Will be p1 if the two lines are identical. </param>
        /// <returns></returns>
        public static bool IntersectBoundless(Vector2_64 p1, Vector2_64 p2, Vector2_64 p3, Vector2_64 p4, out Vector2_64 intersectPoint)
        {
            Line line1 = new Line(p1, p2);
            Line line2 = new Line(p3, p4);

            if (line1.m == line2.m)
            {
                if (line1.b == line2.b)
                {
                    intersectPoint = p1;
                    return true;
                }
                intersectPoint = Vector2_64.Zero;
                return false;
            }

            double x = (line2.b - line1.b) / (line1.m - line2.m);
            double y = line1.GetPoint(x);
            intersectPoint = new Vector2_64(x, y);
            return true;
        }

        /// <summary>
        /// Returns the m value of a line (Y = mx + b)
        /// </summary>
        public static double GetMValue(Vector2_64 p1, Vector2_64 p2)
        {
            return (p2.Y - p1.Y) / (p2.X - p1.X);
        }
        // y = mx + b
        // y - mx = b
        public static double GetBValue(Vector2_64 p1, Vector2_64 p2)
        {
            double m = GetMValue(p1, p2);

            // In the event that p1.x
            if (p1.X > p2.X)
            {
                m = -m;
                return p2.Y - (m * p2.X);
            }
            return p2.Y - (m * p2.X);
        }

        public double GetPoint(double x)
        {
            return m * x + b;
        }
    }
}
