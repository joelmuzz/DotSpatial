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
// *********************************************************************************************************

using System;
using System.Globalization;
using System.Text;
using DotSpatial.Topology.Algorithm;

namespace DotSpatial.Topology.Geometries
{
    /// <summary>
    /// Represents a line segment defined by two <c>Coordinate</c>s.
    /// Provides methods to compute various geometric properties
    /// and relationships of line segments.
    /// This class is designed to be easily mutable (to the extent of
    /// having its contained points public).
    /// This supports a common pattern of reusing a single LineSegment
    /// object as a way of computing segment properties on the
    /// segments defined by arrays or lists of <c>Coordinate</c>s.
    /// </summary>
    [Serializable]
    public class LineSegment : ILineSegment
    {
        #region Fields

        private static readonly CultureInfo _cultureInfo =
            CultureInfo.InvariantCulture;

        private Coordinate _p0, _p1;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of this class using two coordinates
        /// </summary>
        /// <param name="p0">The start-point</param>
        /// <param name="p1">The end-point</param>
        public LineSegment(Coordinate p0, Coordinate p1)
        {
            _p0 = p0;
            _p1 = p1;
        }

        /// <summary>
        /// Creates an instance of this class using another instance
        /// </summary>
        /// <param name="ls"></param>
        public LineSegment(ILineSegmentBase ls) : this(ls.P0, ls.P1) { }

        /// <summary>
        /// Creates a new instance of a LineSegment which implements
        /// ILineSegment and ILineSegmentBase
        /// </summary>
        public LineSegment() : this(new Coordinate(), new Coordinate()) { }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        public LineSegment(double x0, double y0, double x1, double y1)
            : this(new Coordinate(x0, y0), new Coordinate(x1, y1))
        {
        }

        #endregion

        #region Properties

        /// <returns>
        /// The angle this segment makes with the x-axis (in radians).
        /// </returns>
        public virtual double Angle
        {
            get { return Math.Atan2(_p1.Y - _p0.Y, _p1.X - _p0.X); }
        }

        /// <summary>
        /// Tests whether the segment is horizontal.
        /// </summary>
        /// <returns><c>true</c> if the segment is horizontal.</returns>
        public virtual bool IsHorizontal
        {
            get { return _p0.Y == _p1.Y; }
        }

        /// <summary>
        /// Tests whether the segment is vertical.
        /// </summary>
        /// <returns><c>true</c> if the segment is vertical.</returns>
        public virtual bool IsVertical
        {
            get { return _p0.X == _p1.X; }
        }

        /// <summary>
        /// Computes the length of the line segment.
        /// </summary>
        /// <returns>The length of the line segment.</returns>
        public virtual double Length
        {
            get { return _p0.Distance(_p1); }
        }

        ///<summary>The midpoint of the segment</summary>
        public Coordinate MidPoint
        {
            get
            {
                return new Coordinate((_p0.X + _p1.X) / 2,
                                      (_p0.Y + _p1.Y) / 2);
            }
        }

        /// <summary>
        /// The start-point
        /// </summary>
        public virtual Coordinate P0
        {
            get { return _p0; }
            set { _p0 = value; }
        }

        /// <summary>
        /// The end-point
        /// </summary>
        public virtual Coordinate P1
        {
            get { return _p1; }
            set { _p1 = value; }
        }

        #endregion

        #region Operators

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator ==(LineSegment obj1, ILineSegmentBase obj2)
        {
            return Equals(obj1, obj2);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator !=(LineSegment obj1, ILineSegmentBase obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Computes the closest point on this line segment to another point.
        /// </summary>
        /// <param name="p">The point to find the closest point to.</param>
        /// <returns>
        /// A Coordinate which is the closest point on the line segment to the point p.
        /// </returns>
        public virtual Coordinate ClosestPoint(Coordinate p)
        {
            var factor = ProjectionFactor(p);
            if (factor > 0 && factor < 1)
                return Project(p);
            var dist0 = _p0.Distance(p);
            var dist1 = _p1.Distance(p);
            return dist0 < dist1 ? _p0 : _p1;
        }

        /// <summary>
        /// Computes the closest points on a line segment.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>
        /// A pair of Coordinates which are the closest points on the line segments.
        /// </returns>
        public virtual Coordinate[] ClosestPoints(ILineSegmentBase line)
        {
            LineSegment lineSeg = new LineSegment(line);


            // test for intersection
            var intPt = Intersection(lineSeg);
            if (intPt != null)
                return new[] { intPt, intPt };

            /*
            *  if no intersection closest pair contains at least one endpoint.
            * Test each endpoint in turn.
            */
            var closestPt = new Coordinate[2];

            var close00 = ClosestPoint(lineSeg._p0);
            double minDistance = close00.Distance(lineSeg._p0);
            closestPt[0] = close00;
            closestPt[1] = lineSeg._p0;

            var close01 = ClosestPoint(lineSeg._p1);
            double dist = close01.Distance(lineSeg._p1);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestPt[0] = close01;
                closestPt[1] = lineSeg._p1;
            }

            var close10 = lineSeg.ClosestPoint(_p0);
            dist = close10.Distance(_p0);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestPt[0] = _p0;
                closestPt[1] = close10;
            }

            var close11 = lineSeg.ClosestPoint(_p1);
            dist = close11.Distance(_p1);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestPt[0] = _p1;
                closestPt[1] = close11;
            }

