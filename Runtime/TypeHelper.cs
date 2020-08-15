using System;
using UnityEngine;
using UnityEngine.Assertions;


namespace CSharpZombieDetector
{

	/// <summary>
	/// Helper for testing type information.
	/// 
	/// Can test for zombie objects, and whether a string type exists in the assembly.
	/// </summary>
	public class TypeHelper
	{

		/// <summary>
		/// Determines if a type can be a possible Zombie object.
		/// </summary>
		public static bool IsZombieType(Type type)
		{
			if (type == null)
				return false;
			if (type.IsValueType) // Primitives and structs.
				return false;

			if (Type.GetTypeCode(type) != TypeCode.Object)
				return false;

			// For a nullable "Something?", recurse into the "Something" part.
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				return IsZombieType(Nullable.GetUnderlyingType(type));

			// TODO: Presumably test here whether type.isSubclassOf (typeof (UnityEngine.Object))
			return true;
		}


		/// <summary>
		/// Tests the IsZombieType method.
		/// </summary>
		public static void TestIsZombieType()
		{
			// Note* Testing with GetType because of handling with non-numerics. See:
			// http://msdn.microsoft.com/en-us/library/ms366789.aspx

			//*** Not Zombies ***
			Assert.IsFalse(IsZombieType(null));
			Assert.IsFalse(IsZombieType(typeof(DBNull)));
			Assert.IsFalse(IsZombieType(typeof(DateTime)));
			Assert.IsFalse(IsZombieType(DBNull.Value.GetType()));
			Assert.IsFalse(IsZombieType((new DateTime(2009, 1, 1)).GetType()));
			// Using GetType - nullable non-numeric types
			DateTime? nullableDateTime = new DateTime(2009, 1, 1);
			Assert.IsFalse(IsZombieType(nullableDateTime.GetType()));
			Assert.IsFalse(IsZombieType(typeof(DateTime?)));

			// strings and bools
			Assert.IsFalse(IsZombieType(typeof(bool)));
			Assert.IsFalse(IsZombieType(typeof(char)));
			Assert.IsFalse(IsZombieType(typeof(string)));
			Assert.IsFalse(IsZombieType(typeof(bool?)));
			Assert.IsFalse(IsZombieType(typeof(char?)));
			Assert.IsFalse(IsZombieType(true.GetType()));
			Assert.IsFalse(IsZombieType('a'.GetType()));
			Assert.IsFalse(IsZombieType(string.Empty.GetType()));
			bool? nullableBool = true;
			Assert.IsFalse(IsZombieType(nullableBool.GetType()));
			char? nullableChar = ' ';
			Assert.IsFalse(IsZombieType(nullableChar.GetType()));
			// Numeric types
			Assert.IsFalse(IsZombieType(typeof(byte)));
			Assert.IsFalse(IsZombieType(typeof(decimal)));
			Assert.IsFalse(IsZombieType(typeof(double)));
			Assert.IsFalse(IsZombieType(typeof(short)));
			Assert.IsFalse(IsZombieType(typeof(int)));
			Assert.IsFalse(IsZombieType(typeof(long)));
			Assert.IsFalse(IsZombieType(typeof(sbyte)));
			Assert.IsFalse(IsZombieType(typeof(float)));
			Assert.IsFalse(IsZombieType(typeof(ushort)));
			Assert.IsFalse(IsZombieType(typeof(uint)));
			Assert.IsFalse(IsZombieType(typeof(ulong)));
			// Nullable numeric types
			Assert.IsFalse(IsZombieType(typeof(byte?)));
			Assert.IsFalse(IsZombieType(typeof(decimal?)));
			Assert.IsFalse(IsZombieType(typeof(double?)));
			Assert.IsFalse(IsZombieType(typeof(short?)));
			Assert.IsFalse(IsZombieType(typeof(int?)));
			Assert.IsFalse(IsZombieType(typeof(long?)));
			Assert.IsFalse(IsZombieType(typeof(sbyte?)));
			Assert.IsFalse(IsZombieType(typeof(float?)));
			Assert.IsFalse(IsZombieType(typeof(ushort?)));
			Assert.IsFalse(IsZombieType(typeof(uint?)));
			Assert.IsFalse(IsZombieType(typeof(ulong?)));
			// Using GetType - numeric types
			Assert.IsFalse(IsZombieType((new byte()).GetType()));
			Assert.IsFalse(IsZombieType(43.2m.GetType()));
			Assert.IsFalse(IsZombieType(43.2d.GetType()));
			Assert.IsFalse(IsZombieType(((short)2).GetType()));
			Assert.IsFalse(IsZombieType(((int)2).GetType()));
			Assert.IsFalse(IsZombieType(((long)2).GetType()));
			Assert.IsFalse(IsZombieType(((sbyte)2).GetType()));
			Assert.IsFalse(IsZombieType(2f.GetType()));
			Assert.IsFalse(IsZombieType(((ushort)2).GetType()));
			Assert.IsFalse(IsZombieType(((uint)2).GetType()));
			Assert.IsFalse(IsZombieType(((ulong)2).GetType()));

			uint test = 2;
			Assert.IsFalse(IsZombieType(((object)test).GetType()));
			// Using GetType - nullable numeric types
			byte? nullableByte = 12;
			Assert.IsFalse(IsZombieType(nullableByte.GetType()));
			decimal? nullableDecimal = 12.2m;
			Assert.IsFalse(IsZombieType(nullableDecimal.GetType()));
			double? nullableDouble = 12.32;
			Assert.IsFalse(IsZombieType(nullableDouble.GetType()));
			short? nullableInt16 = 12;
			Assert.IsFalse(IsZombieType(nullableInt16.GetType()));
			short? nullableInt32 = 12;
			Assert.IsFalse(IsZombieType(nullableInt32.GetType()));
			short? nullableInt64 = 12;
			Assert.IsFalse(IsZombieType(nullableInt64.GetType()));
			sbyte? nullableSByte = 12;
			Assert.IsFalse(IsZombieType(nullableSByte.GetType()));
			float? nullableSingle = 3.2f;
			Assert.IsFalse(IsZombieType(nullableSingle.GetType()));
			ushort? nullableUInt16 = 12;
			Assert.IsFalse(IsZombieType(nullableUInt16.GetType()));
			ushort? nullableUInt32 = 12;
			Assert.IsFalse(IsZombieType(nullableUInt32.GetType()));
			ushort? nullableUInt64 = 12;
			Assert.IsFalse(IsZombieType(nullableUInt64.GetType()));

			//*** Possible Zombies ***

			Assert.IsTrue(IsZombieType(typeof(object)));
			Assert.IsTrue(IsZombieType((new object()).GetType()));

			// Arrays of numeric and non-numeric types
			Assert.IsTrue(IsZombieType(typeof(object[])));
			Assert.IsTrue(IsZombieType(typeof(DBNull[])));
			Assert.IsTrue(IsZombieType(typeof(bool[])));
			Assert.IsTrue(IsZombieType(typeof(char[])));
			Assert.IsTrue(IsZombieType(typeof(DateTime[])));
			Assert.IsTrue(IsZombieType(typeof(string[])));
			Assert.IsTrue(IsZombieType(typeof(byte[])));
			Assert.IsTrue(IsZombieType(typeof(decimal[])));
			Assert.IsTrue(IsZombieType(typeof(double[])));
			Assert.IsTrue(IsZombieType(typeof(short[])));
			Assert.IsTrue(IsZombieType(typeof(int[])));
			Assert.IsTrue(IsZombieType(typeof(long[])));
			Assert.IsTrue(IsZombieType(typeof(sbyte[])));
			Assert.IsTrue(IsZombieType(typeof(float[])));
			Assert.IsTrue(IsZombieType(typeof(ushort[])));
			Assert.IsTrue(IsZombieType(typeof(uint[])));
			Assert.IsTrue(IsZombieType(typeof(ulong[])));



			Debug.Log("Finished ZombieType Test");
		}

	}

}