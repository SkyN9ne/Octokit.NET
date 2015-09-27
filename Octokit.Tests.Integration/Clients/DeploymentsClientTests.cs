﻿using System;
using System.Threading.Tasks;
using Octokit;
using Octokit.Tests.Integration;
using Xunit;

public class DeploymentsClientTests : IDisposable
{
    readonly IDeploymentsClient _deploymentsClient;
    readonly Repository _repository;
    readonly Commit _commit;
    readonly string _repositoryOwner;

    public DeploymentsClientTests()
    {
        var gitHubClient = Helper.GetAuthenticatedClient();

        _deploymentsClient = gitHubClient.Repository.Deployment;

        var newRepository = new NewRepository(Helper.MakeNameWithTimestamp("public-repo"))
        {
            AutoInit = true
        };

        _repository = gitHubClient.Repository.Create(newRepository).Result;
        _repositoryOwner = _repository.Owner.Login;

        var blob = new NewBlob
        {
            Content = "Hello World!",
            Encoding = EncodingType.Utf8
        };

        var blobResult = gitHubClient.GitDatabase.Blob.Create(_repositoryOwner, _repository.Name, blob).Result;

        var newTree = new NewTree();
        newTree.Tree.Add(new NewTreeItem
        {
            Type = TreeType.Blob,
            Mode = FileMode.File,
            Path = "README.md",
            Sha = blobResult.Sha
        });

        var treeResult = gitHubClient.GitDatabase.Tree.Create(_repositoryOwner, _repository.Name, newTree).Result;
        var newCommit = new NewCommit("test-commit", treeResult.Sha);
        _commit = gitHubClient.GitDatabase.Commit.Create(_repositoryOwner, _repository.Name, newCommit).Result;
    }
  
    [IntegrationTest]
    public async Task CanCreateDeployment()
    {
        var newDeployment = new NewDeployment(_commit.Sha) { AutoMerge = false };

        var deployment = await _deploymentsClient.Create(_repositoryOwner, _repository.Name, newDeployment);

        Assert.NotNull(deployment);
    }

    [IntegrationTest]
    public async Task CanGetDeployments()
    {
        var newDeployment = new NewDeployment(_commit.Sha) { AutoMerge = false };
        await _deploymentsClient.Create(_repositoryOwner, _repository.Name, newDeployment);
        
        var deployments = await _deploymentsClient.GetAll(_repositoryOwner, _repository.Name);

        Assert.NotEmpty(deployments);
    }

    public void Dispose()
    {
        Helper.DeleteRepo(_repository);
    }
}
