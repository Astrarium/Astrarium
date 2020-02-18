using static System.Math;
using static ADK.Angle;

namespace ADK
{
    public static class MartianMoons
    {
        /// <summary>
        /// Count of Mars moons 
        /// </summary>
        private const int MOONS_COUNT = 2;

        /// <summary>
        /// 1 a.u. (astronomical unit) in km
        /// </summary>
        private const double AU = 149597870;

        public static double Semidiameter(int index, double distance)
        {
            double[] d = { 22.5, 12.4 };
            return ToDegrees(Atan2(d[index - 1] / 2.0, AU)) / distance * 3600.0;
        }

        public static CrdsRectangular[] Positions(double jd, CrdsHeliocentrical earth, CrdsHeliocentrical mars)
        {
            CrdsRectangular[] moons = new CrdsRectangular[MOONS_COUNT];

            // Rectangular topocentrical coordinates of Mars
            CrdsRectangular rectMars = mars.ToRectangular(earth);

            // Ecliptical coordinates of Mars
            CrdsEcliptical eclMars = rectMars.ToEcliptical();

            // Distance from Earth to Mars, in AU
            double distanceMars = eclMars.Distance;

            // light-time effect
            double tau = PlanetPositions.LightTimeEffect(distanceMars);

            // ESAPHODEI model
            double t = jd - 2451545.0 + 6491.5 - tau;

            GenerateMarsSatToVSOP87(t, ref mars_sat_to_vsop87);

            // Get rectangular (Mars-reffered) coordinates of moons
            CrdsRectangular[] esaphodeiRect = new CrdsRectangular[MOONS_COUNT];

            for (int body = 0; body < MOONS_COUNT; body++)
            {
                MarsSatBody bp = mars_sat_bodies[body];

                double[] elem = new double[6];
                for (int n = 0; n < 6; n++)
                {
                    elem[n] = bp.constants[n];
                }

                for (int j = 0; j < 2; j++)
                {
                    for (int i = bp.lists[j].size - 1; i >= 0; i--)
                    {
                        double d = bp.lists[j].terms[i].phase + t * bp.lists[j].terms[i].frequency;
                        elem[j] += bp.lists[j].terms[i].amplitude * Cos(d);
                    }
                }

                for (int j = 2; j < 4; j++)
                {
                    for (int i = bp.lists[j].size - 1; i >= 0; i--)
                    {
                        double d = bp.lists[j].terms[i].phase + t * bp.lists[j].terms[i].frequency;
                        elem[2 * j - 2] += bp.lists[j].terms[i].amplitude * Cos(d);
                        elem[2 * j - 1] += bp.lists[j].terms[i].amplitude * Sin(d);
                    }
                }

                elem[1] += (bp.l + bp.acc * t) * t;

                double[] x = new double[3];
                EllipticToRectangularA(mars_sat_bodies[body].mu, elem, ref x);


                esaphodeiRect[body] = new CrdsRectangular();

                esaphodeiRect[body].X = mars_sat_to_vsop87[0] * x[0]
                                        + mars_sat_to_vsop87[1] * x[1]
                                        + mars_sat_to_vsop87[2] * x[2];
                esaphodeiRect[body].Y = mars_sat_to_vsop87[3] * x[0]
                                       + mars_sat_to_vsop87[4] * x[1]
                                       + mars_sat_to_vsop87[5] * x[2];
                esaphodeiRect[body].Z = mars_sat_to_vsop87[6] * x[0]
                                       + mars_sat_to_vsop87[7] * x[1]
                                       + mars_sat_to_vsop87[8] * x[2];

                moons[body] = new CrdsRectangular(
                    rectMars.X + esaphodeiRect[body].X,
                    rectMars.Y + esaphodeiRect[body].Y,
                    rectMars.Z + esaphodeiRect[body].Z
                );
            }

            return moons;
        }

        private const double ome0 = 47.68143;
        private const double inc0 = 37.1135;
        private const double dome = -0.1061;
        private const double dinc = 0.0609;

        private static double[] J2000_to_VSOP87 = {
           9.999999999998848e-01,-4.799655442984222e-07, 0.000000000000000e+00,
           4.403598133110236e-07, 9.174821370868568e-01, 3.977769829016507e-01,
          -1.909192461077750e-07,-3.977769829016049e-01, 9.174821370869625e-01};

        private static double[] mars_sat_to_vsop87 = new double[9];

