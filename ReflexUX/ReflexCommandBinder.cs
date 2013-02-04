// Copyright 2013 Max Battcher. Some rights reserved. Licensed for use under the Microsoft Permissive License (Ms-PL).
// Rewritten/forked based upon ImpromptuInterface.MVVM, Copyright 2011 Ekon Benefits. Licensed for use under the Apache License 2.0.
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using ImpromptuInterface;
using ImpromptuInterface.Dynamic;
using Microsoft.CSharp.RuntimeBinder;
using ReactiveUI.Xaml;

namespace ReflexUX
{
    public class ReflexCommandClosure
    {
        private readonly object target;
        private readonly CacheableInvocation invoke;
        private readonly CacheableInvocation invokeNoArg;

        public ReflexCommandClosure(object target, String_OR_InvokeMemberName invokeName)
        {
            this.target = target;
            this.invoke = new CacheableInvocation(InvocationKind.InvokeMemberAction, invokeName, 1);
            this.invokeNoArg = new CacheableInvocation(InvocationKind.InvokeMemberAction, invokeName, 0);
        }

        public object Invoke(object parameter)
        {
            try
            {
                if (parameter == null)
                {
                    return invokeNoArg.Invoke(target);
                }
            }
            catch (RuntimeBinderException) { /* pass */ }
            catch (TargetParameterCountException) { /* pass */ }
            
            return invoke.Invoke(target, parameter);
        }
    }

    public class ReflexCommandBinder : DynamicObject // TODO: ICustomTypeProvider
    {
        private readonly object parent;
        private readonly Dictionary<string, IReactiveCommand> commands = new Dictionary<string, IReactiveCommand>();
        private bool async;

        public ReflexCommandBinder(object parent, bool async = false)
        {
            this.parent = parent;
            this.async = async;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = value as IReactiveCommand;
            return true;
        }

        public IReactiveCommand this[string name]
        {
            get
            {
                IReactiveCommand result;

                if (!commands.TryGetValue(name, out result))
                {
                    var closure = new ReflexCommandClosure(parent, name);
                    if (async)
                    {
                        var resultasync = new ReactiveAsyncCommand();
                        resultasync.RegisterAsyncAction(p => closure.Invoke(p));
                        result = resultasync;
                    }
                    else
                    {
                        result = new ReactiveCommand();
                        result.Subscribe(p => closure.Invoke(p));
                    }
                    commands.Add(name, result);
                }

                return result;
            }
            set
            {
                if (value != null)
                {
                    var closure = new ReflexCommandClosure(parent, name);
                    var valueasync = value as ReactiveAsyncCommand;
                    if (valueasync != null)
                    {
                        valueasync.RegisterAsyncAction(p => closure.Invoke(p));
                    }
                    else
                    {
                        value.Subscribe(p => closure.Invoke(p));
                    }
                    commands[name] = value;
                }
            }
        }
    }
}
