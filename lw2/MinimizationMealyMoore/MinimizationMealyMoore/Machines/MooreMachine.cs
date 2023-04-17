using System;
using System.Collections.Generic;
using System.Linq;
using ConversionMealyMoore.Types;

namespace ConversionMealyMoore.Machines
{
    public sealed class MooreMachine : IMachine
    {
        private List<string> _outputSignals;
        private List<string> _states;
        private Dictionary<string, List<string>> _actions; // input signal -> states
        private List<List<string>> Transitions => _actions.Values.ToList();

        internal MooreMachine( List<string> outputSignals, List<string> states, Dictionary<string, List<string>> actions )
        {
            _outputSignals = outputSignals;
            _states = states;
            _actions = actions;
        }

        public MooreMachine( List<string> parameters )
        {
            _outputSignals = new List<string>();
            _states = new List<string>();
            _actions = new Dictionary<string, List<string>>();

            if ( parameters == null || parameters.Count == 0 )
            {
                throw new ArgumentException( "Machine parameters can't be null or empty" );
            }

            _outputSignals = parameters.First()
                .Split( ";" )
                .Where( x => !string.IsNullOrWhiteSpace( x ) )
                .ToList();

            _states = parameters.Skip( 1 ).First()
                .Split( ";" )
                .Where( x => !string.IsNullOrWhiteSpace( x ) )
                .ToList();

            if ( _states.Count < 2 )
            {
                throw new ArgumentException( "Invalid states count" );
            }

            foreach ( string action in parameters.Skip( 2 ) )
            {
                List<string> transition = action.Split( ";" ).ToList();
                if ( transition.Count - 1 != _states.Count )
                {
                    throw new ArgumentException( "Invalid action line" );
                }

                _actions.Add( transition.First(), transition.Skip( 1 ).ToList() );
            }
        }

        public List<string> GetParameters()
        {
            List<string> parameters = new();

            parameters.Add( ";" + string.Join( ";", _outputSignals ) );
            parameters.Add( ";" + string.Join( ";", _states ) );
            foreach ( KeyValuePair<string, List<string>> action in _actions )
            {
                parameters.Add( action.Key + ";" + string.Join( ";", action.Value ) );
            }

            return parameters;
        }

        public void Minimize()
        {
            DeleteUnreachableStates();
            Dictionary<string, HashSet<string>> previousMatchingMinimizedStatesToStates = new Dictionary<string, HashSet<string>>();
            Dictionary<string, HashSet<string>> matchingMinimizedStatesToStates = new Dictionary<string, HashSet<string>>();
            foreach (string outputLetter in _outputSignals.ToHashSet())
            {
                HashSet<string> outputLetterStates = new HashSet<string>();
                for (int i = 0; i < _outputSignals.Count; i++)
                {
                    if (_outputSignals[i] == outputLetter)
                    {
                        outputLetterStates.Add(_states[i]);
                    }
                }
                matchingMinimizedStatesToStates.Add(outputLetter, outputLetterStates);
            }
            List<List<string>> newTransitions = GetNewTransitions(matchingMinimizedStatesToStates);

            do
            {
                previousMatchingMinimizedStatesToStates = matchingMinimizedStatesToStates;
                matchingMinimizedStatesToStates = GetMatchingMinimizedStatesToStates(previousMatchingMinimizedStatesToStates, newTransitions);
                newTransitions = GetNewTransitions(matchingMinimizedStatesToStates);
            } while (matchingMinimizedStatesToStates.Count != previousMatchingMinimizedStatesToStates.Count && _states.Count != matchingMinimizedStatesToStates.Count);

            List<string> minimizedStates = matchingMinimizedStatesToStates.Keys.ToList();
            List<List<string>> minimizedTransitions = GetMinimizedTransitions(matchingMinimizedStatesToStates);
            List<string> minimizedOutputAlphabet = GetMinimizedOutputAlphabet(matchingMinimizedStatesToStates);
            _states = minimizedStates;
            _outputSignals = minimizedOutputAlphabet;
            UpdateActions(minimizedTransitions);
        }

        private void DeleteUnreachableStates()
        {
            HashSet<string> reachableStates = new HashSet<string>();
            reachableStates.Add(_states.First());

            foreach (KeyValuePair<string, List<string>> action in _actions)
            {
                foreach (string transition in action.Value)
                {
                    int transitionIndex = action.Value.IndexOf(transition);
                    string destinationState = transition;

                    if (destinationState != _states[transitionIndex])
                    {
                        reachableStates.Add(destinationState);
                    }
                }
            }

            if (reachableStates.Count == _states.Count)
            {
                return;
            }

            foreach (string state in _states)
            {
                if (!reachableStates.Contains(state))
                {
                    RemoveState(state);
                }
            }
        }

