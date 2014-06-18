﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable ExpressionIsAlwaysNull

namespace Rocks.Helpers.Tests
{
	[TestClass]
	public class CollectionExtensionsTests
	{
		[TestMethod]
		public void SkipNull_OneItemNull_ReturnsNoNulls ()
		{
			var result = (new[] { "a", null, "b" }).SkipNull ();

			result.Should ().Equal (new[] { "a", "b" });
		}


		[TestMethod]
		public void SkipNull_Null_ReturnsEmpty ()
		{
			var result = ((string[]) null).SkipNull ();

			result.Should ().BeEmpty ();
		}


		[TestMethod]
		public void SkipNullOrEmpty_OneItemEmptyString_AndOneItemNull_ReturnsNoNullOrEmpty ()
		{
			var result = (new[] { "a", null, "b", string.Empty }).SkipNullOrEmpty ();

			result.Should ().Equal (new[] { "a", "b" });
		}


		[TestMethod]
		public void ShouldSkipNullOrEmpty_Null_ReturnsEmpty ()
		{
			var result = ((string[]) null).SkipNullOrEmpty ();

			result.Should ().BeEmpty ();
		}


		[TestMethod]
		public void TrimAll_ReturnsAllTrimmed ()
		{
			var result = (new[] { "a", "b ", " c" }).TrimAll ();

			result.Should ().Equal (new[] { "a", "b", "c" });
		}


		[TestMethod]
		public void TrimAll_Null_ReturnsEmpty ()
		{
			var result = ((string[]) null).TrimAll ();

			result.Should ().BeEmpty ();
		}


		[TestMethod]
		public void AsReadOnlyList_ReturnsIReadOnlyList ()
		{
			// arrange
			var data = new[] { "a", null, "b" };


			// act
			var result = data.Select (x => x).AsReadOnlyList ();


			// assert
			result.Should ()
			      .BeAssignableTo<IReadOnlyList<string>> ()
			      .And
			      .Equal (new[] { "a", null, "b" });
		}


		[TestMethod]
		public void AsReadOnlyList_Null_ReturnsNull ()
		{
			var result = ((string[]) null).AsReadOnlyList ();

			result.Should ().BeNull ();
		}


		[TestMethod]
		public void AsReadOnlyCollection_ReturnsIReadOnlyCollection ()
		{
			// arrange
			var data = new[] { "a", null, "b" };


			// act
			var result = data.Select (x => x).AsReadOnlyCollection ();


			// assert
			result.Should ()
			      .BeAssignableTo<IReadOnlyCollection<string>> ()
			      .And
			      .Equal (new[] { "a", null, "b" });
		}


		[TestMethod]
		public void AsReadOnlyCollection_Null_ReturnsNull ()
		{
			var result = ((string[]) null).AsReadOnlyCollection ();

			result.Should ().BeNull ();
		}


		[TestMethod]
		public void IsNullOrEmpty_Empty_ReturnsTrue ()
		{
			var result = new string[0].IsNullOrEmpty ();

			result.Should ().BeTrue ();
		}


		[TestMethod]
		public void IsNullOrEmpty_NotEmpty_ReturnsFalse ()
		{
			var result = new[] { "a" }.IsNullOrEmpty ();

			result.Should ().BeFalse ();
		}


		[TestMethod]
		public void IsNullOrEmpty_Null_ReturnsTrue ()
		{
			var result = ((IEnumerable<string>) null).IsNullOrEmpty ();

			result.Should ().BeTrue ();
		}


		[TestMethod]
		public void IsNullOrEmpty_ShouldEnumerateOnce ()
		{
			// arrange
			var data = new TestEnumerable<string> (new[] { "a", "b", "c" });


			// act
			var result = data.Enumerate ().IsNullOrEmpty ();


			// assert
			result.Should ().BeFalse ();
			data.EnumeratedCount.Should ().Be (1);
		}



