// <auto-generated />
#pragma warning disable CS0105
using Demo.Scripts.Master.Item.Model;
using Demo.Scripts.Master.Item2;
using MasterMemory.Validation;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine;
using MD.Tables;

namespace MD
{
   public sealed class DatabaseBuilder : DatabaseBuilderBase
   {
        public DatabaseBuilder() : this(null) { }
        public DatabaseBuilder(MessagePack.IFormatterResolver resolver) : base(resolver) { }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<Item> dataSource)
        {
            AppendCore(dataSource, x => x.Id, System.StringComparer.Ordinal);
            return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<Item2> dataSource)
        {
            AppendCore(dataSource, x => x.Id, System.StringComparer.Ordinal);
            return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<ItemTier> dataSource)
        {
            AppendCore(dataSource, x => x.Id, System.StringComparer.Ordinal);
            return this;
        }

    }
}