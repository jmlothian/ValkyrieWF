ValkyrieWF
==========

Valkyrie Workflow Engine

This is a simple, yet scalable distributed workflow processing engine written in C# using ZeroMQ (0MQ) as a communication layer.  The original design is several years old predating Windows Workflow Foundation and it included support for .Net 1.1 legacy code.  It is a work in progress updating it to modern code and semantics. 

The primary goal of the implementation is to provide a simple, easy to develop with and debug for system.  For those tired of constantly battling WCF service contracts or fighting to know what WWF is doing underneath this should provide a reasonable alternative.

USE WITH CAUTION.  This library is not yet complete and certainly not production ready.  I'm putting it up here in case someone has the time and interest to work on it.

Basic Usage
===========

Workflow processing is divided between a central Processor that is responsible for coordinating efforst between multiple services. Primarily, custom work is done by subclassing ValkProcessorStepHandler, changing the HandlesStep variable, overriding the HandleStep function with the custom task, and registering an instance with the ValkProcessor.  Multiple ValkProcessors can be created anywhere to distribute the load among different threads, processes, or even machines.  Currently, connection strings are hardcoded so they will need to be changed for more complex deployments (eventually, this should move to config file settings).

The system uses an interface (IDatabaseHandler) to persist/load workflow information to a backend.  Currently only one example implementation is provided (SQL Server) and it expects a connection string named "Valkyrie" in the local app.config.