		[TestMethod]
		public void FirstOrNull_ShouldEnumerateOnce ()
		{
			// arrange
			var data = new TestEnumerable<int> (new[] { 1, 2, 3 });


			// act
			var result = data.Enumerate ().FirstOrNull ();


			// assert
			result.Should ().Be (1);
			data.EnumeratedCount.Should ().Be (1);
		}


		[TestMethod]
		public void FirstOrNull_Null_ReturnsNull ()
		{
			var result = ((int[]) null).FirstOrNull ();

			result.Should ().NotHaveValue ();
		}


		[TestMethod]
		public void FirstOrNull_Empty_ReturnsNull ()
		{
			var result = new int[0].FirstOrNull ();

			result.Should ().NotHaveValue ();
		}


		[TestMethod]
		public void SortById_ReturnsCorrectlySorted ()
		{
			// arrange
			var ids = new[] { 1, 2, 3 };
			var items = new[] { new { id = 2 }, new { id = 1 }, new { id = 3 } };


			// act
			var result = items.SortById (ids, x => x.id);


			// assert
			result.ShouldBeEquivalentTo (new[]
			                             {
				                             new { id = 1 },
				                             new { id = 2 },
				                             new { id = 3 }
			                             });
		}


		private class TestDataObject
		{
			public string Name { get; set; }
		}


		[TestMethod]
		public void DistinctBy_ShouldReturnUniqueWithTheSameOrder ()
		{
			// arrange
			var data = new[]
			           {
				           new TestDataObject { Name = "b" },
				           new TestDataObject { Name = "a" },
				           new TestDataObject { Name = "B" }
			           };


			// act
			var result = data.DistinctBy (x => x.Name, StringComparer.OrdinalIgnoreCase);


			// assert
			result.Select (x => x.Name).Should ().Equal (new[] { "b", "a" });
		}


		[TestMethod]
		public void IndexOf_Found_ReturnsCorrectIndex ()
		{
			// arrange
			var data = new[]
			           {
				           new TestDataObject { Name = "a" },
				           new TestDataObject { Name = "b" },
				           new TestDataObject { Name = "c" }
			           };


			// act
			var result = data.IndexOf (x => x.Name == "b");


			// assert
			result.Should ().Be (1);
		}


		[TestMethod]
		public void IndexOf_NotFound_ReturnsMinusOne ()
		{
			// arrange
			var data = new[]
			           {
				           new TestDataObject { Name = "a" },
				           new TestDataObject { Name = "b" }
			           };


			// act
			var result = data.IndexOf (x => x.Name == "c");


			// assert
			result.Should ().Be (-1);
		}


		[TestMethod]
		public void SplitToChunks_Null_ReturnsEmpty ()
		{
			// arrange
			var data = (int[]) null;


			// act
			var result = data.SplitToChunks (10);


			// assert
			result.Should ().BeEmpty ();
		}


		[TestMethod]
		public void SplitToChunks_Empty_ReturnsEmpty ()
		{
			// arrange
			var data = new int[0];


			// act
			var result = data.SplitToChunks (10);


			// assert
			result.Should ().BeEmpty ();
		}


		[TestMethod]
		public void SplitToChunks_EqualNumberOfItemsToChunkSize_ReturnsOneChunkWithAllItems ()
		{
			// arrange
			var data = new[] { 1, 2, 3 };


			// act
			var result = data.SplitToChunks (3);


			// assert
			result.ShouldAllBeEquivalentTo (new[] { data });
		}


		[TestMethod]
		public void SplitToChunks_LessNumberOfItemsThanChunkSize_ReturnsOneChunkWithAllItems ()
		{
			// arrange
			var data = new[] { 1, 2, 3 };


			// act
			var result = data.SplitToChunks (4);


			// assert
			result.ShouldAllBeEquivalentTo (new[] { data });
		}


		[TestMethod]
		public void SplitToChunks_MoreItemsThanChunkSize_ReturnsChunks ()
		{
			// arrange
			var data = new[] { 1, 2, 3 };


			// act
			var result = data.SplitToChunks (2);


			// assert
			result.ShouldAllBeEquivalentTo (new[] { new[] { 1, 2 }, new[] { 3 } });
		}
	}
}