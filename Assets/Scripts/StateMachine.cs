using UnityEngine;
using System.Collections;

using Eppy;

public class StateMachine {
	public string CurrentState = "";
	
	ArrayList m_states = new ArrayList();
	ArrayList m_transitions = new ArrayList();
	
	string m_nextState = "";
	bool m_forceNextState = false;
	
	public delegate void EnterDelegate( string fromState, string toState );
	public delegate void UpdateDelegate();
	public delegate void ExitDelegate( string fromState, string toState );
	
	public void RegisterState( string stateName, UpdateDelegate updateFunc, EnterDelegate enterFunc, ExitDelegate exitFunc )
	{
		var newState = new Tuple<string, UpdateDelegate, EnterDelegate, ExitDelegate>(stateName, updateFunc, enterFunc, exitFunc);
		m_states.Add( newState );
	}

	public void RegisterTransition( string from, string to )
	{
		var newTransition = new Tuple<string, string> (from, to);
		m_transitions.Add (newTransition);
	}
	
	public void UpdateState()
	{
		foreach( Tuple<string, UpdateDelegate, EnterDelegate, ExitDelegate> state in m_states )
		{
			if( state.Item1 == CurrentState )
			{
				state.Item2();
			}
		}
		
		if (m_nextState != "") {
			DoSwitch ();
			m_nextState = "";
		}
	}
	
	public void SwitchState( string toState )
	{
		SwitchState (toState, false);
	}
	
	public void SwitchState( string toState, bool force )
	{
		m_nextState = toState;
		m_forceNextState = force;
	}
	
	void DoSwitch()
	{
		bool fromStateFound = false;
		Tuple<string, UpdateDelegate, EnterDelegate, ExitDelegate> fromStateInfo = null;
		foreach( Tuple<string, UpdateDelegate, EnterDelegate, ExitDelegate> state in m_states )
		{
			if( state.Item1 == CurrentState )
			{
				fromStateFound = true;
				fromStateInfo = state;
			}
		}
		
		bool toStateFound = false;
		Tuple<string, UpdateDelegate, EnterDelegate, ExitDelegate> toStateInfo = null;
		foreach( Tuple<string, UpdateDelegate, EnterDelegate, ExitDelegate> state in m_states )
		{
			if( state.Item1 == m_nextState )
			{
				toStateFound = true;
				toStateInfo = state;
			}
		}
		
		string fromState = CurrentState;
		bool transitionAllowed = false;
		
		foreach (Tuple<string, string> transition in m_transitions)
		{
			if( transition.Item1 == fromState && transition.Item2 == m_nextState )
			{
				transitionAllowed = true;
			}
		}
		
		if ( !fromStateFound && fromState != "" ) {
			MonoBehaviour.print ( "State " + CurrentState + " unregistered" );
			return;
		}
		
		if ( !toStateFound ) {
			MonoBehaviour.print ( "State " + m_nextState + " unregistered" );
			return;
		}
		
		if( ( !m_forceNextState && transitionAllowed ) || m_forceNextState )
		{
			MonoBehaviour.print ( "Transition from " + fromState.ToString() + " to " + m_nextState.ToString() );
			if( fromStateFound && fromStateInfo.Item4 != null )
			{
				fromStateInfo.Item4( fromState, m_nextState );
			}
			CurrentState = m_nextState;
			if( toStateInfo.Item3 != null )
			{
				toStateInfo.Item3( fromState, m_nextState );
			}
		}
		else
		{
			MonoBehaviour.print ( "Transition from " + fromState.ToString() + " to " + m_nextState.ToString() + " not permitted" );
		}
	}
}