using System;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Octopi.Endpoints;
using Octopi.Http;
using Octopi.Tests.Helpers;
using Xunit;

namespace Octopi.Tests
{
    /// <summary>
    /// Endpoint tests mostly just need to make sure they call the IApiClient with the correct 
    /// relative Uri. No need to fake up the response. All *those* tests are in ApiClientTests.cs.
    /// </summary>
    public class RepositoriesEndpointTests
    {
        public class TheConstructor
        {
            [Fact]
            public void EnsuresNonNullArguments()
            {
                Assert.Throws<ArgumentNullException>(() => new RepositoriesEndpoint(null));
            }
        }

        public class TheGetMethod
        {
            [Fact]
            public void RequestsCorrectUrl()
            {
                var client = Substitute.For<IApiClient<Repository>>();
                var repositoriesClient = new RepositoriesEndpoint(client);

                repositoriesClient.Get("fake", "repo");

                client.Received().Get(Arg.Is<Uri>(u => u.ToString() == "/repos/fake/repo"));
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var repositoriesClient = new RepositoriesEndpoint(Substitute.For<IApiClient<Repository>>());

                await AssertEx.Throws<ArgumentNullException>(async () => await repositoriesClient.Get(null, "name"));
                await AssertEx.Throws<ArgumentNullException>(async () => await repositoriesClient.Get("owner", null));
            }
        }

        public class TheGetAllForCurrentMethod
        {
            [Fact]
            public void RequestsTheCorrectUrlAndReturnsOrganizations()
            {
                var client = Substitute.For<IApiClient<Repository>>();
                var repositoriesClient = new RepositoriesEndpoint(client);

                repositoriesClient.GetAllForCurrent();

                client.Received()
                    .GetAll(Arg.Is<Uri>(u => u.ToString() == "user/repos"));
            }
        }

        public class TheGetAllForUserMethod
        {
            [Fact]
            public void RequestsTheCorrectUrlAndReturnsOrganizations()
            {
                var client = Substitute.For<IApiClient<Repository>>();
                var repositoriesClient = new RepositoriesEndpoint(client);

                repositoriesClient.GetAllForUser("username");

                client.Received()
                    .GetAll(Arg.Is<Uri>(u => u.ToString() == "/users/username/repos"));
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var reposEndpoint = new RepositoriesEndpoint(Substitute.For<IApiClient<Repository>>());

                AssertEx.Throws<ArgumentNullException>(async () => await reposEndpoint.GetAllForUser(null));
            }
        }

        public class TheGetAllForOrgMethod
        {
            [Fact]
            public void RequestsTheCorrectUrlAndReturnsOrganizations()
            {
                var client = Substitute.For<IApiClient<Repository>>();
                var repositoriesClient = new RepositoriesEndpoint(client);

                repositoriesClient.GetAllForOrg("orgname");

                client.Received()
                    .GetAll(Arg.Is<Uri>(u => u.ToString() == "/orgs/orgname/repos"));
            }

            [Fact]
            public void EnsuresNonNullArguments()
            {
                var reposEndpoint = new RepositoriesEndpoint(Substitute.For<IApiClient<Repository>>());

                AssertEx.Throws<ArgumentNullException>(async () => await reposEndpoint.GetAllForOrg(null));
            }
        }

        public class TheGetReadmeMethod
        {
            [Fact]
            public async Task ReturnsReadme()
            {
                string encodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello world"));
                var readmeInfo = new ReadmeResponse
                {
                    Content = encodedContent,
                    Encoding = "base64",
                    Name = "README.md",
                    Url = "https://github.example.com/readme.md",
                    HtmlUrl = "https://github.example.com/readme"
                };
                var client = Substitute.For<IApiClient<Repository>>();
                client.GetItem<ReadmeResponse>(Args.Uri).Returns(Task.FromResult(readmeInfo));
                client.GetHtml(Args.Uri).Returns(Task.FromResult("<html>README</html>"));
                var reposEndpoint = new RepositoriesEndpoint(client);

                var readme = await reposEndpoint.GetReadme("fake", "repo");

                readme.Name.Should().Be("README.md");
                client.Received().GetItem<ReadmeResponse>(Arg.Is<Uri>(u => u.ToString() == "/repos/fake/repo/readme"));
                client.DidNotReceive().GetHtml(Arg.Is<Uri>(u => u.ToString() == "https://github.example.com/readme"));
                var htmlReadme = await readme.GetHtmlContent();
                htmlReadme.Should().Be("<html>README</html>");
                client.Received().GetHtml(Arg.Is<Uri>(u => u.ToString() == "https://github.example.com/readme"));
            }
        }
    }
}