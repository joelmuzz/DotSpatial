﻿using System.Collections.Generic;
using DotSpatial.Topology.Algorithm;
using DotSpatial.Topology.Geometries;

namespace DotSpatial.Topology.Noding
{
    ///<summary>
    /// Detects and records an intersection between two <see cref="ISegmentString"/>s,
    /// if one exists.  Only a single intersection is recorded.
    ///</summary>
    /// <remarks>
    /// This strategy can be configured to search for <b>proper</b> intersections.
    /// In this case, the presence of <i>any</i> intersection will still be recorded,
    /// but searching will continue until either a proper intersection has been found
    /// or no intersections are detected.
    /// </remarks>
    public class SegmentIntersectionDetector : ISegmentIntersector
    {
        #region Fields

        private readonly LineIntersector _li;

        #endregion

        #region Constructors

        ///<summary>
        /// Creates an intersection finder using a <see cref="RobustLineIntersector"/>
        ///</summary>
        public SegmentIntersectionDetector() : this(new RobustLineIntersector()) { }

        ///<summary>
        /// Creates an intersection finder using a given <see cref="LineIntersector"/>
        ///</summary>
        /// <param name="li">The LineIntersector to use</param>
        public SegmentIntersectionDetector(LineIntersector li)
        {
            _li = li;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Tests whether processing can terminate, because all required information has been obtained
        /// (e.g. an intersection of the desired type has been detected).
        /// </summary>
        public bool IsDone
        {
            get
            {
                // If finding all types, we can stop when both possible types have been found.
                if (FindAllIntersectionTypes)
                    return HasProperIntersection && HasNonProperIntersection;

                //If searching for a proper intersection, only stop if one is found
                return FindProper ? HasProperIntersection : HasIntersection;
            }
        }

        /// <summary>
        /// Gets or sets whether processing can terminate once any intersection is found.
        /// </summary>
        public bool FindAllIntersectionTypes { get; set; }

        /// <summary>
        /// Gets or sets whether processing must continue until a proper intersection is found
        /// </summary>
        public bool FindProper { get; set; }

        ///<summary>
        /// Tests whether an intersection was found.
        ///</summary>
        public bool HasIntersection { get; private set; }

        ///<summary>
        /// Tests whether a non-proper intersection was found.
        ///</summary>
        public bool HasNonProperIntersection { get; private set; }

        ///<summary>
        /// Tests whether a proper intersection was found.
        ///</summary>
        public bool HasProperIntersection { get; private set; }

        ///<summary>
        /// Gets the computed location of the intersection. Due to round-off, the location may not be exact.
        ///</summary>
        public Coordinate Intersection { get; private set; }

        ///<summary>Gets the endpoints of the intersecting segments.
        ///</summary>
        /// <remarks>An array of the segment endpoints (p00, p01, p10, p11)</remarks>
        public IList<Coordinate> IntersectionSegments { get; private set; }

        #endregion

        #region Methods

        ///<summary>
        /// This method is called by clients of the <see cref="ISegmentIntersector"/> class to process
        /// intersections for two segments of the <see cref="ISegmentString"/>s being intersected.
        ///</summary>
        /// <remarks>
        /// Note that some clients (such as <c>MonotoneChain</c>s) may optimize away
        /// this call for segment pairs which they have determined do not intersect
        /// (e.g. by an disjoint envelope test).
        /// </remarks>
        public void ProcessIntersections(ISegmentString e0, int segIndex0, ISegmentString e1, int segIndex1)
        {
            // don't bother intersecting a segment with itself
            if (e0 == e1 && segIndex0 == segIndex1) return;

            var coords = e0.Coordinates;
            var p00 = coords[segIndex0];
            var p01 = coords[segIndex0 + 1];

            coords = e1.Coordinates;
            var p10 = coords[segIndex1];
            var p11 = coords[segIndex1 + 1];

            _li.ComputeIntersection(p00, p01, p10, p11);
            if (!_li.HasIntersection) return;
            // record intersection info
            HasIntersection = true;

            var isProper = _li.IsProper;
            if (isProper) HasProperIntersection = true;
            if (!isProper) HasNonProperIntersection = true;

            // If this is the kind of intersection we are searching for OR no location has 
            // yet been recorded save the location data
            var saveLocation = !(FindProper && !isProper);

            if (Intersection != null && !saveLocation) return;
            // record intersection location (approximate)
            Intersection = _li.GetIntersection(0);

            // record intersecting segments
            IntersectionSegments = new Coordinate[4];
            IntersectionSegments[0] = p00;
            IntersectionSegments[1] = p01;
            IntersectionSegments[2] = p10;
            IntersectionSegments[3] = p11;
        }

        #endregion
    }
}