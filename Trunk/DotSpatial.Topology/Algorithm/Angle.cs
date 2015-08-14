// ********************************************************************************************************
// Product Name: DotSpatial.Topology.dll
// Description:  The basic topology module for the new dotSpatial libraries
// ********************************************************************************************************
// The contents of this file are subject to the Lesser GNU Public License (LGPL)
// you may not use this file except in compliance with the License. You may obtain a copy of the License at
// http://dotspatial.codeplex.com/license  Alternately, you can access an earlier version of this content from
// the Net Topology Suite, which is also protected by the GNU Lesser Public License and the sourcecode
// for the Net Topology Suite can be obtained here: http://sourceforge.net/projects/nts.
//
// Software distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTY OF
// ANY KIND, either expressed or implied. See the License for the specific language governing rights and
// limitations under the License.
//
// The Original Code is from the Net Topology Suite, which is a C# port of the Java Topology Suite.
//
// The Initial Developer to integrate this code into MapWindow 6.0 is Ted Dunsford.
//
// Contributor(s): (Open source contributors should list themselves and their modifications here).
// |         Name         |    Date    |                              Comment
// |----------------------|------------|------------------------------------------------------------
// |                      |            |
// ********************************************************************************************************

using System;
using DotSpatial.Topology.Geometries;

namespace DotSpatial.Topology.Algorithm
{
    /// <summary>
    /// Angle orientation
    /// </summary>
    public enum Orientation
    {
        ///<summary>Constant representing no orientation</summary>
        None = CgAlgorithms.Collinear,

        ///<summary>Constant representing straight orientation</summary>
        Straight = None,

        ///<summary>Constant representing counterclockwise orientation</summary>
        CounterClockwise = CgAlgorithms.CounterClockwise,

        ///<summary>Constant representing left orientation</summary>
        Left = CounterClockwise,

        ///<summary>Constant representing clockwise orientation</summary>
        Clockwise = CgAlgorithms.Clockwise,

        ///<summary>Constant representing right orientation</summary>
        Right = Clockwise
    }

    ///<summary>
    /// Utility functions for working with angles.
    /// Unless otherwise noted, methods in this class express angles in radians.
    /// </summary>
    public static class AngleUtility
    {
        #region Constant Fields

        public const double PiOver2 = Math.PI / 2.0;
        public const double PiOver4 = Math.PI / 4.0;
        public const double PiTimes2 = 2.0 * Math.PI;

        #endregion

        #region Methods

        ///<summary>
        /// Returns the angle of the vector from p0 to p1, relative to the positive X-axis.
        /// </summary>
        /// <remarks>The angle is normalized to be in the range [ -Pi, Pi ].</remarks>
        /// <param name="p0">The start-point</param>
        /// <param name="p1">The end-point</param>
        /// <returns>The normalized angle (in radians) that p0-p1 makes with the positive X-axis</returns>
        public static double Angle(Coordinate p0, Coordinate p1)
        {
            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;
            return Math.Atan2(dy, dx);
        }

        ///<summary>
        /// Returns the angle that the vector from (0,0) to p, relative to the positive X-axis.
        ///</summary>
        /// <remarks>
        /// The angle is normalized to be in the range ( -Pi, Pi ].
        /// </remarks>
        /// <param name="p">The point</param>
        /// <returns>The normalized angle (in radians) that (0,0)-p makes with the positive X-axis.</returns>
        public static double Angle(Coordinate p)
        {
            return Math.Atan2(p.Y, p.X);
        }

        ///<summary>
        /// Returns the unoriented smallest angle between two vectors.
        ///</summary>
        /// <remarks>
        /// The computed angle will be in the range [0, Pi).
        /// </remarks>
        /// <param name="tip1">The tip of one vector</param>
        /// <param name="tail">The tail of each vector</param>
        /// <param name="tip2">The tip of the other vector</param>
        public static double AngleBetween(Coordinate tip1, Coordinate tail, Coordinate tip2)
        {
            double a1 = Angle(tail, tip1);
            double a2 = Angle(tail, tip2);

            return Diff(a1, a2);
        }

