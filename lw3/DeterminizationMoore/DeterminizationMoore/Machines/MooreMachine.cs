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
                .Skip( 1 )
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

        public void Determine()
        {
            // Инициализируем новое состояние автомата
            List<string> determinedStates = new List<string>( _states );
            List<string> determinedOutputAlphabet = new List<string>( _outputSignals );
            List<string> determinedInputAlphabet = new List<string>( InputSignals );
            Dictionary<string, string> eclosures = new Dictionary<string, string>();
            List<string> finishStates = new List<string>();
            for ( int i = 0; i < _outputSignals.Count; i++ )
            {
                if ( _outputSignals[ i ] == FINISH_OUTPUT_SYMBOL )
                {
                    finishStates.Add( determinedStates[ i ] );
                }
            }
            List<string> newStates = new List<string>();
            List<List<string>> determinedTransitionFunctions = new List<List<string>>( Transitions );

            do
            {
                newStates = new List<string>();
                // Определение новых состояний в ДКА
                if ( determinedInputAlphabet.Contains( EMPTY_SYMBOL ) )
                {
                    int indexOfEmptySymbol = determinedInputAlphabet.IndexOf( EMPTY_SYMBOL );
                    for ( int i = 0; i < _states.Count; i++ )
                    {
                        if ( determinedTransitionFunctions[ indexOfEmptySymbol ][ i ] != "" )
                        {
                            int stateIndex = i;
                            ISet<string> statesSet = new HashSet<string>();
                            Queue<int> statesIndexQueue = new Queue<int>();

                            statesIndexQueue.Enqueue( stateIndex );
                            while ( stateIndex != -1 )
                            {
                                statesSet.Add( determinedStates[ stateIndex ] );
                                if ( determinedTransitionFunctions[ indexOfEmptySymbol ][ stateIndex ] != "" )
                                {
                                    if ( determinedTransitionFunctions[ indexOfEmptySymbol ][ stateIndex ].Contains( "," ) )
                                    {
                                        List<string> states = determinedTransitionFunctions[ indexOfEmptySymbol ][ stateIndex ].Split( "," ).ToList();

                                        foreach ( string state in states )
                                        {
                                            int indexOfState = determinedStates.IndexOf( state );
                                            statesIndexQueue.Enqueue( indexOfState );
                                        }
                                    }
                                    else
                                    {
                                        int indexOfState = determinedStates.IndexOf( determinedTransitionFunctions[ indexOfEmptySymbol ][ stateIndex ] );
                                        statesIndexQueue.Enqueue( indexOfState );
                                    }
                                }
                                int newStateIndex = stateIndex;
                                while ( newStateIndex == stateIndex && statesIndexQueue.Count > 0 && !statesSet.Contains( determinedTransitionFunctions[ indexOfEmptySymbol ][ newStateIndex ] ) )
                                {
                                    newStateIndex = statesIndexQueue.Dequeue();
                                };
                                if ( newStateIndex == stateIndex )
                                {
                                    stateIndex = -1;
                                }
                                else
                                {
                                    stateIndex = newStateIndex;
                                }
                            }
                            eclosures.Add( _states[ i ], new string( String.Join( ",", statesSet ) ) );
                        }
                        else
                        {
                            eclosures.Add( _states[ i ], _states[ i ] );
                        }
                    }
                    determinedInputAlphabet.RemoveAt( indexOfEmptySymbol );
                    determinedTransitionFunctions.RemoveAt( indexOfEmptySymbol );
                    newStates = new List<string>() { eclosures[ _states[ 0 ] ] };
                }
                else
                {
                    ISet<string> newSet = new HashSet<string>( determinedStates.GetRange( eclosures.Count, determinedStates.Count - eclosures.Count ) );
                    foreach ( List<string> inputSymbolTransitionFunction in determinedTransitionFunctions )
                    {
                        if ( eclosures.Count != 0 )
                        {
                            for ( int i = eclosures.Count; i < inputSymbolTransitionFunction.Count; i++ )
                            {
                                if ( !newSet.Contains( new string( inputSymbolTransitionFunction[ i ].OrderBy( ch => ch ).ToArray() ) ) && inputSymbolTransitionFunction[ i ] != "" )
                                {
                                    newStates.Add( inputSymbolTransitionFunction[ i ] );
                                }
                            }
                        }
                        else
                        {
                            foreach ( string transitionFunction in inputSymbolTransitionFunction )
                            {
                                if ( transitionFunction.Contains( "," ) )
                                {
                                    newStates.Add( transitionFunction );
                                }
                            }
                        }
                    }
                }

                foreach ( string newState in newStates )
                {
                    string determinedState = newState.Replace( ",", "" );
                    if ( eclosures.Count != 0 )
                    {
                        string state = new string( determinedState.OrderBy( ch => ch ).ToArray() );
                        if ( determinedStates.Contains( state ) && eclosures.Count != 0 && determinedStates.IndexOf( state ) >= _states.Count )
                        {
                            // Убираем запятые в функциях перехода
                            foreach ( List<string> inputSymbolTransitionFunctions in determinedTransitionFunctions )
                            {
                                ISet<char> determinedStateCharHashSet = determinedState.ToHashSet<char>();
                                for ( int indexOfTransitionFunction = 0; indexOfTransitionFunction < inputSymbolTransitionFunctions.Count; indexOfTransitionFunction++ )
                                {
                                    string transitionFunction = inputSymbolTransitionFunctions[ indexOfTransitionFunction ];

                                    if ( transitionFunction.Replace( ",", "" ).ToHashSet<char>().SetEquals( determinedStateCharHashSet ) )
                                    {
                                        inputSymbolTransitionFunctions[ indexOfTransitionFunction ] = determinedState;
                                    }
                                }
                            }
                            continue;
                        }
                    }
                    if ( determinedStates.Contains( determinedState ) && eclosures.Count == 0 )
                    {
                        // Убираем запятые в функциях перехода
                        foreach ( List<string> inputSymbolTransitionFunctions in determinedTransitionFunctions )
                        {
                            ISet<char> determinedStateCharHashSet = determinedState.ToHashSet<char>();
                            for ( int indexOfTransitionFunction = 0; indexOfTransitionFunction < inputSymbolTransitionFunctions.Count; indexOfTransitionFunction++ )
                            {
                                string transitionFunction = inputSymbolTransitionFunctions[ indexOfTransitionFunction ];

                                if ( transitionFunction.Replace( ",", "" ).ToHashSet<char>().SetEquals( determinedStateCharHashSet ) )
                                {
                                    inputSymbolTransitionFunctions[ indexOfTransitionFunction ] = determinedState;
                                }
                            }
                        }
                        continue;
                    }
                    // Сортировка символов по алфавиту
                    determinedState = new string( determinedState.OrderBy( ch => ch ).ToArray() );
                    determinedOutputAlphabet.Add( "" );
                    ISet<string> determinedStatesCharHashSet = newState.Split( "," ).ToHashSet<string>();
                    for ( int i = 0; i < finishStates.Count; i++ )
                    {
                        if ( determinedStatesCharHashSet.Contains( finishStates[ i ] ) )
                        {
                            determinedOutputAlphabet[ determinedOutputAlphabet.Count - 1 ] = FINISH_OUTPUT_SYMBOL;
                        }
                    }
                    List<string> states = newState.Split( "," ).ToList();
                    foreach ( List<string> inputSymbolTransitionFunctions in determinedTransitionFunctions )
                    {
                        inputSymbolTransitionFunctions.Add( "" );
                    }

                    // Смотрим каждое состояние из newStates
                    // Добавляем функции перехода для нового состояния
                    foreach ( string state in states )
                    {
                        int indexOfState = determinedStates.IndexOf( state );
                        foreach ( List<string> inputSymbolTransitionFunctions in determinedTransitionFunctions )
                        {
                            string transitionFunction = inputSymbolTransitionFunctions[ indexOfState ];
                            if ( transitionFunction == "" )
                            {
                                continue;
                            }
                            if ( eclosures.ContainsKey( transitionFunction ) )
                            {
                                transitionFunction = eclosures[ transitionFunction ];
                            }
                            else
                            {
                                if ( eclosures.Count != 0 )
                                {
                                    List<string> transitionFunctionStates = transitionFunction.Split( "," ).ToList();
                                    transitionFunction = "";
                                    for ( int i = 0; i < transitionFunctionStates.Count; i++ )
                                    {
                                        if ( transitionFunction == "" )
                                        {
                                            transitionFunction += eclosures[ transitionFunctionStates[ i ] ];
                                        }
                                        else if ( !transitionFunction.Contains( transitionFunctionStates[ i ] ) )
                                        {
                                            transitionFunction += "," + eclosures[ transitionFunctionStates[ i ] ];
                                        }
                                    }
                                }
                            }
                            if ( inputSymbolTransitionFunctions[ inputSymbolTransitionFunctions.Count - 1 ] == "" )
                            {
                                inputSymbolTransitionFunctions[ inputSymbolTransitionFunctions.Count - 1 ] += transitionFunction;
                            }
                            else
                            {
                                if ( !inputSymbolTransitionFunctions[ inputSymbolTransitionFunctions.Count - 1 ].Contains( transitionFunction ) )
                                {
                                    string[] transitions = transitionFunction.Split( "," );
                                    foreach ( string trans in transitions )
                                    {
                                        if ( !inputSymbolTransitionFunctions[ inputSymbolTransitionFunctions.Count - 1 ].Contains( trans ) )
                                        {
                                            inputSymbolTransitionFunctions[ inputSymbolTransitionFunctions.Count - 1 ] += "," + trans;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    determinedStates.Add( determinedState );
                }
            } while ( newStates.Count != 0 );

            // Если были eps-замыкания, то удаляем ненужные состояния
            Dictionary<string, string> newStatesToDeterminedStates = new Dictionary<string, string>();
            for ( int i = eclosures.Count; i < determinedStates.Count; i++ )
            {
                newStatesToDeterminedStates.Add( determinedStates[ i ], "S" + ( i - eclosures.Count ) );
            }
            List<List<string>> newTransitionFunctions = new List<List<string>>();
            foreach ( List<string> inputSymbolTransitionFunctions in determinedTransitionFunctions )
            {
                newTransitionFunctions.Add( inputSymbolTransitionFunctions.GetRange( eclosures.Count, inputSymbolTransitionFunctions.Count - eclosures.Count ) );
            }
            determinedOutputAlphabet = determinedOutputAlphabet.GetRange( eclosures.Count, determinedOutputAlphabet.Count - eclosures.Count );

            // Обновляем функции переходов, добавляя новые названия для состояний
            foreach ( List<string> inputSymbolTransitionFunctions in newTransitionFunctions )
            {
                for ( int i = 0; i < inputSymbolTransitionFunctions.Count; i++ )
                {
                    if ( inputSymbolTransitionFunctions[ i ] != "" )
                    {
                        inputSymbolTransitionFunctions[ i ] = newStatesToDeterminedStates[ new string( inputSymbolTransitionFunctions[ i ].OrderBy( ch => ch ).ToArray() ) ];
                    }
                }
            }

            _states = newStatesToDeterminedStates.Values.ToList();
            UpdateActions( determinedInputAlphabet, newTransitionFunctions );
            _outputSignals = determinedOutputAlphabet;
        }

        private void DeleteUnreachableStates()
        {
            HashSet<string> reachableStates = new HashSet<string>();
            reachableStates.Add( _states.First() );

            foreach ( KeyValuePair<string, List<string>> action in _actions )
            {
                foreach ( string transition in action.Value )
                {
                    int transitionIndex = action.Value.IndexOf( transition );
                    string destinationState = transition;

                    if ( destinationState != _states[ transitionIndex ] )
                    {
                        reachableStates.Add( destinationState );
                    }
                }
            }

            if ( reachableStates.Count == _states.Count )
            {
                return;
            }

            foreach ( string state in _states )
            {
                if ( !reachableStates.Contains( state ) )
                {
                    RemoveState( state );
                }
            }
        }

        private void RemoveState( string state )
        {
            if ( !_states.Contains( state ) )
            {
                return;
            }

            int stateIndex = _states.IndexOf( state );
            foreach ( KeyValuePair<string, List<string>> action in _actions )
            {
                action.Value.RemoveAt( stateIndex );
            }
            _states.Remove( state );
        }

        private void UpdateActions( List<string> inputSignals, List<List<string>> newTransitions )
        {
            _actions.Clear();

            foreach ( List<string> transitions in newTransitions )
            {
                string newInputSignal = inputSignals[ newTransitions.IndexOf( transitions ) ];
                _actions.Add( newInputSignal, transitions );
            }
        }
    }
}
