using System;
﻿using Akka.Actor;

namespace WinTail
{
    #region Program
    class Program
    {
        public static ActorSystem MyActorSystem;

        static void Main(string[] args)
        {
            // initialize MyActorSystem
            MyActorSystem = ActorSystem.Create("MyActorSystem"); //<<-- 1.1

            // time to make your first actors!
            
            // These are top level actors:
            // i.e. /user/consoleWriterActor

            // make consoleWriterActor using these props: Props.Create(() => new ConsoleWriterActor())
            Props consoleWriterProps = Props.Create<ConsoleWriterActor>(); //<-- Generic syntax
            IActorRef consoleWriterActor = MyActorSystem.ActorOf(consoleWriterProps, "consoleWriterActor");

            // make a TailCoordinator actor...
            Props tailCoordinatorProps = Props.Create(() => new TailCoordinatorActor());
            IActorRef tailCoordinatorActor = MyActorSystem.ActorOf(tailCoordinatorProps, "tailCoordinatorActor");

            // make validationActor using these props: Props.Create(() => new ValidationActor())
            Props fileValidationActorProps = Props.Create(() => new FileValidationActor(consoleWriterActor)); //<-- Lambda syntax
            IActorRef validationActor = MyActorSystem.ActorOf(fileValidationActorProps, "validationActor");

            // make consoleReaderActor using these props: Props.Create(() => new ConsoleReaderActor(consoleWriterActor))
            Props consoleReaderProps = Props.Create(() => new ConsoleReaderActor());
            IActorRef consoleReaderActor = MyActorSystem.ActorOf(consoleReaderProps, "consoleReaderActor");

            // tell console reader to begin
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand); //<<-- 1.2

            // blocks the main thread from exiting until the actor system is shut down
            MyActorSystem.WhenTerminated.Wait();
        }
    }
    #endregion
}
