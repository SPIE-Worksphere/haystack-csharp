//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectHaystack
{
    public class HRow : HDict
    {
        private List<HVal> m_cells;
        private Lazy<IDictionary<string, int>> m_lazyKeyIndexes;

        // Internal constructor
        internal HRow(HGrid grid, List<HVal> cells) : base(new Dictionary<string, HVal>(11))
        {
            m_cells = cells;
            m_lazyKeyIndexes = new Lazy<IDictionary<string, int>>(() =>
                Enumerable.Range(0, grid.numCols)
                    .ToDictionary(idx => grid.col(idx).Name, idx => idx));
            Grid = grid;
        }

        //////////////////////////////////////////////////////////////////////////
        // Access 
        //////////////////////////////////////////////////////////////////////////
        public HGrid Grid { get; }

        public HDict ToDict()
        {
            return new HDict(GetKeys().ToDictionary(key => key, key => GetValue(key)));
        }

        public override void Add(string key, HVal value)
        {
            throw new NotImplementedException("Cannot add values to a row as it will affect the entire grid");
        }

        public override bool Remove(string key)
        {
            throw new NotImplementedException("Cannot remove values from a row as it will affect the entire grid");
        }

        protected override HVal GetValue(string key) =>
            m_lazyKeyIndexes.Value.ContainsKey(key) ? m_cells[m_lazyKeyIndexes.Value[key]] : null;

        protected override void SetValue(string key, HVal value)
        {
            if (!m_lazyKeyIndexes.Value.ContainsKey(key))
                throw new UnknownNameException(key);
            m_cells[m_lazyKeyIndexes.Value[key]] = value;
        }

        protected override ICollection<HVal> GetValues() => m_cells;

        protected override ICollection<string> GetKeys() => m_lazyKeyIndexes.Value.Keys;
    }
}