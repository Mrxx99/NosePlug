﻿using System;

namespace NosePlug
{
    public static class INasalPlugExtensions
    {
        public static void Returns<TReturn>(this INasalPlug plug, TReturn returnValue)
        {
            if (plug is null)
            {
                throw new ArgumentNullException(nameof(plug));
            }

            plug.Returns(() => returnValue);
        }

        public static void Returns<TProperty>(this INasalPropertyPlug<TProperty> plug, TProperty returnValue)
        {
            if (plug is null)
            {
                throw new ArgumentNullException(nameof(plug));
            }

            plug.Returns(() => returnValue);
        }
    }
}