        private static void GenerateMarsSatToVSOP87(double t, ref double[] mars_sat_to_vsop87)
        {
            t -= 6491.5;

            double ome = (ome0 + dome * t / 36525.0) * (PI / 180.0);
            double inc = (inc0 + dinc * t / 36525.0) * (PI / 180.0);
            double co = Cos(ome);
            double so = Sin(ome);
            double ci = Cos(inc);
            double si = Sin(inc);

            double[] m = new double[] {
                                    co,-ci*so, si*so,
                                    so, ci*co,-si*co,
                                    0.0, si, ci };

            MultMat(J2000_to_VSOP87, m, ref mars_sat_to_vsop87);
        }

        private static void EllipticToRectangularA(double mu, double[] elem, ref double[] xyz)
        {
            double a = elem[0];
            EllipticToRectangular(mu, a, elem, ref xyz);
        }

        private static void EllipticToRectangular(double mu, double a, double[] elem, ref double[] xyz)
        {
            double L = IEEERemainder(elem[1], 2.0 * PI);

            double Le = L - elem[2] * Sin(L) + elem[3] * Cos(L);
            for (; ; )
            {
                double cLe = Cos(Le);
                double sLe = Sin(Le);
                double dLe = (L - Le + elem[2] * sLe - elem[3] * cLe)
                                / (1.0 - elem[2] * cLe - elem[3] * sLe);
                Le += dLe;
                if (Abs(dLe) <= 1e-14) break; /* L1: <1e-12 */
            }

            {
                double cLe = Cos(Le);
                double sLe = Sin(Le);

                double dlf = -elem[2] * sLe + elem[3] * cLe;
                double phi = Sqrt(1.0 - elem[2] * elem[2] - elem[3] * elem[3]);
                double psi = 1.0 / (1.0 + phi);

                double x1 = a * (cLe - elem[2] - psi * dlf * elem[3]);
                double y1 = a * (sLe - elem[3] + psi * dlf * elem[2]);

                double elem_4q = elem[4] * elem[4];
                double elem_5q = elem[5] * elem[5];
                double dwho = 2.0 * Sqrt(1.0 - elem_4q - elem_5q);
                double rtp = 1.0 - elem_5q - elem_5q;
                double rtq = 1.0 - elem_4q - elem_4q;
                double rdg = 2.0 * elem[5] * elem[4];

                xyz[0] = x1 * rtp + y1 * rdg;
                xyz[1] = x1 * rdg + y1 * rtq;
                xyz[2] = (-x1 * elem[5] + y1 * elem[4]) * dwho;
            }
        }

        private static void MultMat(double[] a, double[] b, ref double[] c)
        {
            int i, j;
            for (i = 0; i < 3; i++)
            {
                for (j = 0; j < 3; j++)
                {
                    c[i * 3 + j] = a[i * 3] * b[j] + a[i * 3 + 1] * b[3 + j] + a[i * 3 + 2] * b[6 + j];
                }
            }
        }

        private class MarsSatTerm
        {
            public double phase;
            public double frequency;
            public double amplitude;
            public MarsSatTerm(double phase, double frequency, double amplitude)
            {
                this.phase = phase;
                this.frequency = frequency;
                this.amplitude = amplitude;
            }
        }

        private class MarsSatTermList
        {
            public MarsSatTerm[] terms;
            public int size;

            public MarsSatTermList(MarsSatTerm[] terms, int size)
            {
                this.terms = terms;
                this.size = size;
            }
        }

        private class MarsSatBody
        {
            public double mu;
            public double l;
            public double acc;
            public double[] constants;         // 6 items
            public MarsSatTermList[] lists;    // 4 items

            public MarsSatBody(double mu, double l, double acc, double[] constants, MarsSatTermList[] lists)
            {
                this.mu = mu;
                this.l = l;
                this.acc = acc;
                this.constants = constants;
                this.lists = lists;
            }

        }

        #region Satellite Terms

