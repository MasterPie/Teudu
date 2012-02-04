// -----------------------------------------------------------------------
// <copyright file="IHelpService.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Teudu.InfoDisplay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IHelpService
    {
        /// <summary>
        /// Initializes the HelpService
        /// </summary>
        void Initialize();
        /// <summary>
        /// Depending on implementation, may run an instructional help sequence for the new user
        /// </summary>
        /// <param name="state">The new user's state</param>
        void NewUser(UserState state);
        /// <summary>
        /// Depending on the implementation, may raise NewHelpMessage events to handle specific UserStates
        /// </summary>
        /// <param name="state">the user's state</param>
        void UserStateUpdated(UserState state);
        /// <summary>
        /// Releases all resources
        /// </summary>
        void Cleanup();
        /// <summary>
        /// Event raised when a new help message for the user is generated
        /// </summary>
        event EventHandler<HelpMessageEventArgs> NewHelpMessage;
    }
}
