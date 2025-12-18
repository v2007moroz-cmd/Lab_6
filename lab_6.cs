using System;
using System.Collections.Generic;
using System.Linq;


class Calculator<T>
{
    public delegate T Operation(T a, T b);

    public T Execute(T a, T b, Operation operation)
    {
        return operation(a, b);
    }
}


class Repository<T>
{
    private List<T> _items = new List<T>();

    public delegate bool Criteria<TItem>(TItem item);

    public void Add(T item)
    {
        _items.Add(item);
    }

    public List<T> Find(Criteria<T> criteria)
    {
        List<T> result = new List<T>();
        foreach (var item in _items)
        {
            if (criteria(item))
                result.Add(item);
        }
        return result;
    }
}

class FunctionCache<TKey, TResult>
{
    public delegate TResult Func(TKey key);

    private Dictionary<TKey, (TResult Result, DateTime Expire)> _cache
        = new Dictionary<TKey, (TResult, DateTime)>();

    public TResult Execute(TKey key, Func function, int ttlSeconds)
    {
        if (_cache.ContainsKey(key))
        {
            var entry = _cache[key];
            if (DateTime.Now < entry.Expire)
                return entry.Result;
        }

        TResult result = function(key);
        _cache[key] = (result, DateTime.Now.AddSeconds(ttlSeconds));
        return result;
    }
}


class TaskScheduler<TTask, TPriority>
{
    public delegate void TaskExecution(TTask task);

    private SortedDictionary<TPriority, Queue<TTask>> _tasks =
        new SortedDictionary<TPriority, Queue<TTask>>(Comparer<TPriority>.Create((a, b) => b.CompareTo(a)));

    public void AddTask(TTask task, TPriority priority)
    {
        if (!_tasks.ContainsKey(priority))
            _tasks[priority] = new Queue<TTask>();

        _tasks[priority].Enqueue(task);
    }

    public void ExecuteNext(TaskExecution executor)
    {
        if (_tasks.Count == 0)
        {
            Console.WriteLine("No tasks to execute");
            return;
        }

        var highest = _tasks.First();
        var task = highest.Value.Dequeue();

        if (highest.Value.Count == 0)
            _tasks.Remove(highest.Key);

        executor(task);
    }
}


class Program
{
    static void Main()
    {

        Console.WriteLine("=== GENERIC CALCULATOR ===");
        var calc = new Calculator<int>();
        Console.WriteLine(calc.Execute(10, 5, (a, b) => a + b));
        Console.WriteLine(calc.Execute(10, 5, (a, b) => a * b));

        Console.WriteLine("\n=== GENERIC REPOSITORY ===");
        var repo = new Repository<int>();
        repo.Add(10);
        repo.Add(25);
        repo.Add(5);

        var filtered = repo.Find(x => x > 10);
        filtered.ForEach(Console.WriteLine);

        Console.WriteLine("\n=== FUNCTION CACHE ===");
        var cache = new FunctionCache<int, int>();

        int Square(int x)
        {
            Console.WriteLine("Calculating...");
            return x * x;
        }

        Console.WriteLine(cache.Execute(4, Square, 5));
        Console.WriteLine(cache.Execute(4, Square, 5)); 

        Console.WriteLine("\n=== TASK SCHEDULER ===");
        var scheduler = new TaskScheduler<string, int>();

        scheduler.AddTask("Low priority task", 1);
        scheduler.AddTask("High priority task", 10);

        scheduler.ExecuteNext(task => Console.WriteLine(task));
        scheduler.ExecuteNext(task => Console.WriteLine(task));

        Console.ReadKey();
    }
}
