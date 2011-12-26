// -----------------------------------------------------------------------
// <copyright file="HelpService.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Teudu.InfoDisplay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Threading;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class InstructionalHelpService : IHelpService
    {
        private Queue<string[]> welcomeMessages;

        private DispatcherTimer welcomeTickerTimer;
        public void Initialize()
        {
            welcomeMessages = new Queue<string[]>();

            welcomeTickerTimer = new DispatcherTimer();
            welcomeTickerTimer.Interval = TimeSpan.FromSeconds(5);
            welcomeTickerTimer.Tick += new EventHandler(welcomeTickerTimer_Tick);
        }

        
        public void NewUser(UserState state)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke((Action)(()=> welcomeTickerTimer.Stop()));

            if(!welcomeTickerTimer.IsEnabled)
                BuildWelcomeMessages();

            if (NewHelpMessage != null)
                NewHelpMessage(this, new HelpMessageEventArgs() { Message = "Welcome!" });
            
            Dispatcher.CurrentDispatcher.BeginInvoke((Action)(()=>welcomeTickerTimer.Start()));
        }

        private void BuildWelcomeMessages()
        {
            welcomeMessages.Clear();
            welcomeMessages.Enqueue(new string[] { "This device uses an invisible touch screen.", "/Teudu.InfoDisplay;component/Images/InvisScreen.png" });
            welcomeMessages.Enqueue(new string[]{"Extend your arm towards the screen to TOUCH the invisible screen.","/Teudu.InfoDisplay;component/Images/HandOut.png"});
            welcomeMessages.Enqueue(new string[] { "Pull back your arm to STOP TOUCHING the invisible screen.", "/Teudu.InfoDisplay;component/Images/HandBack.png" });
            welcomeMessages.Enqueue(new string[]{"Indicators on the top right will guide you.",""});
            welcomeMessages.Enqueue(new string[]{"Enjoy! We hope you find something fun to do!",""});
        }

        void welcomeTickerTimer_Tick(object sender, EventArgs e)
        {
            if (welcomeMessages.Count <= 0)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() => welcomeTickerTimer.Stop()));
                return;
            }
            string[] welcomeMessage = welcomeMessages.Dequeue();
            if (NewHelpMessage != null)
                NewHelpMessage(this, new HelpMessageEventArgs() { Message = welcomeMessage[0], SupplementaryImage = welcomeMessage[1] });
        }


        public void UserStateUpdated(UserState state)
        {
            if (state.TooClose)
                SendWarning("Please stand back a bit so Teudu can see you better.");
        }

        private void SendWarning(string warning)
        {
            if (NewWarningMessage != null)
                NewWarningMessage(this, new HelpMessageEventArgs() { Message = warning });
        }

        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<HelpMessageEventArgs> NewHelpMessage;
        public event EventHandler<HelpMessageEventArgs> NewWarningMessage;

    }
}