        /// <summary>
        /// Returns the oriented smallest angle between two vectors.
        /// The computed angle will be in the range (-Pi, Pi].
        /// A positive result corresponds to a <see cref="Orientation.CounterClockwise"/> rotation (CCW) from v1 to v2;
        /// a negative result corresponds to a <see cref="Orientation.Clockwise"/> (CW) rotation;
        /// a zero result corresponds to no rotation.
        /// </summary>
        /// <param name="tip1">The tip of v1</param>
        /// <param name="tail">The tail of each vector</param>
        /// <param name="tip2">The tip of v2</param>
        /// <returns>The angle between v1 and v2, relative to v1</returns>
        public static double AngleBetweenOriented(Coordinate tip1, Coordinate tail, Coordinate tip2)
        {
            double a1 = Angle(tail, tip1);
            double a2 = Angle(tail, tip2);
            double angDel = a2 - a1;

            // normalize, maintaining orientation
            if (angDel <= -Math.PI)
                return angDel + PiTimes2;
            if (angDel > Math.PI)
                return angDel - PiTimes2;
            return angDel;
        }

        ///<summary>
        /// Computes the unoriented smallest difference between two angles.
        ///</summary>
        ///<remarks>
        /// <list type="Bulltet">
        /// <item>The angles are assumed to be normalized to the range [-Pi, Pi].</item>
        /// <item>The result will be in the range [0, Pi].</item>
        /// </list>
        /// </remarks>
        /// <param name="ang1">The angle of one vector (in [-Pi, Pi] )</param>
        /// <param name="ang2">The angle of the other vector (in range [-Pi, Pi] )</param>
        /// <returns>The angle (in radians) between the two vectors (in range [0, Pi] )</returns>
        public static double Diff(double ang1, double ang2)
        {
            double delAngle;

            if (ang1 < ang2)
            {
                delAngle = ang2 - ang1;
            }
            else
            {
                delAngle = ang1 - ang2;
            }

            if (delAngle > Math.PI)
            {
                delAngle = (2 * Math.PI) - delAngle;
            }

            return delAngle;
        }

        ///<summary>
        /// Returns whether an angle must turn clockwise or counterclockwise to overlap another angle.
        ///</summary>
        /// <param name="ang1">An angle (in radians)</param>
        /// <param name="ang2">An angle (in radians)</param>
        /// <returns>Whether a1 must turn <see cref="Orientation.Clockwise"/>, <see cref="Orientation.CounterClockwise"/> or <see cref="Orientation.None"/> to overlap a2.</returns>
        public static Orientation GetTurn(double ang1, double ang2)
        {
            double crossproduct = Math.Sin(ang2 - ang1);

            if (crossproduct > 0)
            {
                return Orientation.CounterClockwise;
            }

            if (crossproduct < 0)
            {
                return Orientation.Clockwise;
            }

            return Orientation.None;
        }

        ///<summary>
        /// Computes the interior angle between two segments of a ring.
        /// The ring is assumed to be oriented in a clockwise direction.
        ///</summary>
        /// <remarks>The computed angle will be in the range [0, 2Pi]</remarks>
        /// <param name="p0">A point of the ring</param>
        /// <param name="p1">The next point of the ring</param>
        /// <param name="p2">The next point of the ring</param>
        /// <returns>The interior angle based at <see paramref="p1"/>p1</returns>
        public static double InteriorAngle(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            double anglePrev = Angle(p1, p0);
            double angleNext = Angle(p1, p2);
            return Math.Abs(angleNext - anglePrev);
        }

        ///<summary>
        /// Tests whether the angle between p0-p1-p2 is acute.
        ///</summary>
        /// <remarks>
        /// <para>An angle is acute if it is less than 90 degrees.</para>
        /// <para>Note: this implementation is not precise (deterministic) for angles very close to 90 degrees.</para>    
        /// </remarks>
        /// <param name="p0">An endpoint of the angle</param>
        /// <param name="p1">The base of the angle</param>
        /// <param name="p2">Another endpoint of the angle</param>
        public static Boolean IsAcute(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            // relies on fact that A dot B is positive iff A ang B is acute
            double dx0 = p0.X - p1.X;
            double dy0 = p0.Y - p1.Y;
            double dx1 = p2.X - p1.X;
            double dy1 = p2.Y - p1.Y;
            double dotprod = dx0 * dx1 + dy0 * dy1;
            return dotprod > 0;
        }

