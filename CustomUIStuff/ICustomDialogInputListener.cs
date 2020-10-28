using ShinyShoe;

namespace AdvancedRunHistory.CustomUIStuff
{
    /// <summary>
    /// A "listener" interface to allow custom input handling for <c>ScreenDialog</c>s.
    /// </summary>
    /// <remarks>
    /// Currently not in use, see <see cref="HandleCustomDialogInputPatch"/>
    /// </remarks>
    public interface ICustomDialogInputListener
    {
        /// <summary>
        /// If a <c>ScreenDialog</c> has a listener assigned to it, when its <c>ApplyScreenInput</c> method
        /// is called, it first calls this method. If this method returns <c>false</c>, the original method is
        /// executed afterwards.
        /// </summary>
        /// <remarks>
        /// Currently not in use, see <see cref="HandleCustomDialogInputPatch"/>
        /// </remarks>
        /// <param name="mapping">Mostly key mappings I think? Haven't used it for anything.</param>
        /// <param name="triggeredUI">The triggered UI game element.</param>
        /// <param name="triggeredMappingID">The kind of interaction or something along those lines.</param>
        /// <returns><c>true</c> on a "successful" interaction, stops the original method from executing. <c>false</c> otherwise.</returns>
        bool ApplyScreenInput(CoreInputControlMapping mapping, IGameUIComponent triggeredUI, InputManager.Controls triggeredMappingID);
	}
}
