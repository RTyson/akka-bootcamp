using System;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Actor responsible for reading FROM the console. 
    /// Also responsible for calling <see cref="ActorSystem.Terminate"/>.
    /// </summary>
    class ConsoleReaderActor : UntypedActor
    {
        public const string ExitCommand = "exit";
        public const string StartCommand = "start";//<<-- 1.2

        protected override void OnReceive(object message)
        {
            //<<-- 1.2
            if(message.Equals(StartCommand))
            {
                DoPrintInstructions();
            }

            GetAndValidateInput();
        }
        
        #region Internal Methods
        private void DoPrintInstructions()//<<-- 1.2
        {
            Console.WriteLine("Please provide the URI of a log file on disk.\n");
        } 

        /// <summary>
        /// Reads input from console, validates it (via the validation actor), 
        /// then signals appropriate response (continue processing, error, success, etc.).
        /// </summary>
        private void GetAndValidateInput()
        {
            var message = Console.ReadLine();

            if(!string.IsNullOrEmpty(message) && String.Equals(message, ExitCommand, StringComparison.OrdinalIgnoreCase))
            {
                // if user typed ExitCommand, shut down the entire actor system (allows the process to exit.)
                Context.System.Terminate();
                return;
            }

            //otherwise, just hand the message off for validation
            // We are using ActorSelection so we no longer have coupling between this object, and the fileValidationActor object.
            Context.ActorSelection("akka://MyActorSystem/user/validationActor").Tell(message);
        }
        #endregion
    }
}