        private static MarsSatTerm[] mars_sat_phobos_0 /*16*/ = {
            new MarsSatTerm (5.013490350126586e+00, 2.715567382195733e+01, 4.537539999999999e-09),
            new MarsSatTerm (6.839910590780000e-01, 1.969446151585829e+01, 7.312000000000000e-10),
            new MarsSatTerm (4.514031245592586e+00, 4.073350897245015e+01, 7.785599999999993e-10),
            new MarsSatTerm (5.531351678889586e+00, 1.357783661756435e+01, 3.529399999999985e-10),
            new MarsSatTerm (5.700307292822586e+00, 4.685013393001369e+01, 2.094599999999994e-10),
            new MarsSatTerm (4.228062220664587e+00, 5.431134764391466e+01, 9.139999999999999e-11),
            new MarsSatTerm (1.167907583017500e+00, 7.461211875946264e+00, 6.204000000000000e-11),
            new MarsSatTerm (5.197274703601086e+00, 6.042797388545593e+01, 5.295999999999951e-11),
            new MarsSatTerm (5.160045430868086e+00, 3.938582310889583e+01, 1.811999999999999e-11),
            new MarsSatTerm (2.223277229900500e+00, 5.777452001970681e+01, 1.563999999999897e-11),
            new MarsSatTerm (6.921616240614999e-01, 2.103904892768837e+01, 1.535999999999999e-11),
            new MarsSatTerm (6.215802752555586e+00, 3.327229783044108e+01, 1.304000000000000e-11),
            new MarsSatTerm (1.368231494011000e+00, 3.938892179708230e+01, 1.661999999999999e-11),
            new MarsSatTerm (2.601539199675000e-01, 7.446011234647927e+00, 1.328000000000000e-11),
            new MarsSatTerm (5.903845687100000e-02, 3.941932465627423e+01, 8.559999999999978e-12),
            new MarsSatTerm (6.179310844209586e+00, 5.911910805492755e+01, 7.240000000000000e-12)
        };

        private static MarsSatTerm[] mars_sat_phobos_1 /*42*/ = {
            new MarsSatTerm ( 5.334169568271586e+00, 9.145914066599032e-03, 5.016848130000000e-05),
            new MarsSatTerm ( 5.334169148295587e+00, 9.145914113327495e-03, 5.016848130000000e-05),
            new MarsSatTerm ( 3.805345265960000e-01, 1.540952541904328e-03, 3.350264970000000e-05),
            new MarsSatTerm ( 3.805339484960000e-01, 1.540952613104260e-03, 3.350264903000000e-05),
            new MarsSatTerm ( 5.288363718535586e+00, 1.829243888046791e-02, 2.827839093000000e-05),
            new MarsSatTerm ( 5.288363809359586e+00, 1.829243887034939e-02, 2.827839093000000e-05),
            new MarsSatTerm ( 3.412685718051586e+00, 7.604696023359408e-03, 1.578862428000000e-05),
            new MarsSatTerm ( 3.412685998796586e+00, 7.604695992157328e-03, 1.578862427000000e-05),
            new MarsSatTerm ( 3.448999855869586e+00, 2.715567616927210e+01, 3.008844320000000e-05),
            new MarsSatTerm ( 9.194445444030001e-01, 3.101144183892481e-03, 8.178563170000001e-06),
            new MarsSatTerm ( 9.194443192780000e-01, 3.101144208596499e-03, 8.178563340000000e-06),
            new MarsSatTerm ( 5.387625479307586e+00, 1.969446275049270e+01, 2.044327843999999e-05),
            new MarsSatTerm ( 2.848996739890000e+00, 2.743829098982358e-02, 6.516951720000000e-06),
            new MarsSatTerm ( 2.848996748767000e+00, 2.743829098862537e-02, 6.516951720000000e-06),
            new MarsSatTerm ( 3.954411167839586e+00, 1.357783808463605e+01, 1.013316988000000e-05),
            new MarsSatTerm ( 2.912659283736000e+00, 4.073351425390815e+01, 7.483468759999740e-06),
            new MarsSatTerm ( 4.067858164824586e+00, 2.589727369338346e-02, 2.589359390000000e-06),
            new MarsSatTerm ( 4.067857973471586e+00, 2.589727371473152e-02, 2.589359390000000e-06),
            new MarsSatTerm ( 3.776271613850000e-01, 3.658417828853650e-02, 1.125147420000000e-06),
            new MarsSatTerm ( 3.776267985560000e-01, 3.658417832900902e-02, 1.125147420000000e-06),
            new MarsSatTerm ( 4.121936540919586e+00, 4.685014091671000e+01, 1.523532599999976e-06),
            new MarsSatTerm ( 1.231836210236000e+00, 1.674957754399965e-02, 4.976512300000000e-07),
            new MarsSatTerm ( 1.231836585135000e+00, 1.674957750247454e-02, 4.976512300000000e-07),
            new MarsSatTerm ( 2.488083095032000e+00, 1.520386207875350e-02, 6.841966800000000e-07),
            new MarsSatTerm ( 2.488083105051000e+00, 1.520386207728190e-02, 6.841966800000000e-07),
            new MarsSatTerm ( 1.576141880659500e+00, 3.504321962396548e-02, 1.124868580000000e-06),
            new MarsSatTerm ( 6.145328698142587e+00, 6.116624841770791e+00, 7.873411399999999e-07),
            new MarsSatTerm ( 2.626756105114500e+00, 5.431135233854420e+01, 1.040645720000000e-06),
            new MarsSatTerm ( 2.759579551372000e+00, 7.461213382164448e+00, 7.166789399999994e-07),
            new MarsSatTerm ( 6.005673273380586e+00, 1.314187651672942e+00, 6.022022799999936e-07),
            new MarsSatTerm ( 9.353090056600000e-02, 7.532115638546220e-04, 1.964976900000000e-07),
            new MarsSatTerm ( 9.343265452500001e-02, 7.532237916966188e-04, 1.964964500000000e-07),
            new MarsSatTerm ( 4.302928383505000e-01, 3.938582557777600e+01, 4.987307800000000e-07),
            new MarsSatTerm ( 3.587439959397586e+00, 6.042797969706294e+01, 4.804827999999994e-07),
            new MarsSatTerm ( 4.183547650369587e+00, 4.572998195120822e-02, 1.747133100000000e-07),
            new MarsSatTerm ( 4.183547815926586e+00, 4.572998193123842e-02, 1.747133100000000e-07),
            new MarsSatTerm ( 5.589444380180586e+00, 6.068411018120026e-03, 1.793076700000000e-07),
            new MarsSatTerm ( 5.589445119118587e+00, 6.068410936124942e-03, 1.793076800000000e-07),
            new MarsSatTerm ( 6.062696336024086e+00, 3.938892673561993e+01, 2.653755199999908e-07),
            new MarsSatTerm ( 4.628882088425586e+00, 3.327230135427769e+01, 2.539272800000000e-07),
            new MarsSatTerm ( 4.736462571780586e+00, 1.224558395734566e-02, 1.254715400000000e-07),
            new MarsSatTerm ( 4.736462588777586e+00, 1.224558396211886e-02, 1.254715500000000e-07)
        };

