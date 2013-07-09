// Copyright 2013 Max Battcher. Some rights reserved. Licensed for use under the Microsoft Permissive License (Ms-PL).
// Rewritten/forked based upon ImpromptuInterface.MVVM, Copyright 2011 Ekon Benefits. Licensed for use under the Apache License 2.0.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ImpromptuInterface;
using ImpromptuInterface.Dynamic;
using ReactiveUI;
using System.Linq.Expressions;
using System.Reactive.Concurrency;

namespace ReflexUX
{
    public class Reflex : ImpromptuDictionary
    {
        protected readonly ReflexCommandBinder commandBinder;
        protected readonly ReflexCommandBinder commandAsyncBinder;

        protected dynamic Dynamic
        {
            get { return this; }
        }

        public dynamic Command
        {
            get { return commandBinder; }
        }

        public dynamic CommandAsync
        {
            get { return commandAsyncBinder; }
        }

        public Reflex()
            : base()
        {
            commandBinder = new ReflexCommandBinder(this);
            commandAsyncBinder = new ReflexCommandBinder(this, async: true);
        }

        public IObservable<IObservedChange<Reflex, object>> Observe(string propname)
        {
            return this.ObservableForProperty(new[] { propname });
        }

        public ObservableAsPropertyHelper<T> React<T>(string name, IObservable<T> observable, T initialValue = default(T), IScheduler scheduler = null)
        {
            this[name] = initialValue;
            return new ObservableAsPropertyHelper<T>(observable, value =>
                {
                    this[name] = value;
                },
                initialValue,
                scheduler);
        }
    }

    public class Reflex<TProxy> : Reflex where TProxy : class
    {
        private readonly TProxy proxy;
        public TProxy Proxy
        {
            get { return proxy; }
        }

        public Reflex()
            : base()
        {
            proxy = Impromptu.ActLike<TProxy>(this, typeof(INotifyPropertyChanged));
        }

        public IObservable<IObservedChange<TProxy, U>> Observe<U>(Expression<Func<TProxy, U>> propf)
        {
            return Proxy.ObservableForProperty(propf);
        }
    }
}
