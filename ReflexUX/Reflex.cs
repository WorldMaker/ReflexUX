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

namespace ReflexUX
{
    public class Reflex : ImpromptuDictionary, IReactiveNotifyPropertyChanged
    {
        private readonly MakeObjectReactiveHelper reactiveHelper;
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
            reactiveHelper = new MakeObjectReactiveHelper(this);
            commandBinder = new ReflexCommandBinder(this);
            commandAsyncBinder = new ReflexCommandBinder(this, async: true);
        }

        public IObservable<IObservedChange<Reflex, object>> Observe(string propname)
        {
            return this.ObservableForProperty(new[] { propname });
        }

        public IObservable<IObservedChange<object, object>> Changed
        {
            get { return reactiveHelper.Changed; }
        }

        public IObservable<IObservedChange<object, object>> Changing
        {
            get { return reactiveHelper.Changing; }
        }

        public IDisposable SuppressChangeNotifications()
        {
            return reactiveHelper.SuppressChangeNotifications();
        }

        public event PropertyChangingEventHandler PropertyChanging;
    }

    public class Reflex<TContract> : Reflex where TContract : class
    {
        private readonly TContract contract;
        public TContract Contract
        {
            get { return contract; }
        }

        public Reflex()
            : base()
        {
            contract = Impromptu.ActLike<TContract>(this, typeof(IReactiveNotifyPropertyChanged), typeof(INotifyPropertyChanged));
        }

        public IObservable<IObservedChange<TContract, U>> Observe<U>(Expression<Func<TContract, U>> propf)
        {
            return Contract.ObservableForProperty(propf);
        }
    }
}
