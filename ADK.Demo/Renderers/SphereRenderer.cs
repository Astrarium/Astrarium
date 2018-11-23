using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace ADK.Demo.Renderers
{
    public class RendererOptions
    {
        public string TextureFilePath { get; set; }
        public uint OutputImageSize { get; set; }
        public double LatitudeShift { get; set; }
        public double LongutudeShift { get; set; }
    }

    public class SphereRenderer
    {
        private class RendererOptionsInOut
        {
            public Action<Bitmap> OnComplete { get; }
            public RendererOptions Options { get; }

            public RendererOptionsInOut(RendererOptions opts, Action<Bitmap> onComplete)
            {
                Options = opts;
                OnComplete = onComplete;
            }
        }

        public void Render(RendererOptions options, Action<Bitmap> onComplete)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(RenderSTA));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "RenderThread";
            thread.Start(new RendererOptionsInOut(options, onComplete));
        }

        private double ToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public void RenderSTA(object param)
        {
            RendererOptionsInOut inOut = (RendererOptionsInOut)param;

            // The main object model group.
            Model3DGroup group = new Model3DGroup();

            // The camera.
            PerspectiveCamera camera = new PerspectiveCamera();

            Viewport3D viewport = new Viewport3D();

            // Give the camera its initial position.
            camera.FieldOfView = 5.8;
            viewport.Camera = camera;

            // The camera's current location.

            string textureFilePath = inOut.Options.TextureFilePath;
            int size = (int)inOut.Options.OutputImageSize;
            double cameraPhi = ToRadians(inOut.Options.LatitudeShift);
            double cameraTheta = ToRadians(180 - inOut.Options.LongutudeShift);
            double cameraR = 20;

            // Calculate the camera's position in Cartesian coordinates.
            double y = cameraR * Math.Sin(cameraPhi);
            double hyp = cameraR * Math.Cos(cameraPhi);
            double x = hyp * Math.Cos(cameraTheta);
            double z = hyp * Math.Sin(cameraTheta);
            camera.Position = new Point3D(x, y, z);

            // Look toward the origin.
            camera.LookDirection = new Vector3D(-x, -y, -z);

            // Set the Up direction.
            camera.UpDirection = new Vector3D(0, 1, 0);

            // Define lights.
            AmbientLight ambientLight = new AmbientLight(Colors.White);

            //DirectionalLight directional_light = new DirectionalLight(Colors.Red, new Vector3D(-x, -y, -z));

            group.Children.Add(ambientLight);
            //group.Children.Add(directional_light);


            // Create the model.
            // Globe. Place it in a new model so we can transform it.
            Model3DGroup globe = new Model3DGroup();
            group.Children.Add(globe);

            ImageBrush globeBrush = new ImageBrush(new BitmapImage(new Uri(textureFilePath, UriKind.RelativeOrAbsolute)));
            Material globeMaterial = new DiffuseMaterial(globeBrush);

            MeshGeometry3D globeMesh = MakeSphere(globe, globeMaterial, 1, 0, 0, 0, 64, 64);

            // Add the group of models to a ModelVisual3D.
            ModelVisual3D visual = new ModelVisual3D();
            visual.Content = group;

            // Display the main visual to the viewport.
            viewport.Children.Add(visual);

            viewport.Width = size;
            viewport.Height = size;
            viewport.Measure(new System.Windows.Size(size, size));
            viewport.Arrange(new System.Windows.Rect(0, 0, size, size));

            viewport.Dispatcher.Invoke(() => {

                viewport.InvalidateVisual();

                RenderTargetBitmap bmp = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);

                //RenderOptions.SetEdgeMode(viewport, EdgeMode.Aliased);

                bmp.Render(viewport);

                inOut.OnComplete?.BeginInvoke(ToWinFormsBitmap(bmp), null, null);
            },
            DispatcherPriority.Render);
        }

        // Make a sphere.
        private MeshGeometry3D MakeSphere(Model3DGroup globe, Material material,
            double radius, double cx, double cy, double cz, int phiCount, int thetaCount)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            GeometryModel3D model = new GeometryModel3D(mesh, material);
            globe.Children.Add(model);

            double dphi = Math.PI / phiCount;
            double dtheta = 2 * Math.PI / thetaCount;

            // Remember the first point.
            int pt0 = mesh.Positions.Count;

            // Make the points.
            double phi1 = Math.PI / 2;
            for (int p = 0; p <= phiCount; p++)
            {
                double r1 = radius * Math.Cos(phi1);
                double y1 = radius * Math.Sin(phi1);

                double theta = 0;
                for (int t = 0; t <= thetaCount; t++)
                {
                    mesh.Positions.Add(new Point3D(
                        cx + r1 * Math.Cos(theta), cy + y1, cz + -r1 * Math.Sin(theta)));
                    mesh.TextureCoordinates.Add(new System.Windows.Point(
                        (double)t / thetaCount, (double)p / phiCount));
                    theta += dtheta;
                }
                phi1 -= dphi;
            }

            // Make the triangles.
            int i1, i2, i3, i4;
            for (int p = 0; p <= phiCount - 1; p++)
            {
                i1 = p * (thetaCount + 1);
                i2 = i1 + (thetaCount + 1);
                for (int t = 0; t <= thetaCount - 1; t++)
                {
                    i3 = i1 + 1;
                    i4 = i2 + 1;
                    mesh.TriangleIndices.Add(pt0 + i1);
                    mesh.TriangleIndices.Add(pt0 + i2);
                    mesh.TriangleIndices.Add(pt0 + i4);

                    mesh.TriangleIndices.Add(pt0 + i1);
                    mesh.TriangleIndices.Add(pt0 + i4);
                    mesh.TriangleIndices.Add(pt0 + i3);
                    i1 += 1;
                    i2 += 1;
                }
            }

            return mesh;
        }

        private Bitmap ToWinFormsBitmap(BitmapSource bitmapsource)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(stream);

                using (var tempBitmap = new Bitmap(stream))
                {
                    // According to MSDN, one "must keep the stream open for the lifetime of the Bitmap."
                    // So we return a copy of the new bitmap, allowing us to dispose both the bitmap and the stream.
                    return new Bitmap(tempBitmap);
                }
            }
        }
    }
}
