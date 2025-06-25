using Hyprx.Collections.Generic;
using Hyprx.Results;
using Hyprx.Rex.Execution;

namespace Hyprx.Rex.Collections;

public class DependencyMap<TValue> : OrderedMap<TValue>
    where TValue : INeedful
{
    public DependencyMap()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    public DependencyMap(IDictionary<string, TValue> dictionary)
        : base(dictionary)
    {
    }

    public DependencyMap(IEqualityComparer<string>? comparer)
        : base(comparer ?? StringComparer.OrdinalIgnoreCase)
    {
    }

    public DependencyMap(int capacity)
        : base(capacity)
    {
    }

    public static Result<List<TValue>> Flatten(
        DependencyMap<TValue> map,
        List<TValue> targets)
    {
        ArgumentNullException.ThrowIfNull(map, nameof(map));
        ArgumentNullException.ThrowIfNull(targets, nameof(targets));

        var results = new List<TValue>();

        foreach (var item in targets)
        {
            var needs = item.Needs;

            foreach (var dep in needs)
            {
                if (!map.TryGetValue(dep, out var dependency))
                {
                    return new ResourceNotFoundException(dep, typeof(TValue).FullName ?? typeof(TValue).Name);
                }

                var depResult = Flatten(map, [dependency]);
                if (depResult.IsError)
                {
                    return depResult;
                }

                results.AddRange(depResult.Value);
                if (!results.Contains(dependency))
                {
                    results.Add(dependency);
                }
            }

            if (!results.Contains(item))
            {
                results.Add(item);
            }
        }

        return results;
    }

    public Result<List<TValue>> Flatten(List<TValue> targets)
    {
        ArgumentNullException.ThrowIfNull(targets, nameof(targets));
        return Flatten(this, targets);
    }

    public List<(TValue item, List<string> missing)> DetectMissingDependencies()
    {
        var results = new List<(TValue item, List<string> missing)>();

        foreach (var item in this.Values)
        {
            var missing = new List<string>();

            foreach (var dep in item.Needs)
            {
                if (!this.ContainsKey(dep))
                {
                    missing.Add(dep);
                }
            }

            if (missing.Count == 0)
            {
                continue;
            }

            results.Add((item, missing));
        }

        return results;
    }

    public List<TValue> DetectCyclycalReferences()
    {
        var stack = new HashSet<TValue>();
        var cycles = new List<TValue>();

        bool Resolve(TValue item)
        {
            if (stack.Contains(item))
            {
                return false;
            }

            stack.Add(item);

            var needs = item.Needs;
            foreach (var dep in needs)
            {
                if (!this.TryGetValue(dep, out var dependency))
                {
                    continue; // Dependency not found, skip
                }

                if (!Resolve(dependency))
                {
                    return false;
                }
            }

            stack.Remove(item);
            return true;
        }

        foreach (var item in this.Values)
        {
            if (!Resolve(item))
            {
                cycles.Add(item);
            }
        }

        return cycles;
    }
}