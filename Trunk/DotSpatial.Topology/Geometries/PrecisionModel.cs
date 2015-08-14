﻿// ********************************************************************************************************
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

namespace DotSpatial.Topology.Geometries
{
    /// <summary>
    /// Specifies the precision model of the <c>Coordinate</c>s in a <c>Geometry</c>.
    /// In other words, specifies the grid of allowable
    /// points for all <c>Geometry</c>s.
    /// </summary>
    /// <remarks>
    /// The <c>MakePrecise</c> method allows rounding a coordinate to
    /// a "precise" value; that is, one whose
    /// precision is known exactly.
    /// Coordinates are assumed to be precise in geometries.
    /// That is, the coordinates are assumed to be rounded to the
    /// precision model given for the point.
    /// NTS input routines automatically round coordinates to the precision model
    /// before creating Geometries.
    /// All internal operations
    /// assume that coordinates are rounded to the precision model.
    /// Constructive methods (such as bool operations) always round computed
    /// coordinates to the appropriate precision model.
    /// Currently three types of precision model are supported:
    /// <para>
    /// Floating: represents full double precision floating point.
    /// This is the default precision model used in NTS
    /// FloatingSingle: represents single precision floating point.
    /// Fixed: represents a model with a fixed number of decimal places.
    /// </para>
    /// A Fixed Precision Model is specified by a scale factor.
    /// The scale factor specifies the size of the grid which numbers are rounded to.
    /// Input coordinates are mapped to fixed coordinates according to the following
    /// equations:
    /// <list>
    /// <item>jtsPt.x = round( (inputPt.x * scale ) / scale</item>
    /// <item>jtsPt.y = round( (inputPt.y * scale ) / scale</item>
    /// </list>
    /// <para>
    /// For example, to specify 3 decimal places of precision, use a scale factor
    /// of 1000. To specify -3 decimal places of precision (i.e. rounding to
    /// the nearest 1000), use a scale factor of 0.001.
    /// </para>
    /// Coordinates are represented internally as Java double-precision values.
    /// Since .NET uses the IEEE-394 floating point standard, this
    /// provides 53 bits of precision. (Thus the maximum precisely representable
    /// <i>integer</i> is 9,007,199,254,740,992 - or almost 16 decimal digits of precision).
    /// <para/>
    /// NTS binary methods currently do not handle inputs which have different precision models.
    /// The precision model of any constructed geometric value is undefined.
    /// </remarks>
    [Serializable]
    public class PrecisionModel : IPrecisionModel
    {
        #region Constant Fields

        /// <summary>
        /// The maximum precise value representable in a double. Since IEE754
        /// double-precision numbers allow 53 bits of mantissa, the value is equal to
        /// 2^53 - 1.  This provides <i>almost</i> 16 decimal digits of precision.
        /// </summary>
        public const double MaximumPreciseValue = 9007199254740992.0;

        private const int FixedPrecisionDigits = 1;
        private const int FloatingPrecisionDigits = 16;
        private const int FloatingSinglePrecisionDigits = 6;

        #endregion

        #region Fields

        /// <summary>
        /// The type of PrecisionModel this represents.
        /// </summary>
        private readonly PrecisionModelType _modelType;

        /// <summary>
        /// The scale factor which determines the number of decimal places in fixed precision.
        /// </summary>
        private double _scale;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a <c>PrecisionModel</c> with a default precision
        /// of Floating.
        /// </summary>
        public PrecisionModel()
        {
            // default is floating precision
            _modelType = PrecisionModelType.Floating;
        }

        /// <summary>
        /// Creates a <c>PrecisionModel</c> that specifies
        /// an explicit precision model type.
        /// If the model type is Fixed the scale factor will default to 1.
        /// </summary>
        /// <param name="modelType">
        /// The type of the precision model.
        /// </param>
        public PrecisionModel(PrecisionModelType modelType)
        {
            _modelType = modelType;

            if (modelType == PrecisionModelType.Fixed)
                _scale = 1.0;
        }

        /// <summary>
        /// Creates a <c>PrecisionModel</c> that specifies Fixed precision.
        /// Fixed-precision coordinates are represented as precise internal coordinates,
        /// which are rounded to the grid defined by the scale factor.
        /// </summary>
        /// <param name="scale">
        /// Amount by which to multiply a coordinate after subtracting
        /// the offset, to obtain a precise coordinate.
        /// </param>
        public PrecisionModel(double scale)
        {
            _modelType = PrecisionModelType.Fixed;
            _scale = scale;
        }