        private Dictionary<string, HashSet<string>> GetMatchingMinimizedStatesToStates(Dictionary<string, HashSet<string>> matchingEquivalenceClassesToStates, List<List<string>> transitions)
        {
            Dictionary<string, HashSet<string>> matchingNewStatesToPreviousStates = new Dictionary<string, HashSet<string>>();
            Dictionary<string, List<string>> matchingNewStatesToTransitions = new Dictionary<string, List<string>>();

            foreach (string state in _states)
            {
                int stateIndex = _states.IndexOf(state);

                List<string> transitionsSequence = new List<string>();
                foreach (List<string> innerStateTransitions in transitions)
                {
                    transitionsSequence.Add(innerStateTransitions[stateIndex]);
                }
                bool isExistMinimizedState = false;
                foreach (KeyValuePair<string, List<string>> matchingNewStateToTransitions in matchingNewStatesToTransitions)
                {
                    if (matchingNewStateToTransitions.Value.SequenceEqual(transitionsSequence))
                    {
                        string firstElementOfEquivalenceClass = matchingNewStatesToPreviousStates[matchingNewStateToTransitions.Key].First();
                        string firstEquivalenceClass = "";
                        string secondEquivalenceClass = "";
                        foreach (KeyValuePair<string, HashSet<string>> matchingEquivalenceClassToStates in matchingEquivalenceClassesToStates)
                        {
                            if (matchingEquivalenceClassToStates.Value.Contains(firstElementOfEquivalenceClass))
                            {
                                firstEquivalenceClass = matchingEquivalenceClassToStates.Key;
                            }
                            if (matchingEquivalenceClassToStates.Value.Contains(state))
                            {
                                secondEquivalenceClass = matchingEquivalenceClassToStates.Key;
                            }
                        }
                        if (firstEquivalenceClass == secondEquivalenceClass)
                        {
                            matchingNewStatesToPreviousStates[matchingNewStateToTransitions.Key].Add(state);
                            isExistMinimizedState = true;
                            break;
                        }
                    }
                }
                if (!isExistMinimizedState)
                {
                    string newState = "q" + matchingNewStatesToPreviousStates.Count.ToString();
                    matchingNewStatesToPreviousStates.Add(newState, new HashSet<string>() { state });
                    matchingNewStatesToTransitions.Add(newState, transitionsSequence);
                }
            }

            return matchingNewStatesToPreviousStates;
        }

        private List<List<string>> GetNewTransitions(Dictionary<string, HashSet<string>> matchingNewStatesToStates)
        {
            List<List<string>> newTransitions = new List<List<string>>();

            foreach (List<string> innerStateTransitions in Transitions)
            {
                List<string> newInnerStateTransitions = new List<string>();
                foreach (string oldTransitionFunction in innerStateTransitions)
                {
                    string oldState = oldTransitionFunction;
                    foreach (KeyValuePair<string, HashSet<string>> matchingNewStateToStates in matchingNewStatesToStates)
                    {
                        if (matchingNewStateToStates.Value.Contains(oldState))
                        {
                            newInnerStateTransitions.Add(matchingNewStateToStates.Key);
                        }
                    }
                }
                newTransitions.Add(newInnerStateTransitions);
            }

            return newTransitions;
        }

        private List<List<string>> GetMinimizedTransitions(Dictionary<string, HashSet<string>> matchingMinimizedStatesToStates)
        {
            List<List<string>> minimizedTransitions = new List<List<string>>();

            foreach (KeyValuePair<string, HashSet<string>> matchingMinimizedStateToStates in matchingMinimizedStatesToStates)
            {
                List<string> innerStateMinimizedTransitions = new List<string>();
                foreach (List<string> innerStateTransitions in Transitions)
                {
                    foreach (KeyValuePair<string, HashSet<string>> localMatchingMinimizedStateToStates in matchingMinimizedStatesToStates)
                    {
                        if (localMatchingMinimizedStateToStates.Value.Contains(innerStateTransitions[_states.IndexOf(matchingMinimizedStateToStates.Value.First())]))
                        {
                            innerStateMinimizedTransitions.Add(localMatchingMinimizedStateToStates.Key);
                        }
                    }
                }
                if (minimizedTransitions.Count != innerStateMinimizedTransitions.Count)
                {
                    foreach (string transitionFunction in innerStateMinimizedTransitions)
                    {
                        minimizedTransitions.Add(new List<string>() { transitionFunction });
                    }
                }
                else
                {
                    for (int i = 0; i < minimizedTransitions.Count; i++)
                    {
                        minimizedTransitions[i].Add(innerStateMinimizedTransitions[i]);
                    }
                }
            }

            return minimizedTransitions;
        }

        private void RemoveState(string state)
        {
            if (!_states.Contains(state))
            {
                return;
            }

            int stateIndex = _states.IndexOf(state);
            foreach (KeyValuePair<string, List<string>> action in _actions)
            {
                action.Value.RemoveAt(stateIndex);
            }
            _states.Remove(state);
        }

        private List<string> GetMinimizedOutputAlphabet(Dictionary<string, HashSet<string>> matchingMinimizedStatesToStates)
        {
            List<string> minimizedOutputAlphabet = new List<string>();

            foreach (KeyValuePair<string, HashSet<string>> matchingMinimizedStateToStates in matchingMinimizedStatesToStates)
            {
                minimizedOutputAlphabet.Add(_outputSignals[_states.IndexOf(matchingMinimizedStateToStates.Value.First())]);
            }

            return minimizedOutputAlphabet;
        }

        private void UpdateActions(List<List<string>> newTransitions)
        {
            foreach (List<string> transitions in newTransitions)
            {
                string currentSignal = _actions.Keys.ToList()[newTransitions.IndexOf(transitions)];
                _actions[currentSignal] = transitions;
            }
        }
    }
}
