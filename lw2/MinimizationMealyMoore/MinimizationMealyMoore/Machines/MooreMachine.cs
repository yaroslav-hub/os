using System;
using System.Collections.Generic;
using System.Linq;
using ConversionMealyMoore.Types;

namespace ConversionMealyMoore.Machines
{
    public sealed class MooreMachine : IMachine
    {
        private readonly List<string> _outputSignals;
        private readonly List<string> _states;
        private readonly Dictionary<string, List<string>> _actions; // input signal -> states

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

        public IMachine Convert( ConversionType conversionType )
        {
            return conversionType switch
            {
                ConversionType.ToMoore => this,
                ConversionType.ToMealy => GetMealyMachine(),
                _ => throw new ArgumentOutOfRangeException( nameof( conversionType ) ),
            };
        }

        private MealyMachine GetMealyMachine()
        {
            List<string> newStates = _states.ToList();
            Dictionary<string, List<string>> newActions = new();

            foreach ( KeyValuePair<string, List<string>> action in _actions )
            {
                List<string> transitions = new();
                action.Value.ForEach( x => transitions.Add(
                    x
                    + "/"
                    + _outputSignals[ _states.IndexOf( x ) ] ) );

                newActions.Add( action.Key, transitions );
            }

            return new MealyMachine( newStates, newActions );
        }
    }
}
