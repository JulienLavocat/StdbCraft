// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.

using System;
using SpacetimeDB;
using System.Collections.Generic;

namespace StdbCraft.Scripts.SpacetimeDb
{
	[Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
	public partial class BlockChange : IDatabaseTable
	{
		[Newtonsoft.Json.JsonProperty("id")]
		public ulong Id;
		[Newtonsoft.Json.JsonProperty("x")]
		public int X;
		[Newtonsoft.Json.JsonProperty("y")]
		public int Y;
		[Newtonsoft.Json.JsonProperty("z")]
		public int Z;
		[Newtonsoft.Json.JsonProperty("block_id")]
		public int BlockId;

		private static Dictionary<ulong, BlockChange> Id_Index = new Dictionary<ulong, BlockChange>(16);

		private static void InternalOnValueInserted(object insertedValue)
		{
			var val = (BlockChange)insertedValue;
			Id_Index[val.Id] = val;
		}

		private static void InternalOnValueDeleted(object deletedValue)
		{
			var val = (BlockChange)deletedValue;
			Id_Index.Remove(val.Id);
		}

		public static SpacetimeDB.SATS.AlgebraicType GetAlgebraicType()
		{
			return SpacetimeDB.SATS.AlgebraicType.CreateProductType(new SpacetimeDB.SATS.ProductTypeElement[]
			{
				new SpacetimeDB.SATS.ProductTypeElement("id", SpacetimeDB.SATS.AlgebraicType.CreatePrimitiveType(SpacetimeDB.SATS.BuiltinType.Type.U64)),
				new SpacetimeDB.SATS.ProductTypeElement("x", SpacetimeDB.SATS.AlgebraicType.CreatePrimitiveType(SpacetimeDB.SATS.BuiltinType.Type.I32)),
				new SpacetimeDB.SATS.ProductTypeElement("y", SpacetimeDB.SATS.AlgebraicType.CreatePrimitiveType(SpacetimeDB.SATS.BuiltinType.Type.I32)),
				new SpacetimeDB.SATS.ProductTypeElement("z", SpacetimeDB.SATS.AlgebraicType.CreatePrimitiveType(SpacetimeDB.SATS.BuiltinType.Type.I32)),
				new SpacetimeDB.SATS.ProductTypeElement("block_id", SpacetimeDB.SATS.AlgebraicType.CreatePrimitiveType(SpacetimeDB.SATS.BuiltinType.Type.I32)),
			});
		}

		public static explicit operator BlockChange(SpacetimeDB.SATS.AlgebraicValue value)
		{
			if (value == null) return null;
			var productValue = value.AsProductValue();
			return new BlockChange
			{
				Id = productValue.elements[0].AsU64(),
				X = productValue.elements[1].AsI32(),
				Y = productValue.elements[2].AsI32(),
				Z = productValue.elements[3].AsI32(),
				BlockId = productValue.elements[4].AsI32(),
			};
		}

		public static System.Collections.Generic.IEnumerable<BlockChange> Iter()
		{
			foreach(var entry in SpacetimeDBClient.clientDB.GetEntries("BlockChange"))
			{
				yield return (BlockChange)entry.Item2;
			}
		}
		public static int Count()
		{
			return SpacetimeDBClient.clientDB.Count("BlockChange");
		}
		public static BlockChange FilterById(ulong value)
		{
			Id_Index.TryGetValue(value, out var r);
			return r;
		}

		public static System.Collections.Generic.IEnumerable<BlockChange> FilterByX(int value)
		{
			foreach(var entry in SpacetimeDBClient.clientDB.GetEntries("BlockChange"))
			{
				var productValue = entry.Item1.AsProductValue();
				var compareValue = (int)productValue.elements[1].AsI32();
				if (compareValue == value)
				{
					yield return (BlockChange)entry.Item2;
				}
			}
		}

		public static System.Collections.Generic.IEnumerable<BlockChange> FilterByY(int value)
		{
			foreach(var entry in SpacetimeDBClient.clientDB.GetEntries("BlockChange"))
			{
				var productValue = entry.Item1.AsProductValue();
				var compareValue = (int)productValue.elements[2].AsI32();
				if (compareValue == value)
				{
					yield return (BlockChange)entry.Item2;
				}
			}
		}

		public static System.Collections.Generic.IEnumerable<BlockChange> FilterByZ(int value)
		{
			foreach(var entry in SpacetimeDBClient.clientDB.GetEntries("BlockChange"))
			{
				var productValue = entry.Item1.AsProductValue();
				var compareValue = (int)productValue.elements[3].AsI32();
				if (compareValue == value)
				{
					yield return (BlockChange)entry.Item2;
				}
			}
		}

		public static System.Collections.Generic.IEnumerable<BlockChange> FilterByBlockId(int value)
		{
			foreach(var entry in SpacetimeDBClient.clientDB.GetEntries("BlockChange"))
			{
				var productValue = entry.Item1.AsProductValue();
				var compareValue = (int)productValue.elements[4].AsI32();
				if (compareValue == value)
				{
					yield return (BlockChange)entry.Item2;
				}
			}
		}

		public static bool ComparePrimaryKey(SpacetimeDB.SATS.AlgebraicType t, SpacetimeDB.SATS.AlgebraicValue v1, SpacetimeDB.SATS.AlgebraicValue v2)
		{
			var primaryColumnValue1 = v1.AsProductValue().elements[0];
			var primaryColumnValue2 = v2.AsProductValue().elements[0];
			return SpacetimeDB.SATS.AlgebraicValue.Compare(t.product.elements[0].algebraicType, primaryColumnValue1, primaryColumnValue2);
		}

		public static SpacetimeDB.SATS.AlgebraicValue GetPrimaryKeyValue(SpacetimeDB.SATS.AlgebraicValue v)
		{
			return v.AsProductValue().elements[0];
		}

		public static SpacetimeDB.SATS.AlgebraicType GetPrimaryKeyType(SpacetimeDB.SATS.AlgebraicType t)
		{
			return t.product.elements[0].algebraicType;
		}

		public delegate void InsertEventHandler(BlockChange insertedValue, StdbCraft.Scripts.SpacetimeDb.ReducerEvent dbEvent);
		public delegate void UpdateEventHandler(BlockChange oldValue, BlockChange newValue, StdbCraft.Scripts.SpacetimeDb.ReducerEvent dbEvent);
		public delegate void DeleteEventHandler(BlockChange deletedValue, StdbCraft.Scripts.SpacetimeDb.ReducerEvent dbEvent);
		public delegate void RowUpdateEventHandler(BlockChange oldValue, BlockChange newValue, StdbCraft.Scripts.SpacetimeDb.ReducerEvent dbEvent);
		public static event InsertEventHandler OnInsert;
		public static event UpdateEventHandler OnUpdate;
		public static event DeleteEventHandler OnBeforeDelete;
		public static event DeleteEventHandler OnDelete;
		public static event RowUpdateEventHandler OnRowUpdate;

		public static void OnInsertEvent(object newValue, ClientApi.Event dbEvent)
		{
			OnInsert?.Invoke((BlockChange)newValue,(ReducerEvent)dbEvent?.FunctionCall.CallInfo);
		}

		public static void OnUpdateEvent(object oldValue, object newValue, ClientApi.Event dbEvent)
		{
			OnUpdate?.Invoke((BlockChange)oldValue,(BlockChange)newValue,(ReducerEvent)dbEvent?.FunctionCall.CallInfo);
		}

		public static void OnBeforeDeleteEvent(object oldValue, ClientApi.Event dbEvent)
		{
			OnBeforeDelete?.Invoke((BlockChange)oldValue,(ReducerEvent)dbEvent?.FunctionCall.CallInfo);
		}

		public static void OnDeleteEvent(object oldValue, ClientApi.Event dbEvent)
		{
			OnDelete?.Invoke((BlockChange)oldValue,(ReducerEvent)dbEvent?.FunctionCall.CallInfo);
		}

		public static void OnRowUpdateEvent(object oldValue, object newValue, ClientApi.Event dbEvent)
		{
			OnRowUpdate?.Invoke((BlockChange)oldValue,(BlockChange)newValue,(ReducerEvent)dbEvent?.FunctionCall.CallInfo);
		}
	}
}
