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
		public static bool IsPotentialZombieType(Type type)
		{
			if (type == null)
				return false;
			if (type.IsValueType) // Primitives and structs.
				return false;

			if (Type.GetTypeCode(type) != TypeCode.Object)
				return false;

			// For a nullable "Something?", recurse into the "Something" part.
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				return IsPotentialZombieType(Nullable.GetUnderlyingType(type));

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
			Assert.IsFalse(IsPotentialZombieType(null));
			Assert.IsFalse(IsPotentialZombieType(typeof(DBNull)));
			Assert.IsFalse(IsPotentialZombieType(typeof(DateTime)));
			Assert.IsFalse(IsPotentialZombieType(DBNull.Value.GetType()));
			Assert.IsFalse(IsPotentialZombieType((new DateTime(2009, 1, 1)).GetType()));
			// Using GetType - nullable non-numeric types
			DateTime? nullableDateTime = new DateTime(2009, 1, 1);
			Assert.IsFalse(IsPotentialZombieType(nullableDateTime.GetType()));
			Assert.IsFalse(IsPotentialZombieType(typeof(DateTime?)));

			// strings and bools
			Assert.IsFalse(IsPotentialZombieType(typeof(bool)));
			Assert.IsFalse(IsPotentialZombieType(typeof(char)));
			Assert.IsFalse(IsPotentialZombieType(typeof(string)));
			Assert.IsFalse(IsPotentialZombieType(typeof(bool?)));
			Assert.IsFalse(IsPotentialZombieType(typeof(char?)));
			Assert.IsFalse(IsPotentialZombieType(true.GetType()));
			Assert.IsFalse(IsPotentialZombieType('a'.GetType()));
			Assert.IsFalse(IsPotentialZombieType(string.Empty.GetType()));
			bool? nullableBool = true;
			Assert.IsFalse(IsPotentialZombieType(nullableBool.GetType()));
			char? nullableChar = ' ';
			Assert.IsFalse(IsPotentialZombieType(nullableChar.GetType()));
			// Numeric types
			Assert.IsFalse(IsPotentialZombieType(typeof(byte)));
			Assert.IsFalse(IsPotentialZombieType(typeof(decimal)));
			Assert.IsFalse(IsPotentialZombieType(typeof(double)));
			Assert.IsFalse(IsPotentialZombieType(typeof(short)));
			Assert.IsFalse(IsPotentialZombieType(typeof(int)));
			Assert.IsFalse(IsPotentialZombieType(typeof(long)));
			Assert.IsFalse(IsPotentialZombieType(typeof(sbyte)));
			Assert.IsFalse(IsPotentialZombieType(typeof(float)));
			Assert.IsFalse(IsPotentialZombieType(typeof(ushort)));
			Assert.IsFalse(IsPotentialZombieType(typeof(uint)));
			Assert.IsFalse(IsPotentialZombieType(typeof(ulong)));
			// Nullable numeric types
			Assert.IsFalse(IsPotentialZombieType(typeof(byte?)));
			Assert.IsFalse(IsPotentialZombieType(typeof(decimal?)));
			Assert.IsFalse(IsPotentialZombieType(typeof(double?)));
			Assert.IsFalse(IsPotentialZombieType(typeof(short?)));
			Assert.IsFalse(IsPotentialZombieType(typeof(int?)));
			Assert.IsFalse(IsPotentialZombieType(typeof(long?)));
			Assert.IsFalse(IsPotentialZombieType(typeof(sbyte?)));
			Assert.IsFalse(IsPotentialZombieType(typeof(float?)));
			Assert.IsFalse(IsPotentialZombieType(typeof(ushort?)));
			Assert.IsFalse(IsPotentialZombieType(typeof(uint?)));
			Assert.IsFalse(IsPotentialZombieType(typeof(ulong?)));
			// Using GetType - numeric types
			Assert.IsFalse(IsPotentialZombieType((new byte()).GetType()));
			Assert.IsFalse(IsPotentialZombieType(43.2m.GetType()));
			Assert.IsFalse(IsPotentialZombieType(43.2d.GetType()));
			Assert.IsFalse(IsPotentialZombieType(((short)2).GetType()));
			Assert.IsFalse(IsPotentialZombieType(((int)2).GetType()));
			Assert.IsFalse(IsPotentialZombieType(((long)2).GetType()));
			Assert.IsFalse(IsPotentialZombieType(((sbyte)2).GetType()));
			Assert.IsFalse(IsPotentialZombieType(2f.GetType()));
			Assert.IsFalse(IsPotentialZombieType(((ushort)2).GetType()));
			Assert.IsFalse(IsPotentialZombieType(((uint)2).GetType()));
			Assert.IsFalse(IsPotentialZombieType(((ulong)2).GetType()));

			uint test = 2;
			Assert.IsFalse(IsPotentialZombieType(((object)test).GetType()));
			// Using GetType - nullable numeric types
			byte? nullableByte = 12;
			Assert.IsFalse(IsPotentialZombieType(nullableByte.GetType()));
			decimal? nullableDecimal = 12.2m;
			Assert.IsFalse(IsPotentialZombieType(nullableDecimal.GetType()));
			double? nullableDouble = 12.32;
			Assert.IsFalse(IsPotentialZombieType(nullableDouble.GetType()));
			short? nullableInt16 = 12;
			Assert.IsFalse(IsPotentialZombieType(nullableInt16.GetType()));
			short? nullableInt32 = 12;
			Assert.IsFalse(IsPotentialZombieType(nullableInt32.GetType()));
			short? nullableInt64 = 12;
			Assert.IsFalse(IsPotentialZombieType(nullableInt64.GetType()));
			sbyte? nullableSByte = 12;
			Assert.IsFalse(IsPotentialZombieType(nullableSByte.GetType()));
			float? nullableSingle = 3.2f;
			Assert.IsFalse(IsPotentialZombieType(nullableSingle.GetType()));
			ushort? nullableUInt16 = 12;
			Assert.IsFalse(IsPotentialZombieType(nullableUInt16.GetType()));
			ushort? nullableUInt32 = 12;
			Assert.IsFalse(IsPotentialZombieType(nullableUInt32.GetType()));
			ushort? nullableUInt64 = 12;
			Assert.IsFalse(IsPotentialZombieType(nullableUInt64.GetType()));

			//*** Possible Zombies ***

			Assert.IsTrue(IsPotentialZombieType(typeof(object)));
			Assert.IsTrue(IsPotentialZombieType((new object()).GetType()));

			// Arrays of numeric and non-numeric types
			Assert.IsTrue(IsPotentialZombieType(typeof(object[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(DBNull[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(bool[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(char[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(DateTime[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(string[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(byte[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(decimal[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(double[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(short[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(int[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(long[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(sbyte[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(float[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(ushort[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(uint[])));
			Assert.IsTrue(IsPotentialZombieType(typeof(ulong[])));



			Debug.Log("Finished ZombieType Test");
		}

	}

}