        private static MarsSatTerm[] mars_sat_phobos_2 /*27*/ = {
            new MarsSatTerm ( 1.404382124885000e+00, 7.595588511174286e-03, 1.514110912521000e-02),
            new MarsSatTerm ( 2.088385523034000e+00, 1.970205682773437e+01, 3.849620867400000e-04),
            new MarsSatTerm ( 3.895377361148586e+00, 1.069670280268190e-02, 6.903413242000000e-05),
            new MarsSatTerm ( 4.989572094750000e-01,-7.605272825959576e-03, 4.946101994000000e-05),
            new MarsSatTerm ( 8.215210221720000e-01, 4.685772969905357e+01, 3.671320788000000e-05),
            new MarsSatTerm ( 3.378076387479586e+00,-7.453616316894914e+00, 3.267983782000000e-05),
            new MarsSatTerm ( 2.772375719698000e+00, 3.939651873194720e+01, 8.750483050000000e-06),
            new MarsSatTerm ( 2.839303414769000e+00, 6.124220388371520e+00, 6.170938810000000e-06),
            new MarsSatTerm ( 3.184503170430000e-01, 6.043556479692822e+01, 6.957005940000000e-06),
            new MarsSatTerm ( 1.410329121503000e+00, 1.984322681200531e-02, 5.734088960000000e-06),
            new MarsSatTerm ( 1.336796551701000e+00, 3.327989343110183e+01, 3.431577860000000e-06),
            new MarsSatTerm ( 3.853646745987586e+00,-2.103145308554279e+01, 4.037409510000000e-06),
            new MarsSatTerm ( 2.547067366210000e-01, 1.549665594221090e-03, 2.115296220000000e-06),
            new MarsSatTerm ( 1.508224230072000e+00,-5.911151611514718e+01, 1.904827620000000e-06),
            new MarsSatTerm ( 3.462607551400000e-02,-5.165030141480156e+01, 8.696094700000001e-07),
            new MarsSatTerm ( 3.198655314207586e+00, 9.152774292423818e-03, 6.329868200000000e-07),
            new MarsSatTerm ( 5.163954670916587e+00, 1.674080796065678e-02, 6.130317800000000e-07),
            new MarsSatTerm ( 7.803937436990001e-01,-1.550610622055567e-03, 5.737217100000000e-07),
            new MarsSatTerm ( 3.264588234538586e+00, 2.716326950857758e+01, 8.181088300000000e-07),
            new MarsSatTerm ( 5.211329429707586e+00, 2.898903518276566e-02, 7.434145300000000e-07),
            new MarsSatTerm ( 2.691716763490000e+00,-2.714807769012140e+01, 5.441647800000001e-07),
            new MarsSatTerm ( 1.001733960626000e+00,-4.553367734785991e+01, 5.044515000000000e-07),
            new MarsSatTerm ( 3.212149602842586e+00,-1.968376490094933e+01, 4.284599100000000e-07),
            new MarsSatTerm ( 4.145658249560586e+00,-3.460928932187340e+01, 4.224937300000000e-07),
            new MarsSatTerm ( 5.063879137454586e+00, 2.589097180762378e-02, 3.581949800000000e-07),
            new MarsSatTerm ( 2.558639707373000e+00, 6.064928432880783e-03, 3.808881200000000e-07),
            new MarsSatTerm ( 5.292499025555586e+00, 3.075065445597794e-03, 4.055739200000000e-07)
        };

