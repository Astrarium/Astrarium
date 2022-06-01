using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Astrarium.Plugins.Planner.ImportExport
{
    /// <summary>
    /// Does reading/writing logic for observation plans
    /// </summary>
    public interface IPlanManager
    {
        /// <summary>
        /// Reads plan from the file
        /// </summary>
        PlanImportData Read(string filePath, CancellationToken? token = null, IProgress<double> progress = null);
 
        /// <summary>
        /// Writes plan to the file
        /// </summary>
        void Write(PlanExportData plan, string filePath);
    }
}