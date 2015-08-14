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

using System.Collections.Generic;

namespace DotSpatial.Topology.GeometriesGraph.Index
{
    /// <summary>
    /// An <c>EdgeSetIntersector</c> computes all the intersections between the
    /// edges in the set.  It adds the computed intersections to each edge
    /// they are found on.  It may be used in two scenarios:
    /// determining the internal intersections between a single set of edges
    /// determining the mutual intersections between two different sets of edges
    /// It uses a <c>SegmentIntersector</c> to compute the intersections between
    /// segments and to record statistics about what kinds of intersections were found.
    /// </summary>
    public abstract class EdgeSetIntersector
    {
        #region Methods

        /// <summary>
        /// Computes all self-intersections between edges in a set of edges,
        /// allowing client to choose whether self-intersections are computed.
        /// </summary>
        /// <param name="edges">A list of edges to test for intersections.</param>
        /// <param name="si">The SegmentIntersector to use.</param>
        /// <param name="testAllSegments"><c>true</c> if self-intersections are to be tested as well.</param>
        abstract public void ComputeIntersections(IList<Edge> edges, SegmentIntersector si, bool testAllSegments);

        /// <summary>
        /// Computes all mutual intersections between two sets of edges.
        /// </summary>
        abstract public void ComputeIntersections(IList<Edge> edges0, IList<Edge> edges1, SegmentIntersector si);

        #endregion
    }
}