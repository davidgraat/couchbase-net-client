﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Couchbase.Configuration;
using System.Configuration;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Couchbase.Tests.Utils;

namespace Couchbase.Tests
{
	[TestFixture]
	public class DefaultConfigurationSettingsTests
	{
		#region HTTP Factory Tests

		[Test]
		public void When_Using_Code_Config_And_Http_Client_Factory_Is_Not_Set_Hammock_Factory_Is_Default()
		{
			var config = new CouchbaseClientConfiguration();
			config.Urls.Add(new Uri("http://localhost:8091/pools"));
			Assert.That(config.HttpClientFactory, Is.InstanceOf<HammockHttpClientFactory>());

			//HammockHttpClient is an internal class to the Couchbase assembly,
			//therefore the explicit type can't be checked for using Is.InstanceOf<T>
			var typeName = (config.HttpClientFactory.Create(config.Urls[0], "", "", TimeSpan.FromMinutes(1), true).GetType().Name);
			Assert.That(typeName, Is.StringContaining("HammockHttpClient"));
		}

		[Test]
		public void When_Using_App_Config_And_Http_Client_Factory_Is_Not_Set_Hammock_Factory_Is_Default()
		{
			var config = ConfigurationManager.GetSection("min-config") as CouchbaseClientSection;

			Assert.That(config, Is.Not.Null, "min-config section missing from app.config");
			Assert.That(config.HttpClientFactory, Is.InstanceOf<ProviderElement<IHttpClientFactory>>());

			//HammockHttpClient is an internal class to the Couchbase assembly,
			//therefore the explicit type can't be checked for using Is.InstanceOf<T>
			var typeName = (config.HttpClientFactory.CreateInstance().Create(config.Servers.Urls.ToUriCollection()[0], "", "", TimeSpan.FromMinutes(1), true).GetType().Name);
			Assert.That(typeName, Is.StringContaining("HammockHttpClient"));
		}

		[Test]
		public void When_Using_App_Config_And_Http_Client_Factory_Is_Not_Set_Operations_Succeed()
		{
			var config = ConfigurationManager.GetSection("min-config") as CouchbaseClientSection;

			Assert.That(config, Is.Not.Null, "min-config section missing from app.config");
			Assert.That(config.HttpClientFactory, Is.InstanceOf<ProviderElement<IHttpClientFactory>>());

			var client = new CouchbaseClient(config);
			var kv = KeyValueUtils.GenerateKeyAndValue("default_config");

			var result = client.Store(StoreMode.Add, kv.Item1, kv.Item2);
			Assert.That(result, Is.True, "Store failed");

			var value = client.Get(kv.Item1);
			Assert.That(value, Is.StringMatching(kv.Item2));
		}

		[Test]
		public void When_Using_Code_Config_And_Http_Client_Factory_Is_Not_Set_Operations_Succeed()
		{
			var config = new CouchbaseClientConfiguration();
			config.Urls.Add(new Uri("http://localhost:8091/pools"));
			Assert.That(config.HttpClientFactory, Is.InstanceOf<HammockHttpClientFactory>());

			Assert.That(config, Is.Not.Null, "min-config section missing from app.config");
			Assert.That(config.HttpClientFactory, Is.InstanceOf<HammockHttpClientFactory>());

			var client = new CouchbaseClient(config);
			var kv = KeyValueUtils.GenerateKeyAndValue("default_config");

			var result = client.Store(StoreMode.Add, kv.Item1, kv.Item2);
			Assert.That(result, Is.True, "Store failed");

			var value = client.Get(kv.Item1);
			Assert.That(value, Is.StringMatching(kv.Item2));
		}

		#endregion

		#region Design Doc Name Transformer Tests

		[Test]
		public void When_Using_Code_Config_And_Design_Document_Name_Transformer_Is_Not_Set_Production_Mode_Is_Default()
		{
			var config = new CouchbaseClientConfiguration();
			config.Urls.Add(new Uri("http://localhost:8091/pools"));
			var client = new CouchbaseClient(config); //client sets up transformer

			Assert.That(config.DesignDocumentNameTransformer, Is.InstanceOf<ProductionModeNameTransformer>());		}

		[Test]
		public void When_Using_App_Config_And_Design_Document_Name_Transformer_Is_Not_Set_Production_Mode_Is_Default()
		{
			var config = ConfigurationManager.GetSection("min-config") as CouchbaseClientSection;
			var client = new CouchbaseClient(config); //client sets up transformer

			Assert.That(config.DocumentNameTransformer.Type.Name, Is.StringMatching("ProductionModeNameTransformer"));

		}

		#endregion