        ///<summary>
        /// Tests whether the angle between p0-p1-p2 is obtuse
        ///</summary>
        /// <remarks>
        /// <para>An angle is obtuse if it is greater than 90 degrees.</para>
        /// <para>Note: this implementation is not precise (deterministic) for angles very close to 90 degrees.</para>    
        /// </remarks>
        /// <param name="p0">An endpoint of the angle</param>
        /// <param name="p1">The base of the angle</param>
        /// <param name="p2">Another endpoint of the angle</param>
        public static Boolean IsObtuse(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            // relies on fact that A dot B is negative iff A ang B is obtuse
            double dx0 = p0.X - p1.X;
            double dy0 = p0.Y - p1.Y;
            double dx1 = p2.X - p1.X;
            double dy1 = p2.Y - p1.Y;
            double dotprod = dx0 * dx1 + dy0 * dy1;
            return dotprod < 0;
        }

        ///<summary>
        /// Computes the normalized value of an angle, which is the equivalent angle in the range ( -Pi, Pi ].
        ///</summary>
        /// <param name="angle">The angle to normalize</param>
        /// <returns>An equivalent angle in the range (-Pi, Pi]</returns>
        public static double Normalize(double angle)
        {
            while (angle > Math.PI)
                angle -= PiTimes2;
            while (angle <= -Math.PI)
                angle += PiTimes2;
            return angle;
        }

        ///<summary>
        /// Computes the normalized positive value of an angle, which is the equivalent angle in the range [ 0, 2*Pi ).
        /// <para>
        /// E.g.
        /// <list>
        /// <item></item>
        /// <item>NormalizePositive(0.0) = 0.0</item>
        /// <item>NormalizePositive(-PI) = <see cref="System.Math.PI"/></item>
        /// <item>NormalizePositive(-2PI) = 0.0</item>
        /// <item>NormalizePositive(-3PI) = <see cref="System.Math.PI"/></item>
        /// <item>NormalizePositive(-4PI) = 0</item>
        /// <item>NormalizePositive(PI) = <see cref="System.Math.PI"/></item>
        /// <item>NormalizePositive(2PI) = 0.0</item>
        /// <item>NormalizePositive(3PI) = <see cref="System.Math.PI"/></item>
        /// <item>NormalizePositive(4PI) = 0.0</item>
        /// </list>
        /// </para>
        ///</summary>
        /// <remarks></remarks>
        /// <param name="angle">The angle to normalize, in radians.</param>
        /// <returns>An equivalent positive angle</returns>
        public static double NormalizePositive(double angle)
        {
            if (angle < 0.0)
            {
                while (angle < 0.0)
                    angle += PiTimes2;
                // in case round-off error bumps the value over 
                if (angle >= PiTimes2)
                    angle = 0.0;
            }
            else
            {
                while (angle >= PiTimes2)
                    angle -= PiTimes2;
                // in case round-off error bumps the value under 
                if (angle < 0.0)
                    angle = 0.0;
            }
            return angle;
        }

        ///<summary>
        /// Converts from radians to degrees.
        ///</summary>
        /// <param name="radians">An angle in radians</param>
        /// <returns>The angle in degrees</returns>
        public static double ToDegrees(double radians)
        {
            return (radians * 180) / (Math.PI);
        }

        ///<summary>
        /// Converts from degrees to radians.
        ///</summary>
        /// <param name="angleDegrees">An angle in degrees</param>
        /// <returns>The angle in radians</returns>
        public static double ToRadians(double angleDegrees)
        {
            return (angleDegrees * Math.PI) / 180.0;
        }

        #endregion
    }
}