        /// <summary>
        /// Copy constructor to create a new <c>PrecisionModel</c>
        /// from an existing one.
        /// </summary>
        /// <param name="pm"></param>
        public PrecisionModel(PrecisionModel pm)
        {
            _modelType = pm._modelType;
            _scale = pm._scale;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Tests whether the precision model supports floating point.
        /// </summary>
        /// <returns><c>true</c> if the precision model supports floating point.</returns>
        public virtual bool IsFloating
        {
            get
            {
                return _modelType == PrecisionModelType.Floating || _modelType == PrecisionModelType.FloatingSingle;
            }
        }

        /// <summary>
        /// Returns the maximum number of significant digits provided by this
        /// precision model.
        /// Intended for use by routines which need to print out precise values.
        /// </summary>
        /// <returns>
        /// The maximum number of decimal places provided by this precision model.
        /// </returns>
        public virtual int MaximumSignificantDigits
        {
            get
            {
                switch (_modelType)
                {
                    case PrecisionModelType.Floating:
                        return FloatingPrecisionDigits;
                    case PrecisionModelType.FloatingSingle:
                        return FloatingSinglePrecisionDigits;
                    case PrecisionModelType.Fixed:
                        return FixedPrecisionDigits + (int)Math.Ceiling(Math.Log(Scale) / Math.Log(10));
                    default:
                        throw new ArgumentOutOfRangeException(_modelType.ToString());
                }
            }
        }

        /// <summary>
        /// Returns the x-offset used to obtain a precise coordinate.
        /// </summary>
        /// <returns>
        /// The amount by which to subtract the x-coordinate before
        /// multiplying by the scale.
        /// </returns>
        [Obsolete("Offsets are no longer used")]
        public virtual double OffsetX
        {
            get
            {
                //We actually don't use offsetX and offsetY anymore ... [Jon Aquino]
                return 0;
            }
        }

        /// <summary>
        /// Returns the y-offset used to obtain a precise coordinate.
        /// </summary>
        /// <returns>
        /// The amount by which to subtract the y-coordinate before
        /// multiplying by the scale
        /// </returns>
        [Obsolete("Offsets are no longer used")]
        public virtual double OffsetY
        {
            get { return 0; }
        }

        ///// <summary>
        ///// Gets the type of this PrecisionModel.
        ///// </summary>
        ///// <returns></returns>
        //public PrecisionModels GetPrecisionModelType()
        //{
        //    return _modelType;
        //}

        /// <summary>
        /// Gets the type of this PrecisionModel.
        /// </summary>
        /// <returns></returns>
        public PrecisionModelType PrecisionModelType
        {
            get { return _modelType; }
        }

        /// <summary>
        /// Returns the scale factor used to specify a fixed precision model.
        /// </summary>
        /// <remarks>
        /// The number of decimal places of precision is
        /// equal to the base-10 logarithm of the scale factor.
        /// Non-integral and negative scale factors are supported.
        /// Negative scale factors indicate that the places
        /// of precision is to the left of the decimal point.
        /// </remarks>
        /// <returns>
        /// The scale factor for the fixed precision model
        /// </returns>
        public virtual double Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = Math.Abs(value);
            }
        }

        #endregion

