﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace NosePlug
{

    internal interface IPlug : IDisposable
    {
        Task PatchAsync();
    }

    internal class Plug<TReturn> : IPlug
    {
        private static Dictionary<InterceptorKey, Func<TReturn>> Callbacks { get; } = new();

        private static MethodInfo PrefixInfo { get; }
            = typeof(Plug<TReturn>).GetMethod(nameof(Prefix)) ?? throw new MissingMethodException();

        public Plug(PatchProcessor processor, string id,
            MethodBase original,
            Func<TReturn> prefix)
        {
            Processor = processor ?? throw new ArgumentNullException(nameof(processor));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Key = InterceptorKey.FromMethod(original ?? throw new ArgumentNullException(nameof(original)));
            Interceptor = prefix ?? throw new ArgumentNullException(nameof(prefix));

            Processor = Processor.AddPrefix(PrefixInfo);
        }


        public PatchProcessor Processor { get; }
        public string Id { get; }
        public InterceptorKey Key { get; }
        public Func<TReturn> Interceptor { get; }

        public async Task PatchAsync()
        {
            await Key.LockAsync();
            lock (Callbacks)
            {
                Callbacks[Key] = Interceptor;
            }
            _ = Processor.Patch();
        }

        public void Dispose()
        {
            lock (Callbacks)
            {
                Callbacks.Remove(Key);
            }
            Processor.Unpatch(HarmonyPatchType.All, Id);
            Key.Unlock();
        }

        public static bool Prefix(ref TReturn __result, MethodBase __originalMethod)
        {
            bool gotValue;
            Func<TReturn>? interceptor;
            lock (Callbacks)
            {
                gotValue = Callbacks.TryGetValue(InterceptorKey.FromMethod(__originalMethod), out interceptor);
            }
            if (gotValue && interceptor is not null)
            {
                __result = interceptor();
                return false;
            }
            return true;
        }
    }
}
