using System;
using System.Collections.Generic;
using System.Linq;

/*
    Symbolic finite-state automaton.
*/
public class Sfsa
{
    public Sfsa(IEnumerable<int> states,
        IEnumerable<int> initial,
        IEnumerable<int> final,
        IEnumerable<(int, Range, int)> transitions)
    {
        States = states.ToList();
        Initial = initial.ToList();
        Final = final.ToList();
        Transitions = transitions.ToList();
    }

    public IReadOnlyCollection<int> States { get; private set; }
    public IReadOnlyCollection<int> Initial { get; private set; }
    public IReadOnlyCollection<int> Final { get; private set; }
    public IReadOnlyCollection<(int From, Range Label, int To)> Transitions { get; private set; }

    public bool Recognize(string word)
    {
        IEnumerable<int> currentStates = this.Initial;

        foreach (var symbol in word)
        {
            var nextStates = currentStates
                .SelectMany(EpsilonClosure)
                .SelectMany(s => this.GetTransitions(s, symbol));

            currentStates = nextStates;

            if (!currentStates.Any())
                break;
        }

        return this.Final
            .Intersect(currentStates.SelectMany(EpsilonClosure))
            .Any();
    }

     public IEnumerable<int> EpsilonClosure(int state) => 
        this.Transitions
            .Where(t => t.Label == null)
            .Select(t => (t.From, t.To))
            .ToHashSet()
            .TransitiveClosure()
            .Where(p => p.Item1 == state)
            .Select(p => p.Item2)
            .Union(new[] { state });

    IEnumerable<int> GetTransitions(int state, char symbol) => 
        this.Transitions
            .Where(t => t.From == state && t.Label != null && t.Label.Includes(symbol))
            .Select(t => t.To);

    IDictionary<int, IEnumerable<int>> PrecomputeEpsilonClosure() => 
        this.Transitions
            .Where(t => t.Label == null)
            .Select(t => (t.From, t.To))
            .ToHashSet()
            .TransitiveClosure()
            .Union(this.States.Select(s => (From: s, To: s)))
            .GroupBy(p => p.Item1, p => p.Item2)
            .ToDictionary(g => g.Key, g => g.AsEnumerable());
    
}