            return closestPt;
        }

        /// <summary>
        /// Compares this object with the specified object for order.
        /// Uses the standard lexicographic ordering for the points in the LineSegment.
        /// </summary>
        /// <param name="o">
        /// The <c>LineSegment</c> with which this <c>LineSegment</c>
        /// is being compared.
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this <c>LineSegment</c>
        /// is less than, equal to, or greater than the specified <c>LineSegment</c>.
        /// </returns>
        public virtual int CompareTo(object o)
        {
            var other = (LineSegment)o;
            var comp0 = _p0.CompareTo(other._p0);
            return comp0 != 0 ? comp0 : _p1.CompareTo(other._p1);
        }

        /// <summary>
        /// Computes the distance between this line segment and another one.
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public virtual double Distance(ILineSegmentBase ls)
        {
            return CgAlgorithms.DistanceLineLine(_p0, _p1, ls.P0, ls.P1);
        }

        /// <summary>
        /// Computes the distance between this line segment and a point.
        /// </summary>
        public virtual double Distance(Coordinate p)
        {
            return CgAlgorithms.DistancePointLine(p, _p0, _p1);
        }

        /// <summary>
        /// Computes the perpendicular distance between the (infinite) line defined
        /// by this line segment and a point.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual double DistancePerpendicular(Coordinate p)
        {
            return CgAlgorithms.DistancePointLinePerpendicular(p, _p0, _p1);
        }

