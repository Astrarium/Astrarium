using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;

namespace Astrarium.Plugins.Simbad
{
    public static class VoTableFields
    {
        public static Dictionary<string, string> FieldToGroupMap { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public static Dictionary<string, IEphemFormatter> FieldFormatters { get; private set; } = new Dictionary<string, IEphemFormatter>(StringComparer.OrdinalIgnoreCase);

        public const string GroupCommon = "Common";
        public const string GroupIdentifiers = "Identifiers";
        public const string GroupCoordinates = "Coordinates";
        public const string GroupProperMotion = "ProperMotion";
        public const string GroupParallax = "Parallax";
        public const string GroupPhotometryB = "Photometry_B";
        public const string GroupPhotometryV = "Photometry_V";
        public const string GroupRadialVelocity = "RadialVelocity";
        public const string GroupSpectralType = "SpectralType";
        public const string GroupMorphology = "Morphology";
        public const string GroupGalaxyDimensions = "GalaxyDimensions";

        private static IEphemFormatter IdsFormatter = new IdsFormatter();
        private static IEphemFormatter TypeFormatter = new TypeFormatter();
        private static IEphemFormatter BibcodeFormatter = new BibcodeFormatter();
        private static IEphemFormatter ExcludedFormatter = new ExcludedFormatter();
        private static IEphemFormatter RAFormatter = new RAFormatter();
        private static IEphemFormatter DecFormatter = new DecFormatter();
        private static IEphemFormatter CoordPrecisionFormatter = new CoordPrecisionFormatter();
        private static IEphemFormatter MasFormatter = new UnitsFormatter(" mas");
        private static IEphemFormatter RadialVelocityFormatter = new UnitsFormatter(" km/s");
        private static IEphemFormatter FluxFormatter = new UnitsFormatter("ᵐ");
        private static IEphemFormatter DegreesFormatter = new UnitsFormatter("°");
        private static IEphemFormatter ProperMotionFormatter = new UnitsFormatter(" mas/yr");
        private static IEphemFormatter GalaxyDimensionsFormatter = new UnitsFormatter("′");
        private static IEphemFormatter QualityFormatter = new QualityFormatter();

        static VoTableFields()
        {
            AddFieldToGroup("MAIN_ID", GroupCommon);
            AddFieldToGroup("OTYPE_v", GroupCommon, TypeFormatter);

            AddFieldToGroup("RA", GroupCoordinates, RAFormatter);
            AddFieldToGroup("RA_PREC", GroupCoordinates, CoordPrecisionFormatter);
            AddFieldToGroup("DEC", GroupCoordinates, DecFormatter);
            AddFieldToGroup("DEC_PREC", GroupCoordinates, CoordPrecisionFormatter);
            AddFieldToGroup("COO_ERR_MAJA", GroupCoordinates, MasFormatter);
            AddFieldToGroup("COO_ERR_MINA", GroupCoordinates, MasFormatter);
            AddFieldToGroup("COO_ERR_ANGLE", GroupCoordinates, DegreesFormatter);
            AddFieldToGroup("COO_QUAL", GroupCoordinates, QualityFormatter);
            AddFieldToGroup("COO_WAVELENGTH", GroupCoordinates);
            AddFieldToGroup("COO_BIBCODE", GroupCoordinates, BibcodeFormatter);

            AddFieldToGroup("PMRA", GroupProperMotion, ProperMotionFormatter);
            AddFieldToGroup("PMDEC", GroupProperMotion, ProperMotionFormatter);
            AddFieldToGroup("PMRA_PREC", GroupProperMotion, CoordPrecisionFormatter);
            AddFieldToGroup("PMDEC_PREC", GroupProperMotion, CoordPrecisionFormatter);
            AddFieldToGroup("PM_ERR_MAJA", GroupProperMotion, ProperMotionFormatter);
            AddFieldToGroup("PM_ERR_MINA", GroupProperMotion, ProperMotionFormatter);
            AddFieldToGroup("PM_ERR_ANGLE", GroupProperMotion, DegreesFormatter);
            AddFieldToGroup("PM_QUAL", GroupProperMotion, QualityFormatter);
            AddFieldToGroup("PM_BIBCODE", GroupProperMotion, BibcodeFormatter);

            AddFieldToGroup("PLX_VALUE", GroupParallax, MasFormatter);
            AddFieldToGroup("PLX_PREC", GroupParallax, CoordPrecisionFormatter);
            AddFieldToGroup("PLX_ERROR", GroupParallax, MasFormatter);
            AddFieldToGroup("PLX_QUAL", GroupParallax, QualityFormatter);
            AddFieldToGroup("PLX_BIBCODE", GroupParallax, BibcodeFormatter);

            AddFieldToGroup("FILTER_NAME_B", GroupPhotometryB, ExcludedFormatter);
            AddFieldToGroup("FLUX_B", GroupPhotometryB, FluxFormatter);
            AddFieldToGroup("FLUX_ERROR_B", GroupPhotometryB, FluxFormatter);
            AddFieldToGroup("FLUX_SYSTEM_B", GroupPhotometryB);
            AddFieldToGroup("FLUX_BIBCODE_B", GroupPhotometryB, BibcodeFormatter);
            AddFieldToGroup("FLUX_VAR_B", GroupPhotometryB);
            AddFieldToGroup("FLUX_MULT_B", GroupPhotometryB);
            AddFieldToGroup("FLUX_QUAL_B", GroupPhotometryB, QualityFormatter);
            AddFieldToGroup("FLUX_UNIT_B", GroupPhotometryB, ExcludedFormatter);

            AddFieldToGroup("FILTER_NAME_V", GroupPhotometryV, ExcludedFormatter);
            AddFieldToGroup("FLUX_V", GroupPhotometryV, FluxFormatter);
            AddFieldToGroup("FLUX_ERROR_V", GroupPhotometryV, FluxFormatter);
            AddFieldToGroup("FLUX_SYSTEM_V", GroupPhotometryV);
            AddFieldToGroup("FLUX_BIBCODE_V", GroupPhotometryV, BibcodeFormatter);
            AddFieldToGroup("FLUX_VAR_V", GroupPhotometryV);
            AddFieldToGroup("FLUX_MULT_V", GroupPhotometryV);
            AddFieldToGroup("FLUX_QUAL_V", GroupPhotometryV, QualityFormatter);
            AddFieldToGroup("FLUX_UNIT_V", GroupPhotometryV, ExcludedFormatter);

            AddFieldToGroup("RVZ_TYPE", GroupRadialVelocity);
            AddFieldToGroup("RVZ_RADVEL", GroupRadialVelocity, RadialVelocityFormatter);
            AddFieldToGroup("RVZ_ERROR", GroupRadialVelocity, RadialVelocityFormatter);
            AddFieldToGroup("RVZ_QUAL", GroupRadialVelocity, QualityFormatter);
            AddFieldToGroup("RVZ_WAVELENGTH", GroupRadialVelocity);
            AddFieldToGroup("RVZ_BIBCODE", GroupRadialVelocity, BibcodeFormatter);

            AddFieldToGroup("SP_TYPE", GroupSpectralType);
            AddFieldToGroup("SP_QUAL", GroupSpectralType, QualityFormatter);
            AddFieldToGroup("SP_BIBCODE", GroupSpectralType, BibcodeFormatter);

            AddFieldToGroup("MORPH_TYPE", GroupMorphology);
            AddFieldToGroup("MORPH_QUAL", GroupMorphology, QualityFormatter);
            AddFieldToGroup("MORPH_BIBCODE", GroupMorphology, BibcodeFormatter);

            AddFieldToGroup("GALDIM_MAJAXIS", GroupGalaxyDimensions, GalaxyDimensionsFormatter);
            AddFieldToGroup("GALDIM_MINAXIS", GroupGalaxyDimensions, GalaxyDimensionsFormatter);
            AddFieldToGroup("GALDIM_ANGLE", GroupGalaxyDimensions, DegreesFormatter);
            AddFieldToGroup("GALDIM_QUAL", GroupGalaxyDimensions, QualityFormatter);
            AddFieldToGroup("GALDIM_WAVELENGTH", GroupGalaxyDimensions);
            AddFieldToGroup("GALDIM_BIBCODE", GroupGalaxyDimensions, BibcodeFormatter);

            AddFieldToGroup("IDS", GroupIdentifiers, IdsFormatter);
        }

        private static void AddFieldToGroup(string fieldId, string groupName, IEphemFormatter formatter = null)
        {
            FieldToGroupMap[fieldId] = groupName;
            FieldFormatters[fieldId] = formatter ?? Formatters.Simple;
        }
    }

