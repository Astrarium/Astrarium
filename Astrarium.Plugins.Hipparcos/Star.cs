using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Astrarium.Plugins.Hipparcos
{
    public class Star : CelestialObject, IMagnitudeObject, IObservableObject
    {
        /// <summary>
        /// Celestial object type for Hipparcos stars
        /// </summary>
        public static string BodyType = "Star";

        /// <inheritdoc />
        public override string Type => BodyType;

        /// <inheritdoc />
        public override string CommonName => $"HIP {HIP}";

        /// <summary>
        /// Function to get star names
        /// </summary>
        internal static Func<Star, IEnumerable<string>> GetNamesFunc { get; set; } = null;

        /// <summary>
        /// Proper name of the star, if available
        /// </summary>
        public string ProperName { get; set; }

        /// <summary>
        /// Star index in catalogue
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 1...120416 Hipparcos Catalog number
        /// </summary>
        public uint HIP { get; set; }

        /// <summary>
        /// RA at catalogue epoch, degrees 
        /// </summary>
        public float Alpha0 { get; set; }

        /// <summary>
        /// Dec at catalogue epoch, degrees 
        /// </summary>
        public float Delta0 { get; set; }

        /// <summary>
        /// Proper motion in RA, mas/year
        /// </summary>
        public float PmAlpha { get; set; }

        /// <summary>
        /// Proper motion in Dec, mas/year
        /// </summary>
        public float PmDelta { get; set; }

        /// <summary>
        /// Spectral type, 12 bytes
        /// </summary>
        public string SpType { get; set; }

        /// <summary>
        /// Trigonometric parallax, mas
        /// </summary>
        public float Plx { get; set; }

        /// <summary>
        /// Gets star spectral class (one character: O, W, B, A, F, G, K, M)
        /// </summary>
        public char Color => SpType[0];

        #region Multiplicity

        /// <summary>
        /// Flag indicating star is multiple system
        /// </summary>
        public bool IsMultiple => NComps > 1 || !string.IsNullOrWhiteSpace(Comps);

        /// <summary>
        /// Number of components
        /// </summary>
        public byte NComps { get; set; }

        /// <summary>
        /// Components identifiers, 2 bytes (ex: AB)
        /// </summary>
        public string Comps { get; set; }

        /// <summary>
        /// Position angle between components (0...255), degrees
        /// </summary>
        public byte PosA { get; set; }

        /// <summary>
        /// Angular separation between component, arcsec
        /// </summary>
        public float Sep { get; set; }

        /// <summary>
        /// Magnitude difference of components
        /// </summary>
        public float MagDiff { get; set; }

        #endregion Multiplicity

        #region Variability

        /// <summary>
        /// Flag incicating star is variable
        /// </summary>
        public bool IsVariable => VarType == 'P' || VarFlag > 1;

        /// <summary>
        /// Variability flag.
        /// Values: 1: < 0.06mag ; 2: 0.06-0.6mag ; 3: >0.6mag
        /// </summary>
        public byte VarFlag { get; set; }

        /// <summary>
        /// Hipparcos-defined type of variability (a blank entry
        /// signifies that the entry could not be classified as variable or constant):
        /// C : no variability detected("constant")
        /// D : duplicity-induced variability
        /// M : possibly micro-variable(amplitude< 0.03mag)
        /// P : periodic variable
        /// R : V-I colour index was revised due to variability analysis
        /// U : unsolved variable which does not fall in the other categories
        /// </summary>
        public byte VarType { get; set; }

        /// <summary>
        /// Variablity period, days
        /// </summary>
        public float VarPeriod { get; set; }

        #endregion Variability

        #region Identifiers

        /// <summary>
        /// 1...257937 Henry Draper Catalog Number
        /// </summary>
        public uint HD { get; set; }

        /// <summary>
        /// Durchmusterung Identification from HD Catalog (12 bytes)
        /// </summary>
        public string DM { get; set; } = new string(' ', 12);

        /// <summary>
        /// 1...33342 Boss General Catalog number if one exists, otherwise zero
        /// </summary>
        public ushort GC { get; set; }

        /// <summary>
        /// 1...9110 Harvard Revised Number = Bright Star Number if one exists, otherwise zero
        /// </summary>
        public ushort HR { get; set; }

        /// <summary>
        /// Flamsteed number (0...255)
        /// </summary>
        public byte Flamsteed { get; set; }

        /// <summary>
        /// Bayer designation (5 bytes)
        /// </summary>
        public string Bayer { get; set; } = new string(' ', 5);

        /// <summary>
        /// Constellation for Bayer/Flamsteed names (3 bytes)
        /// </summary>
        public string Con { get; set; } = new string(' ', 3);

        #endregion Identifiers

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public override string[] DisplaySettingNames => new[] { "Stars" };

        /// <inheritdoc />
        public float Magnitude { get; set; }

        /// <inheritdoc />
        public override string[] Names => GetNamesFunc(this)?.ToArray();
    }
}
