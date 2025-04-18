using System;
using System.Collections.Generic;
using System.Linq;

namespace CimBios.Tools.ModelDebug.Services;

public class ServiceLocator
{
    private ServiceLocator()
    {
    }

    public static ServiceLocator GetInstance()
    {
        if (_locator == null)
        {
            lock (syncRoot)
            {
                _locator = new ServiceLocator();
            }       
        }

        return _locator;
    }

    public bool RegisterService<T>(T service) where T : class
    {
        if (_services.TryGetValue(typeof(T), out var list))
        {
            if (list.Contains(service))
            {
                return false;
            }

            _services[typeof(T)].Add(service);
            return true;
        }

        _services.Add(typeof(T), new List<object>() { service });
        return true;
    }

    public void UnregisterService<T>(T service) where T : class
    {
        if (_services.TryGetValue(typeof(T), out var list))
        {
            list.Remove(service);

            if (list.Count == 0)
            {
                _services.Remove(typeof(T));
            }
        }
    }

    public bool TryGetService<T>(out T? service, 
        int? hash = null) where T : class
    {
        service = null;

        if (_services.TryGetValue(typeof(T), out var list))
        {
            if (hash == null)
            {
                service = list.FirstOrDefault() as T;
                if (service == null)
                {
                    return false;
                }

                return true;
            }
            else
            {
                var sel = list.Where(s => s.GetHashCode() == hash);
                if (sel.Count() == 0)
                {
                    return false;
                }
                else
                {
                    service = sel.FirstOrDefault() as T;
                    if (service == null)
                    {
                        return false;
                    }

                    return true; 
                }
            }
        }

        return false;
    }

    private Dictionary<System.Type, List<object>> _services 
        = new Dictionary<System.Type, List<object>>();

    private static ServiceLocator? _locator;
    private static object syncRoot = new object();
}
