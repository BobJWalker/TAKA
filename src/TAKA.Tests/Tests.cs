using NUnit.Framework;
using TAKA.Web.Models;

namespace TAKA.Tests
{    
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            Quote.Initialize();
        }

        [Test]
        public void WhenGettingRandomQuote_WorksWithoutError()
        {
            Assert.That(Quote.GetRandomQuote().QuoteText != "Something went wrong");
        }

        [Test]
        public void WhenGettingRandomQuote_AuthorIsNotSystem()
        {
            Assert.That(Quote.GetRandomQuote().QuoteText != "System");
        }
    }
}