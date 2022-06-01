namespace Astrarium.Types
{
    /// <summary>
    /// Base class for all modules which perform astronomical calculations.
    /// </summary>
    public abstract class BaseCalc : PropertyChangedBase
    {
        /// <summary>
        /// Performs starting initialization of the calculator.
        /// It's a good place to load data required by the module.
        /// Base implementation does nothing, you could skip this method if no initialization is required.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// This method is called when it's needed to perform astronomical calculations before rendering the sky map.
        /// </summary>
        /// <param name="context"></param>
        public abstract void Calculate(SkyContext context);
    }
}
