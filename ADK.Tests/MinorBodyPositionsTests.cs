using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ADK.Tests
{
    [TestClass]
    public class MinorBodyPositionsTests
    {
        private readonly CrdsGeographical location = new CrdsGeographical(-44.0, 56.3333);

        private CrdsRectangular SunRectangular(double jd, double epsilon)
        {
            CrdsHeliocentrical hEarth = PlanetPositions.GetPlanetCoordinates(3, jd, true, false);

            var eSun = new CrdsEcliptical(Angle.To360(hEarth.L + 180), -hEarth.B, hEarth.R);

            // Corrected solar coordinates to FK5 system
            // NO correction for nutation and aberration should be performed here (ch. 26, p. 171)
            eSun += PlanetPositions.CorrectionForFK5(jd, eSun);

            return eSun.ToRectangular(epsilon);
        }

        private void DoTest(CrdsGeographical location, OrbitalElements oe, string testData, double errorR, double errorEq)
        {
            Regex regex = new Regex("^(\\S+)\\s+(\\S+)\\s+(\\S+)\\s+(\\S+)\\s+(\\S+ \\S+ \\S+) (\\S+ \\S+ \\S+)$");

            string[] lines = testData.Split('\n');
            foreach (string line in lines)
            {
                string dataLine = line.Trim();
                if (!string.IsNullOrEmpty(dataLine))
                {
                    var match = regex.Match(dataLine);
                    double jd = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                    double X = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                    double Y = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                    double Z = double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                    string ra = match.Groups[5].Value;
                    string dec = match.Groups[6].Value;

                    var eqTest = new CrdsEquatorial(new HMS(ra), new DMS(dec));

                    var nutation = Nutation.NutationElements(jd);

                    var aberration = Aberration.AberrationElements(jd);

                    // True obliquity
                    double epsilon = Date.TrueObliquity(jd, nutation.deltaEpsilon);

                    // final difference to stop iteration process, 1 second of time
                    double deltaTau = TimeSpan.FromSeconds(1).TotalDays;

                    // time taken by the light to reach the Earth
                    double tau = 0;

                    // previous value of tau to calculate the difference
                    double tau0 = 1;

                    // Rectangular coordinates of minor body
                    CrdsRectangular r = null;

                    // Rectangular coordinates of the Sun
                    var sun = SunRectangular(jd, epsilon);

                    // Distance to the Earth
                    double Delta = 0;

                    // Iterative process to find rectangular coordinates of minor body
                    while (Math.Abs(tau - tau0) > deltaTau)
                    {
                        // Rectangular coordinates of minor body
                        r = MinorBodyPositions.GetRectangularCoordinates(oe, jd - tau, epsilon);

                        double ksi = sun.X + r.X;
                        double eta = sun.Y + r.Y;
                        double zeta = sun.Z + r.Z;

                        // Distance to the Earth
                        Delta = Math.Sqrt(ksi * ksi + eta * eta + zeta * zeta);

                        tau0 = tau;
                        tau = PlanetPositions.LightTimeEffect(Delta);
                    }

                    // Test heliocentric rectangular coordinates
                    Assert.AreEqual(X, r.X, errorR);
                    Assert.AreEqual(Y, r.Y, errorR);
                    Assert.AreEqual(Z, r.Z, errorR);

                    double x = sun.X + r.X;
                    double y = sun.Y + r.Y;
                    double z = sun.Z + r.Z;

                    double alpha = Angle.ToDegrees(Math.Atan2(y, x));
                    double delta = Angle.ToDegrees(Math.Asin(z / Delta));

                    var eq0 = new CrdsEquatorial(alpha, delta);

                    var theta0 = Date.ApparentSiderealTime(jd, nutation.deltaPsi, epsilon);

                    var parallax = PlanetEphem.Parallax(Delta);

                    var eq = eq0.ToTopocentric(location, theta0, parallax);

                    // Test equatorial coordinates
                    Assert.AreEqual(eqTest.Alpha, eq.Alpha, errorEq / 3600.0);
                    Assert.AreEqual(eqTest.Delta, eq.Delta, errorEq / 3600.0);
                }
            }
        }

        [TestMethod]
        public void TestEllipticOrbit()
        {
            // Comet 46P/Wirtanen, elliptic orbit
            // Orbital elements obtained from https://minorplanetcenter.net
            var oe = new OrbitalElements()
            {
                Epoch = 2458465.4393,
                Omega = 82.1527,
                e = 0.658678,
                i = 11.745,
                omega = 356.3589,
                q = 1.055378
            };

            // Rectangular heliocentric coorinates and
            // Equatorial topocentric coordinates for J2000.0 epoch            
            // Test data obtained from https://cgi.minorplanetcenter.net/cgi-bin/mpeph2.cgi

            //  [Julian Day   ]    [X          ]   [Y          ]     [Z        ]    [RA      ] [Dec     ]
            string testData = @"
                2458635.5000000     -2.168125383    -0.452458775     0.281166749    11 07 51.1 +19 26 55
                2458636.5000000     -2.174652552    -0.463133341     0.277657817    11 09 05.8 +19 13 08
                2458637.5000000     -2.181122527    -0.473795725     0.274141583    11 10 20.6 +18 59 25
                2458638.5000000     -2.187535752    -0.484445783     0.270618216    11 11 35.5 +18 45 44
                2458639.5000000     -2.193892669    -0.495083371     0.267087882    11 12 50.5 +18 32 07
                2458640.5000000     -2.200193714    -0.505708352     0.263550746    11 14 05.6 +18 18 33
                2458641.5000000     -2.206439317    -0.516320590     0.260006969    11 15 20.8 +18 05 02
                2458642.5000000     -2.212629906    -0.526919955     0.256456708    11 16 36.1 +17 51 34
                2458643.5000000     -2.218765903    -0.537506320     0.252900120    11 17 51.5 +17 38 09
                2458644.5000000     -2.224847727    -0.548079562     0.249337355    11 19 07.0 +17 24 47
                2458645.5000000     -2.230875790    -0.558639562     0.245768566    11 20 22.5 +17 11 28
                2458646.5000000     -2.236850502    -0.569186204     0.242193898    11 21 38.1 +16 58 12
                2458647.5000000     -2.242772269    -0.579719375     0.238613496    11 22 53.8 +16 44 59
                2458648.5000000     -2.248641491    -0.590238965     0.235027503    11 24 09.6 +16 31 49
                2458649.5000000     -2.254458565    -0.600744869     0.231436059    11 25 25.4 +16 18 42
                2458650.5000000     -2.260223883    -0.611236983     0.227839301    11 26 41.2 +16 05 38
                2458651.5000000     -2.265937834    -0.621715207     0.224237364    11 27 57.1 +15 52 37
                2458652.5000000     -2.271600802    -0.632179445     0.220630381    11 29 13.1 +15 39 39
                2458653.5000000     -2.277213168    -0.642629603     0.217018481    11 30 29.1 +15 26 43
                2458654.5000000     -2.282775307    -0.653065589     0.213401794    11 31 45.1 +15 13 50
                2458655.5000000     -2.288287593    -0.663487315     0.209780446    11 33 01.2 +15 01 00
                2458656.5000000     -2.293750394    -0.673894695     0.206154559    11 34 17.4 +14 48 13
                2458657.5000000     -2.299164076    -0.684287647     0.202524257    11 35 33.5 +14 35 29
                2458658.5000000     -2.304528998    -0.694666090     0.198889658    11 36 49.8 +14 22 47
                2458659.5000000     -2.309845520    -0.705029945     0.195250881    11 38 06.0 +14 10 08
                2458660.5000000     -2.315113993    -0.715379138     0.191608040    11 39 22.3 +13 57 31
                2458661.5000000     -2.320334769    -0.725713595     0.187961251    11 40 38.7 +13 44 57
                2458662.5000000     -2.325508194    -0.736033246     0.184310624    11 41 55.1 +13 32 26
                2458663.5000000     -2.330634610    -0.746338022     0.180656269    11 43 11.5 +13 19 57
                2458664.5000000     -2.335714357    -0.756627857     0.176998296    11 44 28.0 +13 07 31";

            // Test with possible error in rectangular coordinates: 0.0001 AU
            // Test with possible error in equatorial coordinates: 7" (seconds of arc)
            DoTest(location, oe, testData, 1e-4, 7); 
        }

        [TestMethod]
        public void TestHyperbolicOrbit()
        {
            // Comet C/2008 J6 (Hill), hyperbolic orbit
            // Orbital elements obtained from https://minorplanetcenter.net
            var oe = new OrbitalElements()
            {
                Epoch = 2454568.4386,
                Omega = 298.0993,
                e = 1.000963,
                i = 45.2105,
                omega = 10.5465,
                q = 1.9843
            };

            // Rectangular heliocentric coorinates and
            // Equatorial topocentric coordinates for J2000.0 epoch            
            // Test data obtained from https://cgi.minorplanetcenter.net/cgi-bin/mpeph2.cgi

            //  [Julian Day   ]    [X          ]   [Y          ]    [Z         ]    [RA      ] [Dec     ]
            string testData = @"
                2458635.5000000     -5.528494753    19.969247731    16.087237288     06 55 50.4 +37 28 33  
                2458636.5000000     -5.530441267    19.973095971    16.089262835     06 56 00.1 +37 27 58  
                2458637.5000000     -5.532387693    19.976943898    16.091288125     06 56 09.9 +37 27 23  
                2458638.5000000     -5.534334029    19.980791513    16.093313158     06 56 19.8 +37 26 49  
                2458639.5000000     -5.536280275    19.984638815    16.095337935     06 56 29.7 +37 26 15  
                2458640.5000000     -5.538226433    19.988485805    16.097362455     06 56 39.7 +37 25 41  
                2458641.5000000     -5.540172502    19.992332484    16.099386719     06 56 49.8 +37 25 08  
                2458642.5000000     -5.542118482    19.996178850    16.101410726     06 57 00.0 +37 24 35  
                2458643.5000000     -5.544064374    20.000024905    16.103434478     06 57 10.2 +37 24 02  
                2458644.5000000     -5.546010176    20.003870648    16.105457973     06 57 20.5 +37 23 30  
                2458645.5000000     -5.547955889    20.007716080    16.107481212     06 57 30.8 +37 22 58  
                2458646.5000000     -5.549901514    20.011561201    16.109504196     06 57 41.3 +37 22 27  
                2458647.5000000     -5.551847050    20.015406011    16.111526924     06 57 51.8 +37 21 56  
                2458648.5000000     -5.553792497    20.019250509    16.113549396     06 58 02.3 +37 21 25  
                2458649.5000000     -5.555737856    20.023094697    16.115571613     06 58 12.9 +37 20 55  
                2458650.5000000     -5.557683126    20.026938574    16.117593575     06 58 23.5 +37 20 25  
                2458651.5000000     -5.559628307    20.030782140    16.119615281     06 58 34.2 +37 19 56  
                2458652.5000000     -5.561573400    20.034625396    16.121636732     06 58 45.0 +37 19 27  
                2458653.5000000     -5.563518405    20.038468342    16.123657928     06 58 55.7 +37 18 58  
                2458654.5000000     -5.565463321    20.042310977    16.125678870     06 59 06.6 +37 18 30  
                2458655.5000000     -5.567408148    20.046153302    16.127699556     06 59 17.4 +37 18 02  
                2458656.5000000     -5.569352887    20.049995317    16.129719988     06 59 28.3 +37 17 35  
                2458657.5000000     -5.571297538    20.053837023    16.131740165     06 59 39.3 +37 17 08  
                2458658.5000000     -5.573242101    20.057678418    16.133760088     06 59 50.2 +37 16 42  
                2458659.5000000     -5.575186575    20.061519504    16.135779757     07 00 01.2 +37 16 16  
                2458660.5000000     -5.577130961    20.065360280    16.137799171     07 00 12.2 +37 15 50  
                2458661.5000000     -5.579075258    20.069200747    16.139818331     07 00 23.3 +37 15 25  
                2458662.5000000     -5.581019468    20.073040905    16.141837238     07 00 34.4 +37 15 00  
                2458663.5000000     -5.582963589    20.076880753    16.143855890     07 00 45.4 +37 14 36  
                2458664.5000000     -5.584907623    20.080720293    16.145874288     07 00 56.5 +37 14 12";

            // Test with possible error in rectangular coordinates: 0.01 AU
            // Test with possible error in equatorial coordinates: 15" (seconds of arc)
            DoTest(location, oe, testData, 1e-2, 15);
        }

        [TestMethod]
        public void TestParabolicOrbit()
        {
            // Comet C/2018 F3 (Johnson), parabolic orbit
            // Orbital elements obtained from https://minorplanetcenter.net
            var oe = new OrbitalElements()
            {
                Epoch = 2457980.7313,
                Omega = 173.0311,
                e = 1,
                i = 105.5348,
                omega = 293.0113,
                q = 2.483172
            };

            // Rectangular heliocentric coorinates and
            // Equatorial topocentric coordinates for J2000.0 epoch            
            // Test data obtained from https://cgi.minorplanetcenter.net/cgi-bin/mpeph2.cgi

            //  [Julian Day   ]    [X          ]    [Y          ]    [Z        ]      [RA      ] [Dec    ]
            string testData = @"
                2458635.5000000     -5.102436229     0.014600824     4.321735186      11 17 39.4 +44 10 36
                2458636.5000000     -5.104481057     0.013594852     4.330858940      11 17 36.7 +44 08 13
                2458637.5000000     -5.106520851     0.012588867     4.339978425      11 17 34.7 +44 05 49
                2458638.5000000     -5.108555626     0.011582869     4.349093647      11 17 33.5 +44 03 21
                2458639.5000000     -5.110585398     0.010576859     4.358204612      11 17 33.1 +44 00 52
                2458640.5000000     -5.112610180     0.009570839     4.367311324      11 17 33.4 +43 58 20
                2458641.5000000     -5.114629988     0.008564809     4.376413790      11 17 34.4 +43 55 47
                2458642.5000000     -5.116644835     0.007558770     4.385512014      11 17 36.1 +43 53 11
                2458643.5000000     -5.118654738     0.006552724     4.394606002      11 17 38.6 +43 50 34
                2458644.5000000     -5.120659709     0.005546670     4.403695760      11 17 41.8 +43 47 55
                2458645.5000000     -5.122659764     0.004540611     4.412781292      11 17 45.6 +43 45 15
                2458646.5000000     -5.124654916     0.003534548     4.421862605      11 17 50.2 +43 42 33
                2458647.5000000     -5.126645181     0.002528480     4.430939704      11 17 55.4 +43 39 49
                2458648.5000000     -5.128630571     0.001522410     4.440012594      11 18 01.3 +43 37 05
                2458649.5000000     -5.130611102     0.000516337     4.449081281      11 18 07.9 +43 34 19
                2458650.5000000     -5.132586788    -0.000489736     4.458145770      11 18 15.1 +43 31 32
                2458651.5000000     -5.134557642    -0.001495810     4.467206066      11 18 23.0 +43 28 43
                2458652.5000000     -5.136523679    -0.002501882     4.476262175      11 18 31.5 +43 25 54
                2458653.5000000     -5.138484912    -0.003507953     4.485314101      11 18 40.6 +43 23 04
                2458654.5000000     -5.140441355    -0.004514021     4.494361852      11 18 50.4 +43 20 13
                2458655.5000000     -5.142393023    -0.005520086     4.503405431      11 19 00.8 +43 17 22
                2458656.5000000     -5.144339928    -0.006526146     4.512444844      11 19 11.7 +43 14 29
                2458657.5000000     -5.146282085    -0.007532201     4.521480097      11 19 23.3 +43 11 37
                2458658.5000000     -5.148219507    -0.008538249     4.530511195      11 19 35.5 +43 08 43
                2458659.5000000     -5.150152209    -0.009544291     4.539538143      11 19 48.2 +43 05 49
                2458660.5000000     -5.152080202    -0.010550324     4.548560946      11 20 01.5 +43 02 55
                2458661.5000000     -5.154003502    -0.011556348     4.557579610      11 20 15.4 +43 00 01
                2458662.5000000     -5.155922121    -0.012562363     4.566594141      11 20 29.8 +42 57 06
                2458663.5000000     -5.157836072    -0.013568367     4.575604543      11 20 44.8 +42 54 11
                2458664.5000000     -5.159745370    -0.014574359     4.584610822      11 21 00.4 +42 51 16";

            // Test with possible error in rectangular coordinates: 0.001 AU
            // Test with possible error in equatorial coordinates: 15" (seconds of arc)
            DoTest(location, oe, testData, 1e-3, 15);
        }


        [TestMethod]
        public void TestAsteroidOrbit()
        {
            // Ceres, elliptic orbit
            // Orbital elements obtained from https://minorplanetcenter.net
            var oe = new OrbitalElements()
            {
                Epoch = 2458200.5,
                M = 352.23052,
                Omega = 80.30992,
                a = 2.7670463,
                e = 0.0755347,
                i = 10.59351,
                omega = 73.11528
            };

            // Rectangular heliocentric coorinates and
            // Equatorial topocentric coordinates for J2000.0 epoch            
            // Test data obtained from https://cgi.minorplanetcenter.net/cgi-bin/mpeph2.cgi

            //  [Julian Day   ]    [X          ]   [Y          ]   [Z           ]      [RA      ] [Dec     ]
            string testData = @"
                2458635.5000000     -1.049405060    -2.390935335    -0.913500107       16 20 36.9 -17 44 11
                2458636.5000000     -1.040334125    -2.394463639    -0.917011692       16 19 39.4 -17 45 39
                2458637.5000000     -1.031248674    -2.397958521    -0.920510476       16 18 42.2 -17 47 09
                2458638.5000000     -1.022148844    -2.401419964    -0.923996422       16 17 45.3 -17 48 41
                2458639.5000000     -1.013034776    -2.404847946    -0.927469494       16 16 48.7 -17 50 13
                2458640.5000000     -1.003906607    -2.408242449    -0.930929652       16 15 52.6 -17 51 47
                2458641.5000000     -0.994764479    -2.411603455    -0.934376861       16 14 57.0 -17 53 23
                2458642.5000000     -0.985608529    -2.414930946    -0.937811084       16 14 02.0 -17 55 00
                2458643.5000000     -0.976438896    -2.418224905    -0.941232283       16 13 07.7 -17 56 39
                2458644.5000000     -0.967255720    -2.421485313    -0.944640424       16 12 14.0 -17 58 19
                2458645.5000000     -0.958059139    -2.424712156    -0.948035469       16 11 21.0 -18 00 01
                2458646.5000000     -0.948849292    -2.427905417    -0.951417382       16 10 28.9 -18 01 45
                2458647.5000000     -0.939626318    -2.431065081    -0.954786130       16 09 37.5 -18 03 31
                2458648.5000000     -0.930390356    -2.434191132    -0.958141675       16 08 47.1 -18 05 19
                2458649.5000000     -0.921141544    -2.437283557    -0.961483983       16 07 57.6 -18 07 08
                2458650.5000000     -0.911880021    -2.440342342    -0.964813019       16 07 09.1 -18 09 00
                2458651.5000000     -0.902605925    -2.443367473    -0.968128749       16 06 21.7 -18 10 54
                2458652.5000000     -0.893319395    -2.446358937    -0.971431138       16 05 35.2 -18 12 49
                2458653.5000000     -0.884020570    -2.449316722    -0.974720153       16 04 49.9 -18 14 47
                2458654.5000000     -0.874709587    -2.452240815    -0.977995758       16 04 05.7 -18 16 47
                2458655.5000000     -0.865386585    -2.455131206    -0.981257921       16 03 22.7 -18 18 49
                2458656.5000000     -0.856051702    -2.457987884    -0.984506609       16 02 40.9 -18 20 54
                2458657.5000000     -0.846705077    -2.460810838    -0.987741788       16 02 00.4 -18 23 01
                2458658.5000000     -0.837346847    -2.463600057    -0.990963425       16 01 21.1 -18 25 10
                2458659.5000000     -0.827977150    -2.466355533    -0.994171488       16 00 43.1 -18 27 21
                2458660.5000000     -0.818596124    -2.469077256    -0.997365945       16 00 06.5 -18 29 35
                2458661.5000000     -0.809203908    -2.471765218    -1.000546762       15 59 31.2 -18 31 52
                2458662.5000000     -0.799800638    -2.474419411    -1.003713909       15 58 57.3 -18 34 11
                2458663.5000000     -0.790386452    -2.477039827    -1.006867354       15 58 24.9 -18 36 32
                2458664.5000000     -0.780961488    -2.479626459    -1.010007065       15 57 53.8 -18 38 57";

            // Test with possible error in rectangular coordinates: 0.001 AU
            // Test with possible error in equatorial coordinates: 1.5' (1.5 minutes of arc)
            DoTest(location, oe, testData, 1e-3, 90);
        }
    }
}