        private static MarsSatTerm[] mars_sat_phobos_3 /*28*/ = {
            new MarsSatTerm ( 2.058107128488000e+00,-7.604861328578004e-03, 9.408605183120001e-03),
            new MarsSatTerm ( 3.248856489376586e+00,-9.145863467943084e-03, 5.699538102000000e-05),
            new MarsSatTerm ( 4.557280987272586e+00, 1.829238959116374e-02, 2.474369483000000e-05),
            new MarsSatTerm ( 6.113423353213586e+00, 7.595702278066188e-03, 2.062715397000000e-05),
            new MarsSatTerm ( 2.067112903892000e+00, 2.743825478464904e-02, 5.945574160000000e-06),
            new MarsSatTerm ( 1.700689207004000e+00, 9.145729527513899e-03, 3.920190200000000e-06),
            new MarsSatTerm ( 3.577059353926586e+00,-1.829259493439447e-02, 2.167791970000000e-06),
            new MarsSatTerm ( 4.940284891770586e+00,-7.453616316894914e+00, 2.769763750000000e-06),
            new MarsSatTerm ( 2.117885747855000e+00, 3.941171892425920e+01, 1.813539170000000e-06),
            new MarsSatTerm ( 5.176055365230000e-01, 1.970205682773437e+01, 1.083429180000000e-06),
            new MarsSatTerm ( 5.864248782211586e+00, 3.658425999636452e-02, 1.066657010000000e-06),
            new MarsSatTerm ( 3.240692168340586e+00, 2.589639535977297e-02, 7.909853300000000e-07),
            new MarsSatTerm ( 7.570751994340000e-01,-1.321790869500707e+00, 8.880885099999999e-07),
            new MarsSatTerm ( 6.608910796060000e-01, 6.124220388371520e+00, 5.520758900000000e-07),
            new MarsSatTerm ( 5.519669134088586e+00, 4.685772969905357e+01, 4.397886000000000e-07),
            new MarsSatTerm ( 5.844000094873587e+00,-2.743825733444110e-02, 4.583609100000000e-07),
            new MarsSatTerm ( 4.602755395228586e+00,-1.675516082048598e-02, 3.533131100000000e-07),
            new MarsSatTerm ( 2.685649467156000e+00, 1.542518910643103e-03, 3.502988500000000e-07),
            new MarsSatTerm ( 3.353176877809586e+00, 1.225604581839110e+01, 3.828851700000000e-07),
            new MarsSatTerm ( 2.556761413073000e+00, 1.068971473943851e-02, 2.711247300000000e-07),
            new MarsSatTerm ( 4.622369718415587e+00,-2.589682634097252e-02, 1.991745400000000e-07),
            new MarsSatTerm ( 7.497690894330000e-01, 3.504362063026418e-02, 2.036897700000000e-07),
            new MarsSatTerm ( 3.965205602751586e+00, 2.714806830611846e+01, 1.726779600000000e-07),
            new MarsSatTerm ( 4.743858488241586e+00,-2.103145308554279e+01, 1.574472800000000e-07),
            new MarsSatTerm ( 3.405870070134586e+00, 4.572380100924578e-02, 1.698567700000000e-07),
            new MarsSatTerm ( 3.742380849210000e-01, 3.327989343110183e+01, 1.015004500000000e-07),
            new MarsSatTerm ( 1.374678754422000e+00,-1.970206671243377e+01, 8.259149000000001e-08),
            new MarsSatTerm ( 1.140160118874000e+00,-6.108652383295211e-03, 8.504923000000000e-08),
        };

