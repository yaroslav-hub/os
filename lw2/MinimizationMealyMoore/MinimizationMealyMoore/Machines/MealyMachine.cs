using System;
using System.Collections.Generic;
using System.Linq;

namespace ConversionMealyMoore.Machines
{
    public sealed class MealyMachine : IMachine
    {
        private List<string> _states;
        private Dictionary<string, List<string>> _actions; // input signal -> transitions
        private List<List<string>> Transitions => _actions.Values.ToList();

        internal MealyMachine(List<string> states, Dictionary<string, List<string>> actions)
        {
            _states = states;
            _actions = actions;
        }

        public MealyMachine(List<string> parameters)
        {
            _states = new List<string>();
            _actions = new Dictionary<string, List<string>>();

            if (parameters == null || parameters.Count == 0)
            {
                throw new ArgumentException("Machine parameters can't be null or empty");
            }

            _states = parameters.First()
                .Split(";")
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (_states.Count < 2)
            {
                throw new ArgumentException("Invalid states count");
            }

            foreach (string action in parameters.Skip(1))
            {
                List<string> transition = action.Split(";").ToList();
                if (transition.Count - 1 != _states.Count)
                {
                    throw new ArgumentException($"Invalid action line");
                }

                _actions.Add(transition.First(), transition.Skip(1).ToList());
            }
        }

        public List<string> GetParameters()
        {
            List<string> parameters = new();

            parameters.Add(";" + string.Join(";", _states));
            foreach (KeyValuePair<string, List<string>> action in _actions)
            {
                parameters.Add(action.Key + ";" + string.Join(";", action.Value));
            }

            return parameters;
        }

        public void Minimize()
        {
            DeleteUnreachableStates();
            Dictionary<string, HashSet<string>> previousMatchingMinimizedStatesToStates = new Dictionary<string, HashSet<string>>();
            Dictionary<string, HashSet<string>> currentMatchingMinimizedStatesToStates = new Dictionary<string, HashSet<string>>();
            foreach (string state in _states)
            {
                currentMatchingMinimizedStatesToStates.Add(state, _states.ToHashSet());
            }
            List<List<string>> currentTransitions = Transitions;

            do
            {
                previousMatchingMinimizedStatesToStates = currentMatchingMinimizedStatesToStates;
                currentMatchingMinimizedStatesToStates = GetMatchingMinimizedStatesToStates(previousMatchingMinimizedStatesToStates, currentTransitions);
                currentTransitions = GetNewTransitions(currentMatchingMinimizedStatesToStates);
            } while (_states.Count != currentMatchingMinimizedStatesToStates.Count && currentMatchingMinimizedStatesToStates.Count != previousMatchingMinimizedStatesToStates.Count);

            List<string> newStates = currentMatchingMinimizedStatesToStates.Keys.ToList();
            List<List<string>> minimizedTransitions = GetMinimizedTransitions(currentMatchingMinimizedStatesToStates);
            _states = newStates;
            UpdateActions(minimizedTransitions);
        }

        private Dictionary<string, HashSet<string>> GetMatchingMinimizedStatesToStates(Dictionary<string, HashSet<string>> matchingEquivalenceClassesToStates, List<List<string>> transitions)
        {
            Dictionary<string, HashSet<string>> matchingNewStatesToPreviousStates = new Dictionary<string, HashSet<string>>();
            Dictionary<string, List<string>> matchingNewStatesToTransitions = new Dictionary<string, List<string>>();

            foreach (string state in _states)
            {
                int stateIndex = _states.IndexOf(state);

                // Получение всех переходов по классам эквивалентности
                List<string> transitionsSequence = new List<string>();
                foreach (List<string> innerStateTransitions in transitions)
                {
                    if (innerStateTransitions[stateIndex].Contains("/"))
                    {
                        transitionsSequence.Add(innerStateTransitions[stateIndex].Split("/")[1]);
                    }
                    else
                    {
                        transitionsSequence.Add(innerStateTransitions[stateIndex]);
                    }
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
                foreach (string oldTransition in innerStateTransitions)
                {
                    string oldState = oldTransition.Contains("/") ? oldTransition.Split("/")[0] : oldTransition;
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
                        if (localMatchingMinimizedStateToStates.Value.Contains(innerStateTransitions[_states.IndexOf(matchingMinimizedStateToStates.Value.First())].Split("/")[0]))
                        {
                            innerStateMinimizedTransitions.Add(localMatchingMinimizedStateToStates.Key + "/" + (innerStateTransitions[_states.IndexOf(matchingMinimizedStateToStates.Value.First())].Split("/")[1]));
                        }
                    }
                }
                if (minimizedTransitions.Count != innerStateMinimizedTransitions.Count)
                {
                    foreach (string transition in innerStateMinimizedTransitions)
                    {
                        minimizedTransitions.Add(new List<string>() { transition });
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

        private void DeleteUnreachableStates()
        {
            HashSet<string> reachableStates = new HashSet<string>();
            reachableStates.Add(_states.First());

            foreach (KeyValuePair<string, List<string>> action in _actions)
            {
                foreach (string transition in action.Value)
                {
                    int transitionIndex = action.Value.IndexOf(transition);
                    string destinationState = transition.Split("/").First();

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