		#region Timeouts
		[Test]
		public void When_Http_Timeout_Is_Not_Set_And_Using_App_Config_Default_Is_20_Seconds()
		{
			var config = ConfigurationManager.GetSection("httptimeout-default-config") as CouchbaseClientSection;
			Assert.That(config, Is.Not.Null, "Config was null");
			Assert.That(config.Servers.HttpRequestTimeout, Is.EqualTo(TimeSpan.FromSeconds(20)));
		}

		[Test]
		public void When_Http_Timeout_Is_Not_Set_And_Using_Code_Config_Default_Is_20_Seconds()
		{
			var config = new CouchbaseClientConfiguration();
			Assert.That(config, Is.Not.Null, "Config was null");
			Assert.That(config.HttpRequestTimeout, Is.EqualTo(TimeSpan.FromSeconds(20)));
		}

		[Test]
		public void When_Http_Timeout_Is_Set_To_30_And_Using_App_Config_Value_Is_30_Seconds()
		{
			var config = ConfigurationManager.GetSection("httptimeout-explicit-config") as CouchbaseClientSection;
			Assert.That(config, Is.Not.Null, "Config was null");
			Assert.That(config.Servers.HttpRequestTimeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
		}

		[Test]
		public void When_Observe_Timeout_Is_Not_Set_And_Using_App_Config_Default_Is_1_Minute()
		{
			var config = ConfigurationManager.GetSection("observe-default-config") as CouchbaseClientSection;
			Assert.That(config, Is.Not.Null, "Config was null");
			Assert.That(config.Servers.ObserveTimeout, Is.EqualTo(TimeSpan.FromMinutes(1)));
		}

		[Test]
		public void When_Observe_Timeout_Is_Not_Set_And_Using_Code_Config_Default_Is_1_Minute()
		{
			var config = new CouchbaseClientConfiguration();
			Assert.That(config, Is.Not.Null, "Config was null");
			Assert.That(config.ObserveTimeout, Is.EqualTo(TimeSpan.FromMinutes(1)));
		}

		[Test]
		public void When_Observe_Timeout_Is_Set_To_30_And_Using_App_Config_Value_Is_30_Seconds()
		{
			var config = ConfigurationManager.GetSection("observe-explicit-config") as CouchbaseClientSection;
			Assert.That(config, Is.Not.Null, "Config was null");
			Assert.That(config.Servers.ObserveTimeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
		}
		#endregion

		#region HttpClient
		[Test]
		public void When_Initialize_Connection_Is_Not_Set_In_App_Config_Default_Is_True()
		{
			var config = ConfigurationManager.GetSection("min-config") as CouchbaseClientSection;
			Assert.That(config, Is.Not.Null, "Config was null");
			Assert.That(config.HttpClient.InitializeConnection, Is.True);
		}

		[Test]
		public void When_Initialize_Connection_Is_Set_In_App_Config_Property_Changes_From_Default()
		{
			var config = ConfigurationManager.GetSection("httpclient-config-noinitconn") as CouchbaseClientSection;
			Assert.That(config, Is.Not.Null, "Config was null");
			Assert.That(config.HttpClient.InitializeConnection, Is.False);
		}

		[Test]
		public void When_Initialize_Connection_Is_Not_Set_In_Code_Default_Is_True()
		{
			var config = new CouchbaseClientConfiguration();
			Assert.That(config, Is.Not.Null, "Config was null");
			Assert.That(config.HttpClient.InitializeConnection, Is.True);
		}

		[Test]
		public void When_Http_Client_Timeout_Is_Not_Set_In_App_Config_Default_Is_True()
		{
			var config = ConfigurationManager.GetSection("min-config") as CouchbaseClientSection;
			Assert.That(config, Is.Not.Null, "Config was null");
			Assert.That(config.HttpClient.Timeout, Is.EqualTo(TimeSpan.Parse("00:01:15")));
		}

		[Test]
		public void When_Http_Client_Timeout_Is_Set_In_App_Config_Property_Changes_From_Default()
		{
			var config = ConfigurationManager.GetSection("httpclient-config-noinitconn") as CouchbaseClientSection;
			Assert.That(config, Is.Not.Null, "Config was null");
			Assert.That(config.HttpClient.Timeout, Is.EqualTo(TimeSpan.Parse("00:00:45")));
		}

		[Test]
		public void When_Http_Client_Timeout_Is_Not_Set_In_Code_Default_Is_75_Seconds()
		{
			var config = new CouchbaseClientConfiguration();
			Assert.That(config, Is.Not.Null, "Config was null");
			Assert.That(config.HttpClient.Timeout, Is.EqualTo(TimeSpan.Parse("00:01:15")));
		}
		#endregion
	}
}

#region [ License information          ]
/* ************************************************************
 * 
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2012 Couchbase, Inc.
 *    
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *    
 *        http://www.apache.org/licenses/LICENSE-2.0
 *    
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *    
 * ************************************************************/
#endregion