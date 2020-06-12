﻿using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.FOV
{
    public class CameraVM : ViewModelBase
    {
        public ICollection<Camera> Cameras { get; set; } = new List<Camera>();
        public Camera Camera { get; set; } = new Camera() { Id = Guid.NewGuid(), PixelSizeWidth = 1, PixelSizeHeight = 1, HorizontalResolution = 4000, VerticalResolution = 2000 };

        public Command OkCommand { get; }
        public Command CancelCommand { get; }

        public CameraVM()
        {
            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }
    
        private void Ok()
        {
            if (string.IsNullOrWhiteSpace(Camera.Name))
            {
                ViewManager.ShowMessageBox("Warning", "Please specify camera model name.");
            }
            else if (Cameras.Any(t => t.Name == Camera.Name && t.Id != Camera.Id))
            {
                ViewManager.ShowMessageBox("Warning", "Camera with specified model name already exists. Please specify another name.");            
            }
            else
            {
                Close(true);
            }
        }
    }
}
