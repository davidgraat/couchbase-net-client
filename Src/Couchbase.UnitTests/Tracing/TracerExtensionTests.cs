using System.Collections.Generic;
using System.Linq;
using Couchbase.Analytics;
using Couchbase.IO.Operations;
using Couchbase.Logging;
using Couchbase.N1QL;
using Couchbase.Search;
using Couchbase.Search.Queries.Simple;
using Moq;
using NUnit.Framework;
using Couchbase.Tracing;
using OpenTracing.Tag;
using Couchbase.Utils;
using Couchbase.Views;
using OpenTracing.Mock;

namespace Couchbase.UnitTests.Tracing
{
    [TestFixture]
    public class TracerExtensionTests
    {
        private const string DefaultStatement = "SELECT 1 from `default`;";

        [TestCase(RedactionLevel.None)]
        [TestCase(RedactionLevel.Partial)]
        [TestCase(RedactionLevel.Full)]
        public void KV_default_span_tags_are_added_when_not_using_threshold_logging_tracer(RedactionLevel redactionLevel)
        {
            const string bucketName = "the-bucket";
            const string documentKey = "the-key";

            LogManager.RedactionLevel = redactionLevel;

            var expectedTags = new Dictionary<string, string>
            {
                {Tags.Component.Key, ClientIdentifier.GetClientDescription()},
                {Tags.DbType.Key, CouchbaseTags.DbTypeCouchbase},
                {Tags.SpanKind.Key, Tags.SpanKindClient},
                {CouchbaseTags.OperationId, "0x0"},
                {CouchbaseTags.Service, CouchbaseTags.ServiceKv},
                {Tags.DbInstance.Key, bucketName}
            };

            if (redactionLevel == RedactionLevel.None)
            {
                expectedTags.Add(CouchbaseTags.DocumentKey, documentKey);
            }

            var mockTracer = new MockTracer();
            var mockOperation = new Mock<IOperation>();
            mockOperation.Setup(x => x.Key).Returns(documentKey);

            using (mockTracer.StartParentScope(mockOperation.Object, bucketName))
            {
                // start and end scope
            }

            Assert.AreEqual(1, mockTracer.FinishedSpans().Count);
            var span = mockTracer.FinishedSpans().First();

            Assert.AreEqual(expectedTags, span.Tags);
        }

        [Test]
        public void View_default_span_tags_are_added_when_not_using_threshold_logging_tracer()
        {
            // need to reset to ensure request IDs match
            SequenceGenerator.Reset();

            const string designDocName = "design-doc";
            const string viewName = "view";

            var expectedTags = new Dictionary<string, string>
            {
                {Tags.Component.Key, ClientIdentifier.GetClientDescription()},
                {Tags.DbType.Key, CouchbaseTags.DbTypeCouchbase},
                {Tags.SpanKind.Key, Tags.SpanKindClient},
                {CouchbaseTags.OperationId, "1"},
                {CouchbaseTags.ViewDesignDoc, designDocName},
                {CouchbaseTags.ViewName, viewName},
                {CouchbaseTags.Service, CouchbaseTags.ServiceView}
            };

            var mockViewQuery = new Mock<IViewQuery>();
            mockViewQuery.Setup(x => x.DesignDocName).Returns(designDocName);
            mockViewQuery.Setup(x => x.ViewName).Returns(viewName);

            var mockTracer = new MockTracer();
            using (mockTracer.StartParentScope(mockViewQuery.Object))
            {
                // start and end scope
            }

            Assert.AreEqual(1, mockTracer.FinishedSpans().Count);
            var span = mockTracer.FinishedSpans().First();

            Assert.AreEqual(expectedTags, span.Tags);
        }

        [Test]
        public void Query_default_span_tags_are_added_when_not_using_threshold_logging_tracer()
        {
            var expectedTags = new Dictionary<string, string>
            {
                {Tags.Component.Key, ClientIdentifier.GetClientDescription()},
                {Tags.DbType.Key, CouchbaseTags.DbTypeCouchbase},
                {Tags.SpanKind.Key, Tags.SpanKindClient},
                {CouchbaseTags.OperationId, "1"},
                {CouchbaseTags.Service, CouchbaseTags.ServiceQuery},
                {Tags.DbStatement.Key, DefaultStatement}
            };

            var mockQuery = new Mock<IQueryRequest>();
            mockQuery.Setup(x => x.GetOriginalStatement()).Returns(DefaultStatement);
            mockQuery.SetupGet(x => x.CurrentContextId).Returns("1");

            var mockTracer = new MockTracer();
            using (mockTracer.StartParentScope(mockQuery.Object))
            {
                // start and end scope
            }

            Assert.AreEqual(1, mockTracer.FinishedSpans().Count);
            var span = mockTracer.FinishedSpans().First();

            Assert.AreEqual(expectedTags, span.Tags);
        }

        [Test]
        public void Search_default_span_tags_are_added_when_not_using_threshold_logging_tracer()
        {
            // need to reset to ensure request IDs match
            SequenceGenerator.Reset();

            var expectedTags = new Dictionary<string, string>
            {
                {Tags.Component.Key, ClientIdentifier.GetClientDescription()},
                {Tags.DbType.Key, CouchbaseTags.DbTypeCouchbase},
                {Tags.SpanKind.Key, Tags.SpanKindClient},
                {CouchbaseTags.OperationId, "1"},
                {CouchbaseTags.Service, CouchbaseTags.ServiceSearch}
            };

            var searchQuery = new SearchQuery
            {
                Index = "index",
                Query = new MatchQuery("test")
            };

            var mockTracer = new MockTracer();
            using (mockTracer.StartParentScope(searchQuery))
            {
                // start and end scope
            }

            Assert.AreEqual(1, mockTracer.FinishedSpans().Count);
            var span = mockTracer.FinishedSpans().First();

            Assert.AreEqual(expectedTags, span.Tags);
        }

        [Test]
        public void Analytics_default_span_tags_are_added_when_not_using_threshold_logging_tracer()
        {
            var expectedTags = new Dictionary<string, string>
            {
                {Tags.Component.Key, ClientIdentifier.GetClientDescription()},
                {Tags.DbType.Key, CouchbaseTags.DbTypeCouchbase},
                {Tags.SpanKind.Key, Tags.SpanKindClient},
                {CouchbaseTags.OperationId, "1"},
                {CouchbaseTags.Service, CouchbaseTags.ServiceAnalytics},
                {Tags.DbStatement.Key, DefaultStatement}
            };

            var mockAnalyticsQuery = new Mock<IAnalyticsRequest>();
            mockAnalyticsQuery.Setup(x => x.OriginalStatement).Returns(DefaultStatement);
            mockAnalyticsQuery.Setup(x => x.CurrentContextId).Returns("1");

            var mockTracer = new MockTracer();
            using (mockTracer.StartParentScope(mockAnalyticsQuery.Object))
            {
                // start and end scope
            }

            Assert.AreEqual(1, mockTracer.FinishedSpans().Count);
            var span = mockTracer.FinishedSpans().First();

            Assert.AreEqual(expectedTags, span.Tags);
        }
    }
}