        private static MarsSatTerm[] mars_sat_deimos_0 /*25*/ = {
            new MarsSatTerm ( 6.146803530569087e+00, 2.294413036846356e+00, 5.398360000000000e-09),
            new MarsSatTerm ( 3.432805186019586e+00, 9.935735425667586e+00, 7.095199999999997e-10),
            new MarsSatTerm ( 4.356581106602587e+00, 3.441619555269535e+00, 3.737800000000000e-10),
            new MarsSatTerm ( 5.915601483397086e+00, 9.926589650598594e+00, 2.337400000000000e-10),
            new MarsSatTerm ( 1.615243161446500e+00, 1.147206518423178e+00, 1.697000000000000e-10),
            new MarsSatTerm ( 2.114704906116000e+00, 9.917443791162608e+00, 5.314000000000000e-11),
            new MarsSatTerm ( 9.393388277920001e-01, 9.954027953247621e+00, 7.046000000000000e-11),
            new MarsSatTerm ( 3.949494792732586e+00, 9.944881541450725e+00, 4.233999999999998e-11),
            new MarsSatTerm ( 2.089322391969500e+00, 2.294097606691310e+00, 2.813999999999983e-11),
            new MarsSatTerm ( 7.795571756865000e-01, 2.294728469946876e+00, 2.549999999999971e-11),
            new MarsSatTerm ( 2.361851669664500e+00, 4.588826073692712e+00, 1.676000000000000e-11),
            new MarsSatTerm ( 2.770026883578500e+00, 9.954658773106329e+00, 9.719999999999976e-12),
            new MarsSatTerm ( 4.733522308629587e+00, 9.963173826265681e+00, 9.519999999999998e-12),
            new MarsSatTerm ( 3.258781379519086e+00, 1.472506551307767e+01, 9.739999999999990e-12),
            new MarsSatTerm ( 4.596299836113586e+00, 9.908297894321850e+00, 1.027999999999982e-11),
            new MarsSatTerm ( 3.057688443346500e+00, 2.682916339604879e+00, 6.679999999999999e-12),
            new MarsSatTerm ( 5.453932177963086e+00, 2.682285947505377e+00, 6.879999999999995e-12),
            new MarsSatTerm ( 4.328499239462586e+00, 9.936050924435195e+00, 6.199999999999996e-12),
            new MarsSatTerm ( 2.168321380562000e+00, 4.976699000854042e+00, 5.359999999999916e-12),
            new MarsSatTerm ( 2.516818588675000e+00, 9.935420104041299e+00, 3.359999999999995e-12),
            new MarsSatTerm ( 2.997827070725000e-01, 3.441304123224338e+00, 2.919999999999998e-12),
            new MarsSatTerm ( 2.143143420462500e+00, 2.682600911813873e+00, 2.979999999999999e-12),
            new MarsSatTerm ( 5.271781540988586e+00, 3.441934988580561e+00, 2.640000000000000e-12),
            new MarsSatTerm ( 5.382287205930000e-01, 9.926905039305762e+00, 2.039999999999966e-12),
            new MarsSatTerm ( 3.629187963751586e+00, 2.303558945733479e+00, 1.090000000000000e-12),
        };

