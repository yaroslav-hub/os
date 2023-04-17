using System;
using System.Collections.Generic;
using System.Linq;

namespace ConversionMealyMoore.Machines
{
    public sealed class MooreMachine : IMachine
    {
        private const string FINISH_OUTPUT_SYMBOL = "F";
        private const string EMPTY_SYMBOL = "e";

        private List<string> _outputSignals;
        private List<string> _states;
        private Dictionary<string, List<string>> _actions; // input signal -> states
        private List<string> InputSignals => _actions.Keys.ToList();
        private List<List<string>> Transitions => _actions.Values.ToList();

        internal MooreMachine(List<string> outputSignals, List<string> states, Dictionary<string, List<string>> actions)
        {
            _outputSignals = outputSignals;
            _states = states;
            _actions = actions;
        }

        public MooreMachine(List<string> parameters)
        {
            _outputSignals = new List<string>();
            _states = new List<string>();
            _actions = new Dictionary<string, List<string>>();

            if (parameters == null || parameters.Count == 0)
            {
                throw new ArgumentException("Machine parameters can't be null or empty");
            }

            _outputSignals = parameters.First()
                .Split(";")
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            _states = parameters.Skip(1).First()
                .Split(";")
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (_states.Count < 2)
            {
                throw new ArgumentException("Invalid states count");
            }

            foreach (string action in parameters.Skip(2))
            {
                List<string> transition = action.Split(";").ToList();
                if (transition.Count - 1 != _states.Count)
                {
                    throw new ArgumentException("Invalid action line");
                }

                _actions.Add(transition.First(), transition.Skip(1).ToList());
            }
        }

        public List<string> GetParameters()
        {
            List<string> parameters = new();

            parameters.Add(";" + string.Join(";", _outputSignals));
            parameters.Add(";" + string.Join(";", _states));
            foreach (KeyValuePair<string, List<string>> action in _actions)
            {
                parameters.Add(action.Key + ";" + string.Join(";", action.Value));
            }

            return parameters;
        }

