﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitHub.Internals
{
    /// <summary>
    /// An instance of an experiment. This actually runs the control and the candidate and measures the result.
    /// </summary>
    /// <typeparam name="T">The return type of the experiment</typeparam>
    internal class ExperimentInstance<T>
    {
        internal const string CandidateExperimentName = "candidate";
        internal const string ControlExperimentName = "control";

        static Random _random = new Random(DateTimeOffset.UtcNow.Millisecond);

        readonly string _name;
        readonly List<NamedBehavior> _behaviors;
        readonly Func<T, T, bool> _comparator;
        readonly Func<Task> _beforeRun;

        public ExperimentInstance(string name, Func<Task<T>> control, Func<Task<T>> candidate, Func<T, T, bool> comparator, Func<Task> beforeRun)
            : this(name,
                  new NamedBehavior(ControlExperimentName, control),
                  new NamedBehavior(CandidateExperimentName, candidate),
                  comparator,
                  beforeRun)
        {
        }

        internal ExperimentInstance(string name, NamedBehavior control, NamedBehavior candidate, Func<T, T, bool> comparator, Func<Task> beforeRun)
        {
            _name = name;
            _behaviors = new List<NamedBehavior>
            {
                control,
                candidate
            };
            _comparator = comparator;
            _beforeRun = beforeRun;
        }

        public async Task<T> Run()
        {
            // TODO determine if experiments should be run.

            if (_beforeRun != null)
            {
                await _beforeRun();
            }

            // Randomize ordering...
            var observations = new List<Observation<T>>();
            foreach (var behavior in _behaviors.OrderBy(k => _random.Next()))
            {
                observations.Add(await Observation<T>.New(behavior.Name, behavior.Behavior, _comparator));
            }

            var controlObservation = observations.FirstOrDefault(o => o.Name == ControlExperimentName);
            var result = new Result<T>(_name, observations, controlObservation, _comparator);

            // TODO: Make this Fire and forget so we don't have to wait for this
            // to complete before we return a result
            await Scientist.ResultPublisher.Publish(result);

            if (controlObservation.Thrown) throw controlObservation.Exception;
            return controlObservation.Value;
        }
        
        internal class NamedBehavior
        {
            public NamedBehavior(string name, Func<T> behavior)
                : this(name, () => Task.FromResult(behavior()))
            {
            }

            public NamedBehavior(string name, Func<Task<T>> behavior)
            {
                Behavior = behavior;
                Name = name;
            }

            /// <summary>
            /// Gets the behavior to execute during an experiment.
            /// </summary>
            public Func<Task<T>> Behavior { get; }

            /// <summary>
            /// Gets the name of the behavior.
            /// </summary>
            public string Name { get; }
        }
    }
}