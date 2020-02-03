using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Planetarium.Renderers
{
    /// <summary>
    /// Class for rendering spherical images of celestial objects.
    /// </summary>
    /// <remarks>
    /// Implementation of the class is based on the solution from article <see href="http://csharphelper.com/blog/2017/05/make-3d-globe-wpf-c/"/>.
    /// </remarks>
    public class WpfSphereRenderer : ISphereRenderer
    {
        private RenderTargetBitmap targetBitmap = null;

        /// <summary>
        /// Internal rendering options
        /// </summary>
        private class RendererOptionsInOut
        {
            /// <summary>
            /// Action to be called when output bitmap is ready
            /// </summary>
            public Action<Bitmap> OnComplete { get; }

            /// <summary>
            /// Rendering options
            /// </summary>
            public RendererOptions Options { get; }

            public RendererOptionsInOut(RendererOptions opts, Action<Bitmap> onComplete)
            {
                Options = opts;
                OnComplete = onComplete;
            }
        }

        private double ToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public System.Drawing.Image Render(RendererOptions options)
        {
            // The main object model group.
            Model3DGroup group = new Model3DGroup();

            // The camera
            OrthographicCamera camera = new OrthographicCamera();

            Viewport3D viewport = new Viewport3D();

            // Give the camera its initial position.
            //camera.FieldOfView = 5.75;
            viewport.Camera = camera;

            // The camera's current location.

            string textureFilePath = options.TextureFilePath;
            int size = (int)options.OutputImageSize;
            double cameraPhi = ToRadians(options.LatitudeShift);
            double cameraTheta = ToRadians(options.LongutudeShift);
            double cameraR = 10;

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

            group.Children.Add(ambientLight);

            // Create the model.
            // Globe. Place it in a new model so we can transform it.
            Model3DGroup globe = new Model3DGroup();
            group.Children.Add(globe);

            ImageBrush globeBrush = new ImageBrush(new BitmapImage(new Uri(textureFilePath, UriKind.RelativeOrAbsolute)));
            Material globeMaterial = new DiffuseMaterial(globeBrush);

            MakeSphere(globe, globeMaterial, 1, 0, 0, 0, 64, 64);

            // Add the group of models to a ModelVisual3D.
            ModelVisual3D visual = new ModelVisual3D();
            visual.Content = group;

            // Display the main visual to the viewport.
            viewport.Children.Add(visual);

            viewport.Width = size;
            viewport.Height = size;
            viewport.Measure(new System.Windows.Size(size, size));
            viewport.Arrange(new System.Windows.Rect(0, 0, size, size));
            viewport.InvalidateVisual();

            if (targetBitmap == null)
            {
                targetBitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            }

            targetBitmap.Clear();
            targetBitmap.Render(viewport);

            return ToWinFormsBitmap(targetBitmap);
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
            Bitmap bmp = new Bitmap(
                bitmapsource.PixelWidth,
                bitmapsource.PixelHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            BitmapData data = bmp.LockBits(
                new Rectangle(Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            bitmapsource.CopyPixels(
                System.Windows.Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);

            bmp.UnlockBits(data);

            return bmp;
        }
    }
}
