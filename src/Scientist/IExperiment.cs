﻿using System;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    /// <summary>
    /// Provides an interface for defining a synchronous experiment.
    /// </summary>
    /// <typeparam name="T">The return result for the experiment.</typeparam>
    public interface IExperiment<T>
    {
        /// <summary>
        /// Define any expensive setup here before the experiment is run.
        /// </summary>
        void BeforeRun(Action action);

        /// <summary>
        /// Defines the operation to try.
        /// </summary>
        /// <param name="candidate">The delegate to execute.</param>
        void Try(Func<T> candidate);

        /// <summary>
        /// Defines the operation to actually use.
        /// </summary>
        /// <param name="control">The delegate to execute.</param>
        void Use(Func<T> control);

        /// <summary>
        /// Defines a custom func used to compare results.
        /// </summary>
        void Compare(Func<T, T, bool> comparison);
    }

    /// <summary>
    /// Provides an interface for defining an asynchronous experiment.
    /// </summary>
    /// <typeparam name="T">The return result for the experiment.</typeparam>
    public interface IExperimentAsync<T>
    {
        /// <summary>
        /// Define any expensive setup here before the experiment is run.
        /// </summary>
        void BeforeRun(Func<Task> action);

        /// <summary>
        /// Defines the operation to try.
        /// </summary>
        /// <param name="candidate">The delegate to execute.</param>
        void Try(Func<Task<T>> candidate);

        /// <summary>
        /// Defines the operation to actually use.
        /// </summary>
        /// <param name="control">The delegate to execute.</param>
        void Use(Func<Task<T>> control);

        /// <summary>
        /// Defines a func used to compare results.
        /// </summary>
        void Compare(Func<T, T, bool> comparison);
    }
}