        /// <summary>
        /// Returns <c>true</c> if <c>o</c> has the same values for its points.
        /// </summary>
        /// <param name="o">A <c>LineSegment</c> with which to do the comparison.</param>
        /// <returns>
        /// <c>true</c> if <c>o</c> is a <c>LineSegment</c>
        /// with the same values for the x and y ordinates.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o == null)
                return false;
            if (!(o is LineSegment))
                return false;
            var other = (LineSegment)o;
            return _p0.Equals(other._p0) && _p1.Equals(other._p1);
        }

        /// <summary>
        /// Returns <c>true</c> if <c>other</c> is
        /// topologically equal to this LineSegment (e.g. irrespective
        /// of orientation).
        /// </summary>
        /// <param name="other">
        /// A <c>LineSegment</c> with which to do the comparison.
        /// </param>
        /// <returns>
        /// <c>true</c> if <c>other</c> is a <c>LineSegment</c>
        /// with the same values for the x and y ordinates.
        /// </returns>
        public virtual bool EqualsTopologically(ILineSegmentBase other)
        {
            return _p0.Equals(other.P0) && _p1.Equals(other.P1) ||
                   _p0.Equals(other.P1) && _p1.Equals(other.P0);
        }

        /// <summary>
        /// Retrieves the i'th coordiante.  Since there are only two,
        /// i can be either 0 or 1.
        /// </summary>
        /// <param name="i">Integer, specifies the coordiante</param>
        /// <returns>A topologically complete ICoordinate</returns>
        public virtual Coordinate GetCoordinate(int i)
        {
            if (i == 0) return new Coordinate(_p0);
            return new Coordinate(_p1);
        }

        public Coordinate Intersection(ILineSegmentBase line)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return HashCode.
        /// </summary>
        public override int GetHashCode()
        {
            long bits0 = BitConverter.DoubleToInt64Bits(_p0.X);
            bits0 ^= BitConverter.DoubleToInt64Bits(_p0.Y) * 31;
            int hash0 = (((int)bits0) ^ ((int)(bits0 >> 32)));

            long bits1 = BitConverter.DoubleToInt64Bits(_p1.X);
            bits1 ^= BitConverter.DoubleToInt64Bits(_p1.Y) * 31;
            int hash1 = (((int)bits1) ^ ((int)(bits1 >> 32)));

            // XOR is supposed to be a good way to combine hashcodes
            return hash0 ^ hash1;
        }

        /// <summary>
        /// Computes an intersection point between two segments, if there is one.
        /// There may be 0, 1 or many intersection points between two segments.
        /// If there are 0, null is returned. If there is 1 or more, a single one
        /// is returned (chosen at the discretion of the algorithm).  If
        /// more information is required about the details of the intersection,
        /// the {RobustLineIntersector} class should be used.
        /// </summary>
        /// <param name="line">A line segment</param>
        /// <returns> An intersection point, or <c>null</c> if there is none.</returns>
        /// <see cref="RobustLineIntersector"/>
        public Coordinate Intersection(LineSegment line)
        {
            LineIntersector li = new RobustLineIntersector();
            li.ComputeIntersection(_p0, _p1, line._p0, line._p1);
            if (li.HasIntersection)
                return li.GetIntersection(0);
            return null;
        }

        /// <summary>
        /// Performs an intersection of this line segment with the specified envelope
        /// </summary>
        /// <param name="inEnvelope">The envelope to compare against</param>
        /// <returns>An ILineSegment, or null if there is no intersection.</returns>
        public ILineSegment Intersection(Envelope inEnvelope)
        {
            return inEnvelope.Intersection(this);
        }

        /// <summary>
        /// Determines if any portion of this segment intersects the specified extent.
        /// </summary>
        /// <param name="inEnvelope">The</param>
        /// <returns>Boolean, true if this line segment intersects the specified envelope</returns>
        public bool Intersects(Envelope inEnvelope)
        {
            return inEnvelope.Intersects(this);
        }
		
        /// <summary>
        /// Computes the intersection point of the lines defined by two segments, if there is one.
        /// </summary>
        /// <remarks>
        /// There may be 0, 1 or an infinite number of intersection points between two lines.
        /// If there is a unique intersection point, it is returned.
        /// Otherwise, <c>null</c> is returned.
        /// If more information is required about the details of the intersection,
        /// the <see cref="RobustLineIntersector"/> class should be used.
        /// </remarks>
        /// <param name="line">A line segment defining a straight line</param>
        /// <returns>An intersection point, or <c>null</c> if there is none or an infinite number</returns>
        /// <seealso cref="RobustLineIntersector"/>
        public Coordinate LineIntersection(LineSegment line)
        {
            try
            {
                var intPt = HCoordinate.Intersection(_p0, _p1, line._p0, line._p1);
                return intPt;
            }
            catch (NotRepresentableException)
            {
                // eat this exception, and return null;
            }
            return null;
        }

        /// <summary>
        /// Puts the line segment into a normalized form.
        /// This is useful for using line segments in maps and indexes when
        /// topological equality rather than exact equality is desired.
        /// </summary>
        public virtual void Normalize()
        {
            if (_p1.CompareTo(_p0) < 0)
                Reverse();
        }

        /// <summary>
        /// Determines the orientation of a LineSegment relative to this segment.
        /// The concept of orientation is specified as follows:
        /// Given two line segments A and L,
        /// A is to the left of a segment L if A lies wholly in the
        /// closed half-plane lying to the left of L
        /// A is to the right of a segment L if A lies wholly in the
        /// closed half-plane lying to the right of L
        /// otherwise, A has indeterminate orientation relative to L. This
        /// happens if A is collinear with L or if A crosses the line determined by L.
        /// </summary>
        /// <param name="seg">The <c>LineSegment</c> to compare.</param>
        /// <returns>
        /// 1 if <c>seg</c> is to the left of this segment,
        /// -1 if <c>seg</c> is to the right of this segment,
        /// 0 if <c>seg</c> has indeterminate orientation relative to this segment.
        /// </returns>
        public virtual int OrientationIndex(ILineSegmentBase seg)
        {
            int orient0 = CgAlgorithms.OrientationIndex(P0, P1, seg.P0);
            int orient1 = CgAlgorithms.OrientationIndex(P0, P1, seg.P1);
            // this handles the case where the points are Curve or collinear
            if (orient0 >= 0 && orient1 >= 0)
                return Math.Max(orient0, orient1);
            // this handles the case where the points are R or collinear
            if (orient0 <= 0 && orient1 <= 0)
                return Math.Max(orient0, orient1);
            // points lie on opposite sides ==> indeterminate orientation
            return 0;
        }

        ///<summary>
        /// Determines the orientation index of a <see cref="Coordinate"/> relative to this segment.
        /// The orientation index is as defined in <see cref="CGAlgorithms.ComputeOrientation"/>.
        ///</summary>
        ///
        /// <returns>
        /// <list>
        /// <item>1 if <c>p</c> is to the left of this segment</item>
        /// <item>-1 if <c>p</c> is to the right of this segment</item>
        /// <item>0 if <c>p</c> is collinear with this segment</item>
        /// </list>"
        /// </returns>
        ///
        public int OrientationIndex(Coordinate p)
        {
            return CgAlgorithms.OrientationIndex(_p0, _p1, p);
        }

        /// <summary>
        /// Computes the <see cref="Coordinate" /> that lies a given
        /// fraction along the line defined by this segment.
        /// </summary>
        /// <remarks>
        /// A fraction of <code>0.0</code> returns the start point of the segment;
        /// A fraction of <code>1.0</code> returns the end point of the segment.
        /// If the fraction is &lt; 0.0 or &gt; 1.0 the point returned
        /// will lie before the start or beyond the end of the segment.
        /// </remarks>
        /// <param name="segmentLengthFraction"> the fraction of the segment length along the line</param>
        /// <returns> the point at that distance</returns>
        public Coordinate PointAlong(double segmentLengthFraction)
        {
            var coord = new Coordinate();
            coord.X = _p0.X + segmentLengthFraction * (_p1.X - _p0.X);
            coord.Y = _p0.Y + segmentLengthFraction * (_p1.Y - _p0.Y);
            return coord;
        }

        /// <summary>
        /// Computes the <see cref="Coordinate" /> that lies a given
        /// </summary>
        /// <remarks>
        /// A fraction along the line defined by this segment and offset from
        /// the segment by a given distance.
        /// A fraction of <code>0.0</code> offsets from the start point of the segment;
        /// A fraction of <code>1.0</code> offsets from the end point of the segment.
        /// The computed point is offset to the left of the line if the offset distance is
        /// positive, to the right if negative.
        /// </remarks>
        /// <param name="segmentLengthFraction"> the fraction of the segment length along the line</param>
        /// <param name="offsetDistance"> the distance the point is offset from the segment</param>
        /// (positive is to the left, negative is to the right)
        /// <returns> the point at that distance and offset</returns>
        /// <exception cref="ApplicationException"> if the segment has zero length</exception>
        public Coordinate PointAlongOffset(double segmentLengthFraction, double offsetDistance)
        {
            // the point on the segment line
            double segx = _p0.X + segmentLengthFraction * (_p1.X - _p0.X);
            double segy = _p0.Y + segmentLengthFraction * (_p1.Y - _p0.Y);

            double dx = _p1.X - _p0.X;
            double dy = _p1.Y - _p0.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            double ux = 0.0;
            double uy = 0.0;
            if (offsetDistance != 0.0)
            {
                if (len <= 0.0)
                    throw new ApplicationException("Cannot compute offset from zero-length line segment");

                // u is the vector that is the length of the offset, in the direction of the segment
                ux = offsetDistance * dx / len;
                uy = offsetDistance * dy / len;
            }

            // the offset point is the seg point plus the offset vector rotated 90 degrees CCW
            double offsetx = segx - uy;
            double offsety = segy + ux;

            var coord = new Coordinate(offsetx, offsety);
            return coord;
        }

        /// <summary>
        /// Compute the projection of a point onto the line determined
        /// by this line segment.
        /// Note that the projected point  may lie outside the line segment.
        /// If this is the case,  the projection factor will lie outside the range [0.0, 1.0].
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual Coordinate Project(Coordinate p)
        {
            if (p.Equals(_p0) || p.Equals(_p1))
                return new Coordinate(p);

            var r = ProjectionFactor(p);
            var coord = new Coordinate { X = _p0.X + r * (_p1.X - _p0.X), Y = _p0.Y + r * (_p1.Y - _p0.Y) };
            return coord;
        }

        /// <summary>
        /// Project a line segment onto this line segment and return the resulting
        /// line segment.  The returned line segment will be a subset of
        /// the target line line segment.  This subset may be null, if
        /// the segments are oriented in such a way that there is no projection.
        /// Note that the returned line may have zero length (i.e. the same endpoints).
        /// This can happen for instance if the lines are perpendicular to one another.
        /// </summary>
        /// <param name="seg">The line segment to project.</param>
        /// <returns>The projected line segment, or <c>null</c> if there is no overlap.</returns>
        public virtual ILineSegment Project(ILineSegmentBase seg)
        {
            var pf0 = ProjectionFactor(seg.P0);
            var pf1 = ProjectionFactor(seg.P1);
            // check if segment projects at all
            if (pf0 >= 1.0 && pf1 >= 1.0) return null;
            if (pf0 <= 0.0 && pf1 <= 0.0) return null;

            var newp0 = Project(seg.P0);
            if (pf0 < 0.0) newp0 = _p0;
            if (pf0 > 1.0) newp0 = _p1;

            var newp1 = Project(seg.P1);
            if (pf1 < 0.0) newp1 = _p0;
            if (pf1 > 1.0) newp1 = _p1;

            return new LineSegment(newp0, newp1);
        }

        /// <summary>Computes the Projection Factor for the projection of the point p
        /// onto this LineSegment.  The Projection Factor is the constant r
        /// by which the vector for this segment must be multiplied to
        /// equal the vector for the projection of <tt>p</tt> on the line
        /// defined by this segment.
        /// <para/>
        /// The projection factor will lie in the range <tt>(-inf, +inf)</tt>,
        /// or be <c>NaN</c> if the line segment has zero length.
        /// </summary>
        /// <param name="p">The point to compute the factor for</param>
        /// <returns>The projection factor for the point</returns>
        public virtual double ProjectionFactor(Coordinate p)
        {
            if (p.Equals(_p0)) return 0.0;
            if (p.Equals(_p1)) return 1.0;

            // Otherwise, use comp.graphics.algorithms Frequently Asked Questions method
            /*     	          AC dot AB
                        r = ------------
                              ||AB||^2
                        r has the following meaning:
                        r=0 Point = A
                        r=1 Point = B
                        r<0 Point is on the backward extension of AB
                        r>1 Point is on the forward extension of AB
                        0<r<1 Point is interior to AB
            */
            var dx = _p1.X - _p0.X;
            var dy = _p1.Y - _p0.Y;
            var len = dx * dx + dy * dy;

            // handle zero-length segments
            if (len <= 0.0) return Double.NaN;

            double r = ((p.X - _p0.X) * dx + (p.Y - _p0.Y) * dy)
                      / len;
            return r;
        }

        /// <summary>
        /// Reverses the direction of the line segment.
        /// </summary>
        public virtual void Reverse()
        {
            var temp = _p0;
            _p0 = _p1;
            _p1 = temp;
        }

        /// <summary>
        /// Computes the fraction of distance (in [0.0, 1.0])
        /// that the projection of a point occurs along this line segment.
        /// If the point is beyond either ends of the line segment,
        /// the closest fractional value (0.0 or 1.0) is returned.
        /// </summary>
        /// <remarks>
        /// Essentially, this is the <see cref="ProjectionFactor(Coordinate)" /> clamped to
        /// the range [0.0, 1.0].
        /// </remarks>
        /// <param name="inputPt"> the point</param>
        /// <returns> the fraction along the line segment the projection of the point occurs</returns>
        public double SegmentFraction(Coordinate inputPt)
        {
            double segFrac = ProjectionFactor(inputPt);
            if (segFrac < 0.0)
                segFrac = 0.0;
            else if (segFrac > 1.0)
                segFrac = 1.0;
            return segFrac;
        }

        /// <summary>
        /// Defines a new LineSegment based on the previous line segment
        /// </summary>
        /// <param name="ls">The ILineSegmentBase</param>
        public virtual void SetCoordinates(ILineSegmentBase ls)
        {
            SetCoordinates(ls.P0, ls.P1);
        }

        /// <summary>
        /// Sets the new coordinates using the ICoordinate interfaces specified
        /// </summary>
        /// <param name="p0">The first endpoint</param>
        /// <param name="p1">The second endpoint</param>
        public virtual void SetCoordinates(Coordinate p0, Coordinate p1)
        {
            P0.X = p0.X;
            P0.Y = p0.Y;
            P1.X = p1.X;
            P1.Y = p1.Y;
        }

        /// <summary>
        /// Creates a LineString with the same coordinates as this segment
        /// </summary>
        /// <param name="geomFactory">the geometery factory to use</param>
        /// <returns>A LineString with the same geometry as this segment</returns>
        public ILineString ToGeometry(IGeometryFactory geomFactory)
        {
            return geomFactory.CreateLineString(new[] { _p0, _p1 });
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder("LINESTRING( ");
            sb.AppendFormat(_cultureInfo, "{0}", _p0.X).Append(" ");
            sb.AppendFormat(_cultureInfo, "{0}", _p0.Y).Append(", ");
            sb.AppendFormat(_cultureInfo, "{0}", _p1.X).Append(" ");
            sb.AppendFormat(_cultureInfo, "{0}", _p1.Y).Append(")");
            return sb.ToString();
        }

        #endregion
    }
}