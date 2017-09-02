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
    public class VariableCollection : IEnumerable<VariableData>
    {
        private readonly Dictionary<string, VariableData> _variableData;

        internal VariableCollection()
        {
            _variableData = new Dictionary<string, VariableData>();
        }

        private VariableCollection(Dictionary<string, VariableData> variableData)
        {
            _variableData = variableData;
        }

        /// <summary>
        /// Add a variable to this collection.
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        /// <param name="description">Description of the variable.</param>
        /// <param name="value">String value of the variable.</param>
        /// <param name="semantic">
        ///   The semantic of the variable, denoting any special meaning the variable may have.
        ///   Set to <code>null</code> if the variable does not need a semantic.
        ///   The supported semantics can be found in <see cref="VariableSemantics"/>.
        /// </param>
        /// <param name="type">The type of the variable.</param>
        /// <param name="hidden">Hint for GUI to hide this variable from the user.</param>
        public void Add(string name, string description, string value, string semantic, VariableType type, bool hidden)
        {
            var vdata = new VariableData(name, description, value, semantic, type, hidden);
            _variableData[name] = vdata;
        }

        /// <summary>
        /// Set the given variable to true.
        /// </summary>
        /// <param name="name">The name of the variable to set.</param>
        public void Set(string name)
        {
            Set(name, "true");
        }

        /// <summary>
        /// Set the value of the given variable to the given value. Overrides existing values.
        /// </summary>
        /// <param name="name">The name of the variable to set.</param>
        /// <param name="value">The new value of the variable.</param>
        public void Set(string name, string value)
        {
            if (_variableData.ContainsKey(name))
                _variableData[name].Value = value;
        }

        /// <summary>
        /// Unset a variable, this will make <see cref="Get"/> return <see cref="string.Empty"/> for the variable with the given name.
        /// </summary>
        /// <param name="name">The name of the variable to unset.</param>
        public void Unset(string name)
        {
            if (_variableData.ContainsKey(name))
                _variableData[name].Value = null;
        }

        /// <summary>
        /// Get the value of the given variable. Returns <code>null</code> if the variable is not defined.
        /// </summary>
        /// <param name="name">The name of the variable to get the value of.</param>
        /// <returns>The value of the variable.</returns>
        public VariableData Get(string name)
        {
            return _variableData.TryGetValue(name, out VariableData value) ? value : null;
        }

        /// <summary>
        /// Check if the value of the variable is true. A value is true if it is set and it is not equal to "0" or "false" (ignoring casing).
        /// </summary>
        /// <param name="name">The name of the variable to check.</param>
        /// <returns><code>true</code> if the variable evaluates to true, <code>false</code> if it does not.</returns>
        public bool IsTrue(string name)
        {
            if (!_variableData.ContainsKey(name))
                return false;

            var vData = _variableData[name];
            return Util.IsTrue(vData.Value);
        }

        public IEnumerator<VariableData> GetEnumerator()
        {
            return _variableData.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Makes a copy of this <see cref="VariableCollection"/>, calls <see cref="Set(string, string)"/> for each entry in <paramref name="vars"/> and returns the result.
        /// </summary>
        public VariableCollection With(Dictionary<string, string> vars)
        {
            var result = Clone();
            foreach (var kvp in vars)
                result.Set(kvp.Key, kvp.Value);
            return result;
        }

        public VariableCollection Clone()
        {
            return new VariableCollection(new Dictionary<string, VariableData>(_variableData));
        }
    }

    public class VariableData
    {
        public string Name { get; }
        public string Description { get; }
        public string Value { get; set; }
        public string Semantic { get; }
        public VariableType Type { get; }
        public bool Hidden { get; }

        public bool HasSemantic => !string.IsNullOrEmpty(Semantic);
        public bool HasValue => !string.IsNullOrEmpty(Value);

        internal VariableData(string name, string description, string value, string semantic, VariableType type, bool hidden)
        {
            Name = name;
            Description = description;
            Value = value;
            Semantic = semantic;
            Type = type;
            Hidden = hidden;
        }
    }

    public enum VariableType
    {
        String,
        Boolean,
    }
}