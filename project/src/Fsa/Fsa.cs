/*  
    Finite-State Automaton -
    Construction and closure operations 
*/
using System.Collections.Generic;
using System.Linq;

public class Fsa
{
    public Fsa(
        IEnumerable<int> states,
        IEnumerable<int> initial,
        IEnumerable<int> final,
        IEnumerable<(int, string, int)> transitions)
    {
        this.States = states.ToHashSet();
        this.Initial = initial.ToHashSet();
        this.Final = final.ToHashSet();
        this.Transitions = transitions.ToHashSet();
    }

    public IReadOnlyCollection<int> States { get; private set; }
    public IReadOnlyCollection<int> Initial { get; private set; }
    public IReadOnlyCollection<int> Final { get; private set; }
    public IReadOnlyCollection<(int From, string Via, int To)> Transitions { get; private set; }

    public bool Recognize(string word)
    {
        IEnumerable<int> currentStates = this.Initial;

        foreach (var symbol in word)
        {
            var nextStates = currentStates
                .SelectMany(EpsilonClosure)
                .SelectMany(s => this.GetTransitions(s, symbol.ToString()));

            currentStates = nextStates;

            if (!currentStates.Any())
                break;
        }

        return this.Final
            .Intersect(currentStates.SelectMany(EpsilonClosure))
            .Any();
    }

    IEnumerable<int> GetTransitions(int state, string word) => 
        this.Transitions
            .Where(t => (state, word) == (t.From, t.Via))
            .Select(t => t.To);

    public IEnumerable<int> EpsilonClosure(int state)
    {
        void TraverseEpsilonTransitions(int current, HashSet<int> visited)
        {
            var epsilonTransitions = this.GetTransitions(current, string.Empty);
            foreach (var epsilonState in epsilonTransitions)
            {
                if (!visited.Contains(epsilonState))
                {
                    visited.Add(epsilonState);
                    TraverseEpsilonTransitions(epsilonState, visited);
                }
            }
        }

        var result = new HashSet<int>() { state };
        TraverseEpsilonTransitions(state, result);

        return result;
    }
}