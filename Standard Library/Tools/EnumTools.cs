﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace RedStapler.StandardLibrary {
	public static class EnumTools {
		/// <summary>
		/// Gets the values of the specified enumeration type.
		/// </summary>
		// C# doesn't allow constraining the type to an Enum.
		public static IEnumerable<T> GetValues<T>() {
			return Enum.GetValues( typeof( T ) ).Cast<T>();
		}

		/// <summary>
		/// Looks for <see cref="EnumToEnglishAttribute"/> and if available, return its value.
		/// Otherwise, returns the name of the enum value applied-with <see cref="StringTools.CamelToEnglish(string)"/>.
		/// </summary>
		public static string ToEnglish( this Enum e ) {
			var name = e.GetAttribute<EnumToEnglishAttribute>();
			return name != null ? name.English : Enum.GetName( e.GetType(), e ).CamelToEnglish();
		}
	}
}