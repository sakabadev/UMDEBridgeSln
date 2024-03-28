// <auto-generated />
#pragma warning disable CS0105
using Demo.Scripts.Master.Item.Model;
using MasterMemory.Validation;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace MD.Tables
{
   public sealed partial class ItemTierTable : TableBase<ItemTier>, ITableUniqueValidate
   {
        public Func<ItemTier, string> PrimaryKeySelector => primaryIndexSelector;
        readonly Func<ItemTier, string> primaryIndexSelector;


        public ItemTierTable(ItemTier[] sortedData)
            : base(sortedData)
        {
            this.primaryIndexSelector = x => x.Id;
            OnAfterConstruct();
        }

        partial void OnAfterConstruct();


        public ItemTier FindById(string key)
        {
            return FindUniqueCore(data, primaryIndexSelector, System.StringComparer.Ordinal, key, true);
        }
        
        public bool TryFindById(string key, out ItemTier result)
        {
            return TryFindUniqueCore(data, primaryIndexSelector, System.StringComparer.Ordinal, key, out result);
        }

        public ItemTier FindClosestById(string key, bool selectLower = true)
        {
            return FindUniqueClosestCore(data, primaryIndexSelector, System.StringComparer.Ordinal, key, selectLower);
        }

        public RangeView<ItemTier> FindRangeById(string min, string max, bool ascendant = true)
        {
            return FindUniqueRangeCore(data, primaryIndexSelector, System.StringComparer.Ordinal, min, max, ascendant);
        }


        void ITableUniqueValidate.ValidateUnique(ValidateResult resultSet)
        {
#if !DISABLE_MASTERMEMORY_VALIDATOR

            ValidateUniqueCore(data, primaryIndexSelector, "Id", resultSet);       

#endif
        }

#if !DISABLE_MASTERMEMORY_METADATABASE

        public static MasterMemory.Meta.MetaTable CreateMetaTable()
        {
            return new MasterMemory.Meta.MetaTable(typeof(ItemTier), typeof(ItemTierTable), "ItemTier",
                new MasterMemory.Meta.MetaProperty[]
                {
                    new MasterMemory.Meta.MetaProperty(typeof(ItemTier).GetProperty("Id")),
                    new MasterMemory.Meta.MetaProperty(typeof(ItemTier).GetProperty("Name")),
                    new MasterMemory.Meta.MetaProperty(typeof(ItemTier).GetProperty("Price")),
                },
                new MasterMemory.Meta.MetaIndex[]{
                    new MasterMemory.Meta.MetaIndex(new System.Reflection.PropertyInfo[] {
                        typeof(ItemTier).GetProperty("Id"),
                    }, true, true, System.StringComparer.Ordinal),
                });
        }

#endif
    }
}