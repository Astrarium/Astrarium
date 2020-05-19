using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium
{
    /// <summary>
    /// Command line args holder interface
    /// </summary>
    public interface ICommandLineArgs : IEnumerable<string> { }

    /// <summary>
    /// Command line args holder implementation
    /// </summary>
    public class CommandLineArgs : ICommandLineArgs
    {
        private string[] args;

        public CommandLineArgs(string[] args)
        {
            this.args = args;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return args.AsEnumerable<string>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return args.GetEnumerator();
        }
    }
}