        #region Operators

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator ==(PrecisionModel obj1, PrecisionModel obj2)
        {
            return Equals(obj1, obj2);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator !=(PrecisionModel obj1, PrecisionModel obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Compares this <c>PrecisionModel</c> object with the specified object for order.
        /// A PrecisionModel is greater than another if it provides greater precision.
        /// The comparison is based on the value returned by the
        /// {getMaximumSignificantDigits) method.
        /// This comparison is not strictly accurate when comparing floating precision models
        /// to fixed models; however, it is correct when both models are either floating or fixed.
        /// </summary>
        /// <param name="o">
        /// The <c>PrecisionModel</c> with which this <c>PrecisionModel</c>
        /// is being compared.
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this <c>PrecisionModel</c>
        /// is less than, equal to, or greater than the specified <c>PrecisionModel</c>.
        /// </returns>
        public int CompareTo(object o)
        {
            return CompareTo((IPrecisionModel)o);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(IPrecisionModel other)
        {
            int sigDigits = MaximumSignificantDigits;
            int otherSigDigits = other.MaximumSignificantDigits;
            return (sigDigits).CompareTo(otherSigDigits);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (!(other is IPrecisionModel))
                return false;

            return Equals((IPrecisionModel)other);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="otherPrecisionModel"></param>
        /// <returns></returns>
        public bool Equals(IPrecisionModel otherPrecisionModel)
        {
            return _modelType == otherPrecisionModel.PrecisionModelType &&
                    _scale == otherPrecisionModel.Scale;
        }

        /// <summary>
        /// Return HashCode.
        /// </summary>
        public override int GetHashCode()
        {
            return _modelType.GetHashCode() ^ _scale.GetHashCode();
        }

        /// <summary>
        /// Rounds a numeric value to the PrecisionModel grid.
        /// Symmetric Arithmetic Rounding is used, to provide
        /// uniform rounding behaviour no matter where the number is
        /// on the number line.
        /// </summary>
        /// <remarks>
        /// This method has no effect on NaN values
        /// </remarks>
        /// <param name="val"></param>
        public double MakePrecise(double val)
        {
            // don't change NaN values
            if (Double.IsNaN(val)) return val;
            if (_modelType == PrecisionModelType.FloatingSingle)
            {
                float floatSingleVal = (float)val;
                return floatSingleVal;
            }
            if (_modelType == PrecisionModelType.Fixed)
            {
                /*.Net's default rounding algorithm is "Bankers Rounding" which turned
                 * out to be no good for JTS/NTS geometry operations */
                // return Math.Round(val * scale) / scale;          

                // This is "Asymmetric Arithmetic Rounding"
                // http://en.wikipedia.org/wiki/Rounding#Round_half_up
                return Math.Floor(val * _scale + 0.5d) / _scale;
            }
            return val;     // modelType == FLOATING - no rounding necessary
        }

        /// <summary>
        /// Rounds a Coordinate to the PrecisionModel grid.
        /// </summary>
        /// <param name="coord"></param>
        public void MakePrecise(Coordinate coord)
        {
            // optimization for full precision
            if (_modelType == PrecisionModelType.Floating)
                return;

            coord.X = MakePrecise(coord.X);
            coord.Y = MakePrecise(coord.Y);
            //MD says it's OK that we're not makePrecise'ing the z [Jon Aquino]
        }

        ///<summary>
        /// Determines which of two <see cref="IPrecisionModel"/>s is the most precise
        ///</summary>
        /// <param name="pm1">A precision model</param>
        /// <param name="pm2">A precision model</param>
        /// <returns>The PrecisionModel which is most precise</returns>
        public static IPrecisionModel MostPrecise(IPrecisionModel pm1, IPrecisionModel pm2)
        {
            if (pm1.CompareTo(pm2) >= 0)
                return pm1;
            return pm2;
        }

        /// <summary>
        /// Returns the external representation of <c>internal</c>.
        /// </summary>
        /// <param name="cinternal">The original coordinate.</param>
        /// <returns>
        /// The coordinate whose values will be changed to the
        /// external representation of <c>internal</c>.
        /// </returns>
        [Obsolete("No longer needed, since internal representation is same as external representation")]
        public virtual Coordinate ToExternal(Coordinate cinternal)
        {
            Coordinate cexternal = new Coordinate(cinternal);
            return cexternal;
        }

        /// <summary>
        /// Sets <c>external</c> to the external representation of <c>internal</c>.
        /// </summary>
        /// <param name="cinternal">The original coordinate.</param>
        /// <param name="cexternal">
        /// The coordinate whose values will be changed to the
        /// external representation of <c>internal</c>.
        /// </param>
        [Obsolete("No longer needed, since internal representation is same as external representation")]
        public virtual void ToExternal(Coordinate cinternal, Coordinate cexternal)
        {
            cexternal.X = cinternal.X;
            cexternal.Y = cinternal.Y;
        }

        /// <summary>
        /// Sets <c>internal</c> to the precise representation of <c>external</c>.
        /// </summary>
        /// <param name="cexternal">The original coordinate.</param>
        /// <param name="cinternal">
        /// The coordinate whose values will be changed to the
        /// precise representation of <c>external</c>.
        /// </param>
        [Obsolete("Use MakePrecise instead")]
        public virtual void ToInternal(Coordinate cexternal, Coordinate cinternal)
        {
            if (IsFloating)
            {
                cinternal.X = cexternal.X;
                cinternal.Y = cexternal.Y;
            }
            else
            {
                cinternal.X = MakePrecise(cexternal.X);
                cinternal.Y = MakePrecise(cexternal.Y);
            }
            cinternal.Z = cexternal.Z;
        }

        /// <summary>
        /// Returns the precise representation of <c>external</c>.
        /// </summary>
        /// <param name="cexternal">The original coordinate.</param>
        /// <returns>
        /// The coordinate whose values will be changed to the precise
        /// representation of <c>external</c>
        /// </returns>
        [Obsolete("Use MakePrecise instead")]
        public virtual Coordinate ToInternal(Coordinate cexternal)
        {
            Coordinate cinternal = new Coordinate(cexternal);
            MakePrecise(cinternal);
            return cinternal;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string description = "UNKNOWN";
            if (_modelType == PrecisionModelType.Floating)
                description = "Floating";
            else if (_modelType == PrecisionModelType.FloatingSingle)
                description = "Floating-Single";
            else if (_modelType == PrecisionModelType.Fixed)
                description = "Fixed (Scale=" + Scale + ")";
            return description;
        }

        #endregion
    }
}