        private static MarsSatTerm[] mars_sat_deimos_1 /*21*/ = {
            new MarsSatTerm ( 2.488175621789000e+00, 3.153377646687549e-04, 2.483031660190000e-03),
            new MarsSatTerm ( 2.488175634463000e+00, 3.153377641537758e-04, 2.483031659490000e-03),
            new MarsSatTerm ( 5.336889264964586e+00, 9.145915158660612e-03, 2.023426978200000e-04),
            new MarsSatTerm ( 5.336889346905586e+00, 9.145915155199243e-03, 2.023426978200000e-04),
            new MarsSatTerm ( 5.281538321363586e+00, 1.829244561727392e-02, 1.043167564200000e-04),
            new MarsSatTerm ( 5.281537958522586e+00, 1.829244563271799e-02, 1.043167564300000e-04),
            new MarsSatTerm ( 1.433490018835500e+00, 2.294413045224799e+00, 1.637133774599998e-04),
            new MarsSatTerm ( 2.852007788337500e+00, 2.743818628432718e-02, 4.820671420000000e-05),
            new MarsSatTerm ( 3.147176604035586e+00, 1.860783561096842e-02, 1.497066023000000e-05),
            new MarsSatTerm ( 3.147176307034586e+00, 1.860783562364318e-02, 1.497066034000000e-05),
            new MarsSatTerm ( 3.182516662993086e+00, 1.147206709032795e+00, 1.137028858000000e-05),
            new MarsSatTerm ( 5.917181875408586e+00, 3.441620120814550e+00, 8.350609860000001e-06),
            new MarsSatTerm ( 3.544871659744586e+00, 6.190897763118658e-04, 4.383254500000000e-06),
            new MarsSatTerm ( 3.544871649035586e+00, 6.190897769462623e-04, 4.383254500000000e-06),
            new MarsSatTerm ( 4.614193383590000e-01, 3.657933964533806e-02, 8.316176420000001e-06),
            new MarsSatTerm ( 6.754222685930000e-01, 2.775277029221846e-02, 6.447587580000001e-06),
            new MarsSatTerm ( 5.001640850015586e+00, 9.935735582783501e+00, 7.926687400000001e-06),
            new MarsSatTerm ( 4.485218623803586e+00, 8.828439970895009e-03, 3.905064210000000e-06),
            new MarsSatTerm ( 2.221013917980000e-01, 9.464721723028089e-03, 3.117801830000000e-06),
            new MarsSatTerm ( 4.485218622895586e+00, 8.828439971019056e-03, 3.905064210000000e-06),
            new MarsSatTerm ( 2.221013944660000e-01, 9.464721722885516e-03, 3.117801830000000e-06),
        };

        private static MarsSatTerm[] mars_sat_deimos_2 /*15*/ = {
            new MarsSatTerm ( 2.198649419514000e+00, 3.148401892560942e-04, 2.744131534600000e-04),
            new MarsSatTerm ( 4.366636518977586e+00, 4.977013897776327e+00, 6.015912711000000e-05),
            new MarsSatTerm ( 1.463816210370000e+00,-3.153164045300449e-04, 3.614585349000000e-05),
            new MarsSatTerm ( 1.362467588064000e+00, 2.682600831640474e+00, 2.577157903000000e-05),
            new MarsSatTerm ( 9.336792893320000e-01,-4.958721582505435e+00, 6.801997830000000e-06),
            new MarsSatTerm ( 3.152432010349586e+00, 1.535394310272536e+00, 4.452979140000000e-06),
            new MarsSatTerm ( 4.734051493918586e+00,-4.949575691510945e+00, 2.238482920000000e-06),
            new MarsSatTerm ( 5.736642853342587e+00, 2.327524649334390e-04, 1.478747570000000e-06),
            new MarsSatTerm ( 5.084945958505586e+00, 3.912335403634271e-04, 1.457720040000000e-06),
            new MarsSatTerm ( 4.232770650771586e+00, 7.271426904652134e+00, 1.347030310000000e-06),
            new MarsSatTerm ( 1.516439412109000e+00, 1.491274947692300e+01, 7.538179100000001e-07),
            new MarsSatTerm ( 3.100618466711000e+00, 1.797748798526167e-02, 8.716229700000000e-07),
            new MarsSatTerm ( 5.155432690964586e+00, 3.881877876517008e-01, 1.015648880000000e-06),
            new MarsSatTerm ( 2.250768124831000e+00,-4.940429834739583e+00, 5.090989300000000e-07),
            new MarsSatTerm ( 3.422251338868586e+00,-4.977013897776327e+00, 6.537077500000000e-07),
        };

