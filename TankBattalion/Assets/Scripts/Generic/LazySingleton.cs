using System;
using System.Threading;

namespace HughSingleTon
{
    public class LazySingleton<T> where T : class
    {
        private static readonly Lazy<T> instance = new Lazy<T>(CreateInstance, LazyThreadSafetyMode.ExecutionAndPublication);

        private static T CreateInstance()
        {
            return Activator.CreateInstance(typeof(T), true) as T;
        }

        public static T GetInstace
        {
            get
            {
                return instance.Value;
            }
        }

        protected LazySingleton() { }

        ~LazySingleton() { }
    }
}