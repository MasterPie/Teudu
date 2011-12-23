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
        void Initialize();
        void NewUser(UserState state);
        void UserStateUpdated(UserState state);
        void Cleanup();

        event EventHandler NewWelcomeSequence;
        event EventHandler EndWelcomeSequence;
        event EventHandler<HelpMessageEventArgs> NewHelpMessage;
        event EventHandler<HelpMessageEventArgs> NewWarningMessage;
    }
}
