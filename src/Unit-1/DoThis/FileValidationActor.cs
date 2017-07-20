using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using System.IO;

namespace WinTail
{
    /// <summary>
    /// Actor that validates user input and signals results to others
    /// </summary>
    public class FileValidationActor : UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;

        public FileValidationActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }
        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if(string.IsNullOrEmpty(msg))
            {
                // signal that the user needs to supply an input
                _consoleWriterActor.Tell(new Messages.NullInputError("Input was blank.  Please try again."));

                // tell the sineder to continue doing whatever it was doing.
                Sender.Tell(new Messages.ContinueProcessing());
            }
            else
            {
                if(IsFileUri(msg))
                {
                    // signal successful input
                    _consoleWriterActor.Tell(new Messages.InputSuccess($"Start processing for {msg}"));

                    // start the coordinator
                    // We are using ActorSelection to decouple this object from the TailCoordinatorActor object.
                    Context.ActorSelection("akka://MyActorSystem/user/tailCoordinatorActor").Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));
                }
                else
                {
                    // signal that input was bad
                    _consoleWriterActor.Tell(new Messages.ValidationError($"{msg} is not an existing URI on disk."));

                    // Tell the sender to continue doing whatever it does.
                    Sender.Tell(new Messages.ContinueProcessing());
                }
            }

            // Tell the sender to continue doing its thing (whatever that may be, this actor doesn't care)
            Sender.Tell(new Messages.ContinueProcessing());
        }


        /// <summary>
        /// Checks if file exists at the path provided by the user.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsFileUri(string path)
        {
            return File.Exists(path);
        }
    }
}
