﻿using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinskTrans.DesctopClient;

namespace ContextTests
{
	[TestClass]
	public class ContextDesctopTests
	{
		private ContextDesctop context;

		[TestInitialize]
		public void Inicialize()
		{
			context = new ContextDesctop();
		}

		[TestMethod]
		public void SaveTest()
		{
			//Arrange
			Inicialize();

			//Act
			context.Save();

			//Assert
			Assert.IsTrue(File.Exists("data.dat"));
		}

		[TestMethod]
		public void LoadTest()
		{
			//Arrange
			Inicialize();
			ContextDesctop newContext = new ContextDesctop();
			context.Save();

			//Act
			newContext.Load();

			//Assert
			Assert.IsTrue(context.LastUpdateDataDateTime == newContext.LastUpdateDataDateTime &&
						   context.Routs.Count == newContext.Routs.Count &&
						   context.Stops.Count == newContext.Stops.Count &&
						   context.Times.Count == newContext.Times.Count &&
						   context.ActualStops.Count == newContext.ActualStops.Count);
		}
	}
}