    internal class BibcodeFormatter : SimpleFormatter { }

    internal class ExcludedFormatter : SimpleFormatter { }

    internal class IdsFormatter : IEphemFormatter
    {
        public string Format(object value)
        {
            string ids = value as string;
            if (ids == null)
                return null;
            else
                return string.Join(", ", ids.Split('|'));
        }
    }

    internal class TypeFormatter : IEphemFormatter
    {
        public string Format(object value)
        {
            return Text.Get($"Simbad.ObjectType.{value}");
        }
    }

    internal class UnitsFormatter : IEphemFormatter
    {
        private readonly string units;

        public UnitsFormatter(string units)
        {
            this.units = units;
        }

        public string Format(object value)
        {
            return $"{value}{units}";
        }
    }

    internal class RAFormatter : IEphemFormatter
    {
        private IEphemFormatter formatter = Formatters.RA;
        public string Format(object value)
        {
            return formatter.Format(new HMS(value.ToString()).ToDecimalAngle());
        }
    }

    internal class DecFormatter : IEphemFormatter
    {
        private IEphemFormatter formatter = Formatters.Dec;
        public string Format(object value)
        {
            return formatter.Format(new DMS(value.ToString()).ToDecimalAngle());
        }
    }

    internal class CoordPrecisionFormatter : IEphemFormatter
    {
        // https://simbad.u-strasbg.fr/Pages/guide/sim-fscript.htx
        private string[] precisions = new string[] { "0.1°", "0.01°", "0.001°", "0.1′", "0.01′", "0.001′", "0.1′′", "0.01′′", "0.001′′" };

        public string Format(object value)
        {
            if (value is short precision && precision >= 0 && precision <= 8)
                return precisions[precision];
            else
                return null;
        }
    }

    internal class QualityFormatter : IEphemFormatter
    {
        // A:best, E:worst
        public string Format(object value)
        {
            return $"{value}";
        }
    }
}