        public void Determine()
        {
            // Инициализируем новое состояние автомата
            List<string> determinedStates = new List<string>(_states);
            List<string> determinedOutputAlphabet = new List<string>(_outputSignals);
            List<string> determinedInputAlphabet = new List<string>(InputSignals);
            Dictionary<string, string> eclosures = new Dictionary<string, string>();
            List<string> finishStates = new List<string>();
            foreach (string outputSignal in _outputSignals)
            {
                if (outputSignal == FINISH_OUTPUT_SYMBOL)
                {
                    finishStates.Add(determinedStates[_outputSignals.IndexOf(outputSignal)]);
                }
            }
            List<string> newStates = new List<string>();
            List<List<string>> determinedTransitions = new List<List<string>>(Transitions);

            do
            {
                newStates = new List<string>();
                // Определение новых состояний в ДКА
                if (determinedInputAlphabet.Contains(EMPTY_SYMBOL))
                {
                    int emptySymbolIndex = determinedInputAlphabet.IndexOf(EMPTY_SYMBOL);
                    foreach (string state in _states)
                    {
                        int stateIndex = _states.IndexOf(state);

                        if (determinedTransitions[emptySymbolIndex][stateIndex] != "")
                        {
                            ISet<string> statesSet = new HashSet<string>();
                            Queue<int> statesIndexQueue = new Queue<int>();

                            statesIndexQueue.Enqueue(stateIndex);
                            while (stateIndex != -1)
                            {
                                statesSet.Add(determinedStates[stateIndex]);
                                if (determinedTransitions[emptySymbolIndex][stateIndex] != "")
                                {
                                    if (determinedTransitions[emptySymbolIndex][stateIndex].Contains(","))
                                    {
                                        List<string> states = determinedTransitions[emptySymbolIndex][stateIndex].Split(",").ToList();

                                        for (int i = 0; i < states.Count; i++)
                                        {
                                            statesIndexQueue.Enqueue(i);
                                        }
                                    }
                                    else
                                    {
                                        int indexOfState = determinedStates.IndexOf(determinedTransitions[emptySymbolIndex][stateIndex]);
                                        statesIndexQueue.Enqueue(indexOfState);
                                    }
                                }
                                int newStateIndex = stateIndex;
                                while (newStateIndex == stateIndex && statesIndexQueue.Count > 0 && !statesSet.Contains(determinedTransitions[emptySymbolIndex][newStateIndex]))
                                {
                                    newStateIndex = statesIndexQueue.Dequeue();
                                };
                                if (newStateIndex == stateIndex)
                                {
                                    stateIndex = -1;
                                }
                                else
                                {
                                    stateIndex = newStateIndex;
                                }
                            }
                            eclosures.Add(state, new string(String.Join(",", statesSet)));
                        }
                        else
                        {
                            eclosures.Add(state, state);
                        }
                    }
                    determinedInputAlphabet.RemoveAt(emptySymbolIndex);
                    determinedTransitions.RemoveAt(emptySymbolIndex);
                    newStates = new List<string>() { eclosures[_states[0]] };
                }
                else
                {
                    ISet<string> newSet = new HashSet<string>(determinedStates.GetRange(eclosures.Count, determinedStates.Count - eclosures.Count));
                    foreach (List<string> inputSymbolTransitionFunction in determinedTransitions)
                    {
                        if (eclosures.Count != 0)
                        {
                            for (int i = eclosures.Count; i < inputSymbolTransitionFunction.Count; i++)
                            {
                                if (!newSet.Contains(new string(inputSymbolTransitionFunction[i].OrderBy(ch => ch).ToArray())) && inputSymbolTransitionFunction[i] != "")
                                {
                                    newStates.Add(inputSymbolTransitionFunction[i]);
                                }
                            }
                        }
                        else
                        {
                            foreach (string transitionFunction in inputSymbolTransitionFunction)
                            {
                                if (transitionFunction.Contains(","))
                                {
                                    newStates.Add(transitionFunction);
                                }
                            }
                        }
                    }
                }

                foreach (string newState in newStates)
                {
                    string determinedState = newState.Replace(",", "");
                    if (eclosures.Count != 0)
                    {
                        string state = new string(determinedState.OrderBy(ch => ch).ToArray());
                        if (determinedStates.Contains(state) && eclosures.Count != 0 && determinedStates.IndexOf(state) >= _states.Count)
                        {
                            // Убираем запятые в функциях перехода
                            foreach (List<string> inputSymbolTransitions in determinedTransitions)
                            {
                                ISet<char> determinedStateCharHashSet = determinedState.ToHashSet<char>();
                                for (int indexOfTransitionFunction = 0; indexOfTransitionFunction < inputSymbolTransitions.Count; indexOfTransitionFunction++)
                                {
                                    string transitionFunction = inputSymbolTransitions[indexOfTransitionFunction];

                                    if (transitionFunction.Replace(",", "").ToHashSet<char>().SetEquals(determinedStateCharHashSet))
                                    {
                                        inputSymbolTransitions[indexOfTransitionFunction] = determinedState;
                                    }
                                }
                            }
                            continue;
                        }
                    }
                    if (determinedStates.Contains(determinedState) && eclosures.Count == 0)
                    {
                        // Убираем запятые в функциях перехода
                        foreach (List<string> inputSymbolTransitions in determinedTransitions)
                        {
                            ISet<char> determinedStateCharHashSet = determinedState.ToHashSet<char>();
                            for (int indexOfTransitionFunction = 0; indexOfTransitionFunction < inputSymbolTransitions.Count; indexOfTransitionFunction++)
                            {
                                string transitionFunction = inputSymbolTransitions[indexOfTransitionFunction];

                                if (transitionFunction.Replace(",", "").ToHashSet<char>().SetEquals(determinedStateCharHashSet))
                                {
                                    inputSymbolTransitions[indexOfTransitionFunction] = determinedState;
                                }
                            }
                        }
                        continue;
                    }
                    // Сортировка символов по алфавиту
                    determinedState = new string(determinedState.OrderBy(ch => ch).ToArray());
                    determinedOutputAlphabet.Add("");
                    ISet<string> determinedStatesCharHashSet = newState.Split(",").ToHashSet<string>();
                    for (int i = 0; i < finishStates.Count; i++)
                    {
                        if (determinedStatesCharHashSet.Contains(finishStates[i]))
                        {
                            determinedOutputAlphabet[determinedOutputAlphabet.Count - 1] = FINISH_OUTPUT_SYMBOL;
                        }
                    }
                    List<string> states = newState.Split(",").ToList();
                    foreach (List<string> inputSymbolTransitions in determinedTransitions)
                    {
                        inputSymbolTransitions.Add("");
                    }

                    // Смотрим каждое состояние из newStates
                    // Добавляем функции перехода для нового состояния
                    foreach (string state in states)
                    {
                        int indexOfState = determinedStates.IndexOf(state);
                        foreach (List<string> inputSymbolTransitions in determinedTransitions)
                        {
                            string transitionFunction = inputSymbolTransitions[indexOfState];
                            if (transitionFunction == "")
                            {
                                continue;
                            }
                            if (eclosures.ContainsKey(transitionFunction))
                            {
                                transitionFunction = eclosures[transitionFunction];
                            }
                            else
                            {
                                if (eclosures.Count != 0)
                                {
                                    List<string> transitionstates = transitionFunction.Split(",").ToList();
                                    transitionFunction = "";
                                    for (int i = 0; i < transitionstates.Count; i++)
                                    {
                                        if (transitionFunction == "")
                                        {
                                            transitionFunction += eclosures[transitionstates[i]];
                                        }
                                        else if (!transitionFunction.Contains(transitionstates[i]))
                                        {
                                            transitionFunction += "," + eclosures[transitionstates[i]];
                                        }
                                    }
                                }
                            }
                            if (inputSymbolTransitions[inputSymbolTransitions.Count - 1] == "")
                            {
                                inputSymbolTransitions[inputSymbolTransitions.Count - 1] += transitionFunction;
                            }
                            else
                            {
                                if (!inputSymbolTransitions[inputSymbolTransitions.Count - 1].Contains(transitionFunction))
                                {
                                    string[] transitions = transitionFunction.Split(",");
                                    foreach (string trans in transitions)
                                    {
                                        if (!inputSymbolTransitions[inputSymbolTransitions.Count - 1].Contains(trans))
                                        {
                                            inputSymbolTransitions[inputSymbolTransitions.Count - 1] += "," + trans;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    determinedStates.Add(determinedState);
                }
            } while (newStates.Count != 0);

            // Если были eps-замыкания, то удаляем ненужные состояния
            Dictionary<string, string> newStatesToDeterminedStates = new Dictionary<string, string>();
            for (int i = eclosures.Count; i < determinedStates.Count; i++)
            {
                newStatesToDeterminedStates.Add(determinedStates[i], "S" + (i - eclosures.Count));
            }
            List<List<string>> newTransitions = new List<List<string>>();
            foreach (List<string> inputSymbolTransitions in determinedTransitions)
            {
                newTransitions.Add(inputSymbolTransitions.GetRange(eclosures.Count, inputSymbolTransitions.Count - eclosures.Count));
            }
            determinedOutputAlphabet = determinedOutputAlphabet.GetRange(eclosures.Count, determinedOutputAlphabet.Count - eclosures.Count);

            // Обновляем функции переходов, добавляя новые названия для состояний
            foreach (List<string> inputSymbolTransitions in newTransitions)
            {
                for (int i = 0; i < inputSymbolTransitions.Count; i++)
                {
                    if (inputSymbolTransitions[i] != "")
                    {
                        inputSymbolTransitions[i] = newStatesToDeterminedStates[new string(inputSymbolTransitions[i].OrderBy(ch => ch).ToArray())];
                    }
                }
            }

            _states = newStatesToDeterminedStates.Values
                .ToList();
            UpdateActions(determinedInputAlphabet, newTransitions);
            _outputSignals = determinedOutputAlphabet;
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

        private void UpdateActions(List<string> inputSignals, List<List<string>> newTransitions)
        {
            _actions.Clear();

            foreach (List<string> transitions in newTransitions)
            {
                string newInputSignal = inputSignals[newTransitions.IndexOf(transitions)];
                _actions.Add(newInputSignal, transitions);
            }
        }
    }
}
