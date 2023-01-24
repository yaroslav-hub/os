using System;
using System.Collections.Generic;
using System.Linq;
using ConversionMealyMoore.Types;

namespace ConversionMealyMoore.Machines
{
    public sealed class MealyMachine : IMachine
    {
        private readonly List<string> _states;
        private readonly Dictionary<string, List<string>> _actions; // input signal -> transitions

        internal MealyMachine( List<string> states, Dictionary<string, List<string>> actions )
        {
            _states = states;
            _actions = actions;
        }

        public MealyMachine( List<string> parameters )
        {
            _states = new List<string>();
            _actions = new Dictionary<string, List<string>>();

            if ( parameters == null || parameters.Count == 0 )
            {
                throw new ArgumentException( "Machine parameters can't be null or empty" );
            }

            _states = parameters.First()
                .Split( ";" )
                .Where( x => !string.IsNullOrWhiteSpace( x ) )
                .ToList();

            if ( _states.Count < 2 )
            {
                throw new ArgumentException( "Invalid states count" );
            }

            foreach ( string action in parameters.Skip( 1 ) )
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

            parameters.Add( ";" + string.Join( ";", _states ) );
            foreach ( KeyValuePair<string, List<string>> action in _actions )
            {
                parameters.Add( action.Key + ";" + string.Join( ";", action.Value ) );
            }

            return parameters;
        }

        public IMachine Convert( ConversionType conversionType )
        {
            return conversionType switch
            {
                ConversionType.ToMoore => GetMooreMachine(),
                ConversionType.ToMealy => this,
                _ => throw new ArgumentOutOfRangeException( nameof( conversionType ) ),
            };
        }

        private MooreMachine GetMooreMachine()
        {
            List<string> outputSignals = new();
            List<string> newStates = new();
            Dictionary<string, List<string>> newActions = new();

            Dictionary<string, string> mooreStatesMap = new();
            foreach ( KeyValuePair<string, List<string>> action in _actions )
            {
                foreach ( string transition in action.Value )
                {
                    if ( mooreStatesMap.ContainsValue( transition ) )
                    {
                        continue;
                    }

                    string newStateName = "S" + newStates.Count;

                    newStates.Add( newStateName );
                    mooreStatesMap.Add( newStateName, transition );
                }
            }

            foreach ( string state in newStates )
            {
                outputSignals.Add( mooreStatesMap[ state ].Split( "/" ).Skip( 1 ).First() );
            }

            foreach ( KeyValuePair<string, List<string>> action in _actions )
            {
                List<string> transitions = new();
                foreach ( string newState in newStates )
                {
                    string oldState = mooreStatesMap[ newState ].Split( "/" ).First();
                    string oldTransition = _actions[ action.Key ][ _states.IndexOf( oldState ) ];
                    transitions.Add( mooreStatesMap.First( x => x.Value == oldTransition ).Key );
                }

                newActions.Add( action.Key, transitions );
            }

            return new MooreMachine( outputSignals, newStates, newActions );
        }
    }
}
