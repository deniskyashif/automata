using System.Collections.Generic;
using System.Linq;

public class Dfsa
{
    public Dfsa(
        IEnumerable<int> states, 
        int initialState, 
        IEnumerable<int> finalStates,
        IReadOnlyDictionary<(int, char), int> transitions)
    {
        this.States = states.ToHashSet();
        this.Initial = initialState;
        this.Final = finalStates.ToHashSet();
        this.Transitions = transitions;
    }

    public IReadOnlyCollection<int> States { get; private set; }
    
    public int Initial { get; private set; }

    public IReadOnlyCollection<int> Final { get; private set; }

    public IReadOnlyDictionary<(int From, char Label), int> Transitions { get; private set; }

    public bool Recognize(string word)
    {
        var curr = this.Initial;

        foreach (var symbol in word)
        {
            if (!this.Transitions.ContainsKey((curr, symbol)))
                return false;

            curr = this.Transitions[(curr, symbol)];
        }

        return this.Final.Contains(curr);
    }

    public (bool Success, IList<int> Path) RecognitionPathLToR(string word)
    {
        var curr = this.Initial;
        var path = new List<int> { curr };

        foreach (var symbol in word)
        {
            if (!this.Transitions.ContainsKey((curr, symbol)))
                return (false, path);

            curr = this.Transitions[(curr, symbol)];
            path.Add(curr);
        }

        return (this.Final.Contains(curr), path);
    }

    public (bool Success, IList<int> Path) RecognitionPathRToL(string word)
    {
        var current = this.Initial;
        var path = new List<int> { current };

        for (var i = word.Length - 1; i >= 0; i--)
        {
            var symbol = word[i];

            if (!this.Transitions.ContainsKey((current, symbol)))
                return (false, path);

            current = this.Transitions[(current, symbol)];
            path.Add(current);
        }

        return (this.Final.Contains(current), path);
    }
}