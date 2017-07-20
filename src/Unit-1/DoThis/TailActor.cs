using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using System.IO;

namespace WinTail
{
    public class TailActor : UntypedActor
    {
        #region Message Types
        /// <summary>
        /// Signal that the file has changed, and we need to read the next line of the file.
        /// </summary>
        public class FileWrite
        {
            public FileWrite(string fileName)
            {
                FileName = fileName;
            }

            public object FileName { get; private set; }
        }

        /// <summary>
        /// Signal that the OS had an error accessing the file
        /// </summary>
        public class FileError
        {
            public FileError(string fileName, string reason)
            {
                FileName = fileName;
                Reason = reason;
            }

            public object FileName { get; private set; }
            public object Reason { get; private set; }
        }

        /// <summary>
        /// Signal to read the initial contents of the file at actor startup.
        /// </summary>
        public class InitialRead
        {
            public InitialRead(string fileName, string text)
            {
                FileName = fileName;
                Text = text;
            }

            public object FileName { get; private set; }
            public object Text { get; private set; }
        }
        #endregion

        private readonly string _filePath;
        private readonly IActorRef _reporterActor;
        private FileObserver _observer;
        private Stream _fileStream;
        private StreamReader _fileStreamReader;

        public TailActor(IActorRef reporterActor, string filePath)
        {
            _reporterActor = reporterActor;
            _filePath = filePath;
        }

        #region Life cycle management
        /// <summary>
        /// Initialization logic for the actor, that will tail changes to a file.
        /// PreStart() is called by the framework before Starting.. no need to make a call to it.
        /// </summary>
        protected override void PreStart()
        {
            // start watching the file for changes.
            _observer = new FileObserver(Self, Path.GetFullPath(_filePath));
            _observer.Start();

            // open the file stream with shared read/write permissions (so file can be written to while open)
            _fileStream = new FileStream(Path.GetFullPath(_filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);

            // read the initial contents of the file and send it to the console as the first message
            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(_filePath, text));
        }

        /// <summary>
        /// Cleanup OS handles for <see cref="_fileStreamReader"/> and <see cref="FileObserver"/>
        /// </summary>
        protected override void PostStop()
        {
            _observer.Dispose();
            _observer = null;

            _fileStreamReader.Close();
            _fileStreamReader.Dispose();

            base.PostStop();
        }
        #endregion

        protected override void OnReceive(object message)
        {
            if (message is FileWrite)
            {
                // move file coursor forward, pull results from coursor to the end of the file,
                // and write to output.  (this is assuming a log file type format that is append only)
                var text = _fileStreamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(text))
                {
                    _reporterActor.Tell(text);
                }
            }
            else if (message is FileError)
            {
                var fe = message as FileError;
                _reporterActor.Tell($"Tail Error : {fe.Reason}");
            }
            else if (message is InitialRead)
            {
                var ir = message as InitialRead;
                _reporterActor.Tell(ir.Text);
            }
        }
    }
}
