// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace MGPG
{
    /// <summary>
    /// A collection of key-value pairs.
    /// </summary>
    public class VariableCollection : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _variables;

        internal VariableCollection()
        {
            _variables = new Dictionary<string, string>();
        }

        private VariableCollection(Dictionary<string, string> variables)
        {
            _variables = variables;
        }

        /// <summary>
        /// Set the given variable to true.
        /// </summary>
        /// <param name="name">The name of the variable to set.</param>
        public void Set(string name)
        {
            _variables[name] = "true";
        }

        /// <summary>
        /// Set the value of the given variable to the given value. Overrides existing values.
        /// </summary>
        /// <param name="name">The name of the variable to set.</param>
        /// <param name="value">The new value of the variable.</param>
        public void Set(string name, string value)
        {
            _variables[name] = value;
        }

        /// <summary>
        /// Unset a variable, this will make <see cref="Get"/> return <see cref="string.Empty"/> for the variable with the given name.
        /// </summary>
        /// <param name="name">The name of the variable to unset.</param>
        public void Unset(string name)
        {
            _variables.Remove(name);
        }

        /// <summary>
        /// Get the value of the given variable. Returns <code>null</code> if the variable is not defined.
        /// </summary>
        /// <param name="name">The name of the variable to get the value of.</param>
        /// <returns>The value of the variable.</returns>
        public string Get(string name)
        {
            return _variables.TryGetValue(name, out string value) ? value : null;
        }

        /// <summary>
        /// Check if the value of the variable is true. A value is true if it is set and it is not equal to "0" or "false" (ignoring casing).
        /// </summary>
        /// <param name="name">The name of the variable to check.</param>
        /// <returns><code>true</code> if the variable evaluates to true, <code>false</code> if it does not.</returns>
        public bool IsTrue(string name)
        {
            if (!_variables.ContainsKey(name))
                return false;
            var value = _variables[name];
            if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
                return false;
            if (value.Equals("0", StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _variables.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Makes a copy of this <see cref="VariableCollection"/>, calls <see cref="Set(string, string)"/> for each entry in <paramref name="vars"/> and returns the result.
        /// </summary>
        public VariableCollection With(VariableCollection vars)
        {
            var result = Clone();
            foreach (var kvp in vars)
                result.Set(kvp.Key, kvp.Value);
            return result;
        }

        public VariableCollection Clone()
        {
            return new VariableCollection(new Dictionary<string, string>(_variables));
        }
    }
}