        private static MarsSatTerm[] mars_sat_deimos_3/*27*/ = {
            new MarsSatTerm ( 2.981506933511000e+00,-3.154811355556041e-04, 1.562693319959000e-02),
            new MarsSatTerm ( 4.557500894366586e+00, 1.829233626168517e-02, 1.321818631100000e-04),
            new MarsSatTerm ( 3.248124065112586e+00,-9.145934570587952e-03, 3.833652719000000e-05),
            new MarsSatTerm ( 1.701551338710000e+00, 9.145665986601770e-03, 2.660211842000000e-05),
            new MarsSatTerm ( 2.067662003165000e+00, 2.743821959085702e-02, 2.882438535000000e-05),
            new MarsSatTerm ( 4.805327222478586e+00, 3.158564952832469e-04, 2.713213911000000e-05),
            new MarsSatTerm ( 2.318788380092000e+00, 1.860776219134252e-02, 9.296324950000000e-06),
            new MarsSatTerm ( 3.613580709014586e+00,-1.829261290520875e-02, 4.460960290000000e-06),
            new MarsSatTerm ( 5.867810550672586e+00, 3.658405181871115e-02, 4.904156030000000e-06),
            new MarsSatTerm ( 5.496515547817586e+00,-9.461294099629817e-03, 2.366881150000000e-06),
            new MarsSatTerm ( 6.120362957845586e+00, 2.775361064625920e-02, 2.057552850000000e-06),
            new MarsSatTerm ( 3.612389208841586e+00, 8.830178627407989e-03, 2.365750140000000e-06),
            new MarsSatTerm ( 5.554163001934587e+00,-1.860800990265229e-02, 1.228814440000000e-06),
            new MarsSatTerm ( 3.561485983136586e+00, 1.797648948023003e-02, 1.215189460000000e-06),
            new MarsSatTerm ( 1.941975361869000e+00,-6.259843043363501e-04, 9.602856999999999e-07),
            new MarsSatTerm ( 5.845244776759587e+00,-2.743819348074506e-02, 1.174471770000000e-06),
            new MarsSatTerm ( 1.821048365911000e+00, 9.460829229838182e-03, 1.058441050000000e-06),
            new MarsSatTerm ( 7.587135454610000e-01, 1.682177864988152e-04, 2.383147490000000e-06),
            new MarsSatTerm ( 3.292891638604586e+00, 4.573365667404765e-02, 7.656303800000000e-07),
            new MarsSatTerm ( 3.766016108380586e+00, 3.683140215171970e-02, 4.075824300000000e-07),
            new MarsSatTerm ( 5.747244956219586e+00, 9.954342722363187e+00, 4.962340900000000e-07),
            new MarsSatTerm ( 1.724062868741000e+00,-2.774017744689255e-02, 3.003077900000000e-07),
            new MarsSatTerm ( 4.429747729642586e+00,-8.904754331211824e-03, 2.613416400000000e-07),
            new MarsSatTerm ( 8.659161909010000e-01, 2.718340176462802e-02, 2.891945200000000e-07),
            new MarsSatTerm ( 2.259603352363000e+00,-3.660152378331885e-02, 2.213788200000000e-07),
            new MarsSatTerm ( 1.108725025984000e+00, 9.935732440466172e+00, 2.449950300000000e-07),
            new MarsSatTerm ( 1.680391963791000e+00, 9.076040299352415e-03, 1.652416600000000e-07),
        };

        #endregion Satellite Terms

        private static MarsSatBody[] mars_sat_bodies =
        {
            new MarsSatBody (
                9.549547741038312e-11,
                1.970205562831390e+01,
                1.657852042683113e-10,
                new double[]
                {
                    0.0000626916188000,
                    2.0912973926417302,
                    -0.0000005774003141,
                    0.0000006185052790,
                    -0.0000575018919826,
                    -0.0000540676493351
                },
                new MarsSatTermList[]
                {
                    new MarsSatTermList(mars_sat_phobos_0, 16),
                    new MarsSatTermList(mars_sat_phobos_1, 42),
                    new MarsSatTermList(mars_sat_phobos_2, 27),
                    new MarsSatTermList(mars_sat_phobos_3, 28)
                }
            ),
            new MarsSatBody (
                9.549547622768120e-11,
                4.977013889652300e+00,
                -2.331793571572226e-14,
                new double[]
                {
                    0.0001568134086700,
                    -1.9167537810740201,
                    -0.0000239579419518,
                    0.0000257893300229,
                    -0.0056443721379635,
                    -0.0053121806978560
                },
                new MarsSatTermList[]
                {
                    new MarsSatTermList(mars_sat_deimos_0, 25),
                    new MarsSatTermList(mars_sat_deimos_1, 21),
                    new MarsSatTermList(mars_sat_deimos_2, 15),
                    new MarsSatTermList(mars_sat_deimos_3, 27)
                }
            )
        };
    }
}
